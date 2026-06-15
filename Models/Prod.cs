using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeePlace.Models
{
    [Table("producto")]
    public class Prod
    {
        [Key]
        public int id_producto { get; set; }
        public int id_categoria { get; set; }
        [Required] public string nombre { get; set; } = string.Empty;
        public string? descripcion { get; set; }
        [Required] [Column(TypeName = "decimal(10, 2)")] public decimal precio_venta { get; set; }
        public string? imagen { get; set; }
        public DateTime fecha_creacion { get; set; } = DateTime.Now;
        public bool estado { get; set; } = true;

        [ForeignKey("id_categoria")]
        public virtual Cat? Categoria { get; set; }
    }
}