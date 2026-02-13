using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthorizationDemo.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace AuthorizationDemo.Services;

public class AuthTokenService(IConfiguration config) : IAuthTokenService
{
    public LoginResponse Login(string username, string role)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required.", nameof(username));

        if (!Roles.All.Contains(role))
            throw new ArgumentException(
                $"Unknown role '{role}'. Valid roles: {string.Join(", ", Roles.All)}", nameof(role));

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["Jwt:Key"]!));

        var expires = DateTime.UtcNow.AddHours(1);

        var claims = new Claim[]
        {
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new LoginResponse(
            Token: new JwtSecurityTokenHandler().WriteToken(token),
            Role: role,
            ExpiresUtc: expires);
    }
}
