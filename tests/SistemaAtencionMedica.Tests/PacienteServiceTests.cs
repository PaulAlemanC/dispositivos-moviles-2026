using Microsoft.EntityFrameworkCore;
using SistemaAtencionMedica.Api.Data;
using SistemaAtencionMedica.Api.DTOs;
using SistemaAtencionMedica.Api.Repositories;
using SistemaAtencionMedica.Api.Services;

namespace SistemaAtencionMedica.Tests;

public class PacienteServiceTests
{
    [Fact]
    public async Task CreateAsync_ConMatriculaDuplicada_LanzaConflicto()
    {
        await using var context = CreateContext();
        var service = new PacienteService(new PacienteRepository(context));
        var request = ValidRequest("A01234");

        await service.CreateAsync(request, CancellationToken.None);

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(ValidRequest("A01234"), CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_ConFechaFutura_LanzaValidacion()
    {
        await using var context = CreateContext();
        var service = new PacienteService(new PacienteRepository(context));
        var request = ValidRequest("A01235");
        request.FechaNacimiento = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            service.CreateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task GetAllAsync_FiltraPorNombre()
    {
        await using var context = CreateContext();
        var service = new PacienteService(new PacienteRepository(context));
        await service.CreateAsync(ValidRequest("A01", "María"), CancellationToken.None);
        await service.CreateAsync(ValidRequest("A02", "Carlos"), CancellationToken.None);

        var result = await service.GetAllAsync(null, "María", CancellationToken.None);

        var paciente = Assert.Single(result);
        Assert.Equal("A01", paciente.Matricula);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static PacienteRequestDto ValidRequest(string matricula, string nombres = "Ana") => new()
    {
        Matricula = matricula,
        Nombres = nombres,
        ApellidoPaterno = "López",
        FechaNacimiento = new DateOnly(2002, 5, 10),
        Semestre = 4,
        Correo = "ana@universidad.edu"
    };
}
