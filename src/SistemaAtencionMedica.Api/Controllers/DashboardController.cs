using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaAtencionMedica.Api.DTOs;
using SistemaAtencionMedica.Api.Services;

namespace SistemaAtencionMedica.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    /// <summary>Obtiene el resumen de métricas, próximas citas y alertas del dashboard.</summary>
    [HttpGet("summary")]
    [ProducesResponseType<DashboardSummaryResponseDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardSummaryResponseDto>> GetSummary(CancellationToken cancellationToken) =>
        Ok(await dashboardService.GetSummaryAsync(cancellationToken));
}
