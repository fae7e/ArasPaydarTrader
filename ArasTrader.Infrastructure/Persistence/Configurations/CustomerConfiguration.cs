using ArasTrader.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArasTrader.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration: IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.NationalCode).IsRequired().HasMaxLength(10);
        builder.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.LastName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.FatherName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.BirthCertificationNumber).IsRequired().HasMaxLength(20);
        builder.Property(c => c.RegistrationNumber).IsRequired().HasMaxLength(20);
        builder.Property(c => c.BranchName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.MobileNumber).IsRequired().HasMaxLength(15);

        builder.HasIndex(c => c.NationalCode).IsUnique();

        builder.HasOne(c => c.Wallet)
               .WithOne(w => w.Customer)
               .HasForeignKey<Wallet>(w => w.CustomerId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Orders)
               .WithOne(o => o.Customer)
               .HasForeignKey(o => o.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
