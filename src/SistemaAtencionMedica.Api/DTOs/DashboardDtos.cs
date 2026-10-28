namespace SistemaAtencionMedica.Api.DTOs;

public record DashboardMetricsDto(
    int TotalPacientesRegistrados,
    int MedicosActivos,
    int CitasProgramadasHoy);

public record CitaDelDiaDto(
    int IdCita,
    string Hora,
    string PacienteNombre,
    string Matricula,
    string MedicoNombre,
    string Motivo,
    string Estado);

public record AlertaSistemaDto(
    int IdAlerta,
    string Tipo,
    string Mensaje,
    string Prioridad);

public record DashboardSummaryResponseDto(
    DashboardMetricsDto Metricas,
    IReadOnlyList<CitaDelDiaDto> ProximasCitas,
    IReadOnlyList<AlertaSistemaDto> Alertas);
