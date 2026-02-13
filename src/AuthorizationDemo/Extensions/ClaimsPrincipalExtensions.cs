using System.Security.Claims;

namespace AuthorizationDemo.Extensions;


public static class ClaimsPrincipalExtensions
{
    public static Dictionary<string, object> ToDictionary(this ClaimsPrincipal principal) =>
        principal.Claims
            .GroupBy(c => c.Type)
            .ToDictionary(
                g => g.Key,
                g => g.Count() == 1
                    ? (object)g.First().Value
                    : g.Select(c => c.Value).ToList());
}
