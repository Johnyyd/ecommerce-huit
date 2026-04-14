using System;
using System.Threading.Tasks;
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

        // GET: /User/
        public async Task<ActionResult> Index(string search, string role, string status)
        {
            ViewBag.Title = "Quản lý người dùng";
            ViewBag.Search = search;
            ViewBag.Role = role;
            ViewBag.Status = status;

            var users = await _userService.GetUsersAsync(search, role, status);
            return View(users);
        }

        // GET: /User/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return HttpNotFound();

            ViewBag.Title = "Chỉnh sửa người dùng";
            return View(user);
        }

        // POST: /User/Edit/5
        [HttpPost]
        public async Task<ActionResult> Edit(int id, string role, string status)
        {
            bool success = true;
            success &= await _userService.UpdateUserRoleAsync(id, role);
            success &= await _userService.UpdateUserStatusAsync(id, status);

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
        public async Task<ActionResult> ToggleStatus(int id, string currentStatus)
        {
            string newStatus = (currentStatus == "ACTIVE") ? "SUSPENDED" : "ACTIVE";
            bool success = await _userService.UpdateUserStatusAsync(id, newStatus);
            
            if (success)
            {
                TempData["SuccessMessage"] = (newStatus == "ACTIVE" ? "Kích hoạt" : "Vô hiệu hóa") + " người dùng thành công.";
            }
            return RedirectToAction("Index");
        }

        // GET: /User/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return HttpNotFound();

            ViewBag.Title = "Chi tiết người dùng";
            return View(user);
        }
    }
}
