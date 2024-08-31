using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Globalization;
using System.Security.Claims;
using System.Security.Principal;
namespace StockTracker.Client.Services.TokenService

{


    public class TokenService : ITokenService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public TokenService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient("AuthClient");
        }

        public async Task<string> GetAccessTokenAsync()
        {
            return await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
        }

        public async Task SaveTokensAsync(string accessToken, string refreshToken, DateTime expiresAt)
        {
            var authInfo = await _httpContextAccessor.HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (authInfo?.Principal == null || !authInfo.Principal.Identity.IsAuthenticated)
            {
                // L'utilisateur n'est pas authentifié, nous devons créer une nouvelle identité authentifiée
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "DefaultUser"), // Ajoutez un nom d'utilisateur par défaut ou une autre claim pertinente
            // Ajoutez d'autres claims si nécessaire
        };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                var properties = new AuthenticationProperties();
                properties.StoreTokens(new[]
                {
            new AuthenticationToken { Name = "access_token", Value = accessToken },
            new AuthenticationToken { Name = "refresh_token", Value = refreshToken },
            new AuthenticationToken { Name = "expires_at", Value = expiresAt.ToString("o", CultureInfo.InvariantCulture) }
        });

                await _httpContextAccessor.HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    properties);
            }
            else
            {
                // L'utilisateur est déjà authentifié, nous mettons à jour les tokens
                authInfo.Properties.UpdateTokenValue("access_token", accessToken);
                authInfo.Properties.UpdateTokenValue("refresh_token", refreshToken);
                authInfo.Properties.UpdateTokenValue("expires_at", expiresAt.ToString("o", CultureInfo.InvariantCulture));

                await _httpContextAccessor.HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    authInfo.Principal,
                    authInfo.Properties);
            }
        }
        public async Task<bool> RefreshTokenAsync()
        {
            var refreshToken = await _httpContextAccessor.HttpContext.GetTokenAsync("refresh_token");
            if (string.IsNullOrEmpty(refreshToken))
            {
                return false;
            }

            var tokenEndpoint = _configuration["OpenIdConnect:Authority"] + "/connect/token";

            var refreshRequest = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "refresh_token",
                    ["client_id"] = _configuration["OpenIdConnect:ClientId"],
                    ["refresh_token"] = refreshToken
                })
            };

            var response = await _httpClient.SendAsync(refreshRequest);

            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
                if (tokenResponse != null)
                {
                    var authInfo = await _httpContextAccessor.HttpContext.AuthenticateAsync("Cookies");
                    authInfo.Properties.UpdateTokenValue("access_token", tokenResponse.AccessToken);
                    authInfo.Properties.UpdateTokenValue("refresh_token", tokenResponse.RefreshToken);
                    await _httpContextAccessor.HttpContext.SignInAsync("Cookies", authInfo.Principal, authInfo.Properties);
                    return true;
                }
            }

            return false;
        }
    }

}