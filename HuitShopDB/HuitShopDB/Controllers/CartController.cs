using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using HuitShopDB.Services.Interfaces;
using HuitShopDB.Services;
using HuitShopDB.Models.DTOs.Cart;

namespace HuitShopDB.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController()
        {
            _cartService = new CartService();
        }

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        // GET: /Cart
        public async Task<ActionResult> Index()
        {
            int userId = Session["UserId"] != null ? (int)Session["UserId"] : 0;
            if (userId == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem giỏ hàng.";
                return RedirectToAction("Login", "Auth", new { returnUrl = Request.Url?.PathAndQuery });
            }

            var cart = await _cartService.GetCartByUserIdAsync(userId);
            return View(cart);
        }

        // GET: /Cart/GetMiniCartJson
        [HttpGet]
        public async Task<JsonResult> GetMiniCartJson()
        {
            int userId = Session["UserId"] != null ? (int)Session["UserId"] : 0;
            if (userId == 0)
            {
                return Json(new { success = false, message = "unauthorized" }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var cart = await _cartService.GetCartByUserIdAsync(userId);
                
                // Map to dynamic list to avoid JSON circular/depth issues
                var items = cart.Items.Select(i => new {
                    id = i.Id,
                    variantId = i.Variant.Id,
                    name = i.Variant.VariantName,
                    price = i.Variant.Price,
                    img = i.Variant.ThumbnailUrl,
                    quantity = i.Quantity,
                    lineTotal = i.LineTotal
                }).ToList();

                return Json(new { 
                    success = true, 
                    cart = new {
                        subtotal = cart.Subtotal,
                        discount = cart.Discount,
                        total = cart.Total,
                        items = items,
                        cartCount = items.Sum(x => x.quantity)
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: /Cart/AddToCart
        [HttpPost]
        public async Task<JsonResult> AddToCart(int variantId, int quantity = 1)
        {
            int userId = Session["UserId"] != null ? (int)Session["UserId"] : 0;
            if (userId == 0)
            {
                return Json(new { success = false, message = "unauthorized" });
            }

            try
            {
                var request = new AddCartItemRequest
                {
                    VariantId = variantId,
                    Quantity = quantity
                };

                bool result = await _cartService.AddItemToCartAsync(userId, request);
                if (result)
                {
                    var cart = await _cartService.GetCartByUserIdAsync(userId);
                    int totalCount = cart.Items.Sum(i => i.Quantity);
                    return Json(new { success = true, message = "Đã thêm sản phẩm vào giỏ hàng thành công!", cartCount = totalCount });
                }

                return Json(new { success = false, message = "Không thể thêm sản phẩm vào giỏ hàng." });
            }
            catch (ArgumentException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // POST: /Cart/UpdateQuantity
        [HttpPost]
        public async Task<JsonResult> UpdateQuantity(int cartItemId, int quantity)
        {
            int userId = Session["UserId"] != null ? (int)Session["UserId"] : 0;
            if (userId == 0)
            {
                return Json(new { success = false, message = "unauthorized" });
            }

            try
            {
                if (quantity < 1)
                {
                    // Remove if set to less than 1
                    bool removed = await _cartService.RemoveCartItemAsync(userId, cartItemId);
                    var updatedCart = await _cartService.GetCartByUserIdAsync(userId);
                    int totalCount = updatedCart.Items.Sum(i => i.Quantity);
                    return Json(new { 
                        success = true, 
                        removed = true,
                        subtotal = updatedCart.Subtotal,
                        discount = updatedCart.Discount,
                        total = updatedCart.Total,
                        cartCount = totalCount
                    });
                }

                bool result = await _cartService.UpdateCartItemQuantityAsync(userId, cartItemId, quantity);
                if (result)
                {
                    var updatedCart = await _cartService.GetCartByUserIdAsync(userId);
                    var item = updatedCart.Items.FirstOrDefault(i => i.Id == cartItemId);
                    int totalCount = updatedCart.Items.Sum(i => i.Quantity);
                    
                    return Json(new { 
                        success = true, 
                        removed = false,
                        lineTotal = item?.LineTotal ?? 0,
                        subtotal = updatedCart.Subtotal,
                        discount = updatedCart.Discount,
                        total = updatedCart.Total,
                        cartCount = totalCount
                    });
                }

                return Json(new { success = false, message = "Không thể cập nhật số lượng." });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // POST: /Cart/RemoveItem
        [HttpPost]
        public async Task<JsonResult> RemoveItem(int cartItemId)
        {
            int userId = Session["UserId"] != null ? (int)Session["UserId"] : 0;
            if (userId == 0)
            {
                return Json(new { success = false, message = "unauthorized" });
            }

            try
            {
                bool result = await _cartService.RemoveCartItemAsync(userId, cartItemId);
                if (result)
                {
                    var updatedCart = await _cartService.GetCartByUserIdAsync(userId);
                    int totalCount = updatedCart.Items.Sum(i => i.Quantity);
                    return Json(new { 
                        success = true, 
                        subtotal = updatedCart.Subtotal,
                        discount = updatedCart.Discount,
                        total = updatedCart.Total,
                        cartCount = totalCount
                    });
                }

                return Json(new { success = false, message = "Sản phẩm không có trong giỏ hoặc không thể xóa." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }
    }
}
