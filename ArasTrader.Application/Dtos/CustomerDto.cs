namespace ArasTrader.Application.Dtos;

public record CustomerDto(
    Guid Id,
    string NationalCode,
    string FirstName,
    string LastName,
    string FatherName,
    string BirthCertificationNumber,
    string RegistrationNumber,
    DateTime BirthDate,
    string BranchName,
    string MobileNumber
);
