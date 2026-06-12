using ArasTrader.Domain.Enums;

namespace ArasTrader.Web.Api.Endpoints.Order;
public record PlaceOrderRequestDto(
    Guid CustomerId,
    OrderType Type,
    decimal Amount,
    string IdempotencyKey
);

