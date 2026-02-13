using System.Security.Claims;
using AuthorizationDemo.Authorization;
using AuthorizationDemo.Domain;
using AuthorizationDemo.Repositories;
using Microsoft.AspNetCore.Authorization;
using OpenTelemetry.Trace;

namespace AuthorizationDemo.Services;

public class CompanyService(
    ICompanyRepository repository,
    IAuthorizationService authService,
    ILogger<CompanyService> log,
    Tracer tracer) : ICompanyService
{
    public async Task<List<Company>> GetAuthorizedCompaniesAsync(ClaimsPrincipal user, CancellationToken ct)
    {
        using var span = tracer.StartActiveSpan("companiesGetAll");

        log.LogInformation("Getting all companies");
        var companies = await repository.GetAllAsync(ct);
        var authorized = new List<Company>();

        foreach (var company in companies)
        {
            var result = await authService.AuthorizeAsync(user, company, Policies.CanAccessCompany);
            if (result.Succeeded)
            {
                log.LogInformation("AuthorizeAsync successful for {User} and company {Company}", user.Identity?.Name, company.Name);
                authorized.Add(company);
            }
        }

        return authorized;
    }

    public async Task<(Company? company, bool authorized)> GetAuthorizedCompanyByIdAsync(Guid id, ClaimsPrincipal user,
        CancellationToken ct)
    {
        using var span = tracer.StartActiveSpan("companiesGetById");

        log.LogInformation("Getting by Id {Id}", id);

        var company = await repository.GetByIdAsync(id, ct);
        if (company is null)
            return (null, false);

        var result = await authService.AuthorizeAsync(user, company, Policies.CanAccessCompany);
        if (result.Succeeded)
        {
            return (company, true);
        }

        return (new Company() { Id = Guid.NewGuid(), Name = "", City = "", Country = "", TaxId = "" }, false);
    }
}
