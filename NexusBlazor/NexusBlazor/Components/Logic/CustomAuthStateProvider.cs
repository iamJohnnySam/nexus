using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

public class LoginInstance
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LoginInstance(IHttpContextAccessor accessor)
    {
        _httpContextAccessor = accessor;
    }

    public async Task SignInAsync(string name, string role, int employeeId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Role, role),
            new Claim(ClaimTypes.SerialNumber, employeeId.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(4)
        };

        await _httpContextAccessor.HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            authProperties);
    }

    public async Task SignOutAsync()
    {
        await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
