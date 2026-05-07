using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaigonRide.Models;

namespace SaigonRide.Controllers
{
    public class RentalController : Controller
    {
        private readonly AppDbContext _context;

        public RentalController(AppDbContext context)
        {
            _context = context;
        }

        // Admin xem tất cả rental
        public async Task<IActionResult> Index()
        {
            var rentals = await _context.Rentals
                .Include(r => r.Vehicle)
                .Include(r => r.User)
                .ToListAsync();

            return View(rentals);
        }

        // CONFIRM RENTAL

        // GET: Confirm
        public async Task<IActionResult> Confirm(int vehicleId)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var vehicle = await _context.Vehicles
                .Include(v => v.Station)
                .FirstOrDefaultAsync(v => v.VehicleID == vehicleId);

            if (vehicle == null || vehicle.Status != "Available")
            {
                TempData["Error"] = "Xe này không khả dụng!";
                return RedirectToAction("Index", "Vehicle");
            }

            return View(vehicle);
        }

        // POST: Confirm
        [HttpPost]
        public async Task<IActionResult> Confirm(int vehicleId, bool confirmed)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            if (!confirmed)
            {
                TempData["Error"] = "Bạn đã hủy thuê xe.";
                return RedirectToAction("Index", "Vehicle");
            }

            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.VehicleID == vehicleId);

            if (vehicle == null || vehicle.Status != "Available")
            {
                TempData["Error"] = "Xe này không khả dụng!";
                return RedirectToAction("Index", "Vehicle");
            }

            var rental = new Rental
            {
                VehicleID = vehicleId,
                UserID = userId.Value,
                StartStationID = vehicle.StationID,
                StartTime = DateTime.Now,
                Status = "Active"
            };

            vehicle.Status = "Rented";

            // Trừ inventory trạm gốc ngay khi thuê
            if (vehicle.StationID.HasValue)
            {
                var startStation = await _context.Stations
                    .FirstOrDefaultAsync(s => s.StationID == vehicle.StationID);
                if (startStation != null && startStation.CurrentInventory > 0)
                    startStation.CurrentInventory--;
            }

            _context.Rentals.Add(rental);
            _context.Vehicles.Update(vehicle);
            await _context.SaveChangesAsync();

            return RedirectToAction("Success", new { rentalId = rental.RentalID });
        }

        // SUCCESS

        public async Task<IActionResult> Success(int rentalId)
        {
            var rental = await _context.Rentals
                .Include(r => r.Vehicle)
                    .ThenInclude(v => v.Station)
                .FirstOrDefaultAsync(r => r.RentalID == rentalId);

            if (rental == null)
                return RedirectToAction("Index", "Vehicle");

            return View(rental);
        }

        // RETURN VEHICLE

        // GET: Return — load danh sách trạm để user chọn nơi trả
        public async Task<IActionResult> Return(int rentalId)
        {
            var rental = await _context.Rentals
                .Include(r => r.Vehicle)
                    .ThenInclude(v => v.Station)
                .FirstOrDefaultAsync(r => r.RentalID == rentalId);

            if (rental == null || rental.Status != "Active")
            {
                TempData["Error"] = "Rental not found.";
                return RedirectToAction("MyRentals");
            }

            if (rental.Vehicle == null)
            {
                TempData["Error"] = "Vehicle not found.";
                return RedirectToAction("MyRentals");
            }

            // Tính giá gốc (chưa biết discount vì chưa chọn trạm)
            var now = DateTime.Now;
            double minutes = (now - rental.StartTime).TotalMinutes;
            double fare = minutes * rental.Vehicle.PricePerMinute;

            ViewBag.Now = now;
            ViewBag.Minutes = Math.Round(minutes, 1);
            ViewBag.OriginalFare = Math.Round(fare, 0);

            // Load tất cả trạm cho dropdown
            ViewBag.Stations = await _context.Stations.ToListAsync();

            return View(rental);
        }

        // POST: Rental/Return
        [HttpPost]
        public async Task<IActionResult> Return(int rentalId, int returnStationId, string paymentMethod)
        {
            var rental = await _context.Rentals
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.RentalID == rentalId);

            if (rental == null || rental.Status != "Active")
            {
                TempData["Error"] = "Rental not found.";
                return RedirectToAction("MyRentals");
            }

            if (rental.Vehicle == null)
            {
                TempData["Error"] = "Vehicle not found.";
                return RedirectToAction("MyRentals");
            }

            // Lấy trạm gốc (nơi xe đang đứng khi được thuê)
            var startStation = await _context.Stations
                .FirstOrDefaultAsync(s => s.StationID == rental.Vehicle.StationID);

            // Lấy trạm trả mà user chọn
            var returnStation = await _context.Stations
                .FirstOrDefaultAsync(s => s.StationID == returnStationId);

            if (returnStation == null)
            {
                TempData["Error"] = "Trạm không hợp lệ!";
                return RedirectToAction("Return", new { rentalId });
            }

            rental.EndTime = DateTime.Now;
            rental.ReturnStationID = returnStationId;

            double minutes = (rental.EndTime.Value - rental.StartTime).TotalMinutes;
            double fare = minutes * rental.Vehicle.PricePerMinute;

            // Kiểm tra điều kiện giảm 15%:
            // Trạm trả có số xe hiện tại < 20% sức chứa
            bool hasDiscount = returnStation.Capacity > 0
                && returnStation.CurrentInventory < returnStation.Capacity * 0.20;

            double discount = hasDiscount ? fare * 0.15 : 0;
            double finalFare = fare - discount;

            rental.TotalFare = Math.Round(finalFare, 0);
            rental.Discount = Math.Round(discount, 0);
            rental.Status = "Completed";

            // Xe chuyển sang trạm mới + trạm mới cộng 1 xe
            // (trạm gốc đã được trừ lúc thuê xe, không trừ lại)
            rental.Vehicle.StationID = returnStationId;
            rental.Vehicle.Status = "Available";
            returnStation.CurrentInventory++;

            await _context.SaveChangesAsync();

            if (paymentMethod == "Cash")
            {
                TempData["Success"] = $"Thanh toán tiền mặt thành công! Tổng: {finalFare:N0} VND";
                return RedirectToAction("PaymentSuccess", new
                {
                    rentalId = rental.RentalID,
                    method = "Cash",
                    amount = rental.TotalFare
                });
            }

            return RedirectToAction("PaymentGateway", new
            {
                rentalId = rental.RentalID,
                method = paymentMethod,
                amount = rental.TotalFare
            });
        }

        // GET: PaymentGateway
        public IActionResult PaymentGateway(int rentalId, string method, double amount)
        {
            ViewBag.RentalId = rentalId;
            ViewBag.Method = method;
            ViewBag.Amount = amount;
            return View();
        }

        // GET: Thanh toán thành công
        public IActionResult PaymentSuccess(int rentalId, string method, double amount)
        {
            ViewBag.RentalId = rentalId;
            ViewBag.Method = method;
            ViewBag.Amount = amount;
            return View();
        }

        // GET: Thanh toán thất bại
        public IActionResult PaymentFailedView(int rentalId, string method, double amount)
        {
            ViewBag.RentalId = rentalId;
            ViewBag.Method = method;
            ViewBag.Amount = amount;
            return View();
        }

        // POST: Thanh toán thất bại → rollback rental về Active, quay lại màn hình Return
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PaymentFailed(int rentalId, string method, double amount)
        {
            var rental = await _context.Rentals
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.RentalID == rentalId);

            if (rental != null && rental.Status == "Completed" && rental.Vehicle != null)
            {
                // Rollback: trạm trả trừ lại 1 xe
                if (rental.ReturnStationID.HasValue)
                {
                    var returnStation = await _context.Stations
                        .FirstOrDefaultAsync(s => s.StationID == rental.ReturnStationID);
                    if (returnStation != null && returnStation.CurrentInventory > 0)
                        returnStation.CurrentInventory--;
                }

                // Rollback: xe quay về trạm gốc
                // (không cộng lại inventory trạm gốc vì đã trừ lúc thuê)
                if (rental.StartStationID.HasValue)
                    rental.Vehicle.StationID = rental.StartStationID;

                rental.Status = "Active";
                rental.EndTime = null;
                rental.TotalFare = null;
                rental.Discount = 0;
                rental.ReturnStationID = null;
                rental.Vehicle.Status = "Rented";

                await _context.SaveChangesAsync();
            }

            TempData["Error"] = "Thanh toán thất bại! Vui lòng thử lại.";
            return RedirectToAction("Return", new { rentalId });
        }

        // USER HISTORY

        public async Task<IActionResult> MyRentals()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");

            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var rentals = await _context.Rentals
                .Where(r => r.UserID == userId)
                .Include(r => r.Vehicle)
                .OrderByDescending(r => r.StartTime)
                .ToListAsync();

            return View(rentals);
        }
    }
}