using System.Threading.Tasks;
using HuitShopDB.Models.DTOs.Cart;

namespace HuitShopDB.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartDto> GetCartByUserIdAsync(int userId);
        Task<bool> AddItemToCartAsync(int userId, AddCartItemRequest request);
        Task<bool> UpdateCartItemQuantityAsync(int userId, int cartItemId, int quantity);
        Task<bool> RemoveCartItemAsync(int userId, int cartItemId);
        Task<bool> ClearCartAsync(int userId);
    }
}

