using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCar.Data;
using ShoppingCar.Data.Entities;
using ShoppingCar.Enums;
using ShoppingCar.Helpers;
using ShoppingCar.Models;

namespace ShoppingCar.Controllers {
    public class AccountController : Controller {
        private readonly IUserHelper _userHelper;
        private readonly DataContext _context;
        private readonly ICombosHelper _combosHelper;
        private readonly IBlobHelper _blobHelper;
        private readonly IGetLocation _getLocation;

        public AccountController(
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
        public IActionResult Login() {
            if (User.Identity.IsAuthenticated) {
                return RedirectToAction("Index", "Home");
            }

            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model) {
            if (ModelState.IsValid) {
                Microsoft.AspNetCore.Identity.SignInResult result = await _userHelper.LoginAsync(model);
                
                if (result.Succeeded) {
                    return RedirectToAction("Index", "Home");
                }

                if (result.IsLockedOut) {
                    ModelState.AddModelError(string.Empty, "Ha superado el máximo número de intentos, su cuenta está bloqueada, intente de nuevo en 5 minutos.");
                } else {
                    ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos.");
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout() {
            await _userHelper.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Register() {
            var model = new AddUserViewModel {
                Id = Guid.Empty.ToString(),
                Countries = await _combosHelper.GetComboCountriesAsync(),
                States = await _combosHelper.GetComboStatesAsync(0),
                Cities = await _combosHelper.GetComboCitiesAsync(0),
                UserType = UserType.User,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(AddUserViewModel model) {
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

                var loginViewModel = new LoginViewModel {
                    Password = model.Password,
                    RememberMe = false,
                    Username = model.Username
                };

                var result2 = await _userHelper.LoginAsync(loginViewModel);
                
                if (result2.Succeeded) {
                    return RedirectToAction("Index", "Home");
                }
            }

            model.Countries = await _combosHelper.GetComboCountriesAsync();
            model.States = await _combosHelper.GetComboStatesAsync(0);
            model.Cities = await _combosHelper.GetComboCitiesAsync(0);

            return View(model);
        }

        [HttpGet]
        public IActionResult NotAuthorized() {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ChangeUser() {
            User user = await _userHelper.GetUserAsync(User.Identity.Name);
            
            if (user == null) {
                return NotFound();
            }

            var model = new EditUserViewModel {
                Address = user.Address,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                ImageId = user.ImageId,
                Cities = await _combosHelper.GetComboCitiesAsync(user.City.State.Id),
                CityId = user.City.Id,
                Countries = await _combosHelper.GetComboCountriesAsync(),
                CountryId = user.City.State.Country.Id,
                StateId = user.City.State.Id,
                States = await _combosHelper.GetComboStatesAsync(user.City.State.Country.Id),
                Id = user.Id,
                Document = user.Document
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUser(EditUserViewModel model) {
            if (ModelState.IsValid) {
                Guid imageId = model.ImageId;

                if (model.ImageFile != null) {
                    imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "users");
                }

                User user = await _userHelper.GetUserAsync(User.Identity.Name);

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Address = model.Address;
                user.PhoneNumber = model.PhoneNumber;
                user.ImageId = imageId;
                user.City = await _context.Cities.FindAsync(model.CityId);
                user.Document = model.Document;

                await _userHelper.UpdateUserAsync(user);

                return RedirectToAction("Index", "Home");
            }

            model.Countries = await _combosHelper.GetComboCountriesAsync();
            model.States = await _combosHelper.GetComboStatesAsync(0);
            model.Cities = await _combosHelper.GetComboCitiesAsync(0);
            
            return View(model);
        }

        [HttpGet]
        public IActionResult ChangePassword() {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model) {
            if (ModelState.IsValid) {
                if (model.OldPassword == model.NewPassword) {
                    ModelState.AddModelError(string.Empty, "La nueva contraseña es igual a la contraseña actual");
                    return View(model);
                }

                var user = await _userHelper.GetUserAsync(User.Identity.Name);
                
                if (user != null) {
                    var result = await _userHelper.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                    
                    if (result.Succeeded) {
                        return RedirectToAction("ChangeUser");
                    } else {
                        ModelState.AddModelError(string.Empty, "Contraseña incorrecta");
                    }
                } else {
                    ModelState.AddModelError(string.Empty, "Usuario no encontrado");
                }
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