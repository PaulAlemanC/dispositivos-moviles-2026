using Microsoft.EntityFrameworkCore;
using SistemaAtencionMedica.Api.Data;
using SistemaAtencionMedica.Api.DTOs;
using SistemaAtencionMedica.Api.Repositories;
using SistemaAtencionMedica.Api.Services;

namespace SistemaAtencionMedica.Tests;

public class MedicoServiceTests
{
    [Fact]
    public async Task CreateAsync_ConCedulaDuplicada_LanzaConflicto()
    {
        await using var context = CreateContext();
        var service = new MedicoService(new MedicoRepository(context));
        await service.CreateAsync(ValidRequest("EMP-1"), CancellationToken.None);

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(ValidRequest("EMP-2"), CancellationToken.None));
    }

    [Fact]
    public async Task GetByIdAsync_CuandoNoExiste_LanzaNoEncontrado()
    {
        await using var context = CreateContext();
        var service = new MedicoService(new MedicoRepository(context));

        await Assert.ThrowsAsync<ResourceNotFoundException>(() =>
            service.GetByIdAsync(999, CancellationToken.None));
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static MedicoCreateDto ValidRequest(string numeroEmpleado) => new()
    {
        NumeroEmpleado = numeroEmpleado,
        Nombres = "Elena",
        ApellidoPaterno = "Ruiz",
        CedulaProfesional = "CED-12345",
        Especialidad = "Medicina general",
        Correo = "elena@universidad.edu"
    };
}
