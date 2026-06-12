using ArasTrader.Domain.Entities;

namespace ArasTrader.Domain.Interfaces.Repositories;

public interface ICustomerRepository
{
    Task AddAsync(Customer customer, CancellationToken ct = default);
    Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct = default);
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Customer?> GetByNationalCodeAsync(string nationalCode, CancellationToken ct = default);
    void Update(Customer customer);
}
