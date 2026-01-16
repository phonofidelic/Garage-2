using Garage_2.Data;
using Garage_2.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Garage_2.Controllers
{
    public class ParkingSpotsController : Controller
    {
        //[Authorize(Roles = "Admin")] 
        private readonly GarageContext _context;

        public ParkingSpotsController(GarageContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var spots = await _context.ParkingSpotV2.ToListAsync();

            return View(spots);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ParkingSpotV2 parkingSpot)
        {
            var existingSpot = await _context.ParkingSpotV2
                .FirstOrDefaultAsync(s => s.SpotNumber == parkingSpot.SpotNumber);

            if (existingSpot != null)
            {
                ModelState.AddModelError("SpotNumber", "This parking spot number already exists.");
                return View(parkingSpot);
            }

            parkingSpot.CapacityUnits = 3;

            if (ModelState.IsValid)
            {
                _context.Add(parkingSpot);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(parkingSpot);
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var spot = await _context.ParkingSpotV2.FindAsync(id);

            if (spot == null) return NotFound();

            return View(spot);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, ParkingSpotV2 parkingSpot)
        {
            if (id != parkingSpot.Id) return NotFound();

            var spotWithSameNumber = await _context.ParkingSpotV2
                .FirstOrDefaultAsync(s => s.SpotNumber == parkingSpot.SpotNumber);

            if (spotWithSameNumber != null && spotWithSameNumber.Id != id)
            {
                ModelState.AddModelError("SpotNumber", "This number is already taken by another parking spot.");
                return View(parkingSpot);
            }

            var spotToUpdate = await _context.ParkingSpotV2.FindAsync(id);

            if (spotToUpdate == null) return NotFound();

            spotToUpdate.SpotNumber = parkingSpot.SpotNumber;
            spotToUpdate.IsBlocked = parkingSpot.IsBlocked;

            if (ModelState.IsValid)
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(spotToUpdate);
        }
    }
}