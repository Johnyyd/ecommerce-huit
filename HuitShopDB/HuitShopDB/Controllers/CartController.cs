using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using HuitShopDB.Services.Interfaces;
using HuitShopDB.Services;
using HuitShopDB.Models.DTOs.Cart;
using HuitShopDB.Models.DTOs.Order;
using HuitShopDB.Models;
using Newtonsoft.Json;

namespace HuitShopDB.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly HuitShopDBDataContext _context;

        public CartController()
        {
            _cartService = new CartService();
            _orderService = new OrderService();
            _context = new HuitShopDBDataContext();
        }

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
            _orderService = new OrderService();
            _context = new HuitShopDBDataContext();
        }

        private int GetCurrentUserId()
        {
            return Session["UserId"] != null ? (int)Session["UserId"] : 0;
        }

        // GET: /Cart
        public async Task<ActionResult> Index()
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem giỏ hàng.";
                return RedirectToAction("Login", "Auth", new { returnUrl = Request.Url != null ? Request.Url.PathAndQuery : null });
            }
            var cart = await _cartService.GetCartByUserIdAsync(userId);
            return View(cart);
        }

        // GET: /Cart/GetMiniCartJson
        [HttpGet]
        public async Task<JsonResult> GetMiniCartJson()
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
                return Json(new { success = false, message = "unauthorized" }, JsonRequestBehavior.AllowGet);

            try
            {
                var cart = await _cartService.GetCartByUserIdAsync(userId);
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
            int userId = GetCurrentUserId();
            if (userId == 0)
                return Json(new { success = false, message = "unauthorized" });

            try
            {
                var request = new AddCartItemRequest { VariantId = variantId, Quantity = quantity };
                bool result = await _cartService.AddItemToCartAsync(userId, request);
                if (result)
                {
                    var cart = await _cartService.GetCartByUserIdAsync(userId);
                    return Json(new { success = true, message = "Đã thêm sản phẩm vào giỏ hàng thành công!", cartCount = cart.Items.Sum(i => i.Quantity) });
                }
                return Json(new { success = false, message = "Không thể thêm sản phẩm vào giỏ hàng." });
            }
            catch (ArgumentException ex) { return Json(new { success = false, message = ex.Message }); }
            catch (InvalidOperationException ex) { return Json(new { success = false, message = ex.Message }); }
            catch (Exception ex) { return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message }); }
        }

        // POST: /Cart/UpdateQuantity
        [HttpPost]
        public async Task<JsonResult> UpdateQuantity(int cartItemId, int quantity)
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
                return Json(new { success = false, message = "unauthorized" });

            try
            {
                if (quantity < 1)
                {
                    await _cartService.RemoveCartItemAsync(userId, cartItemId);
                    var uc = await _cartService.GetCartByUserIdAsync(userId);
                    return Json(new { success = true, removed = true, subtotal = uc.Subtotal, discount = uc.Discount, total = uc.Total, cartCount = uc.Items.Sum(i => i.Quantity) });
                }
                bool result = await _cartService.UpdateCartItemQuantityAsync(userId, cartItemId, quantity);
                if (result)
                {
                    var uc = await _cartService.GetCartByUserIdAsync(userId);
                    var item = uc.Items.FirstOrDefault(i => i.Id == cartItemId);
                    return Json(new { success = true, removed = false, lineTotal = item != null ? item.LineTotal : 0, subtotal = uc.Subtotal, discount = uc.Discount, total = uc.Total, cartCount = uc.Items.Sum(i => i.Quantity) });
                }
                return Json(new { success = false, message = "Không thể cập nhật số lượng." });
            }
            catch (InvalidOperationException ex) { return Json(new { success = false, message = ex.Message }); }
            catch (Exception ex) { return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message }); }
        }

        // POST: /Cart/RemoveItem
        [HttpPost]
        public async Task<JsonResult> RemoveItem(int cartItemId)
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
                return Json(new { success = false, message = "unauthorized" });

            try
            {
                bool result = await _cartService.RemoveCartItemAsync(userId, cartItemId);
                if (result)
                {
                    var uc = await _cartService.GetCartByUserIdAsync(userId);
                    return Json(new { success = true, subtotal = uc.Subtotal, discount = uc.Discount, total = uc.Total, cartCount = uc.Items.Sum(i => i.Quantity) });
                }
                return Json(new { success = false, message = "Sản phẩm không có trong giỏ hoặc không thể xóa." });
            }
            catch (Exception ex) { return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message }); }
        }

        // ==================== CHECKOUT FLOW ====================

        // GET: /Cart/Checkout
        public async Task<ActionResult> Checkout()
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Auth", new { returnUrl = "/Cart/Checkout" });

            var cart = await _cartService.GetCartByUserIdAsync(userId);
            if (!cart.Items.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống. Hãy thêm sản phẩm trước khi thanh toán.";
                return RedirectToAction("Index");
            }

            var addresses = _context.addresses.Where(a => a.user_id == userId).ToList();
            ViewBag.Addresses = addresses;

            decimal shippingFee = cart.Subtotal >= 500000m ? 0m : 30000m;
            ViewBag.ShippingFee = shippingFee;

            var cartEntity = _context.carts.FirstOrDefault(c => c.user_id == userId);
            ViewBag.AppliedVoucherCode = cartEntity != null ? cartEntity.voucher_code ?? "" : "";

            // Load thông tin user để điền sẵn
            var user = _context.users.FirstOrDefault(u => u.id == userId);
            ViewBag.UserFullName = user != null ? user.full_name : "";
            ViewBag.UserPhone = user != null ? user.phone : "";

            return View(cart);
        }

        // POST: /Cart/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PlaceOrder(
            string paymentMethod,
            int? savedAddressId,
            string fullName,
            string phone,
            string addressLine,
            string ward,
            string district,
            string city,
            string note)
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Auth");

            string shippingJson;
            if (savedAddressId.HasValue)
            {
                var addr = _context.addresses.FirstOrDefault(a => a.id == savedAddressId.Value && a.user_id == userId);
                if (addr == null)
                {
                    TempData["ErrorMessage"] = "Địa chỉ giao hàng không hợp lệ";
                    return RedirectToAction("Checkout");
                }
                shippingJson = JsonConvert.SerializeObject(new {
                    full_name = addr.receiver_name,
                    phone = addr.receiver_phone,
                    address_line = addr.street_address,
                    ward = addr.ward,
                    district = addr.district,
                    city = addr.province
                });
            }
            else
            {
                if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(addressLine))
                {
                    TempData["ErrorMessage"] = "Vui lòng điền đầy đủ thông tin địa chỉ giao hàng";
                    return RedirectToAction("Checkout");
                }
                shippingJson = JsonConvert.SerializeObject(new {
                    full_name = fullName,
                    phone = phone,
                    address_line = addressLine,
                    ward = ward ?? "",
                    district = district ?? "",
                    city = city ?? ""
                });
            }

            var request = new CreateOrderRequest
            {
                ShippingAddressJson = shippingJson,
                PaymentMethod = paymentMethod,
                Note = note
            };

            try
            {
                var order = await _orderService.CreateOrderAsync(userId, request);
                return RedirectToAction("OrderConfirmation", new { code = order.Code });
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Checkout");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi đặt hàng. Vui lòng thử lại.";
                return RedirectToAction("Checkout");
            }
        }

        // GET: /Cart/OrderConfirmation
        public async Task<ActionResult> OrderConfirmation(string code)
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Auth");

            var order = await _orderService.GetOrderByCodeAsync(code);
            if (order == null || order.UserId != userId)
                return HttpNotFound();

            return View(order);
        }
    }
}
