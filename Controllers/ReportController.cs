using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaigonRide.Models;

namespace SaigonRide.Controllers
{
    public class ReportController : Controller
    {
        private readonly AppDbContext _context;

        public ReportController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserType") != "Admin")
                return RedirectToAction("Index", "Home");

            ViewBag.TotalUsers = _context.Users.Count();
            ViewBag.TotalVehicles = _context.Vehicles.Count();
            ViewBag.TotalRentals = _context.Rentals.Count();

            ViewBag.TotalRevenue = _context.Rentals
                .Where(r => r.Status == "Completed")
                .Sum(r => (double?)r.TotalFare) ?? 0;

            // Doanh thu theo từng trạm (dựa vào ReturnStationID — trạm nơi kết thúc chuyến)
            ViewBag.RevenueByStation = _context.Rentals
                .Where(r => r.Status == "Completed" && r.ReturnStationID != null)
                .Include(r => r.ReturnStation)
                .GroupBy(r => r.ReturnStation!.StationName)
                .Select(g => new
                {
                    StationName = g.Key,
                    Revenue = g.Sum(r => r.TotalFare)
                })
                .ToList();

            // Danh sách station
            ViewBag.Stations = _context.Stations.ToList();

            return View();
        }


        // STATION DETAIL

        public IActionResult StationDetail(int stationId)
        {
            if (HttpContext.Session.GetString("UserType") != "Admin")
                return RedirectToAction("Index", "Home");

            var station = _context.Stations
                .Include(s => s.Vehicles)
                .FirstOrDefault(s => s.StationID == stationId);

            if (station == null)
                return NotFound();

            // Tổng lượt thuê xuất phát từ trạm này (dựa vào StartStationID)
            var totalRentals = _context.Rentals
                .Count(r => r.StartStationID == stationId);

            // Tổng lượt trả xe về trạm này (dựa vào ReturnStationID)
            var totalReturns = _context.Rentals
                .Count(r => r.ReturnStationID == stationId && r.Status == "Completed");

            // Doanh thu từ các chuyến trả về trạm này
            var totalRevenue = _context.Rentals
                .Where(r => r.ReturnStationID == stationId && r.Status == "Completed")
                .Sum(r => (double?)r.TotalFare) ?? 0;

            // Số xe theo trạng thái tại trạm này
            var vehiclesAvailable = station.Vehicles.Count(v => v.Status == "Available");
            var vehiclesRented = station.Vehicles.Count(v => v.Status == "Rented");

            ViewBag.Station = station;
            ViewBag.TotalRentals = totalRentals;
            ViewBag.TotalReturns = totalReturns;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.VehiclesAvailable = vehiclesAvailable;
            ViewBag.VehiclesRented = vehiclesRented;

            return View();
        }
    }
}