using ArasTrader.Application.Interfaces;
using ArasTrader.Application.Services;
using ArasTrader.Domain.Interfaces.Repositories;
using ArasTrader.Infrastructure.ExternalApi;
using ArasTrader.Infrastructure.Jobs;
using ArasTrader.Infrastructure.Persistence;
using ArasTrader.Infrastructure.Persistence.Repositories;
using ArasTrader.Infrastructure.Settings;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using System;

namespace ArasTrader.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
       this IServiceCollection services,
       IConfiguration configuration)
    {

        //Settings
        services.Configure<TokenProviderSettings>(
            configuration.GetSection(TokenProviderSettings.SectionName));

        services.Configure<CustomerApiSettings>(
            configuration.GetSection(CustomerApiSettings.SectionName));

        var apiSettings = configuration
            .GetSection(TokenProviderSettings.SectionName)
            .Get<TokenProviderSettings>()!;

        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Repositories      
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();

        // Caching (used by TokenService)
        services.AddMemoryCache();
        
        // Token Service — singleton so the in-memory token cache is shared across requests
        services.AddHttpClient<ITokenService, TokenService>(client =>
        {
            client.BaseAddress = new Uri(apiSettings.BaseUrl);
        })
        .AddPolicyHandler(GetRetryPolicy());

        // External API client
        services.AddHttpClient<IExternalApiClient, ExternalApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiSettings.BaseUrl);
        })
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetRateLimitRetryPolicy());


        services.AddHangfire(config => config
                  .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                  .UseSimpleAssemblyNameTypeSerializer()
                  .UseRecommendedSerializerSettings()
                  .UseMemoryStorage());

        services.AddHangfireServer();
        services.AddScoped<WalletUpdateJob>();
        
        return services;

    }
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
          HttpPolicyExtensions
              .HandleTransientHttpError()
              .WaitAndRetryAsync(3, retryAttempt =>
                  TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

    private static IAsyncPolicy<HttpResponseMessage> GetRateLimitRetryPolicy() =>
        Policy<HttpResponseMessage>
            .HandleResult(r => r.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(retryAttempt * 10));

}
