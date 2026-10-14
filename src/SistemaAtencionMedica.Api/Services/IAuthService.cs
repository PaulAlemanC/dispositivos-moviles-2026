using SistemaAtencionMedica.Api.DTOs;

namespace SistemaAtencionMedica.Api.Services;

public interface IAuthService
{
    Task<UsuarioResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken);
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken);
}
