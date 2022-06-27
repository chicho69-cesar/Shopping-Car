/*Para agregar una migracion usamos el comando: 
    add-migration nombre
Una vez que queremos que las migraciones se reflejen en la 
base de datos usamos el comando: 
    update-database
Si tuvieramos algun error en la migracion, debemos de borrar 
la base de datos con el comando:
    drop-database
y volvemos a ejecura el update. Cuando haya algun conflicto con alguna
migracion lo que debemos hacer es borrar todas las migraciones con
    remove-migration
y posteriormente crear una nueva migracion para todas las entidades
que borramos, despues hacemos drop-database ya que esta ya no tiene
la historia borramos y finalmente modificamos la base de datos.*/

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShoppingCar.Data;
using ShoppingCar.Data.Entities;
using ShoppingCar.Helpers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Agregamos el db context y la cadena de conexion
builder.Services.AddDbContext<DataContext>(options => {
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

//TODO: Make strongest password in order to go to production
builder.Services.AddIdentity<User, IdentityRole>(config => {
    config.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
    config.SignIn.RequireConfirmedEmail = true;
    config.User.RequireUniqueEmail = true;
    config.Password.RequireDigit = false;
    config.Password.RequiredUniqueChars = 0;
    config.Password.RequireLowercase = false;
    config.Password.RequireNonAlphanumeric = false;
    config.Password.RequireUppercase = false;
    config.Password.RequiredLength = 6;
    config.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    config.Lockout.MaxFailedAccessAttempts = 3;
    config.Lockout.AllowedForNewUsers = true;
})
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<DataContext>();

builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = "/Account/NotAuthorized";
    options.AccessDeniedPath = "/Account/NotAuthorized";
});

builder.Services.AddTransient<SeedDb>();
builder.Services.AddScoped<IUserHelper, UserHelper>();
builder.Services.AddScoped<ICombosHelper, CombosHelper>();
builder.Services.AddScoped<IBlobHelper, BlobHelper>();
builder.Services.AddScoped<IGetLocation, GetLocation>();
builder.Services.AddScoped<IMailHelper, MailHelper>();
builder.Services.AddScoped<IOrdersHelper, OrdersHelper>();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

var app = builder.Build();

SeedData(app);
void SeedData(WebApplication app) {
    IServiceScopeFactory scopedFactory = app.Services.GetService<IServiceScopeFactory>();
    using (IServiceScope scope = scopedFactory.CreateScope()) {
        SeedDb service = scope.ServiceProvider.GetService<SeedDb>();
        service.SeedAsync().Wait();
    }
}

if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/error/{0}");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();