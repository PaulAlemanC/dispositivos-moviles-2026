using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SistemaAtencionMedica.Api.DTOs;
using SistemaAtencionMedica.Api.Services;

namespace SistemaAtencionMedica.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/pacientes")]
public class PacientesController(IPacienteService service) : ControllerBase
{
    /// <summary>Obtiene pacientes y permite filtrar por matrícula y/o nombre.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PacienteResponseDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PacienteResponseDto>>> GetAll(
        [FromQuery] string? matricula,
        [FromQuery] string? nombre,
        CancellationToken cancellationToken) =>
        Ok(await service.GetAllAsync(matricula, nombre, cancellationToken));

    /// <summary>Obtiene un paciente por su identificador.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<PacienteResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PacienteResponseDto>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await service.GetByIdAsync(id, cancellationToken));

    /// <summary>Registra un paciente.</summary>
    [HttpPost]
    [ProducesResponseType<PacienteResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PacienteResponseDto>> Create(
        [FromBody] PacienteRequestDto request,
        CancellationToken cancellationToken)
    {
        var created = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.IdPaciente }, created);
    }

    /// <summary>Actualiza todos los datos editables de un paciente.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType<PacienteResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PacienteResponseDto>> Update(
        int id,
        [FromBody] PacienteRequestDto request,
        CancellationToken cancellationToken) =>
        Ok(await service.UpdateAsync(id, request, cancellationToken));
}
