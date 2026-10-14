using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SistemaAtencionMedica.Api.DTOs;
using SistemaAtencionMedica.Api.Models;
using SistemaAtencionMedica.Api.Repositories;

namespace SistemaAtencionMedica.Api.Services;

public class AuthService(
    IUsuarioRepository repository,
    IPasswordHasher<Usuario> passwordHasher,
    IOptions<JwtOptions> jwtOptions) : IAuthService
{
    public async Task<UsuarioResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken)
    {
        Validate(request);
        var correo = request.Correo.Trim().ToLowerInvariant();
        if (await repository.GetByCorreoAsync(correo, cancellationToken) is not null)
            throw new ConflictException("El correo ya está registrado.");

        var usuario = new Usuario
        {
            Nombre = request.Nombre.Trim(),
            Correo = correo,
            PasswordHash = string.Empty,
            Rol = "Usuario" // El registro público nunca recibe ni asigna roles privilegiados.
        };
        usuario.PasswordHash = passwordHasher.HashPassword(usuario, request.Password);
        await repository.AddAsync(usuario, cancellationToken);
        try
        {
            await repository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            // El índice único también cubre registros simultáneos del mismo correo.
            throw new ConflictException("El correo ya está registrado.");
        }
        return ToResponse(usuario);
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken)
    {
        Validate(request);
        var usuario = await repository.GetByCorreoAsync(request.Correo.Trim().ToLowerInvariant(), cancellationToken);
        if (usuario is null)
            throw new InvalidCredentialsException();

        var result = passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new InvalidCredentialsException();
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            usuario.PasswordHash = passwordHasher.HashPassword(usuario, request.Password);
            await repository.SaveChangesAsync(cancellationToken);
        }

        var options = jwtOptions.Value;
        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, usuario.IdUsuario.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Role, usuario.Rol)
            ],
            notBefore: now,
            expires: now.AddMinutes(options.ExpirationMinutes),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key)),
                SecurityAlgorithms.HmacSha256));
        return new LoginResponseDto(new JwtSecurityTokenHandler().WriteToken(token),
            "Bearer", options.ExpirationMinutes * 60, ToResponse(usuario));
    }

    private static void Validate(object request)
    {
        var errors = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request, new ValidationContext(request), errors, true))
            throw new AuthValidationException(string.Join(" ", errors.Select(e => e.ErrorMessage)));
    }

    private static UsuarioResponseDto ToResponse(Usuario usuario) =>
        new(usuario.IdUsuario, usuario.Nombre, usuario.Correo, usuario.Rol);
}
