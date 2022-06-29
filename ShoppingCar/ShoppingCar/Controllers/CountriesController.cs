using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShoppingCar.Data;
using ShoppingCar.Data.Entities;
using ShoppingCar.Models;
using Vereyon.Web;

namespace ShoppingCar.Controllers {
    [Authorize(Roles = "Admin")]
    public class CountriesController : Controller {
        private readonly DataContext _context;
        private readonly IFlashMessage _flashMessage;

        public CountriesController(DataContext context, IFlashMessage flashMessage) {
            _context = context;
            _flashMessage = flashMessage;
        }

        [HttpGet]
        public async Task<IActionResult> Index() {
            var countries = await _context.Countries
                .Include(c => c.States)
                .ToListAsync();

            return View(countries);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id) {
            if (id == null || _context.Countries == null) {
                return NotFound();
            }

            var country = await _context.Countries
                .Include(c => c.States)
                .ThenInclude(s => s.Cities)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (country is null) {
                return NotFound();
            }

            return View(country);
        }

        [HttpGet]
        public IActionResult Create() {
            var country = new Country {
                States = new List<State>()
            };

            return View(country);
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
                        _flashMessage.Danger("Ya existe un país con el mismo nombre");
                    } else {
                        _flashMessage.Danger(dbUpdateException.InnerException.Message);
                    }
                } catch (Exception exception) {
                    _flashMessage.Danger(exception.Message);
                }
            }

            return View(country);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id) {
            if (id == null || _context.Countries == null) {
                return NotFound();
            }

            var country = await _context.Countries
                .Include(c => c.States)
                .FirstOrDefaultAsync(c => c.Id == id);

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
                        _flashMessage.Danger("Ya existe un país con el mismo nombre");
                    } else {
                        _flashMessage.Danger(dbUpdateException.InnerException.Message);
                    }
                } catch (Exception exception) {
                    _flashMessage.Danger(exception.Message);
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
                .Include(c => c.States)
                .FirstOrDefaultAsync(c => c.Id == id);

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

        [HttpGet]
        public async Task<IActionResult> DetailsState(int? id) {
            if (id == null) {
                return NotFound();
            }

            var state = await _context.States
                .Include(s => s.Country)
                .Include(s => s.Cities)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (state is null) {
                return NotFound();
            }

            return View(state);
        }

        [HttpGet]
        public async Task<IActionResult> AddState(int? id) {
            if (id == null) {
                return NotFound();
            }

            var country = await _context.Countries.FindAsync(id);

            if (country is null) {
                return NotFound();
            }

            var model = new StateViewModel {
                CountryId = country.Id
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddState(StateViewModel model) {
            if (ModelState.IsValid) {
                try {
                    var state = new State {
                        Cities = new List<City>(),
                        Country = await _context.Countries.FindAsync(model.CountryId),
                        Name = model.Name
                    };

                    _context.Add(state);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Details), new { Id = model.CountryId });
                } catch (DbUpdateException dbUpdateException) {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate")) {
                        _flashMessage.Danger("Ya existe un estado con el mismo nombre");
                    } else {
                        _flashMessage.Danger(dbUpdateException.InnerException.Message);
                    }
                } catch (Exception exception) {
                    _flashMessage.Danger(exception.Message);
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditState(int? id) {
            if (id == null) {
                return NotFound();
            }

            var state = await _context.States
                .Include(s => s.Country)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (state is null) {
                return NotFound();
            }

            var model = new StateViewModel {
                CountryId = state.Country.Id,
                Id = state.Id,
                Name = state.Name
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditState(int id, StateViewModel model) {
            if (id != model.Id) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                try {
                    var state = new State {
                        Id = model.Id,
                        Name = model.Name
                    };

                    _context.Update(state);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Details), new { Id = model.CountryId });
                } catch (DbUpdateException dbUpdateException) {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate")) {
                        _flashMessage.Danger("Ya existe un estado con el mismo nombre");
                    } else {
                        _flashMessage.Danger(dbUpdateException.InnerException.Message);
                    }
                } catch (Exception exception) {
                    _flashMessage.Danger(exception.Message);
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteState(int? id) {
            if (id == null) {
                return NotFound();
            }

            var state = await _context.States
                .Include(s => s.Country)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (state is null) {
                return NotFound();
            }

            return View(state);
        }

        [HttpPost, ActionName("DeleteState")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStateConfirmed(int id) {
            if (_context.States == null) {
                return Problem("Entity set 'DataContext.States'  is null.");
            }

            var state = await _context.States
                .Include(s => s.Country)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (state != null) {
                _context.States.Remove(state);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { Id = state.Country.Id });
        }

        [HttpGet]
        public async Task<IActionResult> DetailsCity(int? id) {
            if (id == null) {
                return NotFound();
            }

            var city = await _context.Cities
                .Include(c => c.State)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (city is null) {
                return NotFound();
            }

            return View(city);
        }

        [HttpGet]
        public async Task<IActionResult> AddCity(int? id) {
            if (id == null) {
                return NotFound();
            }

            var state = await _context.States.FindAsync(id);

            if (state is null) {
                return NotFound();
            }

            var model = new CityViewModel {
                StateId = state.Id
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCity(CityViewModel model) {
            if (ModelState.IsValid) {
                try {
                    var city = new City {
                        State = await _context.States.FindAsync(model.StateId),
                        Name = model.Name
                    };

                    _context.Add(city);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(DetailsState), new { Id = model.StateId });
                } catch (DbUpdateException dbUpdateException) {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate")) {
                        _flashMessage.Danger("Ya existe una ciudad con el mismo nombre");
                    } else {
                        _flashMessage.Danger(dbUpdateException.InnerException.Message);
                    }
                } catch (Exception exception) {
                    _flashMessage.Danger(exception.Message);
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditCity(int? id) {
            if (id == null) {
                return NotFound();
            }

            var city = await _context.Cities
                .Include(c => c.State)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (city is null) {
                return NotFound();
            }

            var model = new CityViewModel {
                StateId = city.State.Id,
                Id = city.Id,
                Name = city.Name
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCity(int id, CityViewModel model) {
            if (id != model.Id) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                try {
                    var city = new City {
                        Id = model.Id,
                        Name = model.Name,
                    };

                    _context.Update(city);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(DetailsState), new { Id = model.StateId });
                } catch (DbUpdateException dbUpdateException) {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate")) {
                        _flashMessage.Danger("Ya existe una ciudad con el mismo nombre");
                    } else {
                        _flashMessage.Danger(dbUpdateException.InnerException.Message);
                    }
                } catch (Exception exception) {
                    _flashMessage.Danger(exception.Message);
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteCity(int? id) {
            if (id == null) {
                return NotFound();
            }

            var city = await _context.Cities
                .Include(c => c.State)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (city is null) {
                return NotFound();
            }

            return View(city);
        }

        [HttpPost, ActionName("DeleteCity")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCityConfirmed(int id) {
            if (_context.Cities == null) {
                return Problem("Entity set 'DataContext.Cities'  is null.");
            }

            var city = await _context.Cities
                .Include(c => c.State)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (city != null) {
                _context.Cities.Remove(city);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(DetailsState), new { Id = city.State.Id });
        }
    }
}