using System.ComponentModel.DataAnnotations;

namespace SistemaAtencionMedica.Api.DTOs;

public class PacienteRequestDto : IValidatableObject
{
    [Required, StringLength(30)]
    public string Matricula { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Nombres { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string ApellidoPaterno { get; set; } = string.Empty;

    [StringLength(80)]
    public string? ApellidoMaterno { get; set; }

    public DateOnly FechaNacimiento { get; set; }

    [StringLength(30)]
    public string? Sexo { get; set; }

    [StringLength(20)]
    public string? Telefono { get; set; }

    [EmailAddress, StringLength(254)]
    public string? Correo { get; set; }

    [StringLength(150)]
    public string? Carrera { get; set; }

    [Range(1, 15)]
    public int? Semestre { get; set; }

    [StringLength(10)]
    public string? TipoSangre { get; set; }

    [StringLength(500)]
    public string? Alergias { get; set; }

    [StringLength(150)]
    public string? ContactoEmergencia { get; set; }

    [StringLength(20)]
    public string? TelefonoEmergencia { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FechaNacimiento == default)
            yield return new ValidationResult("La fecha de nacimiento es obligatoria.", [nameof(FechaNacimiento)]);
        else if (FechaNacimiento > DateOnly.FromDateTime(DateTime.Today))
            yield return new ValidationResult("La fecha de nacimiento no puede ser futura.", [nameof(FechaNacimiento)]);
    }
}

public record PacienteResponseDto(
    int IdPaciente,
    string Matricula,
    string Nombres,
    string ApellidoPaterno,
    string? ApellidoMaterno,
    DateOnly FechaNacimiento,
    string? Sexo,
    string? Telefono,
    string? Correo,
    string? Carrera,
    int? Semestre,
    string? TipoSangre,
    string? Alergias,
    string? ContactoEmergencia,
    string? TelefonoEmergencia);
