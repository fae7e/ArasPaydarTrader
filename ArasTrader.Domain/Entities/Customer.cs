namespace ArasTrader.Domain.Entities;

public class Customer
{
    public Guid Id { get; private set; }
    public string NationalCode { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string FatherName { get; private set; } = string.Empty;
    public string BirthCertificationNumber { get; private set; } = string.Empty;
    public string RegistrationNumber { get; private set; } = string.Empty;
    public DateTime BirthDate { get; private set; }
    public string BranchName { get; private set; } = string.Empty;
    public string MobileNumber { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Wallet? Wallet { get; private set; }
    public ICollection<Order> Orders { get; private set; } = new List<Order>();

    private Customer() { }
    public static Customer Create(
    string nationalCode,
    string firstName,
    string lastName,
    string fatherName,
    string birthCertificationNumber,
    string registrationNumber,
    DateTime birthDate,
    string branchName,
    string mobileNumber)
    {
        return new Customer
        {
            Id = Guid.NewGuid(),
            NationalCode = nationalCode,
            FirstName = firstName,
            LastName = lastName,
            FatherName = fatherName,
            BirthCertificationNumber = birthCertificationNumber,
            RegistrationNumber = registrationNumber,
            BirthDate = birthDate,
            BranchName = branchName,
            MobileNumber = mobileNumber,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
    public void Update(
        string firstName,
        string lastName,
        string fatherName,
        string birthCertificationNumber,
        string registrationNumber,
        DateTime birthDate,
        string branchName,
        string mobileNumber)
    {
        FirstName = firstName;
        LastName = lastName;
        FatherName = fatherName;
        BirthCertificationNumber = birthCertificationNumber;
        RegistrationNumber = registrationNumber;
        BirthDate = birthDate;
        BranchName = branchName;
        MobileNumber = mobileNumber;
        UpdatedAt = DateTime.UtcNow;
    }
}
