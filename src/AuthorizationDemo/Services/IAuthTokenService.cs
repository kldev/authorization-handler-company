namespace AuthorizationDemo.Services;

public sealed record LoginRequest(string Username, string Role);
public sealed record LoginResponse(string Token, string Role, DateTime ExpiresUtc);

public interface IAuthTokenService
{
    LoginResponse Login(string username, string role);
}
