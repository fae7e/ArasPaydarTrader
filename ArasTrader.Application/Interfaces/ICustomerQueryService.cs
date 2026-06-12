using ArasTrader.Application.Dtos;

namespace ArasTrader.Application.Interfaces;
public interface ICustomerQueryService
{
    Task<IReadOnlyList<CustomerDto>> GetAllAsync(CancellationToken ct = default);
}
