using ArasTrader.Application.Dtos;

namespace ArasTrader.Application.Interfaces;

public interface IOrderService
{
    Task<PlaceOrderResponse> PlaceOrderAsync(PlaceOrderRequest request, CancellationToken ct = default);
    Task<PlaceOrderResponse?> GetOrderAsync(Guid orderId, CancellationToken ct = default);
}
