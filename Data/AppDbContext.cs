using Microsoft.EntityFrameworkCore;
using CoffeePlace.Models;

namespace CoffeePlace.Data
{
    //
    public class AppDbContext : DbContext
    {
        // 
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Usuarios { get; set; }
        public DbSet<Cat> Categorias { get; set; }
        public DbSet<Prod> Productos { get; set; }

        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<DetallePedido> DetallesPedidos { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Pedido>().ToTable("pedido");
            modelBuilder.Entity<DetallePedido>().ToTable("detalle_pedido");
            modelBuilder.Entity<Prod>().ToTable("producto");
            modelBuilder.Entity<User>().ToTable("usuario");
            modelBuilder.Entity<Cat>().ToTable("categoria");
        }
    }
}