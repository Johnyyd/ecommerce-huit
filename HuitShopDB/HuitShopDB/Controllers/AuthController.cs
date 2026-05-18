using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using HuitShopDB.Services.Interfaces;
using HuitShopDB.Models.DTOs.Auth;

namespace HuitShopDB.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController()
        {
            _authService = new Services.AuthService();
        }

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // GET: /Auth/Login
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            if (Session["UserId"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginDto());
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginDto loginDto, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(loginDto);
            }

            try
            {
                var result = await _authService.LoginAsync(loginDto);
                if (result != null)
                {
                    // Establish Session
                    Session["UserId"] = result.Id;
                    Session["UserEmail"] = result.Email;
                    Session["UserRole"] = result.Role;
                    Session["UserName"] = result.FullName;

                    // Establish Authentication Cookie
                    FormsAuthentication.SetAuthCookie(result.Email, false);

                    TempData["SuccessMessage"] = "Đăng nhập thành công! Chào mừng quay trở lại, " + result.FullName;

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Email hoặc mật khẩu không chính xác.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Đã xảy ra lỗi: " + ex.Message);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(loginDto);
        }

        // GET: /Auth/Register
        [HttpGet]
        public ActionResult Register()
        {
            if (Session["UserId"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new RegisterDto());
        }

        // POST: /Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return View(registerDto);
            }

            try
            {
                var result = await _authService.RegisterAsync(registerDto);
                if (result != null)
                {
                    // Auto Login after successful registration
                    Session["UserId"] = result.Id;
                    Session["UserEmail"] = result.Email;
                    Session["UserRole"] = result.Role;
                    Session["UserName"] = result.FullName;

                    FormsAuthentication.SetAuthCookie(result.Email, false);

                    TempData["SuccessMessage"] = "Đăng ký tài khoản thành công! Chào mừng, " + result.FullName;
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
            }

            return View(registerDto);
        }

        // POST: /Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            TempData["SuccessMessage"] = "Đã đăng xuất tài khoản thành công.";
            return RedirectToAction("Index", "Home");
        }
    }
}
