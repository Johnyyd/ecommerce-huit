using ECommerce.Huit.Application.DTOs.Cart;

namespace ECommerce.Huit.Application.Common.Interfaces;

public interface ICartService
{
    Task<CartDto> GetCartAsync(int userId);
    Task<CartDto> AddItemAsync(int userId, AddCartItemRequest request);
    Task<CartDto> UpdateItemAsync(int userId, int itemId, int quantity);
    Task<bool> RemoveItemAsync(int userId, int itemId);
    Task<CartDto> ApplyVoucherAsync(int userId, string voucherCode);
    Task<CartDto> ClearCartAsync(int userId);
}
