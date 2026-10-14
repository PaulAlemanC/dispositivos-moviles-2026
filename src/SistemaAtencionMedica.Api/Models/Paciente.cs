namespace SistemaAtencionMedica.Api.Models;

public class Paciente
{
    public int IdPaciente { get; set; }
    public required string Matricula { get; set; }
    public required string Nombres { get; set; }
    public required string ApellidoPaterno { get; set; }
    public string? ApellidoMaterno { get; set; }
    public DateOnly FechaNacimiento { get; set; }
    public string? Sexo { get; set; }
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public string? Carrera { get; set; }
    public int? Semestre { get; set; }
    public string? TipoSangre { get; set; }
    public string? Alergias { get; set; }
    public string? ContactoEmergencia { get; set; }
    public string? TelefonoEmergencia { get; set; }
}
