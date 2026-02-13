using System.Security.Claims;
using AuthorizationDemo.Authorization;
using AuthorizationDemo.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using OpenTelemetry.Trace;

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
        ILoggerFactory factory,
        Tracer tracer,
        ICompanyRepository repository,
        IAuthorizationService authService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        using var span = tracer.StartActiveSpan("invoiceCreate");

        var log = factory.CreateLogger("InvoiceEndpoints");
        log.IsEnabled(LogLevel.Information);
        log.LogInformation("Create invoice");

        // 1. Can the user create an invoice for this amount?
        var canCreate = await authService.AuthorizeAsync(
            user, new InvoiceContext(request.Amount), Policies.CanCreateInvoice);
        if (!canCreate.Succeeded)
            return TypedResults.Forbid();

        // 2. Does the company exist?
        var company = await repository.GetByIdAsync(request.CompanyId, ct);
        if (company is null)
            return TypedResults.NotFound();

        // 3. Can the user access this company?
        var canAccess = await authService.AuthorizeAsync(
            user, company, Policies.CanAccessCompany);
        if (!canAccess.Succeeded)
            return TypedResults.Forbid();

        // 4. Create invoice
        var invoice = new InvoiceDto(
            Guid.NewGuid(),
            company.Id,
            company.Name,
            request.Amount,
            request.Description);

        log.LogInformation("Invoice created");

        return TypedResults.Created($"/api/invoices/{invoice.Id}", invoice);
    }
}
