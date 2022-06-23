/*Para agregar una migracion usamos el comando: 
    add-migration nombre
Una vez que queremos que las migraciones se reflejen en la 
base de datos usamos el comando: 
    update-database*/

using Microsoft.EntityFrameworkCore;
using ShoppingCar.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Agregamos el db context y la cadena de conexion
builder.Services.AddDbContext<DataContext>(options => {
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();