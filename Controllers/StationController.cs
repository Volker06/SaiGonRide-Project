using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaigonRide.Models;

namespace SaigonRide.Controllers
{
    public class StationController : Controller
    {
        private readonly AppDbContext _context;

        public StationController(AppDbContext context)
        {
            _context = context;
        }

        // GET: STATIONS (Admin)
        public async Task<IActionResult> Index()
        {
            var stations = await _context.Stations
                .Include(s => s.Vehicles)
                .ToListAsync();
            return View(stations);
        }

        // GET: Station/SelectStation (User chọn trạm)
        public async Task<IActionResult> SelectStation()
        {
            var stations = await _context.Stations
                .Include(s => s.Vehicles)
                .ToListAsync();
            return View(stations);
        }

        // GET: Station/VehiclesAtStation/5 (User chọn xe tại trạm)
        public async Task<IActionResult> VehiclesAtStation(int id)
        {
            var station = await _context.Stations
                .Include(s => s.Vehicles)
                .FirstOrDefaultAsync(s => s.StationID == id);

            if (station == null)
                return RedirectToAction("SelectStation");

            return View(station);
        }

        // GET: STATIONS/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var station = await _context.Stations
                .Include(s => s.Vehicles)
                .FirstOrDefaultAsync(m => m.StationID == id);

            if (station == null) return NotFound();

            return View(station);
        }

        // GET: STATIONS/Create
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("UserType") != "Admin")
                return RedirectToAction("Index", "Home");
            return View();
        }

        // POST: STATIONS/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("StationName,Location,Capacity,CurrentInventory")] Station station)
        {
            if (HttpContext.Session.GetString("UserType") != "Admin")
                return RedirectToAction("Index", "Home");

            _context.Add(station);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Station added successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: STATIONS/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetString("UserType") != "Admin")
                return RedirectToAction("Index", "Home");

            if (id == null) return NotFound();

            var station = await _context.Stations.FindAsync(id);
            if (station == null) return NotFound();

            return View(station);
        }

        // POST: STATIONS/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("StationID,StationName,Location,Capacity,CurrentInventory")] Station station)
        {
            if (HttpContext.Session.GetString("UserType") != "Admin")
                return RedirectToAction("Index", "Home");

            if (id != station.StationID) return NotFound();

            try
            {
                _context.Update(station);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Station updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StationExists(station.StationID))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: STATIONS/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (HttpContext.Session.GetString("UserType") != "Admin")
                return RedirectToAction("Index", "Home");

            if (id == null) return NotFound();

            var station = await _context.Stations
                .Include(s => s.Vehicles)
                .FirstOrDefaultAsync(m => m.StationID == id);

            if (station == null) return NotFound();

            return View(station);
        }

        // POST: STATIONS/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetString("UserType") != "Admin")
                return RedirectToAction("Index", "Home");

            var station = await _context.Stations
                .Include(s => s.Vehicles)
                .FirstOrDefaultAsync(s => s.StationID == id);

            if (station != null)
            {
                // Chặn xóa nếu có xe đang được thuê trong trạm
                bool hasActiveRentals = station.Vehicles != null
                    && station.Vehicles.Any(v => v.Status == "Rented");

                if (hasActiveRentals)
                {
                    TempData["Error"] = "Không thể xóa trạm này vì đang có xe được thuê!";
                    return RedirectToAction(nameof(Index));
                }

                // Null hóa StartStationID trong các Rental liên quan
                var rentalsAsStart = await _context.Rentals
                    .Where(r => r.StartStationID == id)
                    .ToListAsync();
                foreach (var r in rentalsAsStart)
                    r.StartStationID = null;

                // Null hóa ReturnStationID trong các Rental liên quan
                var rentalsAsReturn = await _context.Rentals
                    .Where(r => r.ReturnStationID == id)
                    .ToListAsync();
                foreach (var r in rentalsAsReturn)
                    r.ReturnStationID = null;

                // Null hóa StationID trong các Vehicle liên quan
                if (station.Vehicles != null)
                {
                    foreach (var vehicle in station.Vehicles)
                        vehicle.StationID = null;
                }

                _context.Stations.Remove(station);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Station deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private bool StationExists(int? id)
        {
            return _context.Stations.Any(e => e.StationID == id);
        }
    }
}