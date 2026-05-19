using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HuitShopDB.Models; // Để nhận diện HuitShopDBDataContext và bảng user
using HuitShopDB.Models.DTOs.Auth; // Gọi bộ DTO đăng nhập/đăng ký của Trí tạo

namespace HuitShopDB.Controllers
{
    public class AccountController : Controller
    {
        // Sử dụng DataContext của nhóm thay vì kết nối SQL thô cũ kĩ
        private HuitShopDBDataContext db = new HuitShopDBDataContext();

        public ActionResult Index()
        {
            return View();
        }

        // ================= GET: ĐĂNG KÝ =================
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        // ================= POST: ĐĂNG KÝ (Dùng RegisterDto) =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterDto model)
        {
            if (ModelState.IsValid)
            {
                // 1. Kiểm tra Email trùng bằng LINQ cực gọn
                bool emailExists = db.users.Any(u => u.email == model.Email);
                if (emailExists)
                {
                    ModelState.AddModelError("Email", "Email này đã được đăng ký sử dụng.");
                    return View(model);
                }

                // 2. Chuyển đổi dữ liệu từ RegisterDto sang thực thể bảng user trong DB
                user newAccount = new user
                {
                    full_name = model.FullName,
                    email = model.Email,
                    phone = model.Phone,
                    password_hash = model.Password, // Lưu dạng chuỗi theo data mẫu của bạn
                    role = "CUSTOMER",              // Đăng ký mặc định là khách hàng
                    status = "ACTIVE",
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                };

                // 3. Tiến hành Insert vào database bằng LINQ to SQL
                db.users.InsertOnSubmit(newAccount);
                db.SubmitChanges();

                // Đăng ký thành công -> Chuyển hướng sang trang đăng nhập
                return RedirectToAction("Login");
            }
            return View(model);
        }

        // ================= GET: ĐĂNG NHẬP =================
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        // ================= POST: ĐĂNG NHẬP (Dùng LoginDto) =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginDto model)
        {
            if (ModelState.IsValid)
            {
                // Tìm kiếm tài khoản trùng khớp Email và Password bằng LINQ
                var user = db.users.FirstOrDefault(u => u.email == model.Email && u.password_hash == model.Password);

                if (user != null)
                {
                    // Kiểm tra trạng thái BANNED
                    if (user.status == "BANNED")
                    {
                        ModelState.AddModelError("", "Tài khoản của bạn đã bị khóa.");
                        return View(model);
                    }

                    // Đăng nhập thành công -> Lưu thông tin vào Session để dùng ở Navbar Layout
                    Session["UserId"] = user.id;
                    Session["FullName"] = user.full_name;
                    Session["Role"] = user.role;

                    // Cập nhật lại thời gian đăng nhập gần nhất (Last Login) nếu cần
                    user.last_login = DateTime.Now;
                    db.SubmitChanges();

                    // Chuyển hướng về trang chủ
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Email hoặc mật khẩu không chính xác.");
                }
            }
            return View(model);
        }

        // ================= ĐĂNG XUẤT =================
        public ActionResult Logout()
        {
            Session.Clear(); // Xóa sạch dữ liệu phiên làm việc
            return RedirectToAction("Login");
        }
    }
}