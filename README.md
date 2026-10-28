# Sistema Centralizado de Atención Médica Universitaria

Primera versión funcional del backend del **Módulo A: Pacientes y médicos**, desarrollada con ASP.NET Core 8, Entity Framework Core, SQL Server y Swagger/OpenAPI.

## Arquitectura

```text
Controllers  ->  Services  ->  Repositories  ->  AppDbContext  ->  SQL Server
                    |
                  DTOs / Models
```

- `Controllers`: reciben las solicitudes y generan respuestas HTTP.
- `Services`: aplican reglas de negocio, existencia y duplicados.
- `Repositories`: contienen el acceso a datos con EF Core.
- `DTOs`: definen y validan los contratos de entrada y salida.
- `Data`: contiene el contexto, configuración e historial de migraciones.

## Requisitos

- .NET 8 SDK o posterior.
- SQL Server, SQL Server Express o LocalDB.

La configuración predeterminada usa LocalDB y se encuentra en `src/SistemaAtencionMedica.Api/appsettings.json`. Si se usa otra instancia, debe reemplazarse `ConnectionStrings:DefaultConnection`.

## Ejecutar el proyecto

Desde la raíz del repositorio:

```powershell
dotnet restore
dotnet tool restore
dotnet user-secrets set "Jwt:Key" "<clave-aleatoria-de-al-menos-32-bytes>" --project src/SistemaAtencionMedica.Api
dotnet ef database update --project src/SistemaAtencionMedica.Api --startup-project src/SistemaAtencionMedica.Api
dotnet run --project src/SistemaAtencionMedica.Api
```

Swagger queda disponible en `http://localhost:5080/swagger` o `https://localhost:7080/swagger` cuando el ambiente es `Development`.

## Endpoints

| Método | Ruta | Descripción |
|---|---|---|
| `POST` | `/api/pacientes` | Registrar un paciente |
| `GET` | `/api/pacientes` | Consultar todos los pacientes |
| `GET` | `/api/pacientes?matricula=A0123` | Filtrar por matrícula |
| `GET` | `/api/pacientes?nombre=Ana` | Filtrar por nombre o apellidos |
| `GET` | `/api/pacientes/{id}` | Consultar un paciente |
| `PUT` | `/api/pacientes/{id}` | Actualizar un paciente |
| `POST` | `/api/medicos` | Registrar un médico |
| `GET` | `/api/medicos` | Consultar médicos |
| `GET` | `/api/medicos/{id}` | Consultar un médico |

Los filtros `matricula` y `nombre` pueden combinarse. Las respuestas de error usan el formato estándar `ProblemDetails`.

### Ejemplo de paciente

```json
{
  "matricula": "A01234567",
  "nombres": "Ana María",
  "apellidoPaterno": "López",
  "apellidoMaterno": "García",
  "fechaNacimiento": "2003-08-19",
  "sexo": "Femenino",
  "telefono": "6141234567",
  "correo": "ana@universidad.edu.mx",
  "carrera": "Ingeniería de Software",
  "semestre": 6,
  "tipoSangre": "O+",
  "alergias": "Penicilina",
  "contactoEmergencia": "María García",
  "telefonoEmergencia": "6147654321"
}
```

### Ejemplo de médico

```json
{
  "numeroEmpleado": "MED-001",
  "nombres": "Laura",
  "apellidoPaterno": "Hernández",
  "apellidoMaterno": "Soto",
  "especialidad": "Medicina general",
  "cedulaProfesional": "12345678",
  "telefono": "6141112233",
  "correo": "laura.hernandez@universidad.edu.mx"
}
```

## Validaciones principales

- Matrícula y número de empleado únicos.
- Cédula profesional única cuando se proporciona.
- Fecha de nacimiento obligatoria y no futura.
- Semestre entre 1 y 15 cuando se proporciona.
- Correos con formato válido.
- Campos obligatorios y longitudes máximas validados antes de guardar.
- Índices únicos y restricciones `CHECK` también protegen la base de datos.

## Pruebas

```powershell
dotnet test
```

Las pruebas cubren duplicidad de matrícula y cédula, fecha futura, búsqueda por nombre y recursos inexistentes.

## Ejercicio 13: autenticación

El proyecto incorpora `POST /api/auth/register` y `POST /api/auth/login`.
Todos los endpoints de pacientes y médicos requieren autenticación.
El registro público asigna siempre el rol `Usuario`; no acepta roles privilegiados.
Las contraseñas se almacenan con `PasswordHasher<Usuario>` de ASP.NET Core y nunca se devuelven.
La migración `AddUsuarios` agrega únicamente la tabla de usuarios y el índice único del correo.

### Configurar JWT localmente

La clave no se incluye en Git. En PowerShell se puede generar y guardar en User Secrets:

```powershell
$jwtLocalKey = [Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(48))
@{ 'Jwt:Key' = $jwtLocalKey } | ConvertTo-Json -Compress | dotnet user-secrets set --project src/SistemaAtencionMedica.Api
Remove-Variable jwtLocalKey
```

Alternativamente, configura la variable de entorno `Jwt__Key` antes de ejecutar.
Los valores públicos `Jwt:Issuer`, `Jwt:Audience` y `Jwt:ExpirationMinutes`
se configuran en `appsettings.json`. La duración predeterminada es de 60 minutos.
La aplicación valida la configuración al iniciar y rechaza una clave ausente o menor de 32 bytes.

### Probar en Swagger

1. Ejecuta los comandos de inicio anteriores y abre `http://localhost:5080/swagger`.
2. Ejecuta `POST /api/auth/register` con:

```json
{
  "nombre": "Ana López",
  "correo": "ana@universidad.edu.mx",
  "password": "ClaveDeEjemplo123!"
}
```

El registro devuelve `201` con id, nombre, correo y rol. Un correo duplicado devuelve
`409`; los campos inválidos devuelven `400`. La contraseña debe tener entre 8 y 128 caracteres.

3. Ejecuta `POST /api/auth/login` con el correo y `password` anteriores.
   Devuelve `token`, `tokenType: "Bearer"`, `expiresIn` (segundos) y `usuario`.
   Las credenciales incorrectas devuelven `401`.
4. Copia solo el valor de `token`, pulsa **Authorize**, pégalo y confirma.
5. Ejecuta las rutas de pacientes o médicos. Swagger enviará
   `Authorization: Bearer <token>`. Sin token, con firma/issuer/audience incorrectos
   o con un token vencido, la API devuelve `401`.

Las pruebas HTTP de `AuthEndpointTests` ejercitan los controllers, services, repositories
y el middleware JWT con una base InMemory aislada y una clave sintética exclusiva de pruebas.
Incluyen también validación de Swagger. La aplicación normal sigue usando SQL Server.

## Ejercicio 14: React y AuthContext

Investigación de `useCallback`, `useMemo`, `createContext` y cleanup en hooks, con un ejemplo
demostrativo de `AuthContext` para React Native. No está integrado a una app (el repositorio
aún no tiene una). Ver [docs/ejercicio14/Ejercicio14.md](docs/ejercicio14/Ejercicio14.md) y
[el resumen para escribir a mano](docs/ejercicio14/Ejercicio14_ResumenManuscrito.md).
