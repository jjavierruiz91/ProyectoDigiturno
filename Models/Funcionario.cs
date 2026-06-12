using System.ComponentModel.DataAnnotations;

namespace GobernacionTurnos.Models
{
    public class Funcionario
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Usuario { get; set; } = string.Empty;
        
        [Required]
        public string NombreCompleto { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        public string Rol { get; set; } = "Operador";
        
        public int? VentanillaId { get; set; }
        
        public bool Activo { get; set; } = true;
        
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}