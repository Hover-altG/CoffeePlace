using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeePlace.Data;
using CoffeePlace.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace CoffeePlace.Controllers
{
    public class ProdController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ProdController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        private bool IsAuthorized() => TempData["UserRol"] != null;

        // ==========================================
        // VISTA ADMINISTRADOR (HU-03)
        // ==========================================
        public async Task<IActionResult> Index()
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Auth");
            TempData.Keep();

            var productos = await _db.Productos.Include(p => p.Categoria).ToListAsync();
            ViewBag.Categorias = new SelectList(_db.Categorias.Where(c => c.estado), "id_categoria", "nombre");
            
            return View(productos);
        }

        // ==========================================
        // CREAR PRODUCTO CON IMAGEN (HU-02)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Prod producto, IFormFile? foto)
        {
            if (ModelState.IsValid)
            {
                if (foto != null && foto.Length > 0)
                {
                    string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(foto.FileName);
                    string rutaCarpeta = Path.Combine(_env.WebRootPath, "img", "productos");
                    
                    if (!Directory.Exists(rutaCarpeta)) Directory.CreateDirectory(rutaCarpeta);

                    string rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);
                    using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                    {
                        await foto.CopyToAsync(stream);
                    }

                    producto.imagen = nombreArchivo;
                }

                _db.Productos.Add(producto);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Producto creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Error al crear el producto. Verifique los campos obligatorios.";
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // VISTA CAJERO (HU-04)
        // ==========================================
        public async Task<IActionResult> CajeroIndex()
        {
            
            if (!IsAuthorized()) return RedirectToAction("Login", "Auth");
            TempData.Keep();

            var productosActivos = await _db.Productos
                .Include(p => p.Categoria)
                .Where(p => p.estado == true)
                .ToListAsync();
            return View(productosActivos);
        }

        // ==========================================
        // ACTUALIZACIÓN DE PRECIOS (HU-01)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdPrecio(int id, decimal precio_venta)
        {
            if (precio_venta <= 0)
            {
                TempData["Error"] = "El precio debe ser mayor a cero.";
                return RedirectToAction(nameof(Index));
            }

            var producto = await _db.Productos.FindAsync(id);
            if (producto != null)
            {
                producto.precio_venta = precio_venta;
                await _db.SaveChangesAsync();
                TempData["Success"] = "Precio actualizado correctamente.";
            }
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // REPORTE DE VENTAS E INVENTARIO (HU-05)
        // ==========================================
        public async Task<IActionResult> Reportes()
        {
            // CORRECCIÓN: Se cambió "AuthCtrl" por "Auth"
            if (!IsAuthorized()) return RedirectToAction("Login", "Auth");
            TempData.Keep();

            var productos = await _db.Productos.Include(p => p.Categoria).ToListAsync();
            
            ViewBag.TotalProductos = productos.Count;
            ViewBag.Activos = productos.Count(p => p.estado);
            ViewBag.TotalCategorias = await _db.Categorias.CountAsync();
            ViewBag.VentasDia = 1500.50; 

            return View(productos);
        }

        [HttpPost]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var producto = await _db.Productos.FindAsync(id);
            if (producto != null)
            {
                producto.estado = !producto.estado;
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var producto = await _db.Productos.FindAsync(id);
            if (producto != null)
            {
                if (!string.IsNullOrEmpty(producto.imagen))
                {
                    string rutaImagen = Path.Combine(_env.WebRootPath, "img", "productos", producto.imagen);
                    if (System.IO.File.Exists(rutaImagen)) System.IO.File.Delete(rutaImagen);
                }

                _db.Productos.Remove(producto);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Producto eliminado.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id_producto, string nombre, decimal precio_venta, string descripcion, IFormFile? foto, int id_categoria)
        {
            var producto = await _db.Productos.FindAsync(id_producto);
            if (producto == null) return NotFound();

            if (ModelState.IsValid)
            {
                producto.nombre = nombre;
                producto.precio_venta = precio_venta;
                producto.descripcion = descripcion;
                producto.id_categoria = id_categoria;

                if (foto != null && foto.Length > 0)
                {
                    string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(foto.FileName);
                    string rutaCarpeta = Path.Combine(_env.WebRootPath, "img", "productos");
                    string rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);

                    using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                    {
                        await foto.CopyToAsync(stream);
                    }
                    producto.imagen = nombreArchivo;
                }

                _db.Update(producto);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Producto actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }
        

        
        

    }
}