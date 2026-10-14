using Microsoft.EntityFrameworkCore;
using SistemaAtencionMedica.Api.Data;
using SistemaAtencionMedica.Api.Models;

namespace SistemaAtencionMedica.Api.Repositories;

public class MedicoRepository(AppDbContext context) : IMedicoRepository
{
    public async Task<IReadOnlyList<Medico>> GetAllAsync(CancellationToken cancellationToken) =>
        await context.Medicos
            .AsNoTracking()
            .OrderBy(m => m.ApellidoPaterno)
            .ThenBy(m => m.Nombres)
            .ToListAsync(cancellationToken);

    public Task<Medico?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        context.Medicos.FirstOrDefaultAsync(m => m.IdMedico == id, cancellationToken);

    public Task<bool> ExistsByNumeroEmpleadoAsync(string numeroEmpleado, CancellationToken cancellationToken) =>
        context.Medicos.AnyAsync(m => m.NumeroEmpleado == numeroEmpleado, cancellationToken);

    public Task<bool> ExistsByCedulaAsync(string cedulaProfesional, CancellationToken cancellationToken) =>
        context.Medicos.AnyAsync(m => m.CedulaProfesional == cedulaProfesional, cancellationToken);

    public Task AddAsync(Medico medico, CancellationToken cancellationToken) =>
        context.Medicos.AddAsync(medico, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);
}
