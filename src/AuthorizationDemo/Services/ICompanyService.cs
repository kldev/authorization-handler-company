using System.Security.Claims;
using AuthorizationDemo.Domain;

namespace AuthorizationDemo.Services;

public interface ICompanyService
{
    Task<List<Company>> GetAuthorizedCompaniesAsync(ClaimsPrincipal user, CancellationToken ct);
    Task<(Company? company, bool authorized)> GetAuthorizedCompanyByIdAsync(Guid id, ClaimsPrincipal user, CancellationToken ct);
}
