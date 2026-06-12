
using ArasTrader.Application.Dtos;
using ArasTrader.Application.Interfaces;

namespace ArasTrader.Web.Api.Endpoints.Customer;

public class GetCustomersEndpoint : EndpointBase
{
    public override void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/customers", async (
           ICustomerQueryService customerQueryService,
           CancellationToken ct) =>
        {
            var customers = await customerQueryService.GetAllAsync(ct);
            return Results.Ok(customers);
        })
       .WithTags("Customers")
       .WithSummary("Returns all customers stored in the local database.")
       .Produces<IEnumerable<CustomerDto>>();
    }
}