namespace StockTracker.Client.Services.InventoryService
{
    public class InventoryService : IInventoryService
    {
        private readonly HttpClient _httpClient;

        public InventoryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<InventoryItem>> GetInventoryItemsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<InventoryItem>>("api/InventoryItems");
        }

        public async Task<InventoryItem> GetInventoryItemAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<InventoryItem>($"api/InventoryItems/{id}");
        }

        public async Task UpdateInventoryItemAsync(int id, InventoryItem item)
        {
            await _httpClient.PutAsJsonAsync($"api/InventoryItems/{id}", item);
        }

        public async Task CreateInventoryItemAsync(InventoryItem item)
        {
            await _httpClient.PostAsJsonAsync("api/InventoryItems", item);
        }

        public async Task DeleteInventoryItemAsync(int id)
        {
            await _httpClient.DeleteAsync($"api/InventoryItems/{id}");
        }
        public async Task PatchInventoryItemAsync(int id, JsonPatchDocument<InventoryItem> patchDoc)
        {
            var response = await _httpClient.PatchAsync($"api/inventoryitems/{id}", JsonContent.Create(patchDoc));
            response.EnsureSuccessStatusCode();
        }
        public async Task AddInventoryItemAsync(InventoryItem inventoryItem)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/inventoryitems", inventoryItem);
            response.EnsureSuccessStatusCode();
        }
        public async Task<List<Brand>> GetBrandsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Brand>>("/api/brands");
        }

    }
}
