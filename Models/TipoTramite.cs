using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GobernacionTurnos.Models
{
    public class TipoTramite
    {
        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int DuracionEstimada { get; set; } = 15;
        public int DependenciaId { get; set; }
        public bool Activo { get; set; } = true;
        
        [ForeignKey("DependenciaId")]
        public virtual Dependencia? Dependencia { get; set; }
    }
}