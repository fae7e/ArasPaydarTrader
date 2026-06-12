using ArasTrader.Application.Dtos;
using ArasTrader.Application.Interfaces;
using ArasTrader.Domain.Entities;
using ArasTrader.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace ArasTrader.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrderService> _logger;
    public OrderService(IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    public async Task<PlaceOrderResponse> PlaceOrderAsync(PlaceOrderRequest request, CancellationToken ct = default)
    {  
        // Idempotency: return existing order if same key already used
        var existing = await _orderRepository.GetByIdempotencyKeyAsync(request.IdempotencyKey, ct);
        if (existing is not null)
        {
            _logger.LogInformation("Idempotent order request detected. Key={Key}", request.IdempotencyKey);
            return MapToResponse(existing, isIdempotentDuplicate: true);
        }

        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, ct);
        if (customer is null)
            throw new InvalidOperationException($"Customer {request.CustomerId} not found.");

        var order = Order.Create(request.CustomerId, request.Type, request.Amount, request.IdempotencyKey);
        await _orderRepository.AddAsync(order, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Order {OrderId} created for customer {CustomerId}.", order.Id, order.CustomerId);

        return MapToResponse(order);
    }
    public async Task<PlaceOrderResponse?> GetOrderAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, ct);
        return order is null ? null : MapToResponse(order);
    }

    private static PlaceOrderResponse MapToResponse(Order order, bool isIdempotentDuplicate = false) =>
       new(order.Id, order.CustomerId, order.Type, order.Amount, order.Status, order.CreatedAt, isIdempotentDuplicate);
}
