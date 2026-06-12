using ArasTrader.Application.Interfaces;
using ArasTrader.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ArasTrader.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICustomerSyncService, CustomerSyncService>();
        services.AddScoped<ICustomerQueryService, CustomerQueryService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IWalletQueryService, WalletQueryService>();
        return services;
    }

}
