using Microsoft.EntityFrameworkCore;
using SistemaAtencionMedica.Api.Data;
using SistemaAtencionMedica.Api.Models;

namespace SistemaAtencionMedica.Api.Repositories;

public class PacienteRepository(AppDbContext context) : IPacienteRepository
{
    public async Task<IReadOnlyList<Paciente>> GetAllAsync(
        string? matricula,
        string? nombre,
        CancellationToken cancellationToken)
    {
        IQueryable<Paciente> query = context.Pacientes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(matricula))
        {
            var value = matricula.Trim();
            query = query.Where(p => p.Matricula.Contains(value));
        }

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            var value = nombre.Trim();
            query = query.Where(p =>
                p.Nombres.Contains(value) ||
                p.ApellidoPaterno.Contains(value) ||
                (p.ApellidoMaterno != null && p.ApellidoMaterno.Contains(value)));
        }

        return await query
            .OrderBy(p => p.ApellidoPaterno)
            .ThenBy(p => p.Nombres)
            .ToListAsync(cancellationToken);
    }

    public Task<Paciente?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        context.Pacientes.FirstOrDefaultAsync(p => p.IdPaciente == id, cancellationToken);

    public Task<bool> ExistsByMatriculaAsync(string matricula, int? excludeId, CancellationToken cancellationToken) =>
        context.Pacientes.AnyAsync(
            p => p.Matricula == matricula && (!excludeId.HasValue || p.IdPaciente != excludeId.Value),
            cancellationToken);

    public Task AddAsync(Paciente paciente, CancellationToken cancellationToken) =>
        context.Pacientes.AddAsync(paciente, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);
}
