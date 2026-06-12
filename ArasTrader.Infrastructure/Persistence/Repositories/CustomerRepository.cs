using ArasTrader.Domain.Entities;
using ArasTrader.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;

namespace ArasTrader.Infrastructure.Persistence.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly ApplicationDbContext _context;
    public CustomerRepository(ApplicationDbContext context) => _context = context;

    public async Task AddAsync(Customer customer, CancellationToken ct = default)
      => await _context.Customers.AddAsync(customer, ct);

    public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct = default)
    => await _context.Customers.AsNoTracking().ToListAsync(ct);
    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default)
       => _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<Customer?> GetByNationalCodeAsync(string nationalCode, CancellationToken ct = default)
        => _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.NationalCode == nationalCode, ct);

    public void Update(Customer customer)
       => _context.Customers.Update(customer);
}
