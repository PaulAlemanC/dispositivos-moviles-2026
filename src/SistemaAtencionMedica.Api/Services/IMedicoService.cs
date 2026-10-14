using SistemaAtencionMedica.Api.DTOs;

namespace SistemaAtencionMedica.Api.Services;

public interface IMedicoService
{
    Task<IReadOnlyList<MedicoResponseDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<MedicoResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<MedicoResponseDto> CreateAsync(MedicoCreateDto request, CancellationToken cancellationToken);
}
