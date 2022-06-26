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

        [HttpGet]
        public async Task<IActionResult> Edit(int? id) {
            if (id == null) {
                return NotFound();
            }

            Product product = await _context.Products.FindAsync(id);
            if (product == null) {
                return NotFound();
            }

            var model = new EditProductViewModel {
                Description = product.Description,
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Stock = product.Stock,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateProductViewModel model) {
            if (id != model.Id) {
                return NotFound();
            }

            try {
                Product product = await _context.Products.FindAsync(model.Id);
                product.Description = model.Description;
                product.Name = model.Name;
                product.Price = model.Price;
                product.Stock = model.Stock;
                
                _context.Update(product);
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

            return View(model);
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

            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id) {
            if (id == null) {
                return NotFound();
            }

            Product product = await _context.Products
                .Include(p => p.ProductCategories)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (product == null) {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Product model) {
            Product product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductCategories)
                .FirstOrDefaultAsync(p => p.Id == model.Id);

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            foreach (ProductImage productImage in product.ProductImages) {
                await _blobHelper.DeleteBlobAsync(productImage.ImageId, "products");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> AddImage(int? id) {
            if (id == null) {
                return NotFound();
            }

            Product product = await _context.Products.FindAsync(id);
            if (product == null) {
                return NotFound();
            }

            var model = new AddProductImageViewModel {
                ProductId = product.Id,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddImage(AddProductImageViewModel model) {
            if (ModelState.IsValid) {
                Guid imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "products");
                Product product = await _context.Products.FindAsync(model.ProductId);
                
                var productImage = new ProductImage {
                    Product = product,
                    ImageId = imageId,
                };

                try {
                    _context.Add(productImage);
                    await _context.SaveChangesAsync();
                    
                    return RedirectToAction(nameof(Details), new { Id = product.Id });
                } catch (Exception exception) {
                    ModelState.AddModelError(string.Empty, exception.Message);
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteImage(int? id) {
            if (id == null) {
                return NotFound();
            }

            ProductImage productImage = await _context.ProductImages
                .Include(pi => pi.Product)
                .FirstOrDefaultAsync(pi => pi.Id == id);
            
            if (productImage == null) {
                return NotFound();
            }

            await _blobHelper.DeleteBlobAsync(productImage.ImageId, "products");
            _context.ProductImages.Remove(productImage);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Details), new { Id = productImage.Product.Id });
        }

        [HttpGet]
        public async Task<IActionResult> AddCategory(int? id) {
            if (id == null) {
                return NotFound();
            }

            Product product = await _context.Products
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (product == null) {
                return NotFound();
            }

            List<Category> categories = product.ProductCategories
                .Select(pc => new Category {
                    Id = pc.Category.Id,
                    Name = pc.Category.Name
                })
                .ToList();

            var model = new AddCategoryProductViewModel {
                ProductId = product.Id,
                Categories = await _combosHelper.GetComboCategoriesAsync(categories),
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(AddCategoryProductViewModel model) {
            Product product = await _context.Products
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(p => p.Id == model.ProductId);

            if (ModelState.IsValid) {
                var productCategory = new ProductCategory {
                    Category = await _context.Categories.FindAsync(model.CategoryId),
                    Product = product,
                };

                try {
                    _context.Add(productCategory);
                    await _context.SaveChangesAsync();
                    
                    return RedirectToAction(nameof(Details), new { Id = product.Id });
                } catch (Exception exception) {
                    ModelState.AddModelError(string.Empty, exception.Message);
                }
            }

            List<Category> categories = product.ProductCategories
                .Select(pc => new Category {
                    Id = pc.Category.Id,
                    Name = pc.Category.Name
                })
                .ToList();

            model.Categories = await _combosHelper.GetComboCategoriesAsync(categories);
            return View(model);
        }

        public async Task<IActionResult> DeleteCategory(int? id) {
            if (id == null) {
                return NotFound();
            }

            ProductCategory productCategory = await _context.ProductCategories
                .Include(pc => pc.Product)
                .FirstOrDefaultAsync(pc => pc.Id == id);
            
            if (productCategory == null) {
                return NotFound();
            }

            _context.ProductCategories.Remove(productCategory);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Details), new { Id = productCategory.Product.Id });
        }
    }
}