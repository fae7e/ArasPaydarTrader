using ArasTrader.Domain.Entities;
using ArasTrader.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;

namespace ArasTrader.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IUnitOfWork
{

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {            
    }
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public new async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await base.SaveChangesAsync(ct);
}
