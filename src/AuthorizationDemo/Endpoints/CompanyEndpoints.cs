using System.Security.Claims;
using AuthorizationDemo.Domain;
using AuthorizationDemo.Services;
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
        ICompanyService companyService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var authorized = await companyService.GetAuthorizedCompaniesAsync(user, ct);
        return TypedResults.Ok(authorized);
    }

    private static async Task<Results<Ok<Company>, NotFound, ForbidHttpResult>> GetById(
        Guid id,
        ICompanyService companyService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var (company, authorized) = await companyService.GetAuthorizedCompanyByIdAsync(id, user, ct);

        if (company is null)
            return TypedResults.NotFound();

        if (!authorized)
            return TypedResults.Forbid();

        return TypedResults.Ok(company);
    }
}
