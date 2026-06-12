using ArasTrader.Domain.Entities;
using ArasTrader.Domain.Enums;
using ArasTrader.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArasTrader.Infrastructure.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context) => _context = context;

    public async Task AddAsync(Order order, CancellationToken ct = default)
      => await _context.Orders.AddAsync(order, ct);

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
      => _context.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);

    public Task<Order?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default)
        => _context.Orders.FirstOrDefaultAsync(o => o.IdempotencyKey == idempotencyKey, ct);

    public async Task<IReadOnlyList<Order>> GetByStatusAsync(OrderStatus status, CancellationToken ct = default)
        => await _context.Orders.Where(o => o.Status == status).ToListAsync(ct);

    public async Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
        => await _context.Orders.Where(o => o.CustomerId == customerId).ToListAsync(ct);

    public async Task<bool> TryClaimForProcessingAsync(Guid orderId, CancellationToken ct = default)
    {
        // UPDATE orders SET Status = Processing WHERE Id = @id AND Status = Pending
        // Returns 1 if the order was Pending and we claimed it; 0 if already claimed by another job instance.
        int affected = await _context.Orders
            .Where(o => o.Id == orderId && o.Status == OrderStatus.Pending)
            .ExecuteUpdateAsync(
                s => s.SetProperty(o => o.Status, OrderStatus.Processing),
                ct);

        return affected > 0;
    }
}
