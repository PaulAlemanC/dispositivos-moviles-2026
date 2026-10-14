using Microsoft.EntityFrameworkCore;
using SistemaAtencionMedica.Api.DTOs;
using SistemaAtencionMedica.Api.Models;
using SistemaAtencionMedica.Api.Repositories;

namespace SistemaAtencionMedica.Api.Services;

public class PacienteService(IPacienteRepository repository) : IPacienteService
{
    public async Task<IReadOnlyList<PacienteResponseDto>> GetAllAsync(
        string? matricula,
        string? nombre,
        CancellationToken cancellationToken)
    {
        var pacientes = await repository.GetAllAsync(matricula, nombre, cancellationToken);
        return pacientes.Select(ToResponse).ToList();
    }

    public async Task<PacienteResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var paciente = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"No se encontró el paciente con id {id}.");

        return ToResponse(paciente);
    }

    public async Task<PacienteResponseDto> CreateAsync(PacienteRequestDto request, CancellationToken cancellationToken)
    {
        ValidateBusinessRules(request);
        var matricula = Required(request.Matricula);

        if (await repository.ExistsByMatriculaAsync(matricula, null, cancellationToken))
            throw new ConflictException($"Ya existe un paciente con la matrícula '{matricula}'.");

        var paciente = new Paciente
        {
            Matricula = matricula,
            Nombres = Required(request.Nombres),
            ApellidoPaterno = Required(request.ApellidoPaterno),
            FechaNacimiento = request.FechaNacimiento
        };
        Apply(request, paciente);

        await repository.AddAsync(paciente, cancellationToken);
        await SaveAsync(cancellationToken);
        return ToResponse(paciente);
    }

    public async Task<PacienteResponseDto> UpdateAsync(
        int id,
        PacienteRequestDto request,
        CancellationToken cancellationToken)
    {
        ValidateBusinessRules(request);
        var paciente = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"No se encontró el paciente con id {id}.");
        var matricula = Required(request.Matricula);

        if (await repository.ExistsByMatriculaAsync(matricula, id, cancellationToken))
            throw new ConflictException($"Ya existe otro paciente con la matrícula '{matricula}'.");

        paciente.Matricula = matricula;
        paciente.Nombres = Required(request.Nombres);
        paciente.ApellidoPaterno = Required(request.ApellidoPaterno);
        paciente.FechaNacimiento = request.FechaNacimiento;
        Apply(request, paciente);

        await SaveAsync(cancellationToken);
        return ToResponse(paciente);
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await repository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new ConflictException("No fue posible guardar el paciente porque un dato único ya está registrado.");
        }
    }

    private static void ValidateBusinessRules(PacienteRequestDto request)
    {
        if (request.FechaNacimiento == default)
            throw new BusinessValidationException("La fecha de nacimiento es obligatoria.");
        if (request.FechaNacimiento > DateOnly.FromDateTime(DateTime.Today))
            throw new BusinessValidationException("La fecha de nacimiento no puede ser futura.");
        if (request.Semestre is < 1 or > 15)
            throw new BusinessValidationException("El semestre debe estar entre 1 y 15.");
    }

    private static void Apply(PacienteRequestDto source, Paciente target)
    {
        target.ApellidoMaterno = Optional(source.ApellidoMaterno);
        target.Sexo = Optional(source.Sexo);
        target.Telefono = Optional(source.Telefono);
        target.Correo = Optional(source.Correo)?.ToLowerInvariant();
        target.Carrera = Optional(source.Carrera);
        target.Semestre = source.Semestre;
        target.TipoSangre = Optional(source.TipoSangre)?.ToUpperInvariant();
        target.Alergias = Optional(source.Alergias);
        target.ContactoEmergencia = Optional(source.ContactoEmergencia);
        target.TelefonoEmergencia = Optional(source.TelefonoEmergencia);
    }

    private static string Required(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new BusinessValidationException("Los campos obligatorios no pueden estar vacíos.");
        return value.Trim();
    }
    private static string? Optional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static PacienteResponseDto ToResponse(Paciente p) => new(
        p.IdPaciente, p.Matricula, p.Nombres, p.ApellidoPaterno, p.ApellidoMaterno,
        p.FechaNacimiento, p.Sexo, p.Telefono, p.Correo, p.Carrera, p.Semestre,
        p.TipoSangre, p.Alergias, p.ContactoEmergencia, p.TelefonoEmergencia);
}
