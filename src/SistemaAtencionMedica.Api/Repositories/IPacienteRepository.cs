using SistemaAtencionMedica.Api.Models;

namespace SistemaAtencionMedica.Api.Repositories;

public interface IPacienteRepository
{
    Task<IReadOnlyList<Paciente>> GetAllAsync(string? matricula, string? nombre, CancellationToken cancellationToken);
    Task<Paciente?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<bool> ExistsByMatriculaAsync(string matricula, int? excludeId, CancellationToken cancellationToken);
    Task AddAsync(Paciente paciente, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
