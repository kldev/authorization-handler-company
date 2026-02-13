using System.Security.Claims;
using AuthorizationDemo.Endpoints;

namespace AuthorizationDemo.Services;

public enum InvoiceCreateStatus { Success, Forbidden, NotFound }

public sealed record InvoiceCreateResult(
    InvoiceCreateStatus Status,
    InvoiceEndpoints.InvoiceDto? Invoice = null);

public interface IInvoiceService
{
    Task<InvoiceCreateResult> CreateInvoiceAsync(InvoiceEndpoints.CreateInvoiceRequest request, ClaimsPrincipal user, CancellationToken ct);
}
