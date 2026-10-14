using Microsoft.EntityFrameworkCore;
using SistemaAtencionMedica.Api.Data;
using SistemaAtencionMedica.Api.Models;

namespace SistemaAtencionMedica.Api.Repositories;

public class UsuarioRepository(AppDbContext context) : IUsuarioRepository
{
    public Task<Usuario?> GetByCorreoAsync(string correo, CancellationToken cancellationToken) =>
        context.Usuarios.SingleOrDefaultAsync(u => u.Correo == correo, cancellationToken);

    public Task AddAsync(Usuario usuario, CancellationToken cancellationToken) =>
        context.Usuarios.AddAsync(usuario, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);
}
