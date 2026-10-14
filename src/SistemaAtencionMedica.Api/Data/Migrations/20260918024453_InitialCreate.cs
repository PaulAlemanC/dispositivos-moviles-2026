using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaAtencionMedica.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Medicos",
                columns: table => new
                {
                    IdMedico = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroEmpleado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Nombres = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApellidoPaterno = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ApellidoMaterno = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Especialidad = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    CedulaProfesional = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Correo = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medicos", x => x.IdMedico);
                });

            migrationBuilder.CreateTable(
                name: "Pacientes",
                columns: table => new
                {
                    IdPaciente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Matricula = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Nombres = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApellidoPaterno = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ApellidoMaterno = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    FechaNacimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    Sexo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Correo = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true),
                    Carrera = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Semestre = table.Column<int>(type: "int", nullable: true),
                    TipoSangre = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Alergias = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ContactoEmergencia = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    TelefonoEmergencia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pacientes", x => x.IdPaciente);
                    table.CheckConstraint("CK_Pacientes_FechaNacimiento", "[FechaNacimiento] <= CONVERT(date, GETDATE())");
                    table.CheckConstraint("CK_Pacientes_Semestre", "[Semestre] IS NULL OR ([Semestre] BETWEEN 1 AND 15)");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Medicos_CedulaProfesional",
                table: "Medicos",
                column: "CedulaProfesional",
                unique: true,
                filter: "[CedulaProfesional] IS NOT NULL AND [CedulaProfesional] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_Medicos_NumeroEmpleado",
                table: "Medicos",
                column: "NumeroEmpleado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pacientes_Matricula",
                table: "Pacientes",
                column: "Matricula",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Medicos");

            migrationBuilder.DropTable(
                name: "Pacientes");
        }
    }
}
