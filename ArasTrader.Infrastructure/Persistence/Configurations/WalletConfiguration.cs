using ArasTrader.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArasTrader.Infrastructure.Persistence.Configurations;

public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Balance).IsRequired().HasColumnType("decimal(18,6)");

        // row version in db for  race Condition  
        // EF will include Version in UPDATE WHERE
        builder.Property(w => w.Version)
            .IsRequired()
            .IsConcurrencyToken();

        builder.HasIndex(w => w.CustomerId).IsUnique();
    }
}
