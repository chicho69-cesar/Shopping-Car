using Microsoft.AspNetCore.Mvc;
using ShoppingCar.Helpers;
using ShoppingCar.Models;

namespace ShoppingCar.Controllers {
    public class AccountController : Controller {
        private readonly IUserHelper _userHelper;

        public AccountController(IUserHelper userHelper) {
            _userHelper = userHelper;
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

                ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos.");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout() {
            await _userHelper.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult NotAuthorized() {
            return View();
        }
    }
}