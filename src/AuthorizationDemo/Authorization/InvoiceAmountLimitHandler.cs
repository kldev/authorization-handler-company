using Microsoft.AspNetCore.Authorization;

namespace AuthorizationDemo.Authorization;

/// <summary>
/// FinancePerson can create invoices up to 100 000.
/// Above that — only Root (handled by <see cref="InvoiceCreateHandler"/>).
/// </summary>
public sealed class InvoiceAmountLimitHandler
    : AuthorizationHandler<InvoiceCreateRequirement, InvoiceContext>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        InvoiceCreateRequirement requirement,
        InvoiceContext invoiceContext)
    {
        if (context.User.IsInRole(Roles.FinancePerson) && invoiceContext.Amount <= 100_000m)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
