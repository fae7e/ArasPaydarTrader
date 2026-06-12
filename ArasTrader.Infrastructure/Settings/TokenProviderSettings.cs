using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArasTrader.Infrastructure.Settings;

public class TokenProviderSettings
{
    public const string SectionName = "TokenProviderSettings";
    public string BaseUrl { get; set; } = string.Empty;
    public string TokenEndPoint { get; set; } = string.Empty;
    public string RefreshTokenEndPoint { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
