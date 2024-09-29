using StockTracker.Client.Tools;

namespace StockTracker.Client.Services.OrderService
{
    public class OrderService : IOrderService
    {
        private readonly HttpClient _httpClient;

        public OrderService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public async Task<List<Order>> GetOrdersAsync()
        {
            var response = await _httpClient.GetAsync("api/Orders");
            return await HandleResponse<List<Order>>(response);
        }

        public async Task<Order> GetOrderAsync(int id)
        {
            var order = await _httpClient.GetFromJsonAsync<Order>($"api/Orders/{id}");
            return order ?? throw new Exception("Order not found");
        }

        public async Task<Result> UpdateOrderAsync(int id, Order order)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Orders/{id}", order);
            return await HandleResponse(response);
        }

        public async Task<Result<Order>> CreateOrderAsync(Order order)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Orders", order);
                if (response.IsSuccessStatusCode)
                {
                    var createdOrder = await response.Content.ReadFromJsonAsync<Order>();
                    return new Result<Order> { Success = true, Data = createdOrder };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new Result<Order> { Success = false, ErrorMessage = errorContent };
                }
            }
            catch (Exception ex)
            {
                return new Result<Order> { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<Result> DeleteOrderAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Orders/{id}");
            return await HandleResponse(response);
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
