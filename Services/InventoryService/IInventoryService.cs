namespace StockTracker.Client.Services.InventoryService
{
    public interface IInventoryService
    {
        Task CreateInventoryItemAsync(InventoryItem item);
        Task DeleteInventoryItemAsync(int id);
        Task<InventoryItem> GetInventoryItemAsync(int id);
        Task<List<InventoryItem>> GetInventoryItemsAsync();
        Task UpdateInventoryItemAsync(int id, InventoryItem item);
        Task PatchInventoryItemAsync(int id, JsonPatchDocument<InventoryItem> patchDoc);
        Task<List<Brand>> GetBrandsAsync();

        Task AddInventoryItemAsync(InventoryItem inventoryItem);
    }
}