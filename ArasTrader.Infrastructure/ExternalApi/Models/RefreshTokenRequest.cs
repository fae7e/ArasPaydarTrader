using System.Text.Json.Serialization;

namespace ArasTrader.Infrastructure.ExternalApi.Models;

public record RefreshTokenRequest(
    [property: JsonPropertyName("token")] string Token
);

