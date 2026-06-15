using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeePlace.Models
{
    [Table("categoria")]
    public class Cat
    {
        [Key]
        public int id_categoria { get; set; }
        public int? id_usuario { get; set; }
        [Required] public string nombre { get; set; } = string.Empty;
        public string? descripcion { get; set; }
        public DateTime fecha_creacion { get; set; } = DateTime.Now;
        public bool estado { get; set; } = true;
        
        [ForeignKey("id_usuario")]
        public virtual User? Usuario { get; set; }
    }
}