using ArasTrader.Application.Dtos;
using ArasTrader.Application.Interfaces;

namespace ArasTrader.Web.Api.Endpoints.Order;

public class GetOrderEndpoint : EndpointBase
{
    public override void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders/{id:guid}", async (
            Guid id,
            IOrderService orderService,
            CancellationToken ct) =>
        {
            var order = await orderService.GetOrderAsync(id, ct);
            return order is null ? Results.NotFound() : Results.Ok(order);
        })
        .WithTags("Orders")
        .WithSummary("Returns a single order by ID.")
        .Produces<PlaceOrderResponse>()
        .Produces(StatusCodes.Status404NotFound);
    }
}
