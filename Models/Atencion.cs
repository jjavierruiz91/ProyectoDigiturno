using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GobernacionTurnos.Models
{
    public class Atencion
    {
        [Key]
        public int Id { get; set; }
        public int TurnoId { get; set; }
        public int VentanillaId { get; set; }
        public int? UsuarioId { get; set; }
        public DateTime FechaInicio { get; set; } = DateTime.Now;
        public DateTime? FechaFin { get; set; }
        public string? Comentarios { get; set; }
        public int? Calificacion { get; set; }
        
        [ForeignKey("TurnoId")]
        public virtual Turno? Turno { get; set; }
        
        [ForeignKey("VentanillaId")]
        public virtual Ventanilla? Ventanilla { get; set; }
        
        [ForeignKey("UsuarioId")]
        public virtual Usuario? Usuario { get; set; }
    }
}