using System;
using System.Linq;
using System.Web.Mvc;
using HuitShopDB.Models; // Namespace chứa HuitShopDBDataContext
using HuitShopDB.Models.DTOs.User; // Namespace chứa UserDto và UpdateProfileRequest

namespace HuitShopDB.Controllers
{
    // Chỉ cho phép ADMIN và STAFF vào phân hệ này
    // [Authorize(Roles = "ADMIN,STAFF")] 
    public class UserController : Controller
    {
        // Khởi tạo cầu nối LINQ to SQL chuẩn của nhóm bạn
        private HuitShopDBDataContext db = new HuitShopDBDataContext();

        // --- 1. QUẢN LÝ HỒ SƠ NHÂN VIÊN ---
        public ActionResult StaffList()
        {
            // Lấy từ DB, lọc quyền và ép kiểu sang UserDto để hiển thị
            var staffs = db.users
                .Where(u => u.role == "ADMIN" || u.role == "STAFF" || u.role == "WAREHOUSE")
                .Select(u => new UserDto
                {
                    Id = u.id,
                    FullName = u.full_name,
                    Email = u.email,
                    Phone = u.phone,
                    Role = u.role,
                    AvatarUrl = u.avatar_url,
                    Status = u.status,
                    LastLogin = u.last_login,
                    CreatedAt = u.created_at
                }).ToList();

            ViewBag.Title = "Quản lý Hồ sơ Nhân viên";
            return View("UserList", staffs); // Dùng chung một giao diện lưới
        }

        // --- 2. QUẢN LÝ TÀI KHOẢN NGƯỜI DÙNG (CUSTOMER) ---
        public ActionResult CustomerList()
        {
            var customers = db.users
                .Where(u => u.role == "CUSTOMER")
                .Select(u => new UserDto
                {
                    Id = u.id,
                    FullName = u.full_name,
                    Email = u.email,
                    Phone = u.phone,
                    Role = u.role,
                    AvatarUrl = u.avatar_url,
                    Status = u.status,
                    LastLogin = u.last_login,
                    CreatedAt = u.created_at
                }).ToList();

            ViewBag.Title = "Quản lý Tài khoản Người dùng";
            return View("UserList", customers);
        }

        // --- 3. CHỈNH SỬA HỒ SƠ (GET) ---
        public ActionResult Edit(int id)
        {
            // Tìm user trong DB bằng LINQ
            var user = db.users.FirstOrDefault(u => u.id == id);
            if (user == null) return HttpNotFound();

            // Đổ dữ liệu sang Request để đưa lên giao diện Form
            var model = new UpdateProfileRequest
            {
                FullName = user.full_name,
                AvatarUrl = user.avatar_url
            };

            // Dùng ViewBag để truyền thêm thông tin không cho sửa hiển thị cho vui
            ViewBag.UserId = user.id;
            ViewBag.Email = user.email;
            ViewBag.Role = user.role;

            return View(model);
        }

        // --- CHỈNH SỬA HỒ SƠ (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, UpdateProfileRequest request)
        {
            if (ModelState.IsValid)
            {
                var user = db.users.FirstOrDefault(u => u.id == id);
                if (user != null)
                {
                    // Cập nhật các trường được phép sửa từ DTO
                    user.full_name = request.FullName;
                    user.avatar_url = request.AvatarUrl;
                    user.updated_at = DateTime.Now;

                    db.SubmitChanges(); // Lệnh lưu dữ liệu xuống SQL của LINQ to SQL

                    if (user.role == "CUSTOMER") return RedirectToAction("CustomerList");
                    return RedirectToAction("StaffList");
                }
            }
            return View(request);
        }

        // --- 4. XÓA TÀI KHOẢN ---
        public ActionResult Delete(int id)
        {
            var user = db.users.FirstOrDefault(u => u.id == id);
            if (user != null)
            {
                string currentRole = user.role;
                db.users.DeleteOnSubmit(user); // Lệnh xóa của LINQ to SQL
                db.SubmitChanges();

                if (currentRole == "CUSTOMER") return RedirectToAction("CustomerList");
            }
            return RedirectToAction("StaffList");
        }
        // --- 5. XEM CHI TIẾT NGƯỜI DÙNG VÀ ĐỊA CHỈ ---
        //public ActionResult Details(int id)
        //{
        //    // Truy vấn dữ liệu: Lấy thông tin User và danh sách địa chỉ tương ứng
        //    var userDetail = db.users.Where(u => u.id == id).Select(u => new UserDto
        //    {
        //        Id = u.id,
        //        FullName = u.full_name,
        //        Email = u.email,
        //        Phone = u.phone,
        //        Role = u.role,
        //        Status = u.status,
        //        // Dùng LINQ để lấy danh sách địa chỉ của user đó
        //        Addresses = db.addresses.Where(a => a.user_id == u.id).Select(a => new AddressDto
        //        {
        //            Label = a.label,
        //            FullAddress = a.street_address + ", " + a.ward + ", " + a.district + ", " + a.province
        //        }).ToList()
        //    }).FirstOrDefault();

        //    if (userDetail == null) return HttpNotFound();

        //    return View(userDetail);
        //}

    }
}