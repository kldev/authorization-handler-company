using AuthorizationDemo.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace AuthorizationDemo.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection BuildPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.CanAccessCompany, policy =>
                policy.AddRequirements(new CompanyAccessRequirement()))
            .AddPolicy(Policies.CanCreateInvoice, policy =>
                policy.AddRequirements(new InvoiceCreateRequirement()));

        services.AddSingleton<IAuthorizationHandler, CompanyAccessHandler>();
        services.AddSingleton<IAuthorizationHandler, InvoiceCreateHandler>();
        services.AddSingleton<IAuthorizationHandler, InvoiceAmountLimitHandler>();

        return services;
    }
}