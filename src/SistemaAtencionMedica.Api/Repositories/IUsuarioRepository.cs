using SistemaAtencionMedica.Api.Models;

namespace SistemaAtencionMedica.Api.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByCorreoAsync(string correo, CancellationToken cancellationToken);
    Task AddAsync(Usuario usuario, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
