using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TajamarApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CURSOSTAJAMAR",
                columns: table => new
                {
                    IDCURSO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NOMBRE = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FECHAINICIO = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FECHAFIN = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ACTIVO = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CURSOSTAJAMAR", x => x.IDCURSO);
                });

            migrationBuilder.CreateTable(
                name: "USUARIOSTAJAMAR",
                columns: table => new
                {
                    IDUSUARIO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NOMBRE = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: false),
                    APELLIDOS = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: false),
                    EMAIL = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: false),
                    ESTADO = table.Column<bool>(type: "bit", nullable: false),
                    IMAGEN = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    PASSWORD = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IDROLE = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USUARIOSTAJAMAR", x => x.IDUSUARIO);
                });

            migrationBuilder.CreateTable(
                name: "CURSOSUSUARIOSTAJAMAR",
                columns: table => new
                {
                    IDCURSOSUSUARIOS = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IDCURSO = table.Column<int>(type: "int", nullable: true),
                    IDUSUARIO = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CURSOSUSUARIOSTAJAMAR", x => x.IDCURSOSUSUARIOS);
                    table.ForeignKey(
                        name: "FK_CURSOSUSUARIOSTAJAMAR_CURSOSTAJAMAR_IDCURSO",
                        column: x => x.IDCURSO,
                        principalTable: "CURSOSTAJAMAR",
                        principalColumn: "IDCURSO",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CURSOSUSUARIOSTAJAMAR_USUARIOSTAJAMAR_IDUSUARIO",
                        column: x => x.IDUSUARIO,
                        principalTable: "USUARIOSTAJAMAR",
                        principalColumn: "IDUSUARIO",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EVENTOS_APP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ubicacion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Imagen = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    CreadorId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EVENTOS_APP", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EVENTOS_APP_USUARIOSTAJAMAR_CreadorId",
                        column: x => x.CreadorId,
                        principalTable: "USUARIOSTAJAMAR",
                        principalColumn: "IDUSUARIO",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PERFIL_APP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuarioTajamar = table.Column<int>(type: "int", nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EsConductor = table.Column<bool>(type: "bit", nullable: false),
                    MarcaCoche = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ModeloCoche = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ColorCoche = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Matricula = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    PlazasCoche = table.Column<int>(type: "int", nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PERFIL_APP", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PERFIL_APP_USUARIOSTAJAMAR_IdUsuarioTajamar",
                        column: x => x.IdUsuarioTajamar,
                        principalTable: "USUARIOSTAJAMAR",
                        principalColumn: "IDUSUARIO",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EVENTO_ASISTENTES",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventoId = table.Column<int>(type: "int", nullable: false),
                    UsuarioTajamarId = table.Column<int>(type: "int", nullable: false),
                    FechaInscripcion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EVENTO_ASISTENTES", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EVENTO_ASISTENTES_EVENTOS_APP_EventoId",
                        column: x => x.EventoId,
                        principalTable: "EVENTOS_APP",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EVENTO_ASISTENTES_USUARIOSTAJAMAR_UsuarioTajamarId",
                        column: x => x.UsuarioTajamarId,
                        principalTable: "USUARIOSTAJAMAR",
                        principalColumn: "IDUSUARIO",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TRAYECTOS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConductorPerfilId = table.Column<int>(type: "int", nullable: false),
                    EventoId = table.Column<int>(type: "int", nullable: true),
                    Origen = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Destino = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FechaHoraSalida = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlazasTotales = table.Column<int>(type: "int", nullable: false),
                    PlazasDisponibles = table.Column<int>(type: "int", nullable: false),
                    TipoTrayecto = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Modalidad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DiasSemana = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Turno = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRAYECTOS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TRAYECTOS_EVENTOS_APP_EventoId",
                        column: x => x.EventoId,
                        principalTable: "EVENTOS_APP",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TRAYECTOS_PERFIL_APP_ConductorPerfilId",
                        column: x => x.ConductorPerfilId,
                        principalTable: "PERFIL_APP",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "COLA_ESPERA",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrayectoId = table.Column<int>(type: "int", nullable: false),
                    UsuarioTajamarId = table.Column<int>(type: "int", nullable: false),
                    Posicion = table.Column<int>(type: "int", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COLA_ESPERA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_COLA_ESPERA_TRAYECTOS_TrayectoId",
                        column: x => x.TrayectoId,
                        principalTable: "TRAYECTOS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_COLA_ESPERA_USUARIOSTAJAMAR_UsuarioTajamarId",
                        column: x => x.UsuarioTajamarId,
                        principalTable: "USUARIOSTAJAMAR",
                        principalColumn: "IDUSUARIO",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RESERVAS_TRAYECTO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrayectoId = table.Column<int>(type: "int", nullable: false),
                    PasajeroUsuarioId = table.Column<int>(type: "int", nullable: false),
                    FechaReserva = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RESERVAS_TRAYECTO", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RESERVAS_TRAYECTO_TRAYECTOS_TrayectoId",
                        column: x => x.TrayectoId,
                        principalTable: "TRAYECTOS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RESERVAS_TRAYECTO_USUARIOSTAJAMAR_PasajeroUsuarioId",
                        column: x => x.PasajeroUsuarioId,
                        principalTable: "USUARIOSTAJAMAR",
                        principalColumn: "IDUSUARIO",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_COLA_ESPERA_TrayectoId",
                table: "COLA_ESPERA",
                column: "TrayectoId");

            migrationBuilder.CreateIndex(
                name: "IX_COLA_ESPERA_UsuarioTajamarId",
                table: "COLA_ESPERA",
                column: "UsuarioTajamarId");

            migrationBuilder.CreateIndex(
                name: "IX_CURSOSUSUARIOSTAJAMAR_IDCURSO",
                table: "CURSOSUSUARIOSTAJAMAR",
                column: "IDCURSO");

            migrationBuilder.CreateIndex(
                name: "IX_CURSOSUSUARIOSTAJAMAR_IDUSUARIO",
                table: "CURSOSUSUARIOSTAJAMAR",
                column: "IDUSUARIO");

            migrationBuilder.CreateIndex(
                name: "IX_EVENTO_ASISTENTES_EventoId_UsuarioTajamarId",
                table: "EVENTO_ASISTENTES",
                columns: new[] { "EventoId", "UsuarioTajamarId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EVENTO_ASISTENTES_UsuarioTajamarId",
                table: "EVENTO_ASISTENTES",
                column: "UsuarioTajamarId");

            migrationBuilder.CreateIndex(
                name: "IX_EVENTOS_APP_CreadorId",
                table: "EVENTOS_APP",
                column: "CreadorId");

            migrationBuilder.CreateIndex(
                name: "IX_PERFIL_APP_IdUsuarioTajamar",
                table: "PERFIL_APP",
                column: "IdUsuarioTajamar",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RESERVAS_TRAYECTO_PasajeroUsuarioId",
                table: "RESERVAS_TRAYECTO",
                column: "PasajeroUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_RESERVAS_TRAYECTO_TrayectoId",
                table: "RESERVAS_TRAYECTO",
                column: "TrayectoId");

            migrationBuilder.CreateIndex(
                name: "IX_TRAYECTOS_ConductorPerfilId",
                table: "TRAYECTOS",
                column: "ConductorPerfilId");

            migrationBuilder.CreateIndex(
                name: "IX_TRAYECTOS_EventoId",
                table: "TRAYECTOS",
                column: "EventoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "COLA_ESPERA");

            migrationBuilder.DropTable(
                name: "CURSOSUSUARIOSTAJAMAR");

            migrationBuilder.DropTable(
                name: "EVENTO_ASISTENTES");

            migrationBuilder.DropTable(
                name: "RESERVAS_TRAYECTO");

            migrationBuilder.DropTable(
                name: "CURSOSTAJAMAR");

            migrationBuilder.DropTable(
                name: "TRAYECTOS");

            migrationBuilder.DropTable(
                name: "EVENTOS_APP");

            migrationBuilder.DropTable(
                name: "PERFIL_APP");

            migrationBuilder.DropTable(
                name: "USUARIOSTAJAMAR");
        }
    }
}
