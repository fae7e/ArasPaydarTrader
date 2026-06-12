namespace ArasTrader.Domain.Entities;

public class Wallet
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public decimal Balance { get; private set; }
    public long Version { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Customer? Customer { get; private set; }

    private Wallet() { }

    public static Wallet Create(Guid customerId, decimal initialBalance = 0m)
    {
        return new Wallet
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Balance = initialBalance,
            Version = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Debit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Debit amount must be positive.", nameof(amount));
        if (Balance < amount)
            throw new InvalidOperationException($"Insufficient balance. Current: {Balance}, Required: {amount}.");

        Balance -= amount;
        Version++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Credit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Credit amount must be positive.", nameof(amount));

        Balance += amount;
        Version++;
        UpdatedAt = DateTime.UtcNow;
    }
}
