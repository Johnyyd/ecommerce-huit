using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using HuitShopDB.Services;
using HuitShopDB.Services.Interfaces;
using HuitShopDB.Models.DTOs.Order;
using HuitShopDB.Models.DTOs.Admin;
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

        // GET: /Order/Revenue
        public ActionResult Revenue(string preset = "THIS_MONTH", DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            DateTime nowLocal = DateTime.UtcNow.AddHours(7);
            DateTime start = nowLocal.Date;
            DateTime end = nowLocal.Date.AddDays(1).AddTicks(-1);

            switch (preset)
            {
                case "TODAY":
                    start = nowLocal.Date;
                    end = nowLocal.Date.AddDays(1).AddTicks(-1);
                    break;
                case "YESTERDAY":
                    start = nowLocal.Date.AddDays(-1);
                    end = nowLocal.Date.AddTicks(-1);
                    break;
                case "LAST_7_DAYS":
                    start = nowLocal.Date.AddDays(-6);
                    end = nowLocal.Date.AddDays(1).AddTicks(-1);
                    break;
                case "THIS_MONTH":
                    start = new DateTime(nowLocal.Year, nowLocal.Month, 1);
                    end = nowLocal.Date.AddDays(1).AddTicks(-1);
                    break;
                case "LAST_MONTH":
                    var firstDayOfLastMonth = new DateTime(nowLocal.Year, nowLocal.Month, 1).AddMonths(-1);
                    start = firstDayOfLastMonth;
                    end = new DateTime(nowLocal.Year, nowLocal.Month, 1).AddTicks(-1);
                    break;
                case "THIS_YEAR":
                    start = new DateTime(nowLocal.Year, 1, 1);
                    end = nowLocal.Date.AddDays(1).AddTicks(-1);
                    break;
                case "CUSTOM":
                    if (startDate.HasValue) start = startDate.Value.Date;
                    if (endDate.HasValue) end = endDate.Value.Date.AddDays(1).AddTicks(-1);
                    break;
                default:
                    preset = "THIS_MONTH";
                    start = new DateTime(nowLocal.Year, nowLocal.Month, 1);
                    end = nowLocal.Date.AddDays(1).AddTicks(-1);
                    break;
            }

            DateTime startUtc = start.AddHours(-7);
            DateTime endUtc = end.AddHours(-7);

            var periodOrders = _context.orders
                .Where(o => o.created_at >= startUtc && o.created_at <= endUtc)
                .ToList();

            var completedOrders = periodOrders.Where(o => o.status == "COMPLETED").ToList();
            var pendingOrders = periodOrders.Where(o => o.status == "PENDING" || o.status == "CONFIRMED" || o.status == "SHIPPING").ToList();
            var cancelledOrders = periodOrders.Where(o => o.status == "CANCELLED").ToList();

            var stats = new RevenueStatisticsDto();
            stats.StartDate = start;
            stats.EndDate = end;
            stats.DatePreset = preset;

            stats.TotalOrders = periodOrders.Count;
            stats.CompletedOrdersCount = completedOrders.Count;
            stats.PendingOrdersCount = pendingOrders.Count;
            stats.CancelledOrdersCount = cancelledOrders.Count;

            stats.TotalRevenue = completedOrders.Sum(o => o.total);
            stats.PendingRevenue = pendingOrders.Sum(o => o.total);
            stats.TotalDiscount = completedOrders.Sum(o => o.discount);

            stats.TotalProductsSold = completedOrders
                .SelectMany(o => o.order_items)
                .Sum(oi => oi.quantity);

            stats.AverageOrderValue = stats.CompletedOrdersCount > 0 
                ? stats.TotalRevenue / stats.CompletedOrdersCount 
                : 0m;

            var allStatuses = new[] { "PENDING", "CONFIRMED", "SHIPPING", "COMPLETED", "CANCELLED" };
            foreach (var status in allStatuses)
            {
                stats.OrderStatusCounts[status] = periodOrders.Count(o => o.status == status);
            }

            var dailyData = completedOrders
                .GroupBy(o => o.created_at.AddHours(7).Date)
                .Select(g => new DailyRevenueDto
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Revenue = g.Sum(o => o.total),
                    OrderCount = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();

            stats.DailyRevenue = dailyData;

            var topProducts = completedOrders
                .SelectMany(o => o.order_items)
                .GroupBy(oi => new { oi.variant_id, oi.product_name, oi.sku })
                .Select(g => new TopProductDto
                {
                    VariantId = g.Key.variant_id,
                    ProductName = g.Key.product_name,
                    Sku = g.Key.sku,
                    QuantitySold = g.Sum(oi => oi.quantity),
                    TotalSales = g.Sum(oi => oi.total_price),
                    ThumbnailUrl = g.FirstOrDefault() != null && g.FirstOrDefault().product_variant != null 
                        ? g.FirstOrDefault().product_variant.thumbnail_url 
                        : ""
                })
                .OrderByDescending(p => p.QuantitySold)
                .Take(10)
                .ToList();

            stats.TopSellingProducts = topProducts;

            var topCategories = completedOrders
                .SelectMany(o => o.order_items)
                .Where(oi => oi.product_variant != null && oi.product_variant.product != null && oi.product_variant.product.category != null)
                .GroupBy(oi => oi.product_variant.product.category.name)
                .Select(g => new TopCategoryDto
                {
                    CategoryName = g.Key,
                    QuantitySold = g.Sum(oi => oi.quantity),
                    TotalSales = g.Sum(oi => oi.total_price)
                })
                .OrderByDescending(c => c.TotalSales)
                .ToList();

            stats.TopCategories = topCategories;

            ViewBag.RecentOrders = periodOrders
                .OrderByDescending(o => o.created_at)
                .Take(5)
                .Select(o => {
                    string recipientName = "";
                    if (!string.IsNullOrEmpty(o.shipping_address))
                    {
                        try
                        {
                            var addr = JsonConvert.DeserializeObject<dynamic>(o.shipping_address);
                            if (addr != null)
                            {
                                recipientName = (string)(addr.full_name ?? addr.receiver_name ?? "");
                            }
                        }
                        catch { }
                    }
                    return new OrderResponseDto
                    {
                        Id = o.id,
                        Code = o.code,
                        Status = o.status,
                        Total = o.total,
                        RecipientName = recipientName,
                        CreatedAt = o.created_at
                    };
                })
                .ToList();

            return View(stats);
        }

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
                        VariantId = oi.variant_id,
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
