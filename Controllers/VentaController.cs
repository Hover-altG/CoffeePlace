using Microsoft.AspNetCore.Mvc;
using CoffeePlace.Data;
using CoffeePlace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace CoffeePlace.Controllers
{
    public class VentaController : Controller
    {
        private readonly AppDbContext _context;

        public VentaController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Vista principal: Carga los productos activos para el menú
        public async Task<IActionResult> NuevaVenta()
        {
            // Filtramos solo productos con estado activo (true)
            var productos = await _context.Productos.Where(p => p.estado == true).ToListAsync();
            return View(productos);
        }

        // 2. Acción para guardar el pedido completo mediante AJAX/JSON
        [HttpPost]
        public async Task<IActionResult> ProcesarVenta([FromBody] VentaRequest data)
        {
            // SEGURIDAD: Obtenemos el ID del usuario directamente de la sesión del servidor
            int? userIdSesion = HttpContext.Session.GetInt32("UsuarioId");

            if (userIdSesion == null)
            {
                return Json(new { success = false, message = "Su sesión ha expirado. Por favor, vuelva a iniciar sesión." });
            }

            if (data.carrito == null || !data.carrito.Any())
            {
                return Json(new { success = false, message = "El carrito está vacío." });
            }

            // --- CORRECCIÓN PARA REINTENTOS DE MYSQL ---
            // Definimos la estrategia de ejecución para permitir transacciones con reintentos
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                // Iniciamos transacción para asegurar la integridad de la cabecera y el detalle
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // 1. Creamos la cabecera del pedido
                        var nuevoPedido = new Pedido
                        {
                            id_usuario = userIdSesion.Value, 
                            fecha_hora = DateTime.Now,
                            total = data.carrito.Sum(d => d.subtotal),
                            estado = "Completado"
                        };

                        _context.Pedidos.Add(nuevoPedido);
                        await _context.SaveChangesAsync(); // Genera el id_pedido

                        // 2. Registramos los detalles del pedido
                        foreach (var item in data.carrito)
                        {
                            var detalle = new DetallePedido
                            {
                                id_pedido = nuevoPedido.id_pedido,
                                id_producto = item.id_producto,
                                cantidad = item.cantidad,
                                precio_unitario = item.precio_unitario,
                                subtotal = item.subtotal
                            };
                            
                            _context.DetallesPedidos.Add(detalle);
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return Json(new { success = true, message = "Venta registrada con éxito" });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        string errorMsg = ex.InnerException?.Message ?? ex.Message;
                        return Json(new { success = false, message = "Error en base de datos: " + errorMsg });
                    }
                }
            });
        }
    }

    // Clase para mapear el JSON enviado desde la vista
    public class VentaRequest
    {
        public int id_usuario { get; set; }
        public List<DetallePedido> carrito { get; set; } = new List<DetallePedido>();
    }
}