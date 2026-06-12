using ArasTrader.Application.Dtos;
using ArasTrader.Application.Interfaces;
using ArasTrader.Domain.Interfaces.Repositories;

namespace ArasTrader.Application.Services;

public class WalletQueryService : IWalletQueryService
{
    private readonly IWalletRepository _walletRepository;
    public WalletQueryService(IWalletRepository walletRepository)
            => _walletRepository = walletRepository;

    public async Task<WalletDto?> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        var wallet = await _walletRepository.GetByCustomerIdAsync(customerId, ct);
        return wallet is null ? null : new WalletDto(wallet.Id, wallet.CustomerId, wallet.Balance);
    }
}
