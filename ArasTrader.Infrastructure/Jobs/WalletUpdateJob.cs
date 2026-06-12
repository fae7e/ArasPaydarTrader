using ArasTrader.Domain.Enums;
using ArasTrader.Domain.Interfaces.Repositories;
using ArasTrader.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArasTrader.Infrastructure.Jobs;

public class WalletUpdateJob
{
    private readonly IOrderRepository _orderRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<WalletUpdateJob> _logger;
    private const int MaxConcurrencyRetries = 5;

    public WalletUpdateJob(
       IOrderRepository orderRepository,
       IWalletRepository walletRepository,
       IUnitOfWork unitOfWork,
       ApplicationDbContext context,
       ILogger<WalletUpdateJob> logger)
    {
        _orderRepository = orderRepository;
        _walletRepository = walletRepository;
        _unitOfWork = unitOfWork;
        _context = context;
        _logger = logger;
    }
    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("WalletUpdateJob started.");

        var pendingOrders = await _orderRepository.GetByStatusAsync(OrderStatus.Pending, ct);

        _logger.LogInformation("Found {Count} pending orders.", pendingOrders.Count);

        foreach (var order in pendingOrders)
        {
            await ProcessOrderAsync(order.Id, ct);
        }

        _logger.LogInformation("WalletUpdateJob finished.");
    }
    private async Task ProcessOrderAsync(Guid orderId, CancellationToken ct)
    {
        //1.claim the order via repository and prevents double-processing.
        bool claimed = await _orderRepository.TryClaimForProcessingAsync(orderId, ct);
        if (!claimed)
        {
            _logger.LogDebug("Order {OrderId} already claimed by another instance. Skipping.", orderId);
            return;
        }

        //2: Load the order (now in Processing status).
        var order = await _orderRepository.GetByIdAsync(orderId, ct);
        if (order is null)
        {
            _logger.LogWarning("Order {OrderId} not found after claiming. Skipping.", orderId);
            return;
        }

        //3. update wallet change with optimistic concurrency retry.
        for (int attempt = 1; attempt <= MaxConcurrencyRetries; attempt++)
        {
            try
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(ct);
                try
                {
                    var wallet = await _walletRepository.GetByCustomerIdAsync(order.CustomerId, ct);

                    if (wallet is null)
                    {
                        _logger.LogError(
                            "Wallet not found for customer {CustomerId}. Marking order {OrderId} as Failed.",
                            order.CustomerId, orderId);
                        order.MarkAsFailed();
                        await _unitOfWork.SaveChangesAsync(ct);
                        await transaction.CommitAsync(ct);
                        return;
                    }

                    if (order.Type == OrderType.Buy)
                        wallet.Debit(order.Amount);
                    else
                        wallet.Credit(order.Amount);

                    order.MarkAsProcessed();
                    _walletRepository.Update(wallet);

                    await _unitOfWork.SaveChangesAsync(ct);
                    await transaction.CommitAsync(ct);

                    _logger.LogInformation(
                        "Order {OrderId} processed on attempt {Attempt}.", orderId, attempt);
                    return;
                }
                catch
                {
                    await transaction.RollbackAsync(ct);
                    throw;
                }
            }
            catch (DbUpdateConcurrencyException ex) when (attempt < MaxConcurrencyRetries)
            {
                _logger.LogWarning(
                    "Concurrency conflict on order {OrderId} (attempt {Attempt}/{Max}). Retrying.",
                    orderId, attempt, MaxConcurrencyRetries);

                foreach (var entry in ex.Entries)
                    await entry.ReloadAsync(ct);

                await Task.Delay(50 * attempt, ct);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Business rule violation for order {OrderId}. Marking as Failed.", orderId);
                order.MarkAsFailed();
                await _unitOfWork.SaveChangesAsync(ct);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error for order {OrderId}. Marking as Failed.", orderId);
                order.MarkAsFailed();
                try { await _unitOfWork.SaveChangesAsync(ct); } catch { /* best effort */ }
                return;
            }
        }

        _logger.LogError(
            "Order {OrderId} exhausted {Max} retries. Marking as Failed.", orderId, MaxConcurrencyRetries);
        order.MarkAsFailed();
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
