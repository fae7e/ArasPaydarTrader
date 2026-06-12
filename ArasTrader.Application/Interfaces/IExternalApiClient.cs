using ArasTrader.Application.Dtos;

namespace ArasTrader.Application.Interfaces;

public interface IExternalApiClient
{
    Task<IReadOnlyList<CustomerDto>> GetCustomersAsync(CancellationToken ct = default);
}
