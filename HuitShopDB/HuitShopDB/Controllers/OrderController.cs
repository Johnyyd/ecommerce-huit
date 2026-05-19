using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using HuitShopDB.Services;
using HuitShopDB.Services.Interfaces;
using HuitShopDB.Models.DTOs.Order;
using HuitShopDB.Models;
using Newtonsoft.Json;

namespace HuitShopDB.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly HuitShopDBDataContext _context;

        public OrderController()
        {
            _orderService = new OrderService();
            _context = new HuitShopDBDataContext();
        }

        private int? GetCurrentUserId()
        {
            return Session["UserId"] != null ? (int?)((int)Session["UserId"]) : null;
        }

        private bool IsAdmin()
        {
            var role = Session["UserRole"] as string;
            return role == "ADMIN" || role == "STAFF";
        }

        private ActionResult RequireLogin()
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Request.Url.PathAndQuery });
        }

        // ==================== USER VIEWS ====================

        // GET: /Order/History
        public ActionResult History(string status = "ALL", int page = 1)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RequireLogin();

            var allOrders = _context.orders
                .Where(o => o.user_id == userId.Value)
                .OrderByDescending(o => o.created_at)
                .ToList();

            var filtered = status == "ALL"
                ? allOrders
                : allOrders.Where(o => o.status == status).ToList();

            int pageSize = 10;
            int totalCount = filtered.Count;
            var paged = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var dtoList = paged.Select(o => MapSimpleOrder(o)).ToList();

            ViewBag.Status = status;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.TotalCount = totalCount;

            return View(dtoList);
        }

        // GET: /Order/Details/{code}
        public async Task<ActionResult> Details(string code)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RequireLogin();

            var order = await _orderService.GetOrderByCodeAsync(code);
            if (order == null || order.UserId != userId.Value)
                return HttpNotFound();

            return View(order);
        }

        // POST AJAX: /Order/Cancel
        [HttpPost]
        public async Task<ActionResult> Cancel(int orderId, string reason)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Json(new { success = false, message = "Chưa đăng nhập" });

            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null || order.UserId != userId.Value)
                return Json(new { success = false, message = "Không tìm thấy đơn hàng" });

            if (order.Status != "PENDING")
                return Json(new { success = false, message = "Chỉ có thể hủy đơn hàng đang chờ xử lý" });

            var result = await _orderService.CancelOrderAsync(orderId, reason);
            return Json(new { success = result, message = result ? "Đã hủy đơn hàng thành công" : "Không thể hủy đơn hàng" });
        }

        // ==================== ADMIN VIEWS ====================

        // GET: /Order/Manage
        public async Task<ActionResult> Manage(string status = "ALL", string keyword = "", int page = 1)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            int pageSize = 15;
            var orders = await _orderService.GetAllOrdersAsync(status, keyword, page, pageSize);
            int totalCount = await _orderService.GetAllOrdersCountAsync(status, keyword);

            ViewBag.Status = status;
            ViewBag.Keyword = keyword;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.TotalCount = totalCount;

            // Thống kê nhanh
            ViewBag.PendingCount = _context.orders.Count(o => o.status == "PENDING");
            ViewBag.ConfirmedCount = _context.orders.Count(o => o.status == "CONFIRMED");
            ViewBag.ShippingCount = _context.orders.Count(o => o.status == "SHIPPING");
            ViewBag.CompletedCount = _context.orders.Count(o => o.status == "COMPLETED");
            ViewBag.CancelledCount = _context.orders.Count(o => o.status == "CANCELLED");

            return View(orders);
        }

        // GET: /Order/AdminDetails/{id}
        public async Task<ActionResult> AdminDetails(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null) return HttpNotFound();

            // Lấy danh sách serial numbers khả dụng trong kho để có thể gán
            ViewBag.AvailableSerials = _context.product_serials
                .Where(s => s.status == "AVAILABLE")
                .ToList();

            return View(order);
        }

        // POST AJAX: /Order/UpdateStatus
        [HttpPost]
        public async Task<ActionResult> UpdateStatus(int orderId, string newStatus, string note)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Không có quyền truy cập" });

            bool result = false;
            string message = "";

            try
            {
                switch (newStatus)
                {
                    case "CONFIRMED":
                        int? adminId = Session["UserId"] != null ? (int?)((int)Session["UserId"]) : null;
                        result = await _orderService.ConfirmOrderAsync(orderId, adminId);
                        message = result ? "Đã xác nhận đơn hàng" : "Không thể xác nhận đơn hàng";
                        break;
                    case "SHIPPING":
                        result = await _orderService.ShipOrderAsync(orderId, 1, null);
                        message = result ? "Đơn hàng đang được vận chuyển" : "Không thể cập nhật trạng thái";
                        break;
                    case "COMPLETED":
                        result = await _orderService.CompleteOrderAsync(orderId);
                        message = result ? "Đơn hàng đã hoàn thành" : "Không thể hoàn thành đơn hàng";
                        break;
                    case "CANCELLED":
                        result = await _orderService.CancelOrderAsync(orderId, note ?? "Admin hủy đơn");
                        message = result ? "Đã hủy đơn hàng" : "Không thể hủy đơn hàng";
                        break;
                    default:
                        message = "Trạng thái không hợp lệ";
                        break;
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

            return Json(new { success = result, message = message });
        }

        // POST AJAX: /Order/AssignSerial
        [HttpPost]
        public async Task<ActionResult> AssignSerial(int orderId, string serialNumbersJson)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Không có quyền truy cập" });

            var result = await _orderService.ShipOrderAsync(orderId, 1, serialNumbersJson);
            return Json(new { success = result, message = result ? "Đã gán serial và chuyển trạng thái Đang giao" : "Lỗi khi gán serial" });
        }

        // Helper: map rút gọn cho danh sách
        private OrderResponseDto MapSimpleOrder(order o)
        {
            var dto = new OrderResponseDto();
            dto.Id = o.id;
            dto.Code = o.code;
            dto.UserId = o.user_id;
            dto.Status = o.status;
            dto.PaymentMethod = o.payment_method;
            dto.PaymentStatus = o.payment_status;
            dto.Total = o.total;
            dto.Subtotal = o.subtotal;
            dto.Discount = o.discount;
            dto.ShippingFee = o.shipping_fee;
            dto.CreatedAt = o.created_at;
            dto.UserName = o.user != null ? o.user.full_name : "";
            dto.UserEmail = o.user != null ? o.user.email : "";

            // Parse địa chỉ
            if (!string.IsNullOrEmpty(o.shipping_address))
            {
                try
                {
                    var addr = JsonConvert.DeserializeObject<dynamic>(o.shipping_address);
                    if (addr != null)
                    {
                        dto.RecipientName = (string)(addr.full_name ?? "");
                        dto.RecipientPhone = (string)(addr.phone ?? "");
                        dto.FullAddress = string.Format("{0}, {1}", (string)(addr.address_line ?? ""), (string)(addr.city ?? ""));
                    }
                }
                catch { }
            }

            dto.Items = new List<OrderItemDto>();
            if (o.order_items != null)
            {
                foreach (var oi in o.order_items)
                {
                    dto.Items.Add(new OrderItemDto
                    {
                        Id = oi.id,
                        ProductName = oi.product_name,
                        Sku = oi.sku,
                        Quantity = oi.quantity,
                        UnitPrice = oi.unit_price,
                        TotalPrice = oi.total_price,
                        ThumbnailUrl = oi.product_variant != null ? oi.product_variant.thumbnail_url : ""
                    });
                }
            }

            dto.StatusHistory = new List<OrderStatusHistoryDto>();
            if (o.order_status_histories != null)
            {
                foreach (var sh in o.order_status_histories.OrderBy(h => h.created_at))
                {
                    dto.StatusHistory.Add(new OrderStatusHistoryDto
                    {
                        Id = sh.id,
                        Status = sh.status,
                        Note = sh.note,
                        CreatedAt = sh.created_at
                    });
                }
            }

            return dto;
        }
    }
}
