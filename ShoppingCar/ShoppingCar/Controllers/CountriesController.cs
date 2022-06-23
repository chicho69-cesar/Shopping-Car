using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShoppingCar.Data;
using ShoppingCar.Data.Entities;

namespace ShoppingCar.Controllers {
    public class CountriesController : Controller {
        private readonly DataContext _context;

        public CountriesController(DataContext context) {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index() {
            return View(await _context.Countries.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id) {
            if (id == null || _context.Countries == null) {
                return NotFound();
            }

            var country = await _context.Countries
                .FirstOrDefaultAsync(c => c.Id == id);

            if (country is null) {
                return NotFound();
            }

            return View(country);
        }

        [HttpGet]
        public IActionResult Create() {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Country country) {
            if (ModelState.IsValid) {
                try {
                    _context.Add(country);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                } catch (DbUpdateException dbUpdateException) {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate")) {
                        ModelState.AddModelError(string.Empty, "Ya existe un país con el mismo nombre.");
                    } else {
                        ModelState.AddModelError(string.Empty, dbUpdateException.InnerException.Message);
                    }
                } catch (Exception exception) {
                    ModelState.AddModelError(string.Empty, exception.Message);
                }
            }

            return View(country);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id) {
            if (id == null || _context.Countries == null) {
                return NotFound();
            }

            var country = await _context.Countries.FindAsync(id);

            if (country is null) {
                return NotFound();
            }

            return View(country);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Country country) {
            if (id != country.Id) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                try {
                    _context.Update(country);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                } catch (DbUpdateException dbUpdateException) {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate")) {
                        ModelState.AddModelError(string.Empty, "Ya existe un país con el mismo nombre.");
                    } else {
                        ModelState.AddModelError(string.Empty, dbUpdateException.InnerException.Message);
                    }
                } catch (Exception exception) {
                    ModelState.AddModelError(string.Empty, exception.Message);
                }
            }

            return View(country);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id) {
            if (id == null || _context.Countries == null) {
                return NotFound();
            }

            var country = await _context.Countries
                .FirstOrDefaultAsync(m => m.Id == id);

            if (country is null) {
                return NotFound();
            }

            return View(country);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            if (_context.Countries == null) {
                return Problem("Entity set 'DataContext.Countries'  is null.");
            }

            var country = await _context.Countries.FindAsync(id);

            if (country != null) {
                _context.Countries.Remove(country);
            }
            
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}