using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeePlace.Models
{
    [Table("detalle_pedido")]
    public class DetallePedido
    {
        [Key]
        [Column("id_detalle")]
        public int id_detalle { get; set; }

        [Column("id_pedido")]
        public int id_pedido { get; set; }

        [Column("id_producto")]
        public int id_producto { get; set; }

        [Column("cantidad")]
        public int cantidad { get; set; }

        [Column("precio_unitario")]
        public decimal precio_unitario { get; set; }

        [Column("subtotal")]
        public decimal subtotal { get; set; }

        // Propiedades de navegación (Entity Framework las llena automáticamente)
        [ForeignKey("id_pedido")]
        public virtual Pedido Pedido { get; set; } = null!;

        [ForeignKey("id_producto")]
        public virtual Prod Producto { get; set; } = null!;
    }
}