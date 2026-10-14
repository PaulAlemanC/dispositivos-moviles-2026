using SistemaAtencionMedica.Api.DTOs;

namespace SistemaAtencionMedica.Api.Services;

public interface IPacienteService
{
    Task<IReadOnlyList<PacienteResponseDto>> GetAllAsync(string? matricula, string? nombre, CancellationToken cancellationToken);
    Task<PacienteResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<PacienteResponseDto> CreateAsync(PacienteRequestDto request, CancellationToken cancellationToken);
    Task<PacienteResponseDto> UpdateAsync(int id, PacienteRequestDto request, CancellationToken cancellationToken);
}
