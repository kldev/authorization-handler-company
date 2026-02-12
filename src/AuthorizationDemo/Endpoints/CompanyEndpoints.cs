using System.Security.Claims;
using AuthorizationDemo.Authorization;
using AuthorizationDemo.Domain;
using AuthorizationDemo.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AuthorizationDemo.Endpoints;

public static class CompanyEndpoints
{
    public static IEndpointRouteBuilder MapCompanyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/companies")
            .RequireAuthorization()
            .WithTags("Companies");

        group.MapGet("/", GetAll)
            .WithSummary("List companies the current user is authorized to see");

        group.MapGet("/{id:guid}", GetById)
            .WithSummary("Get a single company by ID (if authorized)");

        return app;
    }

    private static async Task<Ok<List<Company>>> GetAll(
        ICompanyRepository repository,
        IAuthorizationService authService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var companies = await repository.GetAllAsync(ct);
        var authorized = new List<Company>();

        foreach (var company in companies)
        {
            var result = await authService.AuthorizeAsync(user, company, Policies.CanAccessCompany);
            if (result.Succeeded)
                authorized.Add(company);
        }

        return TypedResults.Ok(authorized);
    }

    private static async Task<Results<Ok<Company>, NotFound, ForbidHttpResult>> GetById(
        Guid id,
        ICompanyRepository repository,
        IAuthorizationService authService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var company = await repository.GetByIdAsync(id, ct);
        if (company is null)
            return TypedResults.NotFound();

        var result = await authService.AuthorizeAsync(user, company, Policies.CanAccessCompany);
        if (!result.Succeeded)
            return TypedResults.Forbid();

        return TypedResults.Ok(company);
    }
}
