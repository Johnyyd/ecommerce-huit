using System.Collections.Generic;
using HuitShopDB.Models.DTOs.Order;

namespace HuitShopDB.Services.Interfaces
{
    public interface IOrderService
    {
        OrderResponseDto CreateOrder(int userId, CreateOrderRequest request);
        IEnumerable<OrderResponseDto> GetOrdersByUserId(int userId, int page = 1, int pageSize = 20);
        OrderResponseDto GetOrderByCode(string orderCode);
        OrderResponseDto GetOrderById(int orderId);
        IEnumerable<OrderResponseDto> GetAllOrders(string status, string keyword, int page, int pageSize);
        int GetAllOrdersCount(string status, string keyword);
        bool CancelOrder(int orderId, string reason);
        bool ConfirmOrder(int orderId, int? staffId);
        bool ShipOrder(int orderId, int warehouseId, string serialNumbersJson);
        bool CompleteOrder(int orderId);
    }
}



