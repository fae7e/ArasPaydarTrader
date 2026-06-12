using ArasTrader.Application.Interfaces;
using ArasTrader.Infrastructure.ExternalApi.Models;
using ArasTrader.Infrastructure.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace ArasTrader.Infrastructure.ExternalApi;
public class TokenService : ITokenService
{
    private const string CacheKey = "external_api_token";
    private static readonly TimeSpan CacheBuffer = TimeSpan.FromMinutes(2);

    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly TokenProviderSettings _settings;
    private readonly ILogger<TokenService> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private string? _currentToken;
    public TokenService(
          HttpClient httpClient,
          IMemoryCache cache,
          IOptions<TokenProviderSettings> settings,
          ILogger<TokenService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string> GetValidTokenAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue(CacheKey, out string? cachedToken) && cachedToken is not null)
            return cachedToken;

        await _lock.WaitAsync(ct);
        try
        {
            if (_cache.TryGetValue(CacheKey, out cachedToken) && cachedToken is not null)
                return cachedToken;

            TokenResponse tokenResponse = _currentToken is not null
                ? await RefreshTokenAsync(_currentToken, ct) ?? await GetNewTokenAsync(ct)
                : await GetNewTokenAsync(ct);

            _currentToken = tokenResponse.AccessToken;

            // Cache تا 2 دقیقه قبل از انقضای واقعی توکن
            var ttl = tokenResponse.ExpiresAtUtc - DateTime.UtcNow - CacheBuffer;
            if (ttl > TimeSpan.Zero)
                _cache.Set(CacheKey, tokenResponse.AccessToken, ttl);

            return tokenResponse.AccessToken;
        }
        finally
        {
            _lock.Release();
        }
    }

    public void InvalidateToken()
    {
        _cache.Remove(CacheKey);
        _currentToken = null;
    }

    private async Task<TokenResponse> GetNewTokenAsync(CancellationToken ct)
    {
        _logger.LogInformation("Requesting new token with username/password.");

        var response = await _httpClient.PostAsJsonAsync(
            "/api/auth/token", new TokenRequest(_settings.Username, _settings.Password), ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TokenResponse>(ct)
               ?? throw new InvalidOperationException("Token response was empty.");
    }

    private async Task<TokenResponse?> RefreshTokenAsync(string currentToken, CancellationToken ct)
    {
        _logger.LogInformation("Refreshing existing token.");
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "/api/auth/refresh", new RefreshTokenRequest(currentToken), ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Token refresh failed ({Status}). Will re-authenticate.", response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<TokenResponse>(ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token refresh threw an exception. Will re-authenticate.");
            return null;
        }
    }
}
