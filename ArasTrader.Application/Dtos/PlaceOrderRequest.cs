using ArasTrader.Domain.Enums;

namespace ArasTrader.Application.Dtos;

public record PlaceOrderRequest(
    Guid CustomerId,
    OrderType Type,
    decimal Amount,
    string IdempotencyKey
    );

