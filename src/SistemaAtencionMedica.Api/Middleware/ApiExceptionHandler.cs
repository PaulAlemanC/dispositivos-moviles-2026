using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SistemaAtencionMedica.Api.Services;

namespace SistemaAtencionMedica.Api.Middleware;

public class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title, detail) = exception switch
        {
            InvalidCredentialsException => (StatusCodes.Status401Unauthorized, "Credenciales incorrectas", exception.Message),
            AuthValidationException => (StatusCodes.Status400BadRequest, "Datos inválidos", exception.Message),
            ResourceNotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado", exception.Message),
            ConflictException => (StatusCodes.Status409Conflict, "Conflicto de datos", exception.Message),
            BusinessValidationException => (StatusCodes.Status422UnprocessableEntity, "Regla de negocio no válida", exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "Error interno", "Ocurrió un error inesperado.")
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Error no controlado al procesar {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        }, cancellationToken);

        return true;
    }
}
