using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GobernacionTurnos.Models
{
    public class Turno
    {
        [Key]
        public int Id { get; set; }
        public string NumeroTicket { get; set; } = string.Empty;
        public int TipoTramiteId { get; set; }
        public string Estado { get; set; } = "Espera";
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaLlamado { get; set; }
        public DateTime? FechaInicioAtencion { get; set; }
        public DateTime? FechaFinAtencion { get; set; }
        public int Prioridad { get; set; } = 0;
        public string? IdentificacionCiudadano { get; set; }
        public string? NombreCiudadano { get; set; }
        
        [ForeignKey("TipoTramiteId")]
        public virtual TipoTramite? TipoTramite { get; set; }
    }
}