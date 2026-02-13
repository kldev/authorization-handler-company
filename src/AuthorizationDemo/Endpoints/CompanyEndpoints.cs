using System.Security.Claims;
using AuthorizationDemo.Authorization;
using AuthorizationDemo.Domain;
using AuthorizationDemo.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using OpenTelemetry.Trace;

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
        ILoggerFactory factory,
        ICompanyRepository repository,
        IAuthorizationService authService,
        Tracer tracer,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        using var span = tracer.StartActiveSpan("companiesGetAll");

        var log = factory.CreateLogger("CompanyEndpoints");
        log.IsEnabled(LogLevel.Information);
        log.LogInformation("Getting all companies");
        var companies = await repository.GetAllAsync(ct);
        var authorized = new List<Company>();

        foreach (var company in companies)
        {
            var result = await authService.AuthorizeAsync(user, company, Policies.CanAccessCompany);
            if (result.Succeeded)
            {
                log.LogInformation($"AuthorizeAsync successful for {user.Identity?.Name} and company {company.Name}");
                authorized.Add(company);
            }
        }

        return TypedResults.Ok(authorized);
    }

    private static async Task<Results<Ok<Company>, NotFound, ForbidHttpResult>> GetById(
        Guid id,
        ILoggerFactory factory,
        ICompanyRepository repository,
        IAuthorizationService authService,
        Tracer tracer,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var log = factory.CreateLogger("CompanyEndpoints");
        log.IsEnabled(LogLevel.Information);
        log.LogInformation($"Getting by Id {id}");

        using var span = tracer.StartActiveSpan("companiesGetById");

        var company = await repository.GetByIdAsync(id, ct);
        if (company is null)
            return TypedResults.NotFound();

        var result = await authService.AuthorizeAsync(user, company, Policies.CanAccessCompany);
        if (!result.Succeeded)
            return TypedResults.Forbid();

        return TypedResults.Ok(company);
    }
}
