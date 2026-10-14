using Microsoft.EntityFrameworkCore;
using SistemaAtencionMedica.Api.DTOs;
using SistemaAtencionMedica.Api.Models;
using SistemaAtencionMedica.Api.Repositories;

namespace SistemaAtencionMedica.Api.Services;

public class MedicoService(IMedicoRepository repository) : IMedicoService
{
    public async Task<IReadOnlyList<MedicoResponseDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var medicos = await repository.GetAllAsync(cancellationToken);
        return medicos.Select(ToResponse).ToList();
    }

    public async Task<MedicoResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var medico = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"No se encontró el médico con id {id}.");
        return ToResponse(medico);
    }

    public async Task<MedicoResponseDto> CreateAsync(MedicoCreateDto request, CancellationToken cancellationToken)
    {
        var numeroEmpleado = Required(request.NumeroEmpleado);
        var cedula = Optional(request.CedulaProfesional);

        if (await repository.ExistsByNumeroEmpleadoAsync(numeroEmpleado, cancellationToken))
            throw new ConflictException($"Ya existe un médico con el número de empleado '{numeroEmpleado}'.");
        if (cedula is not null && await repository.ExistsByCedulaAsync(cedula, cancellationToken))
            throw new ConflictException($"Ya existe un médico con la cédula profesional '{cedula}'.");

        var medico = new Medico
        {
            NumeroEmpleado = numeroEmpleado,
            Nombres = Required(request.Nombres),
            ApellidoPaterno = Required(request.ApellidoPaterno),
            ApellidoMaterno = Optional(request.ApellidoMaterno),
            Especialidad = Optional(request.Especialidad),
            CedulaProfesional = cedula,
            Telefono = Optional(request.Telefono),
            Correo = Optional(request.Correo)?.ToLowerInvariant()
        };

        await repository.AddAsync(medico, cancellationToken);
        try
        {
            await repository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new ConflictException("No fue posible guardar el médico porque un dato único ya está registrado.");
        }

        return ToResponse(medico);
    }

    private static string? Optional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string Required(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new BusinessValidationException("Los campos obligatorios no pueden estar vacíos.");
        return value.Trim();
    }

    private static MedicoResponseDto ToResponse(Medico m) => new(
        m.IdMedico, m.NumeroEmpleado, m.Nombres, m.ApellidoPaterno, m.ApellidoMaterno,
        m.Especialidad, m.CedulaProfesional, m.Telefono, m.Correo);
}
