using ArasTrader.Application.Interfaces;

namespace ArasTrader.Web.Api.Endpoints.Customer;

public class SyncCustomersEndpoint : EndpointBase
{    public override void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/customers/sync", async (
            ICustomerSyncService syncService,
            CancellationToken ct) =>
        {
            var count = await syncService.SyncCustomersAsync(ct);
            return Results.Ok(new { message = $"Sync complete. {count} customers upserted." });
        })
        .WithTags("Customers")
        .WithSummary("Pulls customers from the external API and upserts them into the local database.")
        .Produces(StatusCodes.Status200OK);
    }
}
