namespace TajamarApp.Core.DTOs.Trayectos;

public class PasajeroResumenDto
{
    public int IdReserva { get; set; }
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = null!;
    public string Apellidos { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Foto { get; set; }
    public string? Telefono { get; set; }
    public string? Bio { get; set; }
    public DateTime FechaReserva { get; set; }
    public string Estado { get; set; } = null!; // "Confirmada"
}

public class AlumnoEnColaDto
{
    public int IdCola { get; set; }
    public int IdUsuario { get; set; }
    public int Posicion { get; set; }
    public string Nombre { get; set; } = null!;
    public string Apellidos { get; set; } = null!;
    public string? Foto { get; set; }
    public DateTime FechaRegistro { get; set; }
}

public class PasajerosTrayectoDto
{
    public int TrayectoId { get; set; }
    public string Origen { get; set; } = null!;
    public string Destino { get; set; } = null!;
    public DateTime FechaHoraSalida { get; set; }
    public int PlazasTotales { get; set; }
    public int PlazasDisponibles { get; set; }
    public int PlazasOcupadas => PlazasTotales - PlazasDisponibles;
    public string Estado { get; set; } = null!;

    public List<PasajeroResumenDto> PasajerosConfirmados { get; set; } = new();
    public List<AlumnoEnColaDto> AlumnosEnCola { get; set; } = new();
}
