using Microsoft.EntityFrameworkCore;
using CoffeePlace.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar Servicios de MVC
builder.Services.AddControllersWithViews();

// 2. Activar Sesiones (Vital para capturar el ID del cajero en el Sprint 2)
builder.Services.AddDistributedMemoryCache(); // Necesario para que la sesión sea estable
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30); // El cajero puede estar 30 min inactivo
    options.Cookie.HttpOnly = true; // Seguridad: impide acceso por JS
    options.Cookie.IsEssential = true; // Necesario para cumplir normativas de cookies
});

// 3. Conexión MySQL con reintentos automáticos
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(opt => 
    opt.UseMySql(conn, ServerVersion.AutoDetect(conn), sqlOptions => {
        sqlOptions.EnableRetryOnFailure(); // Reintenta si MySQL tarda en responder
    }));

var app = builder.Build();

// 4. Configurar el Pipeline de HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// --- EL ORDEN AQUÍ ES CRÍTICO ---
app.UseSession(); // Primero habilitamos sesiones
app.UseAuthorization(); // Luego la autorización

// 5. Ajuste de Rutas: Inicia siempre en el Login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();