using ArasTrader.Domain.Enums;

namespace ArasTrader.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public OrderType Type { get; private set; }
    public decimal Amount { get; private set; }
    public OrderStatus Status { get; private set; }
    public string IdempotencyKey { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    public Customer? Customer { get; private set; }

    private Order() { }

    public static Order Create(
        Guid customerId,
        OrderType type,
        decimal amount,
        string idempotencyKey)
    {
        if (amount <= 0)
            throw new ArgumentException("Order amount must be positive.", nameof(amount));

        return new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Type = type,
            Amount = amount,
            Status = OrderStatus.Pending,
            IdempotencyKey = idempotencyKey,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsProcessing()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot set order to Processing from status {Status}.");
        Status = OrderStatus.Processing;
    }

    public void MarkAsProcessed()
    {
        Status = OrderStatus.Processed;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed()
    {
        Status = OrderStatus.Failed;
        ProcessedAt = DateTime.UtcNow;
    }
}
