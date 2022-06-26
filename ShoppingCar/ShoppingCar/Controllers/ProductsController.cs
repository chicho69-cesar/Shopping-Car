using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCar.Data;
using ShoppingCar.Data.Entities;
using ShoppingCar.Helpers;
using ShoppingCar.Models;

namespace ShoppingCar.Controllers {
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller {
        private readonly DataContext _context;
        private readonly ICombosHelper _combosHelper;
        private readonly IBlobHelper _blobHelper;

        public ProductsController(DataContext context, ICombosHelper combosHelper, IBlobHelper blobHelper) {
            _context = context;
            _combosHelper = combosHelper;
            _blobHelper = blobHelper;
        }

        [HttpGet]
        public async Task<IActionResult> Index() {
            return View(await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Create() {
            var model = new CreateProductViewModel {
                Categories = await _combosHelper.GetComboCategoriesAsync(),
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductViewModel model) {
            if (ModelState.IsValid) {
                Guid imageId = Guid.Empty;

                if (model.ImageFile != null) {
                    imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "products");
                }

                var product = new Product {
                    Description = model.Description,
                    Name = model.Name,
                    Price = model.Price,
                    Stock = model.Stock,
                };

                product.ProductCategories = new List<ProductCategory>() {
                    new ProductCategory {
                        Category = await _context.Categories.FindAsync(model.CategoryId)
                    }
                };

                if (imageId != Guid.Empty) {
                    product.ProductImages = new List<ProductImage>  {
                        new ProductImage { ImageId = imageId }
                    };
                }

                try {
                    _context.Add(product);
                    await _context.SaveChangesAsync();
                    
                    return RedirectToAction(nameof(Index));
                } catch (DbUpdateException dbUpdateException) {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate")) {
                        ModelState.AddModelError(string.Empty, "Ya existe un producto con el mismo nombre.");
                    } else {
                        ModelState.AddModelError(string.Empty, dbUpdateException.InnerException.Message);
                    }
                } catch (Exception exception) {
                    ModelState.AddModelError(string.Empty, exception.Message);
                }
            }

            model.Categories = await _combosHelper.GetComboCategoriesAsync();
            
            return View(model);
        }
    }
}