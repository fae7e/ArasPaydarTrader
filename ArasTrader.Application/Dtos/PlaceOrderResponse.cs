using ArasTrader.Domain.Enums;

namespace ArasTrader.Application.Dtos;

public record PlaceOrderResponse(
    Guid OrderId,
    Guid CustomerId,
    OrderType Type,
    decimal Amount,
    OrderStatus Status,
    DateTime CreatedAt,
    bool IsIdempotentDuplicate = false
    );