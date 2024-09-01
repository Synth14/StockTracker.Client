using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace StockTracker.Client.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly HttpContext _httpContext;

        public AuthService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContext = httpContextAccessor.HttpContext;
        }

        public async Task SignOutAsync()
        {
            await _httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            await _httpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}
