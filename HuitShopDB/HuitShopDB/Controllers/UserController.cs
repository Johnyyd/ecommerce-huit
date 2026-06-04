using System;
using System.Linq;
using System.Web.Mvc;
using HuitShopDB.Services.Interfaces;
using HuitShopDB.Models.DTOs.User;

namespace HuitShopDB.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController()
        {
            _userService = new Services.UserService();
        }

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        private bool IsAdminOrStaff()
        {
            string role = Session["UserRole"] as string;
            return role == "ADMIN" || role == "STAFF";
        }

        // GET: /User/
        public ActionResult Index(string search, string role, string status)
        {
            if (!IsAdminOrStaff())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang quản trị này.";
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.Title = "Quản lý người dùng";
            ViewBag.Search = search;
            ViewBag.Role = role;
            ViewBag.Status = status;

            var users = _userService.GetUsers(search, role, status);
            return View(users);
        }

        // GET: /User/Edit/5
        public ActionResult Edit(int id)
        {
            if (!IsAdminOrStaff())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang quản trị này.";
                return RedirectToAction("Login", "Auth");
            }

            var user = _userService.GetUserById(id);
            if (user == null) return HttpNotFound();

            ViewBag.Title = "Chỉnh sửa người dùng";
            return View(user);
        }

        // POST: /User/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, string role, string status)
        {
            if (!IsAdminOrStaff())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang quản trị này.";
                return RedirectToAction("Login", "Auth");
            }

            bool success = true;
            success &= _userService.UpdateUserRole(id, role);
            success &= _userService.UpdateUserStatus(id, status);

            if (success)
            {
                TempData["SuccessMessage"] = "Cập nhật người dùng thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật.";
            }

            return RedirectToAction("Index");
        }

        // POST: /User/ToggleStatus/5
        [HttpPost]
        public ActionResult ToggleStatus(int id, string currentStatus)
        {
            if (!IsAdminOrStaff())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang quản trị này.";
                return RedirectToAction("Login", "Auth");
            }

            string newStatus = (currentStatus == "ACTIVE") ? "SUSPENDED" : "ACTIVE";
            bool success = _userService.UpdateUserStatus(id, newStatus);
            
            if (success)
            {
                TempData["SuccessMessage"] = (newStatus == "ACTIVE" ? "Kích hoạt" : "Vô hiệu hóa") + " người dùng thành công.";
            }
            return RedirectToAction("Index");
        }

        // GET: /User/Details/5
        public ActionResult Details(int id)
        {
            if (!IsAdminOrStaff())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang quản trị này.";
                return RedirectToAction("Login", "Auth");
            }

            var user = _userService.GetUserById(id);
            if (user == null) return HttpNotFound();

            ViewBag.Title = "Chi tiết người dùng";
            return View(user);
        }

        // GET: /User/Profile
        [HttpGet]
        [ActionName("Profile")]
        public ActionResult UserProfile()
        {
            int userId = Session["UserId"] != null ? (int)Session["UserId"] : 0;
            if (userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var details = _userService.GetUserDetails(userId);
            if (details == null) return HttpNotFound();

            // Fetch user addresses directly using the context
            using (var context = new HuitShopDB.Models.HuitShopDBDataContext())
            {
                var addresses = context.addresses
                    .Where(a => a.user_id == userId)
                    .OrderByDescending(a => a.is_default)
                    .ThenByDescending(a => a.created_at)
                    .ToList();
                ViewBag.Addresses = addresses;
            }

            ViewBag.Title = "Trang cá nhân";
            return View(details);
        }

        // POST: /User/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(string fullName, string phone, string avatarUrl)
        {
            int userId = Session["UserId"] != null ? (int)Session["UserId"] : 0;
            if (userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (string.IsNullOrEmpty(fullName))
            {
                TempData["ErrorMessage"] = "Họ tên không được để trống.";
                return RedirectToAction("Profile");
            }

            bool success = _userService.UpdateUserProfile(userId, fullName, phone, avatarUrl);
            if (success)
            {
                Session["UserName"] = fullName;
                TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể cập nhật hồ sơ.";
            }

            return RedirectToAction("Profile");
        }

        // POST: /User/AddAddress
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddAddress(string label, string receiverName, string receiverPhone, string province, string district, string ward, string streetAddress, bool isDefault)
        {
            int userId = Session["UserId"] != null ? (int)Session["UserId"] : 0;
            if (userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (string.IsNullOrEmpty(receiverName) || string.IsNullOrEmpty(receiverPhone) || 
                string.IsNullOrEmpty(province) || string.IsNullOrEmpty(district) || 
                string.IsNullOrEmpty(ward) || string.IsNullOrEmpty(streetAddress))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ thông tin địa chỉ.";
                return RedirectToAction("Profile");
            }

            try
            {
                using (var context = new HuitShopDB.Models.HuitShopDBDataContext())
                {
                    // If isDefault is true, remove default flag from other addresses
                    if (isDefault)
                    {
                        var userAddresses = context.addresses.Where(a => a.user_id == userId).ToList();
                        foreach (var addr in userAddresses)
                        {
                            addr.is_default = false;
                        }
                    }

                    var newAddress = new HuitShopDB.Models.address
                    {
                        user_id = userId,
                        label = label ?? "Khác",
                        receiver_name = receiverName,
                        receiver_phone = receiverPhone,
                        province = province,
                        district = district,
                        ward = ward,
                        street_address = streetAddress,
                        is_default = isDefault,
                        created_at = DateTime.Now,
                        updated_at = DateTime.Now
                    };

                    context.addresses.InsertOnSubmit(newAddress);
                    context.SubmitChanges();
                    TempData["SuccessMessage"] = "Thêm địa chỉ giao hàng thành công!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi thêm địa chỉ: " + ex.Message;
            }

            return RedirectToAction("Profile");
        }

        // POST: /User/DeleteAddress
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAddress(int addressId)
        {
            int userId = Session["UserId"] != null ? (int)Session["UserId"] : 0;
            if (userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                using (var context = new HuitShopDB.Models.HuitShopDBDataContext())
                {
                    var address = context.addresses.FirstOrDefault(a => a.id == addressId && a.user_id == userId);
                    if (address != null)
                    {
                        context.addresses.DeleteOnSubmit(address);
                        context.SubmitChanges();
                        TempData["SuccessMessage"] = "Đã xóa địa chỉ giao hàng.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy địa chỉ hợp lệ.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa địa chỉ: " + ex.Message;
            }

            return RedirectToAction("Profile");
        }
    }
}

