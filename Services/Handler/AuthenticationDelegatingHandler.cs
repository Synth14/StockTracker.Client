using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;
using System.Net;
using StockTracker.Client.Services.AuthService;
using StockTracker.Client.Services.TokenService;

namespace StockTracker.Client.Services.Handler
{
    public class AuthenticationDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokenService _tokenService;

        public AuthenticationDelegatingHandler(IHttpContextAccessor httpContextAccessor, ITokenService tokenService)
        {
            _httpContextAccessor = httpContextAccessor;
            _tokenService = tokenService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await AddAuthenticationHeader(request);
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Token might be expired, try to refresh
                if (await _tokenService.RefreshTokenAsync())
                {
                    await AddAuthenticationHeader(request);
                    response = await base.SendAsync(request, cancellationToken);
                }
            }

            return response;
        }

        private async Task AddAuthenticationHeader(HttpRequestMessage request)
        {
            var accessToken = await _tokenService.GetAccessTokenAsync();
            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }
    }
}