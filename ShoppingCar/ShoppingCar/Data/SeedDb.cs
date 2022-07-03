using Microsoft.EntityFrameworkCore;
using ShoppingCar.Common;
using ShoppingCar.Common.Responses;
using ShoppingCar.Data.Entities;
using ShoppingCar.Enums;
using ShoppingCar.Helpers;
using ShoppingCar.Services;

namespace ShoppingCar.Data {
    public class SeedDb {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;
        private readonly IBlobHelper _blobHelper;
        private readonly IApiService _apiService;

        public SeedDb(
            DataContext context, 
            IUserHelper userHelper, 
            IBlobHelper blobHelper,
            IApiService apiService
        ) {
            _context = context;
            _userHelper = userHelper;
            _blobHelper = blobHelper;
            _apiService = apiService;
        }

        public async Task SeedAsync() {
            await _context.Database.EnsureCreatedAsync();
            await CheckCategoriesAsync();
            await CheckCountriesAsync();
            await CheckRolesAsync();
            await CheckUsersAsync();
            await CheckProductsAsync();
        }

        private async Task CheckCategoriesAsync() {
            if (!_context.Categories.Any()) {
                _context.Categories.Add(new Category { Name = "Electronica" });
                _context.Categories.Add(new Category { Name = "Comida" });
                _context.Categories.Add(new Category { Name = "Tecnología" });
                _context.Categories.Add(new Category { Name = "Ropa" });
                _context.Categories.Add(new Category { Name = "Gamer" });
                _context.Categories.Add(new Category { Name = "Belleza" });
                _context.Categories.Add(new Category { Name = "Nutrición" });
                _context.Categories.Add(new Category { Name = "Calzado" });
                _context.Categories.Add(new Category { Name = "Deportes" });
                _context.Categories.Add(new Category { Name = "Mascotas" });
                _context.Categories.Add(new Category { Name = "Apple" });

                await _context.SaveChangesAsync();
            }
        }

        private async Task CheckCountriesAsync() {
            if (!_context.Countries.Any()) {
                Response responseCountries = await _apiService
                    .GetListAsync<CountryResponse>("/v1", "/countries");
                
                if (responseCountries.IsSuccess) {
                    List<CountryResponse> countries = (List<CountryResponse>)responseCountries.Result;
                    
                    foreach (var countryResponse in countries) {
                        Country country = await _context.Countries
                            .FirstOrDefaultAsync(c => c.Name == countryResponse.Name);
                        
                        if (country == null) {
                            country = new() { 
                                Name = countryResponse.Name, 
                                States = new List<State>() 
                            };
                            
                            Response responseStates = await _apiService
                                .GetListAsync<StateResponse>("/v1", $"/countries/{countryResponse.Iso2}/states");
                            
                            if (responseStates.IsSuccess) {
                                List<StateResponse> states = (List<StateResponse>)responseStates.Result;
                                
                                foreach (var stateResponse in states) {
                                    State state = country.States
                                        .FirstOrDefault(s => s.Name == stateResponse.Name);
                                    
                                    if (state == null) {
                                        state = new() { 
                                            Name = stateResponse.Name, 
                                            Cities = new List<City>() 
                                        };
                                        
                                        Response responseCities = await _apiService
                                            .GetListAsync<CityResponse>("/v1", $"/countries/{countryResponse.Iso2}/states/{stateResponse.Iso2}/cities");
                                        
                                        if (responseCities.IsSuccess) {
                                            List<CityResponse> cities = (List<CityResponse>)responseCities.Result;
                                            
                                            foreach (CityResponse cityResponse in cities) {
                                                if (cityResponse.Name == "Mosfellsbær" || cityResponse.Name == "Șăulița") {
                                                    continue;
                                                }

                                                City city = state.Cities
                                                    .FirstOrDefault(c => c.Name == cityResponse.Name);
                                                
                                                if (city == null) {
                                                    state.Cities
                                                        .Add(new City() { Name = cityResponse.Name });
                                                }
                                            }
                                        }

                                        if (state.CitiesNumber > 0) {
                                            country.States
                                                .Add(state);
                                        }
                                    }
                                }
                            }

                            if (country.CitiesNumber > 0) {
                                _context.Countries
                                    .Add(country);

                                await _context.SaveChangesAsync();
                            }
                        }
                    }
                }
            }
        }

        private async Task CheckRolesAsync() {
            await _userHelper.CheckRoleAsync(UserType.Admin.ToString());
            await _userHelper.CheckRoleAsync(UserType.User.ToString());
        }

        private async Task CheckUsersAsync() {
            await AddUserAsyn("1010", "Cesar", "Villalobos Olmos", "cesar@yopmail.com", "3461099207", "Av. Cananal #9", "Cesar.jpg", UserType.Admin);
            await AddUserAsyn("2020", "Lizeth", "Sandoval Vallejo", "liz@yopmail.com", "3461005286", "Calle de la Virgen #47", "Licha.jpg", UserType.Admin);
            await AddUserAsyn("1010", "Lucy", "Macias Martinez", "lucy@yopmail.com", "4952361232", "Calle Negrete #467", "Lucy.jpg", UserType.User);
            await AddUserAsyn("2020", "Joss", "Martinez Acosta", "joss@yopmail.com", "4952365217", "Av. Boulevar #4756", "Joss.jpg", UserType.User);
        }

        private async Task<User> AddUserAsyn(
            string document,
            string firstName,
            string lastName,
            string email,
            string phone,
            string address,
            string image,
            UserType userType
        ) {
            User user = await _userHelper.GetUserAsync(email);

            if (user is null) {
                Guid imageId = await _blobHelper.UploadBlobAsync($"{Environment.CurrentDirectory}\\wwwroot\\images\\users\\{image}", "users");

                user = new User {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    UserName = email,
                    PhoneNumber = phone,
                    Address = address,
                    Document = document,
                    City = _context.Cities.FirstOrDefault(),
                    UserType = userType,
                    ImageId = imageId
                };

                await _userHelper.AddUserAsync(user, "abc-123-ABC");
                await _userHelper.AddUserToRoleAsync(user, userType.ToString());

                string token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                await _userHelper.ConfirmEmailAsync(user, token);
            }

            return user;
        }

        private async Task CheckProductsAsync() {
            if (!_context.Products.Any()) {
                await AddProductAsync("Adidas Barracuda", 2700M, 12F, new List<string>() { "Calzado", "Deportes" }, new List<string>() { "adidas_barracuda.png" });
                await AddProductAsync("Adidas Superstar", 2500M, 12F, new List<string>() { "Calzado", "Deportes" }, new List<string>() { "Adidas_superstar.png" });
                await AddProductAsync("AirPods", 13000M, 12F, new List<string>() { "Tecnología", "Apple" }, new List<string>() { "airpos.png", "airpos2.png" });
                await AddProductAsync("Audifonos Bose", 8700M, 12F, new List<string>() { "Tecnología" }, new List<string>() { "audifonos_bose.png" });
                await AddProductAsync("Bicicleta Ribble", 120000M, 6F, new List<string>() { "Deportes" }, new List<string>() { "bicicleta_ribble.png" });
                await AddProductAsync("Camisa Cuadros", 560M, 24F, new List<string>() { "Ropa" }, new List<string>() { "camisa_cuadros.png" });
                await AddProductAsync("Casco Bicicleta", 8200M, 12F, new List<string>() { "Deportes" }, new List<string>() { "casco_bicicleta.png", "casco.png" });
                await AddProductAsync("iPad", 23000M, 6F, new List<string>() { "Tecnología", "Apple" }, new List<string>() { "ipad.png" });
                await AddProductAsync("iPhone 13", 52000M, 6F, new List<string>() { "Tecnología", "Apple" }, new List<string>() { "iphone13.png", "iphone13b.png", "iphone13c.png", "iphone13d.png" });
                await AddProductAsync("Mac Book Pro", 121000M, 6F, new List<string>() { "Tecnología", "Apple" }, new List<string>() { "mac_book_pro.png" });
                await AddProductAsync("Mancuernas", 3700M, 12F, new List<string>() { "Deportes" }, new List<string>() { "mancuernas.png" });
                await AddProductAsync("Mascarilla Cara", 260M, 100F, new List<string>() { "Belleza" }, new List<string>() { "mascarilla_cara.png" });
                await AddProductAsync("New Balance 530", 1800M, 12F, new List<string>() { "Calzado", "Deportes" }, new List<string>() { "newbalance530.png" });
                await AddProductAsync("New Balance 565", 1790M, 12F, new List<string>() { "Calzado", "Deportes" }, new List<string>() { "newbalance565.png" });
                await AddProductAsync("Nike Air", 2330M, 12F, new List<string>() { "Calzado", "Deportes" }, new List<string>() { "nike_air.png" });
                await AddProductAsync("Nike Zoom", 2499M, 12F, new List<string>() { "Calzado", "Deportes" }, new List<string>() { "nike_zoom.png" });
                await AddProductAsync("Buso Adidas Mujer", 1340M, 12F, new List<string>() { "Ropa", "Deportes" }, new List<string>() { "buso_adidas.png" });
                await AddProductAsync("Suplemento Boots Original", 156M, 12F, new List<string>() { "Nutrición" }, new List<string>() { "Boost_Original.png" });
                await AddProductAsync("Whey Protein", 2520M, 12F, new List<string>() { "Nutrición" }, new List<string>() { "whey_protein.png" });
                await AddProductAsync("Arnes Mascota", 250M, 12F, new List<string>() { "Mascotas" }, new List<string>() { "arnes_mascota.png" });
                await AddProductAsync("Cama Mascota", 990M, 12F, new List<string>() { "Mascotas" }, new List<string>() { "cama_mascota.png" });
                await AddProductAsync("Teclado Gamer", 670M, 12F, new List<string>() { "Gamer", "Tecnología" }, new List<string>() { "teclado_gamer.png" });
                await AddProductAsync("Silla Gamer", 9800M, 12F, new List<string>() { "Gamer", "Tecnología" }, new List<string>() { "silla_gamer.png" });
                await AddProductAsync("Mouse Gamer", 1320M, 12F, new List<string>() { "Gamer", "Tecnología" }, new List<string>() { "mouse_gamer.png" });
                await _context.SaveChangesAsync();
            }
        }

        private async Task AddProductAsync(string name, decimal price, float stock, List<string> categories, List<string> images) {
            var product = new Product {
                Description = name,
                Name = name,
                Price = price,
                Stock = stock,
                ProductCategories = new List<ProductCategory>(),
                ProductImages = new List<ProductImage>()
            };

            foreach (string category in categories) {
                product.ProductCategories
                    .Add(new ProductCategory { 
                        Category = await _context.Categories
                        .FirstOrDefaultAsync(c => c.Name == category) 
                    });
            }

            foreach (string image in images) {
                Guid imageId = await _blobHelper.UploadBlobAsync($"{Environment.CurrentDirectory}\\wwwroot\\images\\products\\{image}", "products");
                product.ProductImages
                    .Add(new ProductImage { 
                        ImageId = imageId 
                    });
            }

            _context.Products.Add(product);
        }
    }
}