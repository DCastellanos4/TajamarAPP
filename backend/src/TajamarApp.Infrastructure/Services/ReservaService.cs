using Microsoft.EntityFrameworkCore;
using TajamarApp.Core.DTOs.Reservas;
using TajamarApp.Core.Entities;
using TajamarApp.Core.Interfaces;
using TajamarApp.Infrastructure.Data;

namespace TajamarApp.Infrastructure.Services;

public class ReservaService : IReservaService
{
    private readonly TajamarAppContext _context;

    public ReservaService(TajamarAppContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MiReservaDto>> GetMisReservasAsync(int idUsuario)
    {
        // 1. Obtener reservas del usuario (Confirmadas y Canceladas)
        var reservas = await _context.ReservasTrayecto
            .AsNoTracking()
            .Include(r => r.Trayecto)
                .ThenInclude(t => t.Conductor)
                    .ThenInclude(c => c.UsuarioTajamar)
            .Include(r => r.Trayecto)
                .ThenInclude(t => t.Evento)
            .Where(r => r.PasajeroUsuarioId == idUsuario)
            .ToListAsync();

        // 2. Obtener inscripciones activas en lista de espera
        var colas = await _context.ColasEspera
            .AsNoTracking()
            .Include(c => c.Trayecto)
                .ThenInclude(t => t.Conductor)
                    .ThenInclude(cond => cond.UsuarioTajamar)
            .Include(c => c.Trayecto)
                .ThenInclude(t => t.Evento)
            .Where(c => c.UsuarioTajamarId == idUsuario && c.Estado == "EnEspera")
            .ToListAsync();

        var resultado = new List<MiReservaDto>();

        // Mapear reservas
        foreach (var r in reservas)
        {
            var t = r.Trayecto;
            var cond = t?.Conductor;
            var u = cond?.UsuarioTajamar;

            resultado.Add(new MiReservaDto
            {
                Id = r.Id,
                TipoEntrada = "Reserva",
                IdReserva = r.Id,
                IdCola = null,
                Estado = r.Estado,
                PosicionCola = null,
                FechaRegistro = r.FechaReserva,

                TrayectoId = r.TrayectoId,
                Origen = t?.Origen ?? string.Empty,
                Destino = t?.Destino ?? string.Empty,
                FechaHoraSalida = t?.FechaHoraSalida ?? DateTime.MinValue,
                TipoTrayecto = t?.TipoTrayecto ?? "Puntual",
                Modalidad = t?.Modalidad ?? "SoloIda",
                Turno = t?.Turno,
                DiasSemana = t?.DiasSemana,
                PlazasTotales = t?.PlazasTotales ?? 0,
                PlazasDisponibles = t?.PlazasDisponibles ?? 0,
                EstadoTrayecto = t?.Estado ?? "Activo",
                EventoId = t?.EventoId,
                EventoTitulo = t?.Evento?.Titulo,

                ConductorIdUsuario = u?.IdUsuario ?? 0,
                ConductorNombre = $"{u?.Nombre} {u?.Apellidos}".Trim(),
                ConductorFoto = u?.Imagen,
                ConductorTelefono = cond?.Telefono,
                Coche = $"{cond?.MarcaCoche} {cond?.ModeloCoche}".Trim(),
                ColorCoche = cond?.ColorCoche,
                Matricula = cond?.Matricula
            });
        }

        // Mapear entradas en lista de espera
        foreach (var c in colas)
        {
            var t = c.Trayecto;
            var cond = t?.Conductor;
            var u = cond?.UsuarioTajamar;

            resultado.Add(new MiReservaDto
            {
                Id = c.Id,
                TipoEntrada = "ColaEspera",
                IdReserva = null,
                IdCola = c.Id,
                Estado = c.Estado,
                PosicionCola = c.Posicion,
                FechaRegistro = c.FechaRegistro,

                TrayectoId = c.TrayectoId,
                Origen = t?.Origen ?? string.Empty,
                Destino = t?.Destino ?? string.Empty,
                FechaHoraSalida = t?.FechaHoraSalida ?? DateTime.MinValue,
                TipoTrayecto = t?.TipoTrayecto ?? "Puntual",
                Modalidad = t?.Modalidad ?? "SoloIda",
                Turno = t?.Turno,
                DiasSemana = t?.DiasSemana,
                PlazasTotales = t?.PlazasTotales ?? 0,
                PlazasDisponibles = t?.PlazasDisponibles ?? 0,
                EstadoTrayecto = t?.Estado ?? "Completo",
                EventoId = t?.EventoId,
                EventoTitulo = t?.Evento?.Titulo,

                ConductorIdUsuario = u?.IdUsuario ?? 0,
                ConductorNombre = $"{u?.Nombre} {u?.Apellidos}".Trim(),
                ConductorFoto = u?.Imagen,
                ConductorTelefono = cond?.Telefono,
                Coche = $"{cond?.MarcaCoche} {cond?.ModeloCoche}".Trim(),
                ColorCoche = cond?.ColorCoche,
                Matricula = cond?.Matricula
            });
        }

        // Ordenar cronológicamente por fecha de salida del viaje
        return resultado.OrderBy(x => x.FechaHoraSalida).ToList();
    }

    public async Task<bool> CancelarReservaAsync(int idUsuario, int idReserva)
    {
        var reserva = await _context.ReservasTrayecto
            .Include(r => r.Trayecto)
            .FirstOrDefaultAsync(r => r.Id == idReserva && r.PasajeroUsuarioId == idUsuario && r.Estado == "Confirmada");

        if (reserva == null)
        {
            return false;
        }

        reserva.Estado = "Cancelada";

        var trayecto = reserva.Trayecto;

        // Comprobar si hay alumnos esperando en la cola FIFO para este trayecto
        var siguienteEnCola = await _context.ColasEspera
            .Where(c => c.TrayectoId == reserva.TrayectoId && c.Estado == "EnEspera")
            .OrderBy(c => c.Posicion)
            .ThenBy(c => c.Id)
            .FirstOrDefaultAsync();

        if (siguienteEnCola != null && trayecto.FechaHoraSalida > DateTime.UtcNow && trayecto.Estado != "Cancelado")
        {
            // Promocionar al primer alumno de la lista de espera
            siguienteEnCola.Estado = "Promocionado";

            var nuevaReserva = new ReservaTrayecto
            {
                TrayectoId = reserva.TrayectoId,
                PasajeroUsuarioId = siguienteEnCola.UsuarioTajamarId,
                FechaReserva = DateTime.UtcNow,
                Estado = "Confirmada"
            };
            _context.ReservasTrayecto.Add(nuevaReserva);

            // Reajustar las posiciones de los alumnos restantes en la cola
            var restantesEnCola = await _context.ColasEspera
                .Where(c => c.TrayectoId == reserva.TrayectoId && c.Estado == "EnEspera" && c.Posicion > siguienteEnCola.Posicion)
                .ToListAsync();

            foreach (var r in restantesEnCola)
            {
                r.Posicion -= 1;
            }
        }
        else
        {
            // Si no hay nadie en cola, liberar la plaza al inventario
            trayecto.PlazasDisponibles += 1;
            if (trayecto.Estado == "Completo")
            {
                trayecto.Estado = "Activo";
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelarColaAsync(int idUsuario, int idCola)
    {
        var entradaCola = await _context.ColasEspera
            .FirstOrDefaultAsync(c => c.Id == idCola && c.UsuarioTajamarId == idUsuario && c.Estado == "EnEspera");

        if (entradaCola == null)
        {
            return false;
        }

        entradaCola.Estado = "Cancelado";
        var posicionCancelada = entradaCola.Posicion;
        var trayectoId = entradaCola.TrayectoId;

        // Reajustar posiciones de quienes estaban detrás en la cola FIFO
        var siguientes = await _context.ColasEspera
            .Where(c => c.TrayectoId == trayectoId && c.Estado == "EnEspera" && c.Posicion > posicionCancelada)
            .ToListAsync();

        foreach (var entrada in siguientes)
        {
            entrada.Posicion -= 1;
        }

        await _context.SaveChangesAsync();
        return true;
    }
}
