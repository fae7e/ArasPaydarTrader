using ArasTrader.Domain.Entities;
using ArasTrader.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;

namespace ArasTrader.Infrastructure.Persistence.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly ApplicationDbContext _context;

    public WalletRepository(ApplicationDbContext context) => _context = context;
    public async Task AddAsync(Wallet wallet, CancellationToken ct = default)
      => await _context.Wallets.AddAsync(wallet, ct);

    public Task<Wallet?> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
      => _context.Wallets.FirstOrDefaultAsync(w => w.CustomerId == customerId, ct);

    public void Update(Wallet wallet)
       => _context.Wallets.Update(wallet);
}
