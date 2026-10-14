using System.ComponentModel.DataAnnotations;

namespace SistemaAtencionMedica.Api.DTOs;

public class MedicoCreateDto
{
    [Required, StringLength(30)]
    public string NumeroEmpleado { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Nombres { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string ApellidoPaterno { get; set; } = string.Empty;

    [StringLength(80)]
    public string? ApellidoMaterno { get; set; }

    [StringLength(120)]
    public string? Especialidad { get; set; }

    [StringLength(30)]
    public string? CedulaProfesional { get; set; }

    [StringLength(20)]
    public string? Telefono { get; set; }

    [EmailAddress, StringLength(254)]
    public string? Correo { get; set; }
}

public record MedicoResponseDto(
    int IdMedico,
    string NumeroEmpleado,
    string Nombres,
    string ApellidoPaterno,
    string? ApellidoMaterno,
    string? Especialidad,
    string? CedulaProfesional,
    string? Telefono,
    string? Correo);
