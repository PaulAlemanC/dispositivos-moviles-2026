using System.ComponentModel.DataAnnotations;

namespace SistemaAtencionMedica.Api.DTOs;

public class RegisterRequestDto
{
    [Required, StringLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Correo { get; set; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
}

public class LoginRequestDto
{
    [Required, EmailAddress, StringLength(254)]
    public string Correo { get; set; } = string.Empty;

    [Required, StringLength(128)]
    public string Password { get; set; } = string.Empty;
}

public record UsuarioResponseDto(int IdUsuario, string Nombre, string Correo, string Rol);
public record LoginResponseDto(string Token, string TokenType, int ExpiresIn, UsuarioResponseDto Usuario);
