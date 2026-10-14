using SistemaAtencionMedica.Api.Models;

namespace SistemaAtencionMedica.Api.Repositories;

public interface IMedicoRepository
{
    Task<IReadOnlyList<Medico>> GetAllAsync(CancellationToken cancellationToken);
    Task<Medico?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<bool> ExistsByNumeroEmpleadoAsync(string numeroEmpleado, CancellationToken cancellationToken);
    Task<bool> ExistsByCedulaAsync(string cedulaProfesional, CancellationToken cancellationToken);
    Task AddAsync(Medico medico, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
