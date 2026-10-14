namespace SistemaAtencionMedica.Api.Models;

public class Usuario
{
    public int IdUsuario { get; set; }
    public required string Nombre { get; set; }
    public required string Correo { get; set; }
    public required string PasswordHash { get; set; }
    public string Rol { get; set; } = "Usuario";
}
