using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SaigonRide.Models;

namespace SaigonRide.Controllers
{
    public class VehicleController : Controller
    {
        private readonly AppDbContext _context;

        public VehicleController(AppDbContext context)
        {
            _context = context;
        }

        // INDEX (SEARCH + SORT)

        public async Task<IActionResult> Index(string search, string sort)
        {
            ViewBag.Search = search;
            ViewBag.Sort = sort;

            var vehicles = _context.Vehicles
                .Include(v => v.Station)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                vehicles = vehicles.Where(v =>
                    v.Type.Contains(search) ||
                    v.Status.Contains(search) ||
                    (v.Station != null && v.Station.StationName.Contains(search)));
            }

            switch (sort)
            {
                case "station":
                    vehicles = vehicles.OrderBy(v => v.Station != null ? v.Station.StationName : "");
                    break;
                case "type":
                    vehicles = vehicles.OrderBy(v => v.Type);
                    break;
                default:
                    vehicles = vehicles.OrderBy(v => v.VehicleID);
                    break;
            }

            return View(await vehicles.ToListAsync());
        }

        // DETAILS

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var vehicle = await _context.Vehicles
                .Include(v => v.Station)
                .FirstOrDefaultAsync(m => m.VehicleID == id);

            if (vehicle == null) return NotFound();

            return View(vehicle);
        }

        // CREATE

        public async Task<IActionResult> Create()
        {
            if (HttpContext.Session.GetString("UserType") != "Admin")
                return RedirectToAction("Index", "Home");

            ViewBag.StationID = new SelectList(
                await _context.Stations.ToListAsync(),
                "StationID",
                "StationName"
            );

            ViewBag.PriceHint = "Standard Bike: 500 VND/phút | E-Scooter: 1500 VND/phút";

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("VehicleID,Type,StationID")] Vehicle vehicle)
        {
            if (HttpContext.Session.GetString("UserType") != "Admin")
                return RedirectToAction("Index", "Home");

            if (vehicle.Type == "Standard Bike")
                vehicle.PricePerMinute = 500;
            else if (vehicle.Type == "E-Scooter")
                vehicle.PricePerMinute = 1500;

            vehicle.Status = "Available";

            _context.Vehicles.Add(vehicle);

            var station = await _context.Stations
                .FirstOrDefaultAsync(s => s.StationID == vehicle.StationID);

            if (station != null)
                station.CurrentInventory++;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Vehicle added successfully!";
            return RedirectToAction(nameof(Index));
        }

        // EDIT

        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetString("UserType") != "Admin")
                return RedirectToAction("Index", "Home");

            if (id == null) return NotFound();

            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound();

            ViewBag.StationID = new SelectList(
                await _context.Stations.ToListAsync(),
                "StationID",
                "StationName",
                vehicle.StationID
            );

            return View(vehicle);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("VehicleID,Type,Status,PricePerMinute,StationID")] Vehicle vehicle)
        {
            if (HttpContext.Session.GetString("UserType") != "Admin")
                return RedirectToAction("Index", "Home");

            if (id != vehicle.VehicleID) return NotFound();

            var oldVehicle = await _context.Vehicles
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.VehicleID == id);

            if (oldVehicle == null) return NotFound();

            try
            {
                _context.Update(vehicle);

                if (oldVehicle.StationID != vehicle.StationID)
                {
                    var oldStation = await _context.Stations
                        .FirstOrDefaultAsync(s => s.StationID == oldVehicle.StationID);

                    var newStation = await _context.Stations
                        .FirstOrDefaultAsync(s => s.StationID == vehicle.StationID);

                    if (oldStation != null && oldStation.CurrentInventory > 0)
                        oldStation.CurrentInventory--;

                    if (newStation != null)
                        newStation.CurrentInventory++;
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = "Vehicle updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehicleExists(vehicle.VehicleID))
                    return NotFound();
                throw;
            }
        }

        // DELETE

        public async Task<IActionResult> Delete(int? id)
        {
            if (HttpContext.Session.GetString("UserType") != "Admin")
                return RedirectToAction("Index", "Home");

            if (id == null) return NotFound();

            var vehicle = await _context.Vehicles
                .Include(v => v.Station)
                .FirstOrDefaultAsync(m => m.VehicleID == id);

            if (vehicle == null) return NotFound();

            return View(vehicle);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetString("UserType") != "Admin")
                return RedirectToAction("Index", "Home");

            var vehicle = await _context.Vehicles.FindAsync(id);

            if (vehicle != null)
            {
                // Chặn xóa nếu xe đang được thuê
                if (vehicle.Status == "Rented")
                {
                    TempData["Error"] = "Không thể xóa xe đang được thuê!";
                    return RedirectToAction(nameof(Index));
                }

                var station = await _context.Stations
                    .FirstOrDefaultAsync(s => s.StationID == vehicle.StationID);

                if (station != null && station.CurrentInventory > 0)
                    station.CurrentInventory--;

                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Vehicle deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private bool VehicleExists(int id)
        {
            return _context.Vehicles.Any(e => e.VehicleID == id);
        }
    }
}