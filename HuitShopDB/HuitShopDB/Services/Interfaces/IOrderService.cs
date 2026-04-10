using System.Collections.Generic;
using System.Threading.Tasks;
using HuitShopDB.Models.DTOs.Order;

namespace HuitShopDB.Services.Interfaces
{
    public interface IOrderService
    {
        Task<OrderResponseDto> CreateOrderAsync(int userId, CreateOrderRequest request);
        Task<IEnumerable<OrderResponseDto>> GetOrdersByUserIdAsync(int userId, int page = 1, int pageSize = 20);
        Task<OrderResponseDto> GetOrderByCodeAsync(string orderCode);
        Task<bool> CancelOrderAsync(int orderId, string reason);
        Task<bool> ConfirmOrderAsync(int orderId, int? staffId);
        Task<bool> ShipOrderAsync(int orderId, int warehouseId, string serialNumbersJson);
        Task<bool> CompleteOrderAsync(int orderId);
    }
}

