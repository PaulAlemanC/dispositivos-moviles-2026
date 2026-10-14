namespace SistemaAtencionMedica.Api.Models;

public class Medico
{
    public int IdMedico { get; set; }
    public required string NumeroEmpleado { get; set; }
    public required string Nombres { get; set; }
    public required string ApellidoPaterno { get; set; }
    public string? ApellidoMaterno { get; set; }
    public string? Especialidad { get; set; }
    public string? CedulaProfesional { get; set; }
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
}
