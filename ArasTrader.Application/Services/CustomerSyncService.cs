using ArasTrader.Application.Interfaces;
using ArasTrader.Domain.Entities;
using ArasTrader.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArasTrader.Application.Services;

public class CustomerSyncService : ICustomerSyncService
{
    private readonly IExternalApiClient _apiClient;
    private readonly ICustomerRepository _customerRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CustomerSyncService> _logger;

    public CustomerSyncService(
      IExternalApiClient apiClient,
      ICustomerRepository customerRepository,
      IWalletRepository walletRepository,
      IUnitOfWork unitOfWork,
      ILogger<CustomerSyncService> logger)
    {
        _apiClient = apiClient;
        _customerRepository = customerRepository;
        _walletRepository = walletRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }


    public async Task<int> SyncCustomersAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Starting customer sync from external API.");

        var apiCustomers = await _apiClient.GetCustomersAsync(ct);
        int upsertedCount = 0;

        foreach (var dto in apiCustomers)
        {
            var existing = await _customerRepository.GetByNationalCodeAsync(dto.NationalCode, ct);

            if (existing is null)
            {
                var customer = Customer.Create(
                    dto.NationalCode,
                    dto.FirstName,
                    dto.LastName,
                    dto.FatherName,
                    dto.BirthCertificationNumber,
                    dto.RegistrationNumber,
                    dto.BirthDate,
                    dto.BranchName,
                    dto.MobileNumber);

                await _customerRepository.AddAsync(customer, ct);

                var wallet = Wallet.Create(customer.Id);
                await _walletRepository.AddAsync(wallet, ct);
            }
            else
            {
                existing.Update(
                    dto.FirstName,
                    dto.LastName,
                    dto.FatherName,
                    dto.BirthCertificationNumber,
                    dto.RegistrationNumber,
                    dto.BirthDate,
                    dto.BranchName,
                    dto.MobileNumber);

                _customerRepository.Update(existing);
            }

            upsertedCount++;
        }

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Customer sync complete. Upserted {Count} customers.", upsertedCount);
        return upsertedCount;
    }
}
