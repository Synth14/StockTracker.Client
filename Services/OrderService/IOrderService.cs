using StockTracker.Client.Tools;

namespace StockTracker.Client.Services.OrderService
{
    public interface IOrderService
    {
        Task<Result<Order>> CreateOrderAsync(Order order);
        Task<Result> DeleteOrderAsync(int id);
        Task<Order> GetOrderAsync(int id);
        Task<List<Order>> GetOrdersAsync();
        Task<Result> UpdateOrderAsync(int id, Order order);
    }
}