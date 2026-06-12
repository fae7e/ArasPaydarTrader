using ArasTrader.Application.Dtos;

namespace ArasTrader.Web.Api.Gateway.Abstractions;

public interface IOrderGatewayHandler
{
    Task<PlaceOrderResponse> HandleAsync(
        PlaceOrderRequest request,
        Func<PlaceOrderRequest, Task<PlaceOrderResponse>> next,
        CancellationToken ct = default);
}
