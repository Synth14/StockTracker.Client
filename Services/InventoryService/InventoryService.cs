using StockTracker.Client.Tools;
using System.Reflection;

public class InventoryService : IInventoryService
{
    private readonly HttpClient _httpClient;

    public InventoryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<InventoryItem>> GetInventoryItemsAsync()
    {
        var response = await _httpClient.GetAsync("api/InventoryItems");
        return await HandleResponse<List<InventoryItem>>(response);
    }

    public async Task<InventoryItem> GetInventoryItemAsync(int id)
    {
        var item = await _httpClient.GetFromJsonAsync<InventoryItem>($"api/InventoryItems/{id}");
        return item ?? throw new Exception("Item not found");
    }

    public async Task<Result> UpdateInventoryItemAsync(int id, InventoryItem item)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/InventoryItems/{id}", item);
        return await HandleResponse(response);
    }
    public async Task<Result<InventoryItem>> CreateInventoryItemAsync(InventoryItem item)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/InventoryItems", item);

            if (response.IsSuccessStatusCode)
            {
                var createdItem = await response.Content.ReadFromJsonAsync<InventoryItem>();
                return new Result<InventoryItem> { Success = true, Data = createdItem };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return new Result<InventoryItem> { Success = false, ErrorMessage = errorContent };
            }
        }
        catch (Exception ex)
        {
            return new Result<InventoryItem> { Success = false, ErrorMessage = ex.Message };
        }
    }
    public async Task<Result> DeleteInventoryItemAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/InventoryItems/{id}");
        return await HandleResponse(response);
    }

    public async Task<Result> PatchInventoryItemAsync(int id, JsonPatchDocument<InventoryItem> patchDoc)
    {
        var response = await _httpClient.PatchAsync($"api/inventoryitems/{id}", JsonContent.Create(patchDoc));
        return await HandleResponse(response);
    }

    public async Task<Result> AddInventoryItemAsync(InventoryItem inventoryItem)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/inventoryitems", inventoryItem);
        return await HandleResponse(response);
    }

    public async Task<List<Brand>> GetBrandsAsync()
    {
        var response = await _httpClient.GetAsync("/api/brands");
        return await HandleResponse<List<Brand>>(response);
    }

    public async Task<List<StockTracker.Client.Models.Type>> GetTypesAsync()
    {
        var response = await _httpClient.GetAsync("/api/types");
        return await HandleResponse<List<StockTracker.Client.Models.Type>>(response);
    }

    private async Task<T> HandleResponse<T>(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<T>();
        }
        throw new ApiException(await response.Content.ReadAsStringAsync(), response.StatusCode);
    }

    private async Task<Result> HandleResponse(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return new Result { Success = true };
        }
        return new Result
        {
            Success = false,
            ErrorMessage = await response.Content.ReadAsStringAsync()
        };
    }
}