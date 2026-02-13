using System.Security.Claims;
using System.Text.Json.Nodes;
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
            .WithSummary("List companies the current user is authorized to see")
            .WithOpenApi(op =>
            {
                op.Responses["200"].Content["application/json"].Example = JsonNode.Parse(
                    """
                    [
                      {
                        "id": "a1b2c3d4-0001-0000-0000-000000000001",
                        "name": "Orlen S.A.",
                        "country": "PL",
                        "taxId": "7740001454",
                        "city": "Plock"
                      },
                      {
                        "id": "a1b2c3d4-0002-0000-0000-000000000002",
                        "name": "CD Projekt S.A.",
                        "country": "PL",
                        "taxId": "7342867148",
                        "city": "Warszawa"
                      }
                    ]
                    """);
                return op;
            });

        group.MapGet("/{id:guid}", GetById)
            .WithSummary("Get a single company by ID (if authorized)")
            .WithOpenApi(op =>
            {
                op.Parameters[0].Description = "Company ID, e.g. a1b2c3d4-0001-0000-0000-000000000001";
                op.Responses["200"].Content["application/json"].Example = JsonNode.Parse(
                    """
                    {
                      "id": "a1b2c3d4-0001-0000-0000-000000000001",
                      "name": "Orlen S.A.",
                      "country": "PL",
                      "taxId": "7740001454",
                      "city": "Plock"
                    }
                    """);
                return op;
            });

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
