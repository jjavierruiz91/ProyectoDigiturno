using System.ComponentModel.DataAnnotations;

namespace GobernacionTurnos.Models
{
    public class Dependencia
    {
        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; } = true;
    }
}