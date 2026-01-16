using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Garage_2.Data;
using Garage_2.Models.Entities;

namespace Garage_2.Controllers
{
    public class ParkingSessionsController : Controller
    {
        private readonly GarageContext _context;

        public ParkingSessionsController(GarageContext context)
        {
            _context = context;
        }

        // GET: ParkingSessions
        public async Task<IActionResult> Index()
        {
            var garageContext = _context.ParkingSessions.Include(p => p.Vehicle);
            return View(await garageContext.ToListAsync());
        }

        // GET: ParkingSessions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parkingSession = await _context.ParkingSessions
                .Include(p => p.Vehicle)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (parkingSession == null)
            {
                return NotFound();
            }

            return View(parkingSession);
        }

        // GET: ParkingSessions/Create
        public IActionResult Create()
        {
            ViewData["VehicleId"] = new SelectList(_context.Vehicles, "Id", "Color");
            return View();
        }

        // POST: ParkingSessions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,VehicleId,ArrivalTime,DepartureTime")] ParkingSession parkingSession)
        {
            if (ModelState.IsValid)
            {
                _context.Add(parkingSession);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["VehicleId"] = new SelectList(_context.Vehicles, "Id", "Color", parkingSession.VehicleId);
            return View(parkingSession);
        }

        // GET: ParkingSessions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parkingSession = await _context.ParkingSessions.FindAsync(id);
            if (parkingSession == null)
            {
                return NotFound();
            }
            ViewData["VehicleId"] = new SelectList(_context.Vehicles, "Id", "Color", parkingSession.VehicleId);
            return View(parkingSession);
        }

        // POST: ParkingSessions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,VehicleId,ArrivalTime,DepartureTime")] ParkingSession parkingSession)
        {
            if (id != parkingSession.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(parkingSession);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ParkingSessionExists(parkingSession.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["VehicleId"] = new SelectList(_context.Vehicles, "Id", "Color", parkingSession.VehicleId);
            return View(parkingSession);
        }

        // GET: ParkingSessions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parkingSession = await _context.ParkingSessions
                .Include(p => p.Vehicle)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (parkingSession == null)
            {
                return NotFound();
            }

            return View(parkingSession);
        }

        // POST: ParkingSessions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var parkingSession = await _context.ParkingSessions.FindAsync(id);
            if (parkingSession != null)
            {
                _context.ParkingSessions.Remove(parkingSession);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ParkingSessionExists(int id)
        {
            return _context.ParkingSessions.Any(e => e.Id == id);
        }
    }
}
