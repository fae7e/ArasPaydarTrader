using ArasTrader.Domain.Entities;

namespace ArasTrader.Domain.Interfaces.Repositories;

public interface IWalletRepository
{
    Task AddAsync(Wallet wallet, CancellationToken ct = default);
    Task<Wallet?> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    void Update(Wallet wallet);
}
