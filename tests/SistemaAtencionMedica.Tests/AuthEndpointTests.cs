using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using SistemaAtencionMedica.Api.Data;
using SistemaAtencionMedica.Api.DTOs;
using SistemaAtencionMedica.Api.Models;

namespace SistemaAtencionMedica.Tests;

public class AuthApiFactory : WebApplicationFactory<Program>
{
    // Clave sintética exclusivamente para pruebas; no se utiliza en la aplicación local.
    public const string TestKey = "test-only-key-0123456789-abcdefghijklmnopqrstuvwxyz";
    private readonly string databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = TestKey,
            ["Jwt:Issuer"] = "AuthTests",
            ["Jwt:Audience"] = "AuthTests.Client",
            ["Jwt:ExpirationMinutes"] = "60"
        }));
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(databaseName));
        });
    }
}

public class AuthEndpointTests : IClassFixture<AuthApiFactory>
{
    private readonly AuthApiFactory factory;
    public AuthEndpointTests(AuthApiFactory factory) => this.factory = factory;

    private HttpClient Client() => factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false
    });

    [Fact]
    public async Task Register_HashesPassword_RejectsDuplicate_AndCannotAssignRole()
    {
        using var client = Client();
        var correo = $"registro-{Guid.NewGuid():N}@universidad.edu";
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            nombre = "Ana López",
            correo,
            password = "ClaveDePrueba123!",
            rol = "Administrador"
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("password", body, StringComparison.OrdinalIgnoreCase);
        var usuario = JsonSerializer.Deserialize<UsuarioResponseDto>(body, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        Assert.Equal("Usuario", usuario.Rol);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var saved = await db.Usuarios.SingleAsync(u => u.Correo == correo);
        Assert.NotEqual("ClaveDePrueba123!", saved.PasswordHash);
        Assert.NotEqual(PasswordVerificationResult.Failed,
            new PasswordHasher<Usuario>().VerifyHashedPassword(saved, saved.PasswordHash, "ClaveDePrueba123!"));

        var duplicate = await client.PostAsJsonAsync("/api/auth/register", new
        {
            nombre = "Otro",
            correo = correo.ToUpperInvariant(),
            password = "ClaveDePrueba123!"
        });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
    }

    [Theory]
    [InlineData("", "ana@universidad.edu", "Clave123!")]
    [InlineData("Ana", "correo-invalido", "Clave123!")]
    [InlineData("Ana", "ana@universidad.edu", "corta")]
    [InlineData("Ana", "", "Clave123!")]
    public async Task Register_InvalidData_Returns400(string nombre, string correo, string password)
    {
        using var client = Client();
        var response = await client.PostAsJsonAsync("/api/auth/register", new { nombre, correo, password });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsBearer_AndAccessToProtectedEndpoints()
    {
        using var client = Client();
        var correo = $"login-{Guid.NewGuid():N}@universidad.edu";
        await client.PostAsJsonAsync("/api/auth/register", new { nombre = "Ana", correo, password = "Clave123!" });
        var response = await client.PostAsJsonAsync("/api/auth/login", new { correo, password = "Clave123!" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var login = (await response.Content.ReadFromJsonAsync<LoginResponseDto>())!;
        Assert.Equal("Bearer", login.TokenType);
        Assert.Equal(3600, login.ExpiresIn);
        Assert.DoesNotContain("password", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/pacientes")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/medicos")).StatusCode);

        var wrong = await client.PostAsJsonAsync("/api/auth/login", new { correo, password = "Incorrecta!" });
        Assert.Equal(HttpStatusCode.Unauthorized, wrong.StatusCode);
        var missing = await client.PostAsJsonAsync("/api/auth/login", new
        {
            correo = "inexistente@universidad.edu",
            password = "Incorrecta!"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, missing.StatusCode);
    }

    [Theory]
    [InlineData("/api/pacientes")]
    [InlineData("/api/medicos")]
    public async Task ProtectedEndpoints_WithoutToken_Return401(string path)
    {
        using var client = Client();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync(path)).StatusCode);
    }

    [Theory]
    [InlineData("malformed")]
    [InlineData("expired")]
    [InlineData("signature")]
    [InlineData("issuer")]
    [InlineData("audience")]
    public async Task ProtectedEndpoints_RejectInvalidTokens(string reason)
    {
        using var client = Client();
        var now = DateTime.UtcNow;
        var jwt = new JwtSecurityToken(
            issuer: reason == "issuer" ? "WrongIssuer" : "AuthTests",
            audience: reason == "audience" ? "WrongAudience" : "AuthTests.Client",
            notBefore: now.AddMinutes(-10),
            expires: reason == "expired" ? now.AddMinutes(-1) : now.AddMinutes(10),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(reason == "signature"
                    ? "wrong-test-key-abcdefghijklmnopqrstuvwxyz-0123456789"
                    : AuthApiFactory.TestKey)), SecurityAlgorithms.HmacSha256));
        var token = reason == "malformed" ? "invalid-token" : new JwtSecurityTokenHandler().WriteToken(jwt);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/pacientes")).StatusCode);
    }

    [Fact]
    public async Task Swagger_DescribesPublicAuthAndProtectedRoutes()
    {
        using var client = Client();
        using var document = JsonDocument.Parse(await client.GetStringAsync("/swagger/v1/swagger.json"));
        var paths = document.RootElement.GetProperty("paths");
        Assert.True(paths.GetProperty("/api/pacientes").GetProperty("get").TryGetProperty("security", out _));
        Assert.False(paths.GetProperty("/api/auth/login").GetProperty("post").TryGetProperty("security", out _));
        Assert.Equal("bearer", document.RootElement.GetProperty("components").GetProperty("securitySchemes")
            .GetProperty("Bearer").GetProperty("scheme").GetString());
    }
}
