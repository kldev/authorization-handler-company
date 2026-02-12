using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthorizationDemo.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.Tokens;

namespace AuthorizationDemo.Endpoints;

public static class AuthEndpoints
{
    public sealed record LoginRequest(string Username, string Role);
    public sealed record LoginResponse(string Token, string Role, DateTime ExpiresUtc);

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapPost("/login", Login)
            .AllowAnonymous()
            .WithSummary("Generate a JWT token for a given username and role");

        group.MapGet("/roles", () => TypedResults.Ok(Roles.All))
            .AllowAnonymous()
            .WithSummary("List available roles");

        return app;
    }

    private static Results<Ok<LoginResponse>, BadRequest<string>> Login(
        LoginRequest request,
        IConfiguration config)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            return TypedResults.BadRequest("Username is required.");

        if (!Roles.All.Contains(request.Role))
            return TypedResults.BadRequest(
                $"Unknown role '{request.Role}'. Valid roles: {string.Join(", ", Roles.All)}");

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["Jwt:Key"]!));

        var expires = DateTime.UtcNow.AddHours(1);

        var claims = new Claim[]
        {
            new(ClaimTypes.Name, request.Username),
            new(ClaimTypes.Role, request.Role)
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        var response = new LoginResponse(
            Token: new JwtSecurityTokenHandler().WriteToken(token),
            Role: request.Role,
            ExpiresUtc: expires);

        return TypedResults.Ok(response);
    }
}
