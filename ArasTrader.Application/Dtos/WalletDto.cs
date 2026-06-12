namespace ArasTrader.Application.Dtos;
public record WalletDto(
    Guid Id,
    Guid CustomerId,
    decimal Balance
);

