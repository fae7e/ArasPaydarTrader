using System.Text.Json.Serialization;

namespace ArasTrader.Infrastructure.ExternalApi.Models;
public record TokenResponse(
    [property: JsonPropertyName("accessToken")] string AccessToken,
    [property: JsonPropertyName("tokenType")] string TokenType,
    [property: JsonPropertyName("expiresInSeconds")] int ExpiresInSeconds,
    [property: JsonPropertyName("expiresAtUtc")] DateTime ExpiresAtUtc
);