using System.Threading.Tasks;
using HuitShopDB.Models.DTOs.Cart;
using HuitShopDB.Models.DTOs.Voucher;

namespace HuitShopDB.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartDto> GetCartByUserIdAsync(int userId);
        Task<bool> AddItemToCartAsync(int userId, AddCartItemRequest request);
        Task<bool> UpdateCartItemQuantityAsync(int userId, int cartItemId, int quantity);
        Task<bool> RemoveCartItemAsync(int userId, int cartItemId);
        Task<bool> ClearCartAsync(int userId);
        Task<ValidateVoucherResponse> ApplyVoucherAsync(int userId, string code);
        Task<bool> RemoveVoucherAsync(int userId);
    }
}

