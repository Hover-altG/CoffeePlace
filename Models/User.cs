using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeePlace.Models
{
    [Table("usuario")]
    public class User
    {
        [Key]
        public int id_usuario { get; set; }
        [Required] public string nombre_usuario { get; set; } = string.Empty;
        [Required] public string contraseña { get; set; } = string.Empty;
        [Required] public string rol { get; set; } = string.Empty;
        public DateTime fecha_registro { get; set; } = DateTime.Now;
        public bool estado { get; set; } = true;
    }
}