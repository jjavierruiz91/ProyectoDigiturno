using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GobernacionTurnos.Models
{
    public class Ventanilla
    {
        [Key]
        public int Id { get; set; }
        public int Numero { get; set; }
        public int DependenciaId { get; set; }
        public string Estado { get; set; } = "Disponible";
        public int? UsuarioId { get; set; }
        
        [ForeignKey("DependenciaId")]
        public virtual Dependencia? Dependencia { get; set; }
        
        [ForeignKey("UsuarioId")]
        public virtual Usuario? Usuario { get; set; }
    }
}