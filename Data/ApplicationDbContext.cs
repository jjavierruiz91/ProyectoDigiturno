using Microsoft.EntityFrameworkCore;
using GobernacionTurnos.Models;

namespace GobernacionTurnos.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets para cada tabla
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Dependencia> Dependencias { get; set; }
        public DbSet<TipoTramite> TiposTramite { get; set; }
        public DbSet<Ventanilla> Ventanillas { get; set; }
        public DbSet<Turno> Turnos { get; set; }
        public DbSet<Atencion> Atenciones { get; set; }
        public DbSet<Configuracion> Configuraciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar nombres de tablas
            modelBuilder.Entity<Usuario>().ToTable("Usuarios");
            modelBuilder.Entity<Dependencia>().ToTable("Dependencias");
            modelBuilder.Entity<TipoTramite>().ToTable("TiposTramite");
            modelBuilder.Entity<Ventanilla>().ToTable("Ventanillas");
            modelBuilder.Entity<Turno>().ToTable("Turnos");
            modelBuilder.Entity<Atencion>().ToTable("Atenciones");
            modelBuilder.Entity<Configuracion>().ToTable("Configuracion");

            // Configurar índice único para NumeroTicket
            modelBuilder.Entity<Turno>()
                .HasIndex(t => t.NumeroTicket)
                .IsUnique();

            // Configurar índice único para Email
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}