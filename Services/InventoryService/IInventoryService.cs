using StockTracker.Client.Tools;

namespace StockTracker.Client.Services.InventoryService
{
    public interface IInventoryService
    {
        Task<List<InventoryItem>> GetInventoryItemsAsync();
        Task<InventoryItem> GetInventoryItemAsync(int id);
        Task<Result> UpdateInventoryItemAsync(int id, InventoryItem item);
        Task<Result<InventoryItem>> CreateInventoryItemAsync(InventoryItem item);
        Task<Result> DeleteInventoryItemAsync(int id);
        Task<Result> PatchInventoryItemAsync(int id, JsonPatchDocument<InventoryItem> patchDoc);
        Task<Result> AddInventoryItemAsync(InventoryItem inventoryItem);
        Task<List<Brand>> GetBrandsAsync();
        Task<List<Models.Type>> GetTypesAsync();
    }
}