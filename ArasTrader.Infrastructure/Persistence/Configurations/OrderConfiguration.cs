using ArasTrader.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArasTrader.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Amount).IsRequired().HasColumnType("decimal(18,6)");
        builder.Property(o => o.Type).IsRequired();
        builder.Property(o => o.Status).IsRequired();
        builder.Property(o => o.IdempotencyKey).IsRequired().HasMaxLength(128);

        builder.HasIndex(o => o.IdempotencyKey).IsUnique();
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.CustomerId);
    }
}