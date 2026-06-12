using ArasTrader.Application.Dtos;

namespace ArasTrader.Application.Interfaces;

public interface IWalletQueryService
{
    Task<WalletDto?> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);

}
