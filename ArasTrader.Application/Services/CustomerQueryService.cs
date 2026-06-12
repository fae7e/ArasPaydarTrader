using ArasTrader.Application.Dtos;
using ArasTrader.Application.Interfaces;
using ArasTrader.Domain.Interfaces.Repositories;
using System;

namespace ArasTrader.Application.Services;

public class CustomerQueryService : ICustomerQueryService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerQueryService(ICustomerRepository customerRepository)
        => _customerRepository = customerRepository;

    public async Task<IReadOnlyList<CustomerDto>> GetAllAsync(CancellationToken ct = default)
    {
        var customers = await _customerRepository.GetAllAsync(ct);

        return customers.Select(c => new CustomerDto(
            c.Id, c.NationalCode, c.FirstName, c.LastName,
            c.FatherName, c.BirthCertificationNumber, c.RegistrationNumber,
            c.BirthDate, c.BranchName, c.MobileNumber
        )).ToList().AsReadOnly();
    }
}
