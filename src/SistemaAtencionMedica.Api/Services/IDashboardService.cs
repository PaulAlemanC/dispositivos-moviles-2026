using SistemaAtencionMedica.Api.DTOs;

namespace SistemaAtencionMedica.Api.Services;

public interface IDashboardService
{
    Task<DashboardSummaryResponseDto> GetSummaryAsync(CancellationToken cancellationToken);
}
