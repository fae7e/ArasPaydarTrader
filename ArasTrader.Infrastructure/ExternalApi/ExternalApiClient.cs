

using ArasTrader.Application.Dtos;
using ArasTrader.Application.Interfaces;
using ArasTrader.Infrastructure.ExternalApi.Models;
using ArasTrader.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;

namespace ArasTrader.Infrastructure.ExternalApi;

public class ExternalApiClient : IExternalApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ITokenService _tokenService;
    private readonly CustomerApiSettings _settings;
    private readonly ILogger<ExternalApiClient> _logger;

    public ExternalApiClient(
        HttpClient httpClient,
        ITokenService tokenService,
        IOptions<CustomerApiSettings> settings,
        ILogger<ExternalApiClient> logger)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CustomerDto>> GetCustomersAsync(CancellationToken ct = default)
    {
        // Retry once on 401 by refreshing token
        for (int attempt = 0; attempt < 2; attempt++)
        {
            var token = await _tokenService.GetValidTokenAsync(ct);
            using var request = new HttpRequestMessage(HttpMethod.Get, _settings.GetCustomers);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request, ct);

            if (response.StatusCode == HttpStatusCode.Unauthorized && attempt == 0)
            {
                _logger.LogWarning("Received 401 on customer fetch. Invalidating token and retrying.");
                _tokenService.InvalidateToken();
                continue;
            }

            response.EnsureSuccessStatusCode();

            var apiModels = await response.Content.ReadFromJsonAsync<List<CustomerApiModel>>(ct)
                            ?? new List<CustomerApiModel>();

            return apiModels.Select(m => new CustomerDto(
                Guid.Empty,
                m.NationalCode,
                m.FirstName,
                m.LastName,
                m.FatherName,
                m.BirthCertificationNumber,
                m.RegistrationNumber,
                m.BirthDate,
                m.BranchName,
                m.MobileNumber
            )).ToList().AsReadOnly();
        }

        throw new InvalidOperationException("Failed to fetch customers after token refresh.");
    }
}
