using System.Text.Json.Serialization;

namespace ArasTrader.Infrastructure.ExternalApi.Models;
public record TokenRequest(
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("password")] string Password
);

