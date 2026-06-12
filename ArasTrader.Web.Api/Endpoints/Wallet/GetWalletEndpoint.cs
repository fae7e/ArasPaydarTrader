using ArasTrader.Application.Dtos;
using ArasTrader.Application.Interfaces;

namespace ArasTrader.Web.Api.Endpoints.Wallet
{
    public class GetWalletEndpoint : EndpointBase
    {
        public override void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/wallets/customer/{customerId:guid}", async (
                Guid customerId,
                IWalletQueryService walletQueryService,
                CancellationToken ct) =>
            {
                var wallet = await walletQueryService.GetByCustomerIdAsync(customerId, ct);
                return wallet is null ? Results.NotFound() : Results.Ok(wallet);
            })
            .WithTags("Wallets")
            .WithSummary("Returns the wallet balance for a given customer.")
            .Produces<WalletDto>()
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
