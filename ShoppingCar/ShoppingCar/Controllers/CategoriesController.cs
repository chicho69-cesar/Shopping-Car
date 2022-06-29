﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCar.Data;
using ShoppingCar.Data.Entities;
using Vereyon.Web;

namespace ShoppingCar.Controllers {
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller {
        private readonly DataContext _context;
        private readonly IFlashMessage _flashMessage;

        public CategoriesController(DataContext context, IFlashMessage flashMessage) {
            _context = context;
            _flashMessage = flashMessage;
        }

        [HttpGet]
        public async Task<IActionResult> Index() {
            return View(await _context.Categories.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id) {
            if (id == null || _context.Categories == null) {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category is null) {
                return NotFound();
            }

            return View(category);
        }

        [HttpGet]
        public IActionResult Create() {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category) {
            if (ModelState.IsValid) {
                try {
                    _context.Add(category);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                } catch (DbUpdateException dbUpdateException) {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate")) {
                        _flashMessage.Danger("Ya existe una categoria con el mismo nombre");
                    } else {
                        _flashMessage.Danger(dbUpdateException.InnerException.Message);
                    }
                } catch (Exception exception) {
                    _flashMessage.Danger(exception.Message);
                }
            }

            return View(category);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id) {
            if (id == null || _context.Categories == null) {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);

            if (category is null) {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category) {
            if (id != category.Id) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                try {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                } catch (DbUpdateException dbUpdateException) {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate")) {
                        _flashMessage.Danger("Ya existe una categoria con el mismo nombre");
                    } else {
                        _flashMessage.Danger(dbUpdateException.InnerException.Message);
                    }
                } catch (Exception exception) {
                    _flashMessage.Danger(exception.Message);
                }
            }

            return View(category);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id) {
            if (id == null || _context.Categories == null) {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category is null) {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            if (_context.Categories == null) {
                return Problem("Entity set 'DataContext.Countries'  is null.");
            }

            var category = await _context.Categories.FindAsync(id);

            if (category != null) {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}