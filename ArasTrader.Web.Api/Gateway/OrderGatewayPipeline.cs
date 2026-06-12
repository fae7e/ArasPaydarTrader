using ArasTrader.Application.Dtos;
using ArasTrader.Application.Interfaces;
using ArasTrader.Web.Api.Gateway.Abstractions;

namespace ArasTrader.Web.Api.Gateway;

public class OrderGatewayPipeline
{
    private readonly IEnumerable<IOrderGatewayHandler> _handlers;
    private readonly IOrderService _orderService;
    public OrderGatewayPipeline(
     IEnumerable<IOrderGatewayHandler> handlers,
     IOrderService orderService)
    {
        _handlers = handlers;
        _orderService = orderService;
    }

    public Task<PlaceOrderResponse> ExecuteAsync(PlaceOrderRequest request, CancellationToken ct = default)
    {
        Func<PlaceOrderRequest, Task<PlaceOrderResponse>> terminal =
            req => _orderService.PlaceOrderAsync(req, ct);

        // Build pipeline right-to-left so the first handler in the list runs first
        var pipeline = _handlers
            .Reverse()
            .Aggregate(terminal, (next, handler) =>
                req => handler.HandleAsync(req, next, ct));

        return pipeline(request);
    }
}
