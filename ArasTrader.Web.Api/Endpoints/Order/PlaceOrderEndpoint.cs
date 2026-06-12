using ArasTrader.Application.Dtos;
using ArasTrader.Web.Api.Gateway;

namespace ArasTrader.Web.Api.Endpoints.Order;

public class PlaceOrderEndpoint : EndpointBase
{
    public override void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/orders", async (
            PlaceOrderRequestDto dto,
            OrderGatewayPipeline gateway,
            CancellationToken ct) =>
        {
            var request = new PlaceOrderRequest(dto.CustomerId, dto.Type, dto.Amount, dto.IdempotencyKey);
            var response = await gateway.ExecuteAsync(request, ct);

            return response.IsIdempotentDuplicate
                ? Results.Ok(response)
                : Results.Created($"/api/orders/{response.OrderId}", response);
        })
        .WithTags("Orders")
        .WithSummary("Places a BUY or SELL order. Wallet is updated asynchronously by the Hangfire job.")
        .Produces<PlaceOrderResponse>(StatusCodes.Status201Created)
        .Produces<PlaceOrderResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}
