using AuthorizationDemo.Domain;
using Microsoft.AspNetCore.Authorization;

namespace AuthorizationDemo.Authorization;

/// <summary>
/// Decides whether the current user can access a specific <see cref="Company"/> resource.
///
/// Rules:
///   Root                 - all companies
///   FinancePerson        - all companies
///   PolandManager        - companies where Country == "PL"
///   InternationalManager - companies where Country != "PL"
/// </summary>
public sealed class CompanyAccessHandler
    : AuthorizationHandler<CompanyAccessRequirement, Company>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CompanyAccessRequirement requirement,
        Company company)
    {
        if (context.User.IsInRole(Roles.Root) || context.User.IsInRole(Roles.FinancePerson))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (context.User.IsInRole(Roles.PolandManager) && company.Country == "PL")
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (context.User.IsInRole(Roles.InternationalManager) && company.Country != "PL")
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}
