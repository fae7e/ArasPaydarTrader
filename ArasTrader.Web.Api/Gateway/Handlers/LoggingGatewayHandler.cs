using ArasTrader.Application.Dtos;
using ArasTrader.Web.Api.Gateway.Abstractions;

namespace ArasTrader.Web.Api.Gateway.Handlers;

public class LoggingGatewayHandler : IOrderGatewayHandler
{
    private readonly ILogger<LoggingGatewayHandler> _logger;

    public LoggingGatewayHandler(ILogger<LoggingGatewayHandler> logger)
        => _logger = logger;

    public async Task<PlaceOrderResponse> HandleAsync(
      PlaceOrderRequest request,
      Func<PlaceOrderRequest, Task<PlaceOrderResponse>> next,
      CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Placing order — Customer: {CustomerId}, Type: {Type}, Amount: {Amount}, Key: {Key}",
            request.CustomerId, request.Type, request.Amount, request.IdempotencyKey);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var result = await next(request);
            stopwatch.Stop();
            _logger.LogInformation(
                "Order placed — OrderId: {OrderId}, Elapsed: {Ms}ms, IsDuplicate: {Dup}",
                result.OrderId, stopwatch.ElapsedMilliseconds, result.IsIdempotentDuplicate);
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Order placement failed after {Ms}ms.", stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
