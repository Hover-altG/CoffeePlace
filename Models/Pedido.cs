using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeePlace.Models
{
    [Table("pedido")]
    public class Pedido
    {
        [Key]
        [Column("id_pedido")]
        public int id_pedido { get; set; }

        [Column("id_usuario")]
        public int id_usuario { get; set; }

        [Column("fecha_hora")]
        public DateTime fecha_hora { get; set; } = DateTime.Now;

        [Column("total")]
        public decimal total { get; set; }

        [Column("estado")]
        public string estado { get; set; } = "Completado";

        // Relación con la tabla usuario
        [ForeignKey("id_usuario")]
        public virtual User Usuario { get; set; } = null!;

        // Relación con los detalles del pedido
        public virtual ICollection<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
    }
}