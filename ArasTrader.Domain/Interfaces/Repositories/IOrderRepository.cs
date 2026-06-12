using ArasTrader.Domain.Entities;
using ArasTrader.Domain.Enums;

namespace ArasTrader.Domain.Interfaces.Repositories;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Order?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> GetByStatusAsync(OrderStatus status, CancellationToken ct = default);
    Task<bool> TryClaimForProcessingAsync(Guid orderId, CancellationToken ct = default);
}
