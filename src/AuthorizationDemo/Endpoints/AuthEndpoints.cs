using System.Security.Claims;
using AuthorizationDemo.Authorization;
using AuthorizationDemo.Extensions;
using AuthorizationDemo.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AuthorizationDemo.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapPost("/login", Login)
            .AllowAnonymous()
            .WithSummary("Generate a JWT token for a given username and role");

        group.MapGet("/roles", () => TypedResults.Ok(Roles.All))
            .AllowAnonymous()
            .WithSummary("List available roles");

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
            var response = authTokenService.Login(request.Username, request.Role);
            return TypedResults.Ok(response);
        }
        catch (ArgumentException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }
}
