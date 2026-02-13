using System.Security.Claims;
using AuthorizationDemo.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AuthorizationDemo.Endpoints;

public static class InvoiceEndpoints
{
    public sealed record CreateInvoiceRequest(Guid CompanyId, decimal Amount, string Description);
    public sealed record InvoiceDto(Guid Id, Guid CompanyId, string CompanyName, decimal Amount, string Description);

    public static IEndpointRouteBuilder MapInvoiceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/invoices")
            .RequireAuthorization()
            .WithTags("Invoices");

        group.MapPost("/", Create)
            .WithSummary("Create an invoice (FinancePerson up to 100k, Root unlimited)");

        return app;
    }

    private static async Task<Results<Created<InvoiceDto>, NotFound, ForbidHttpResult>> Create(
        CreateInvoiceRequest request,
        IInvoiceService invoiceService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var result = await invoiceService.CreateInvoiceAsync(request, user, ct);

        return result.Status switch
        {
            InvoiceCreateStatus.NotFound => TypedResults.NotFound(),
            InvoiceCreateStatus.Forbidden => TypedResults.Forbid(),
            _ => TypedResults.Created($"/api/invoices/{result.Invoice!.Id}", result.Invoice)
        };
    }
}
