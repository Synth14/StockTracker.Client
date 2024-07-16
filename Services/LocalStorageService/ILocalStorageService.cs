
namespace StockTracker.Client.Services
{
    public interface ILocalStorageService
    {
        Task<T> GetItemAsync<T>(string key);
        Task RemoveItemAsync(string key);
        Task SetItemAsync<T>(string key, T item);
    }
}