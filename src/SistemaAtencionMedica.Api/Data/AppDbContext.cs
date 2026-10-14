using Microsoft.EntityFrameworkCore;
using SistemaAtencionMedica.Api.Models;

namespace SistemaAtencionMedica.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<Medico> Medicos => Set<Medico>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuarios");
            entity.HasKey(u => u.IdUsuario);
            entity.Property(u => u.Nombre).HasMaxLength(150).IsRequired();
            entity.Property(u => u.Correo).HasMaxLength(254).IsRequired();
            entity.HasIndex(u => u.Correo).IsUnique();
            entity.Property(u => u.PasswordHash).HasMaxLength(512).IsRequired();
            entity.Property(u => u.Rol).HasMaxLength(30).IsRequired();
        });
        modelBuilder.Entity<Paciente>(entity =>
        {
            entity.ToTable("Pacientes", table =>
            {
                table.HasCheckConstraint("CK_Pacientes_Semestre", "[Semestre] IS NULL OR ([Semestre] BETWEEN 1 AND 15)");
                table.HasCheckConstraint("CK_Pacientes_FechaNacimiento", "[FechaNacimiento] <= CONVERT(date, GETDATE())");
            });
            entity.HasKey(p => p.IdPaciente);
            entity.HasIndex(p => p.Matricula).IsUnique();
            entity.Property(p => p.Matricula).HasMaxLength(30).IsRequired();
            entity.Property(p => p.Nombres).HasMaxLength(100).IsRequired();
            entity.Property(p => p.ApellidoPaterno).HasMaxLength(80).IsRequired();
            entity.Property(p => p.ApellidoMaterno).HasMaxLength(80);
            entity.Property(p => p.Sexo).HasMaxLength(30);
            entity.Property(p => p.Telefono).HasMaxLength(20);
            entity.Property(p => p.Correo).HasMaxLength(254);
            entity.Property(p => p.Carrera).HasMaxLength(150);
            entity.Property(p => p.TipoSangre).HasMaxLength(10);
            entity.Property(p => p.Alergias).HasMaxLength(500);
            entity.Property(p => p.ContactoEmergencia).HasMaxLength(150);
            entity.Property(p => p.TelefonoEmergencia).HasMaxLength(20);
        });

        modelBuilder.Entity<Medico>(entity =>
        {
            entity.ToTable("Medicos");
            entity.HasKey(m => m.IdMedico);
            entity.HasIndex(m => m.NumeroEmpleado).IsUnique();
            entity.HasIndex(m => m.CedulaProfesional)
                .IsUnique()
                .HasFilter("[CedulaProfesional] IS NOT NULL AND [CedulaProfesional] <> ''");
            entity.Property(m => m.NumeroEmpleado).HasMaxLength(30).IsRequired();
            entity.Property(m => m.Nombres).HasMaxLength(100).IsRequired();
            entity.Property(m => m.ApellidoPaterno).HasMaxLength(80).IsRequired();
            entity.Property(m => m.ApellidoMaterno).HasMaxLength(80);
            entity.Property(m => m.Especialidad).HasMaxLength(120);
            entity.Property(m => m.CedulaProfesional).HasMaxLength(30);
            entity.Property(m => m.Telefono).HasMaxLength(20);
            entity.Property(m => m.Correo).HasMaxLength(254);
        });
    }
}
