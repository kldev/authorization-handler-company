using System.Diagnostics.Metrics;
using System.Security.Claims;
using System.Text.Json.Nodes;
using AuthorizationDemo.Authorization;
using AuthorizationDemo.Extensions;
using AuthorizationDemo.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AuthorizationDemo.Endpoints;

public static class AuthEndpoints
{
    private static readonly Meter LoginMeter = new("AuthorizationDemo", "1.0.0");
    private static int _loginCount;

    static AuthEndpoints()
    {
        LoginMeter.CreateObservableGauge("login.count", () => _loginCount, description: "Total number of logins");
    }

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapPost("/login", Login)
            .AllowAnonymous()
            .WithSummary("Generate a JWT token for a given username and role")
            .WithOpenApi(op =>
            {
                op.RequestBody.Content["application/json"].Example = JsonNode.Parse(
                    """{"username": "jan.kowalski", "role": "PolandManager"}""");
                return op;
            });

        group.MapGet("/roles", () => TypedResults.Ok(Roles.All))
            .AllowAnonymous()
            .WithSummary("List available roles")
            .WithOpenApi(op =>
            {
                op.Responses["200"].Content["application/json"].Example = JsonNode.Parse(
                    """["Root", "PolandManager", "InternationalManager", "FinancePerson"]""");
                return op;
            });

        group.MapGet("/user/me", (ClaimsPrincipal cp) => Results.Ok(cp.ToDictionary()))
            .RequireAuthorization()
            .WithSummary("Get user info");

        return app;
    }

    private static Results<Ok<LoginResponse>, BadRequest<string>> Login(
        LoginRequest request,
        IAuthTokenService authTokenService)
    {
        try
        {
            Interlocked.Increment(ref _loginCount);
            var response = authTokenService.Login(request.Username, request.Role);
            return TypedResults.Ok(response);
        }
        catch (ArgumentException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }
}
