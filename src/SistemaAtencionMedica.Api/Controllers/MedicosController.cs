using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SistemaAtencionMedica.Api.DTOs;
using SistemaAtencionMedica.Api.Services;

namespace SistemaAtencionMedica.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/medicos")]
public class MedicosController(IMedicoService service) : ControllerBase
{
    /// <summary>Obtiene todos los médicos.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<MedicoResponseDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MedicoResponseDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await service.GetAllAsync(cancellationToken));

    /// <summary>Obtiene un médico por su identificador.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<MedicoResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MedicoResponseDto>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await service.GetByIdAsync(id, cancellationToken));

    /// <summary>Registra un médico.</summary>
    [HttpPost]
    [ProducesResponseType<MedicoResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MedicoResponseDto>> Create(
        [FromBody] MedicoCreateDto request,
        CancellationToken cancellationToken)
    {
        var created = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.IdMedico }, created);
    }
}
