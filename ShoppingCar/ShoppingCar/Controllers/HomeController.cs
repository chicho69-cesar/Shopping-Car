using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCar.Common;
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
        private readonly IOrdersHelper _ordersHelper;

        public HomeController(
            ILogger<HomeController> logger,
            DataContext context,
            IUserHelper userHelper,
            IOrdersHelper ordersHelper
        ) {
            _logger = logger;
            _context = context;
            _userHelper = userHelper;
            _ordersHelper = ordersHelper;
        }

        [HttpGet]
        public async Task<IActionResult> Index() {
            List<Product> products = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductCategories)
                .Where(p => p.Stock > 0)
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
        public async Task<IActionResult> Details(int? id) {
            if (id == null) {
                return NotFound();
            }

            Product product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) {
                return NotFound();
            }

            string categories = "";
            foreach (ProductCategory category in product.ProductCategories)
                categories += $"{category.Category.Name}, ";
            categories = categories.Substring(0, categories.Length - 2);

            var model = new AddProductToCartViewModel {
                Categories = categories,
                Description = product.Description,
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                ProductImages = product.ProductImages,
                Quantity = 1,
                Stock = product.Stock,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(AddProductToCartViewModel model) {
            if (!User.Identity.IsAuthenticated) {
                return RedirectToAction("Login", "Account");
            }

            Product product = await _context.Products
                .FindAsync(model.Id);

            if (product == null) {
                return NotFound();
            }

            User user = await _userHelper
                .GetUserAsync(User.Identity.Name);
            
            if (user == null) {
                return NotFound();
            }

            var temporalSale = new TemporalSale {
                Product = product,
                Quantity = model.Quantity,
                Remarks = model.Remarks,
                User = user
            };

            _context.TemporalSales.Add(temporalSale);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ShowCart() {
            User user = await _userHelper
                .GetUserAsync(User.Identity.Name);

            if (user == null) {
                return NotFound();
            }

            List<TemporalSale> temporalSales = await _context.TemporalSales
                .Include(ts => ts.Product)
                .ThenInclude(p => p.ProductImages)
                .Where(ts => ts.User.Id == user.Id)
                .ToListAsync();

            var model = new ShowCartViewModel {
                User = user,
                TemporalSales = temporalSales,
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> DecreaseQuantity(int? id) {
            if (id == null) {
                return NotFound();
            }

            TemporalSale temporalSale = await _context.TemporalSales.FindAsync(id);
            if (temporalSale == null) {
                return NotFound();
            }

            if (temporalSale.Quantity > 1) {
                temporalSale.Quantity--;
                _context.TemporalSales.Update(temporalSale);
                
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ShowCart));
        }

        [HttpGet]
        public async Task<IActionResult> IncreaseQuantity(int? id) {
            if (id == null) {
                return NotFound();
            }

            TemporalSale temporalSale = await _context.TemporalSales.FindAsync(id);
            if (temporalSale == null) {
                return NotFound();
            }

            temporalSale.Quantity++;
            _context.TemporalSales.Update(temporalSale);
            
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ShowCart));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id) {
            if (id == null) {
                return NotFound();
            }

            TemporalSale temporalSale = await _context.TemporalSales.FindAsync(id);
            if (temporalSale == null) {
                return NotFound();
            }

            _context.TemporalSales.Remove(temporalSale);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(ShowCart));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id) {
            if (id == null) {
                return NotFound();
            }

            TemporalSale temporalSale = await _context.TemporalSales.FindAsync(id);
            if (temporalSale == null) {
                return NotFound();
            }

            var model = new EditTemporalSaleViewModel {
                Id = temporalSale.Id,
                Quantity = temporalSale.Quantity,
                Remarks = temporalSale.Remarks,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditTemporalSaleViewModel model) {
            if (id != model.Id) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                try {
                    TemporalSale temporalSale = await _context.TemporalSales.FindAsync(id);
                    temporalSale.Quantity = model.Quantity;
                    temporalSale.Remarks = model.Remarks;
                    
                    _context.Update(temporalSale);
                    await _context.SaveChangesAsync();
                } catch (Exception exception) {
                    ModelState.AddModelError(string.Empty, exception.Message);
                    return View(model);
                }

                return RedirectToAction(nameof(ShowCart));
            }

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public IActionResult OrderSuccess() {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShowCart(ShowCartViewModel model) {
            User user = await _userHelper.GetUserAsync(User.Identity.Name);
            if (user == null) {
                return NotFound();
            }

            model.User = user;
            model.TemporalSales = await _context.TemporalSales
                .Include(ts => ts.Product)
                .ThenInclude(p => p.ProductImages)
                .Where(ts => ts.User.Id == user.Id)
                .ToListAsync();

            Response response = await _ordersHelper.ProcessOrderAsync(model);
            if (response.IsSuccess) {
                return RedirectToAction(nameof(OrderSuccess));
            }

            ModelState.AddModelError(string.Empty, response.Message);
            return View(model);
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