namespace SistemaAtencionMedica.Api.Services;

public class ResourceNotFoundException(string message) : Exception(message);
public class ConflictException(string message) : Exception(message);
public class InvalidCredentialsException() : Exception("Correo o contraseña incorrectos.");
public class AuthValidationException(string message) : Exception(message);
public class BusinessValidationException(string message) : Exception(message);
