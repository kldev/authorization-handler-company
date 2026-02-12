using Microsoft.AspNetCore.Authorization;

namespace AuthorizationDemo.Authorization;

/// <summary>
/// Root can create invoices for any amount — no resource check needed.
/// </summary>
public sealed class InvoiceCreateHandler
    : AuthorizationHandler<InvoiceCreateRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        InvoiceCreateRequirement requirement)
    {
        if (context.User.IsInRole(Roles.Root))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
