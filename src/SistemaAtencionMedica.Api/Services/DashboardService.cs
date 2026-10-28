using SistemaAtencionMedica.Api.DTOs;
using SistemaAtencionMedica.Api.Models;
using SistemaAtencionMedica.Api.Repositories;

namespace SistemaAtencionMedica.Api.Services;

public class DashboardService(
    IPacienteRepository pacienteRepository,
    IMedicoRepository medicoRepository) : IDashboardService
{
    private static readonly (string Hora, string Motivo, string Estado)[] PlantillasCita =
    [
        ("09:00", "Consulta general", "Confirmada"),
        ("10:30", "Revisión de signos vitales", "En espera"),
        ("12:00", "Seguimiento médico", "Programada"),
        ("13:30", "Certificado médico universitario", "Programada")
    ];

    public async Task<DashboardSummaryResponseDto> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var pacientes = await pacienteRepository.GetAllAsync(null, null, cancellationToken);
        var medicos = await medicoRepository.GetAllAsync(cancellationToken);

        var proximasCitas = BuildProximasCitas(pacientes, medicos);
        var alertas = BuildAlertas(pacientes);

        var metricas = new DashboardMetricsDto(
            TotalPacientesRegistrados: pacientes.Count,
            MedicosActivos: medicos.Count,
            CitasProgramadasHoy: proximasCitas.Count);

        return new DashboardSummaryResponseDto(metricas, proximasCitas, alertas);
    }

    private static IReadOnlyList<CitaDelDiaDto> BuildProximasCitas(
        IReadOnlyList<Paciente> pacientes,
        IReadOnlyList<Medico> medicos)
    {
        if (pacientes.Count == 0)
        {
            var medicoDemo = medicos.Count > 0
                ? FormatMedicoNombre(medicos[0])
                : "Dra. Laura Hernández Soto";

            return
            [
                new CitaDelDiaDto(
                    IdCita: 1,
                    Hora: "09:00",
                    PacienteNombre: "Ana María López García (Demo)",
                    Matricula: "A01234567",
                    MedicoNombre: medicoDemo,
                    Motivo: "Consulta general",
                    Estado: "Confirmada"),
                new CitaDelDiaDto(
                    IdCita: 2,
                    Hora: "10:30",
                    PacienteNombre: "Carlos Eduardo Ramírez Pérez (Demo)",
                    Matricula: "A01234568",
                    MedicoNombre: medicoDemo,
                    Motivo: "Revisión de signos vitales",
                    Estado: "Programada")
            ];
        }

        var cantidad = Math.Min(pacientes.Count, PlantillasCita.Length);
        var citas = new List<CitaDelDiaDto>(cantidad);

        for (var i = 0; i < cantidad; i++)
        {
            var paciente = pacientes[i];
            var plantilla = PlantillasCita[i];
            var medicoNombre = medicos.Count > 0
                ? FormatMedicoNombre(medicos[i % medicos.Count])
                : "Médico de guardia";

            citas.Add(new CitaDelDiaDto(
                IdCita: i + 1,
                Hora: plantilla.Hora,
                PacienteNombre: FormatPacienteNombre(paciente),
                Matricula: paciente.Matricula,
                MedicoNombre: medicoNombre,
                Motivo: plantilla.Motivo,
                Estado: plantilla.Estado));
        }

        return citas;
    }

    private static IReadOnlyList<AlertaSistemaDto> BuildAlertas(IReadOnlyList<Paciente> pacientes)
    {
        var alertas = new List<AlertaSistemaDto>();
        var nextId = 1;

        var conAlergias = pacientes.Count(p => !string.IsNullOrWhiteSpace(p.Alergias));
        if (conAlergias > 0)
        {
            alertas.Add(new AlertaSistemaDto(
                IdAlerta: nextId++,
                Tipo: "Clínica",
                Mensaje: $"{conAlergias} paciente(s) cuentan con alergias registradas en su expediente.",
                Prioridad: "Alta"));
        }

        var sinTelefonoEmergencia = pacientes.Count(p => string.IsNullOrWhiteSpace(p.TelefonoEmergencia));
        if (sinTelefonoEmergencia > 0)
        {
            alertas.Add(new AlertaSistemaDto(
                IdAlerta: nextId++,
                Tipo: "Expediente",
                Mensaje: $"{sinTelefonoEmergencia} paciente(s) no tienen teléfono de emergencia registrado.",
                Prioridad: "Media"));
        }

        alertas.Add(new AlertaSistemaDto(
            IdAlerta: nextId,
            Tipo: "Operativa",
            Mensaje: "Recordatorio: Verificar stock de insumos básicos del consultorio",
            Prioridad: "Baja"));

        return alertas;
    }

    private static string FormatPacienteNombre(Paciente p) =>
        string.IsNullOrWhiteSpace(p.ApellidoMaterno)
            ? $"{p.Nombres} {p.ApellidoPaterno}"
            : $"{p.Nombres} {p.ApellidoPaterno} {p.ApellidoMaterno}";

    private static string FormatMedicoNombre(Medico m) =>
        string.IsNullOrWhiteSpace(m.ApellidoMaterno)
            ? $"Dr(a). {m.Nombres} {m.ApellidoPaterno}"
            : $"Dr(a). {m.Nombres} {m.ApellidoPaterno} {m.ApellidoMaterno}";
}
