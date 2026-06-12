namespace ArasTrader.Infrastructure.Settings;

public class CustomerApiSettings
{
    public const string SectionName = "CustomerApiSettings";
    public string BaseUrl { get; set; } = string.Empty;
    public string GetCustomers { get; set; } = string.Empty;
}
