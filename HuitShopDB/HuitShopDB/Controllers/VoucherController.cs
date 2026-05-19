using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HuitShopDB.Models;
using HuitShopDB.Models.DTOs.Voucher;
using HuitShopDB.Services;
using HuitShopDB.Services.Interfaces;

namespace HuitShopDB.Controllers
{
    public class VoucherController : Controller
    {
        private readonly HuitShopDBDataContext _context;
        private readonly ICartService _cartService;

        public VoucherController()
        {
            _context = new HuitShopDBDataContext();
            _cartService = new CartService();
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

        // ==================== ADMIN VIEWS ====================

        // GET: /Voucher/Index
        public ActionResult Index(string status = "ALL", string keyword = "", int page = 1)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var query = _context.vouchers.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(v => v.code.Contains(keyword) || v.name.Contains(keyword));

            var now = DateTime.UtcNow;
            if (status == "ACTIVE")
                query = query.Where(v => v.is_active == true && v.end_date >= now);
            else if (status == "EXPIRED")
                query = query.Where(v => v.end_date < now);
            else if (status == "DISABLED")
                query = query.Where(v => v.is_active != true);

            int pageSize = 15;
            int total = query.Count();
            var vouchers = query.OrderByDescending(v => v.id).Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Status = status;
            ViewBag.Keyword = keyword;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);
            ViewBag.TotalVouchers = _context.vouchers.Count();
            ViewBag.ActiveVouchers = _context.vouchers.Count(v => v.is_active == true && v.end_date >= now);
            ViewBag.ExpiredVouchers = _context.vouchers.Count(v => v.end_date < now);

            var dtoList = vouchers.Select(v => new VoucherDto
            {
                Id = v.id,
                Code = v.code,
                Name = v.name,
                Description = v.description,
                DiscountType = v.discount_type,
                DiscountValue = v.discount_value,
                MaxDiscountAmount = v.max_discount_amount,
                MinOrderValue = v.min_order_value,
                StartDate = v.start_date,
                EndDate = v.end_date,
                UsageLimit = v.usage_limit,
                UsageCount = v.usage_count,
                IsActive = v.is_active == true
            }).ToList();

            return View(dtoList);
        }

        // GET: /Voucher/Create
        public ActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            return View(new VoucherCreateDto());
        }

        // POST: /Voucher/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(VoucherCreateDto model)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Vui lòng kiểm tra lại thông tin";
                return View(model);
            }

            // Kiểm tra mã không trùng
            var codeUpper = model.Code.Trim().ToUpper();
            if (_context.vouchers.Any(v => v.code == codeUpper))
            {
                ModelState.AddModelError("Code", "Mã voucher này đã tồn tại trong hệ thống");
                return View(model);
            }

            var voucher = new voucher();
            voucher.code = codeUpper;
            voucher.name = model.Name;
            voucher.description = model.Description;
            voucher.discount_type = model.DiscountType;
            voucher.discount_value = model.DiscountValue;
            voucher.max_discount_amount = model.MaxDiscountAmount;
            voucher.min_order_value = model.MinOrderValue;
            voucher.start_date = model.StartDate;
            voucher.end_date = model.EndDate;
            voucher.usage_limit = model.UsageLimit > 0 ? (int?)model.UsageLimit : null;
            voucher.usage_per_user = model.UsagePerUser > 0 ? model.UsagePerUser : 1;
            voucher.usage_count = 0;
            voucher.is_active = true;
            voucher.created_at = DateTime.UtcNow;

            _context.vouchers.InsertOnSubmit(voucher);
            _context.SubmitChanges();

            TempData["SuccessMessage"] = string.Format("Đã tạo voucher <strong>{0}</strong> thành công!", codeUpper);
            return RedirectToAction("Index");
        }

        // GET: /Voucher/Edit/{id}
        public ActionResult Edit(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var v = _context.vouchers.FirstOrDefault(x => x.id == id);
            if (v == null) return HttpNotFound();

            var model = new VoucherCreateDto
            {
                Id = v.id,
                Code = v.code,
                Name = v.name,
                Description = v.description,
                DiscountType = v.discount_type,
                DiscountValue = v.discount_value,
                MaxDiscountAmount = v.max_discount_amount,
                MinOrderValue = v.min_order_value,
                StartDate = v.start_date,
                EndDate = v.end_date,
                UsageLimit = v.usage_limit ?? 0,
                UsagePerUser = v.usage_per_user
            };

            return View(model);
        }

        // POST: /Voucher/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, VoucherCreateDto model)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var voucher = _context.vouchers.FirstOrDefault(x => x.id == id);
            if (voucher == null) return HttpNotFound();

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Vui lòng kiểm tra lại thông tin";
                return View(model);
            }

            voucher.name = model.Name;
            voucher.description = model.Description;
            voucher.discount_type = model.DiscountType;
            voucher.discount_value = model.DiscountValue;
            voucher.max_discount_amount = model.MaxDiscountAmount;
            voucher.min_order_value = model.MinOrderValue;
            voucher.start_date = model.StartDate;
            voucher.end_date = model.EndDate;
            voucher.usage_limit = model.UsageLimit > 0 ? (int?)model.UsageLimit : null;
            voucher.usage_per_user = model.UsagePerUser > 0 ? model.UsagePerUser : 1;

            _context.SubmitChanges();

            TempData["SuccessMessage"] = "Đã cập nhật voucher thành công!";
            return RedirectToAction("Index");
        }

        // POST AJAX: /Voucher/ToggleStatus
        [HttpPost]
        public ActionResult ToggleStatus(int id)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Không có quyền" });

            var voucher = _context.vouchers.FirstOrDefault(v => v.id == id);
            if (voucher == null)
                return Json(new { success = false, message = "Không tìm thấy voucher" });

            voucher.is_active = !(voucher.is_active == true);
            _context.SubmitChanges();

            return Json(new
            {
                success = true,
                isActive = voucher.is_active == true,
                message = voucher.is_active == true ? "Đã kích hoạt voucher" : "Đã tắt voucher"
            });
        }

        // ==================== PUBLIC API ====================

        // POST AJAX: /Voucher/Apply
        [HttpPost]
        public async System.Threading.Tasks.Task<ActionResult> Apply(string code)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Json(new { success = false, message = "Vui lòng đăng nhập để áp dụng voucher" });

            var result = await _cartService.ApplyVoucherAsync(userId.Value, code);

            if (!result.Valid)
                return Json(new { success = false, message = result.Reason });

            // Tính toán discount
            var cart = await _cartService.GetCartByUserIdAsync(userId.Value);
            decimal discount = 0;
            if (result.Voucher.DiscountType == "PERCENT")
            {
                discount = cart.Subtotal * (result.Voucher.DiscountValue / 100);
                if (result.Voucher.MaxDiscountAmount.HasValue && discount > result.Voucher.MaxDiscountAmount.Value)
                    discount = result.Voucher.MaxDiscountAmount.Value;
            }
            else
            {
                discount = result.Voucher.DiscountValue;
                if (discount > cart.Subtotal) discount = cart.Subtotal;
            }

            return Json(new
            {
                success = true,
                message = string.Format("Áp dụng thành công! Giảm {0:N0}đ", discount),
                voucher = new
                {
                    code = result.Voucher.Code,
                    name = result.Voucher.Name,
                    discountType = result.Voucher.DiscountType,
                    discountValue = result.Voucher.DiscountValue
                },
                discount = discount,
                total = cart.Subtotal - discount
            });
        }

        // POST AJAX: /Voucher/Remove
        [HttpPost]
        public async System.Threading.Tasks.Task<ActionResult> Remove()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Json(new { success = false });

            await _cartService.RemoveVoucherAsync(userId.Value);
            var cart = await _cartService.GetCartByUserIdAsync(userId.Value);
            return Json(new { success = true, total = cart.Total });
        }
    }
}
