using ArasTrader.Application.Dtos;
using ArasTrader.Web.Api.Gateway.Abstractions;

namespace ArasTrader.Web.Api.Gateway.Handlers;

public class ValidationGatewayHandler : IOrderGatewayHandler
{
    public Task<PlaceOrderResponse> HandleAsync(
       PlaceOrderRequest request,
       Func<PlaceOrderRequest, Task<PlaceOrderResponse>> next,
       CancellationToken ct = default)
    {
        var errors = new List<string>();

        if (request.CustomerId == Guid.Empty)
            errors.Add("CustomerId is required.");

        if (request.Amount <= 0)
            errors.Add("Amount must be greater than zero.");

        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
            errors.Add("IdempotencyKey is required.");

        if (request.IdempotencyKey?.Length > 128)
            errors.Add("IdempotencyKey must not exceed 128 characters.");

        if (errors.Count > 0)
            throw new ArgumentException(string.Join(" | ", errors));

        return next(request);
    }
}
