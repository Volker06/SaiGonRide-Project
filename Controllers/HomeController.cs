using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaigonRide.Models;
using System.Diagnostics;

namespace SaigonRide.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Chưa đăng nhập → về Login
            if (HttpContext.Session.GetString("UserType") == null)
                return RedirectToAction("Login", "Auth");

            var userType = HttpContext.Session.GetString("UserType");

            // Admin → về Report Dashboard
            if (userType == "Admin")
                return RedirectToAction("Index", "Report");

            
            // USER DASHBOARD
            
            var userID = HttpContext.Session.GetInt32("UserID");

            // Tổng số chuyến
            ViewBag.TotalTrips = _context.Rentals
                .Count(r => r.UserID == userID);

            // lấy TẤT CẢ xe đang thuê + include Station
            ViewBag.ActiveRentals = _context.Rentals
                .Where(r => r.UserID == userID && r.Status == "Active")
                .Include(r => r.Vehicle)
                    .ThenInclude(v => v.Station)
                .ToList();

            // Tổng tiền đã chi
            ViewBag.TotalSpent = _context.Rentals
      .Where(r => r.UserID == userID && r.Status == "Completed")
      .Sum(r => (double?)r.TotalFare) ?? 0;

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}