using System.Text.Json.Serialization;

namespace ArasTrader.Infrastructure.ExternalApi.Models;

public record CustomerApiModel(
    [property: JsonPropertyName("nationalCode")] string NationalCode,
    [property: JsonPropertyName("firstName")] string FirstName,
    [property: JsonPropertyName("lastName")] string LastName,
    [property: JsonPropertyName("fatherName")] string FatherName,
    [property: JsonPropertyName("birthCertificationNumber")] string BirthCertificationNumber,
    [property: JsonPropertyName("registerationNumber")] string RegistrationNumber,
    [property: JsonPropertyName("birthDate")] DateTime BirthDate,
    [property: JsonPropertyName("branchName")] string BranchName,
    [property: JsonPropertyName("mobileNumber")] string MobileNumber
);