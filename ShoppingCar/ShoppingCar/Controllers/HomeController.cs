using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCar.Data;
using ShoppingCar.Data.Entities;
using ShoppingCar.Helpers;
using ShoppingCar.Models;
using System.Diagnostics;

namespace ShoppingCar.Controllers {
    public class HomeController : Controller {
        private readonly ILogger<HomeController> _logger;
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;

        public HomeController(
            ILogger<HomeController> logger,
            DataContext context,
            IUserHelper userHelper
        ) {
            _logger = logger;
            _context = context;
            _userHelper = userHelper;
        }

        [HttpGet]
        public async Task<IActionResult> Index() {
            List<Product> products = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductCategories)
                .OrderBy(p => p.Description)
                .ToListAsync();

            var model = new HomeViewModel { 
                Products = products 
            };

            User user = await _userHelper
                .GetUserAsync(User.Identity.Name);
            
            if (user != null) {
                model.Quantity = await _context.TemporalSales
                    .Where(ts => ts.User.Id == user.Id)
                    .SumAsync(ts => ts.Quantity);
            }

            return View(model);
        }

        /*TODO: Hacer que el agregar al carrito sea a traves de AJAX
        para evitar que la pagina se recarge*/
        [HttpGet]
        public async Task<IActionResult> Add(int? id) {
            if (id == null) {
                return NotFound();
            }

            if (!User.Identity.IsAuthenticated) {
                return RedirectToAction("Login", "Account");
            }

            Product product = await _context.Products.FindAsync(id);
            if (product == null) {
                return NotFound();
            }

            User user = await _userHelper.GetUserAsync(User.Identity.Name);
            if (user == null) {
                return NotFound();
            }

            var temporalSale = new TemporalSale {
                Product = product,
                Quantity = 1,
                User = user
            };

            _context.TemporalSales.Add(temporalSale);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Privacy() {
            return View();
        }

        [Route("error/404")]
        public IActionResult Error404() {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}