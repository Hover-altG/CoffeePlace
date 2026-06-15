using Microsoft.AspNetCore.Mvc;
using CoffeePlace.Data;
using CoffeePlace.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http; // Necesario para Session

namespace CoffeePlace.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _db;

        public AuthController(AppDbContext db)
        {
            _db = db;
        }

        // ==========================================
        // LOGIN Y ACCESO (HU-00)
        // ==========================================

        public IActionResult Login()
        {
            TempData.Clear();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string user, string pass)
        {
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                ViewBag.Error = "Por favor, complete todos los campos.";
                return View();
            }

            // Buscamos al usuario en la base de datos
            var usuarioValido = _db.Usuarios.FirstOrDefault(u => u.nombre_usuario == user && u.contraseña == pass);

            if (usuarioValido != null)
            {
                if (!usuarioValido.estado)
                {
                    ViewBag.Error = "Este usuario se encuentra desactivado.";
                    return View();
                }

                // --- MEJORA SPRINT 2: GESTIÓN DE SESIÓN ---
                // Guardamos el ID real de la DB en la sesión del servidor
                HttpContext.Session.SetInt32("UsuarioId", usuarioValido.id_usuario);
                
                // TempData para la interfaz de usuario (Layout)
                TempData["UserRol"] = usuarioValido.rol.ToUpper();
                TempData["UserName"] = usuarioValido.nombre_usuario;

                if (usuarioValido.rol.ToLower() == "administrador")
                {
                    return RedirectToAction("Index", "Prod");
                }
                else if (usuarioValido.rol.ToLower() == "cajero")
                {
                    // Redirige al Menú Digital que estamos trabajando
                    return RedirectToAction("CajeroIndex", "Prod");
                }
            }

            ViewBag.Error = "Usuario o contraseña incorrectos. Intente de nuevo.";
            return View();
        }

        public IActionResult Logout()
        {
            TempData.Clear();
            HttpContext.Session.Clear(); // Limpia el ID del usuario al salir
            return RedirectToAction("Login", "Auth");
        }

        // ==========================================
        // GESTIÓN DE USUARIOS (HU-07)
        // ==========================================

        public async Task<IActionResult> Usuarios()
        {
            var usuarios = await _db.Usuarios.ToListAsync();
            return View(usuarios);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(User nuevoUsuario)
        {
            if (ModelState.IsValid)
            {
                var existe = await _db.Usuarios.AnyAsync(u => u.nombre_usuario == nuevoUsuario.nombre_usuario);
                if (existe)
                {
                    TempData["Error"] = "El nombre de usuario ya está en uso.";
                    return RedirectToAction(nameof(Usuarios));
                }

                _db.Usuarios.Add(nuevoUsuario);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Usuario creado correctamente.";
            }
            else
            {
                TempData["Error"] = "Datos inválidos para el nuevo usuario.";
            }
            return RedirectToAction(nameof(Usuarios));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleEstado(int id)
        {
            var user = await _db.Usuarios.FindAsync(id);
            if (user != null)
            {
                user.estado = !user.estado;
                await _db.SaveChangesAsync();
                TempData["Success"] = "Estado actualizado.";
            }
            return RedirectToAction(nameof(Usuarios));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _db.Usuarios.FindAsync(id);
            if (user != null)
            {
                _db.Usuarios.Remove(user);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Usuario eliminado permanentemente.";
            }
            return RedirectToAction(nameof(Usuarios));
        }
    }
}