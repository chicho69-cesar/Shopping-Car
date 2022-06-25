using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCar.Data;
using ShoppingCar.Data.Entities;
using ShoppingCar.Enums;
using ShoppingCar.Helpers;
using ShoppingCar.Models;

namespace ShoppingCar.Controllers {
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller {
        private readonly IUserHelper _userHelper;
        private readonly DataContext _context;
        private readonly ICombosHelper _combosHelper;
        private readonly IBlobHelper _blobHelper;
        private readonly IGetLocation _getLocation;

        public UsersController(
            IUserHelper userHelper, 
            DataContext context, 
            ICombosHelper combosHelper, 
            IBlobHelper blobHelper,
            IGetLocation getLocation
        ) {
            _userHelper = userHelper;
            _context = context;
            _combosHelper = combosHelper;
            _blobHelper = blobHelper;
            _getLocation = getLocation;
        }

        [HttpGet]
        public async Task<IActionResult> Index() {
            var users = await _context.Users
                .Include(u => u.City)
                .ThenInclude(c => c.State)
                .ThenInclude(s => s.Country)
                .ToListAsync();

            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Create() {
            var model = new AddUserViewModel {
                Id = Guid.Empty.ToString(),
                Countries = await _combosHelper.GetComboCountriesAsync(),
                States = await _combosHelper.GetComboStatesAsync(0),
                Cities = await _combosHelper.GetComboCitiesAsync(0),
                UserType = UserType.Admin,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddUserViewModel model) {
            if (ModelState.IsValid) {
                Guid imageId = Guid.Empty;

                if (model.ImageFile != null) {
                    imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "users");
                }

                model.ImageId = imageId;
                User user = await _userHelper.AddUserAsync(model);
                
                if (user == null) {
                    ModelState.AddModelError(string.Empty, "Este correo ya está siendo usado.");

                    model.Countries = await _combosHelper.GetComboCountriesAsync();
                    model.States = await _combosHelper.GetComboStatesAsync(0);
                    model.Cities = await _combosHelper.GetComboCitiesAsync(0);

                    return View(model);
                }

                model.Countries = await _combosHelper.GetComboCountriesAsync();
                model.States = await _combosHelper.GetComboStatesAsync(0);
                model.Cities = await _combosHelper.GetComboCitiesAsync(0);

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        public JsonResult GetStates(int countryId) {
            var states = _getLocation.GetStates(countryId);
            return Json(states);
        }

        public JsonResult GetCities(int stateId) {
            var states = _getLocation.GetCities(stateId);
            return Json(states);
        }
    }
}