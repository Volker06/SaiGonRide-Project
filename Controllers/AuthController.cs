using Microsoft.AspNetCore.Mvc;
using SaigonRide.Models;

namespace SaigonRide.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Auth/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.Email == email && u.Password == password);

            if (user == null)
            {
                ViewBag.Error = "Email hoặc mật khẩu không đúng!";
                return View();
            }

            HttpContext.Session.SetInt32("UserID", user.UserID);
            HttpContext.Session.SetString("UserType", user.UserType);
            HttpContext.Session.SetString("FullName", user.FullName);

            return RedirectToAction("Index", "Home");
        }

        // GET: /Auth/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // GET: /Auth/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User user)
        {
            // Validate chung cho cả Local và Tourist
            if (string.IsNullOrWhiteSpace(user.FullName))
            {
                ViewBag.Error = "Vui lòng nhập họ tên!";
                return View(user);
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                ViewBag.Error = "Vui lòng nhập email!";
                return View(user);
            }

            if (string.IsNullOrWhiteSpace(user.Password))
            {
                ViewBag.Error = "Vui lòng nhập mật khẩu!";
                return View(user);
            }

            if (string.IsNullOrWhiteSpace(user.Phone))
            {
                ViewBag.Error = "Vui lòng nhập số điện thoại!";
                return View(user);
            }

            // Validate thêm cho Tourist
            if (user.UserType == "Tourist")
            {
                if (string.IsNullOrWhiteSpace(user.PassportNumber))
                {
                    ViewBag.Error = "Tourist phải nhập Passport Number!";
                    return View(user);
                }

                if (string.IsNullOrWhiteSpace(user.Nationality))
                {
                    ViewBag.Error = "Tourist phải nhập Nationality!";
                    return View(user);
                }
            }

            // Kiểm tra email đã tồn tại chưa
            var existing = _context.Users
                .FirstOrDefault(u => u.Email == user.Email);

            if (existing != null)
            {
                ViewBag.Error = "Email này đã được đăng ký!";
                return View(user);
            }

            _context.Users.Add(user);
            _context.SaveChanges();

            TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }
    }
}