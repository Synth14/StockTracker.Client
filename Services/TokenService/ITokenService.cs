
namespace StockTracker.Client.Services.TokenService
{
    public interface ITokenService
    {
        Task<string> GetAccessTokenAsync();
        Task<bool> RefreshTokenAsync();
        Task SaveTokensAsync(string accessToken, string refreshToken, DateTime expiresAt);
    }
}