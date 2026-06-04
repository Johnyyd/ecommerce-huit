using HuitShopDB.Models.DTOs.Cart;
using HuitShopDB.Models.DTOs.Voucher;

namespace HuitShopDB.Services.Interfaces
{
    public interface ICartService
    {
        CartDto GetCartByUserId(int userId);
        bool AddItemToCart(int userId, AddCartItemRequest request);
        bool UpdateCartItemQuantity(int userId, int cartItemId, int quantity);
        bool RemoveCartItem(int userId, int cartItemId);
        bool ClearCart(int userId);
        ValidateVoucherResponse ApplyVoucher(int userId, string code);
        bool RemoveVoucher(int userId);
    }
}


