using System.Security.Claims;
using AuthorizationDemo.Authorization;
using AuthorizationDemo.Endpoints;
using AuthorizationDemo.Repositories;
using Microsoft.AspNetCore.Authorization;
using OpenTelemetry.Trace;

namespace AuthorizationDemo.Services;

public class InvoiceService(
    ICompanyRepository repository,
    IAuthorizationService authService,
    ILogger<InvoiceService> log,
    Tracer tracer) : IInvoiceService
{
    public async Task<InvoiceCreateResult> CreateInvoiceAsync(
        InvoiceEndpoints.CreateInvoiceRequest request,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        using var span = tracer.StartActiveSpan("invoiceCreate");

        log.LogInformation("Create invoice");

        // 1. Can the user create an invoice for this amount?
        var canCreate = await authService.AuthorizeAsync(
            user, new InvoiceContext(request.Amount), Policies.CanCreateInvoice);
        if (!canCreate.Succeeded)
            return new InvoiceCreateResult(InvoiceCreateStatus.Forbidden);

        // 2. Does the company exist?
        var company = await repository.GetByIdAsync(request.CompanyId, ct);
        if (company is null)
            return new InvoiceCreateResult(InvoiceCreateStatus.NotFound);

        // 3. Can the user access this company?
        var canAccess = await authService.AuthorizeAsync(
            user, company, Policies.CanAccessCompany);
        if (!canAccess.Succeeded)
            return new InvoiceCreateResult(InvoiceCreateStatus.Forbidden);

        // 4. Create invoice
        var invoice = new InvoiceEndpoints.InvoiceDto(
            Guid.NewGuid(),
            company.Id,
            company.Name,
            request.Amount,
            request.Description);

        log.LogInformation("Invoice created");

        return new InvoiceCreateResult(InvoiceCreateStatus.Success, invoice);
    }
}
