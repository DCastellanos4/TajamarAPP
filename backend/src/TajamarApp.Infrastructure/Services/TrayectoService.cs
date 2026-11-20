using Microsoft.EntityFrameworkCore;
using TajamarApp.Core.DTOs.Trayectos;
using TajamarApp.Core.Entities;
using TajamarApp.Core.Interfaces;
using TajamarApp.Infrastructure.Data;

namespace TajamarApp.Infrastructure.Services;

public class TrayectoService : ITrayectoService
{
    private readonly TajamarAppContext _context;

    public TrayectoService(TajamarAppContext context)
    {
        _context = context;
    }

    public async Task<TrayectoDetalleDto> CrearTrayectoAsync(int idUsuario, CrearTrayectoDto dto)
    {
        // 1. Obtener el perfil extendido del usuario
        var perfil = await _context.PerfilesApp
            .Include(p => p.UsuarioTajamar)
            .FirstOrDefaultAsync(p => p.IdUsuarioTajamar == idUsuario);

        if (perfil == null || !perfil.EsConductor)
        {
            throw new InvalidOperationException("Solo los usuarios con perfil de conductor activo pueden publicar trayectos.");
        }

        // 2. Validar que no ofrezca más plazas que la capacidad de su coche
        if (dto.PlazasTotales > perfil.PlazasCoche)
        {
            throw new ArgumentException($"No puedes ofrecer {dto.PlazasTotales} plazas. Tu vehículo ({perfil.MarcaCoche} {perfil.ModeloCoche}) tiene capacidad máxima de {perfil.PlazasCoche} plazas.");
        }

        // 3. Si se vincula a un evento, verificar que exista
        string? eventoTitulo = null;
        if (dto.EventoId.HasValue)
        {
            var evento = await _context.EventosApp.FindAsync(dto.EventoId.Value);
            if (evento == null)
            {
                throw new ArgumentException($"El evento con ID {dto.EventoId.Value} no existe.");
            }
            eventoTitulo = evento.Titulo;
        }

        // 4. Crear entidad Trayecto
        var trayecto = new Trayecto
        {
            ConductorPerfilId = perfil.Id,
            EventoId = dto.EventoId,
            Origen = dto.Origen.Trim(),
            Destino = string.IsNullOrWhiteSpace(dto.Destino) ? "Tajamar" : dto.Destino.Trim(),
            FechaHoraSalida = dto.FechaHoraSalida,
            PlazasTotales = dto.PlazasTotales,
            PlazasDisponibles = dto.PlazasTotales,
            TipoTrayecto = dto.TipoTrayecto,
            Modalidad = dto.Modalidad,
            DiasSemana = dto.DiasSemana,
            Turno = dto.Turno,
            Estado = "Activo",
            FechaCreacion = DateTime.UtcNow
        };

        await _context.Trayectos.AddAsync(trayecto);
        await _context.SaveChangesAsync();

        return new TrayectoDetalleDto
        {
            Id = trayecto.Id,
            Origen = trayecto.Origen,
            Destino = trayecto.Destino,
            FechaHoraSalida = trayecto.FechaHoraSalida,
            PlazasTotales = trayecto.PlazasTotales,
            PlazasDisponibles = trayecto.PlazasDisponibles,
            TipoTrayecto = trayecto.TipoTrayecto,
            Modalidad = trayecto.Modalidad,
            DiasSemana = trayecto.DiasSemana,
            Turno = trayecto.Turno,
            Estado = trayecto.Estado,
            EventoId = trayecto.EventoId,
            EventoTitulo = eventoTitulo,
            FechaCreacion = trayecto.FechaCreacion,
            Conductor = new ConductorResumenDto
            {
                IdPerfil = perfil.Id,
                IdUsuario = perfil.UsuarioTajamar.IdUsuario,
                Nombre = perfil.UsuarioTajamar.Nombre,
                Apellidos = perfil.UsuarioTajamar.Apellidos,
                Imagen = perfil.UsuarioTajamar.Imagen,
                Telefono = perfil.Telefono,
                MarcaCoche = perfil.MarcaCoche,
                ModeloCoche = perfil.ModeloCoche,
                ColorCoche = perfil.ColorCoche,
                Matricula = perfil.Matricula,
                Bio = perfil.Bio
            }
        };
    }

    public async Task<TrayectoDetalleDto?> GetTrayectoPorIdAsync(int idTrayecto)
    {
        var trayecto = await _context.Trayectos
            .AsNoTracking()
            .Include(t => t.Conductor)
                .ThenInclude(c => c.UsuarioTajamar)
            .Include(t => t.Evento)
            .FirstOrDefaultAsync(t => t.Id == idTrayecto);

        if (trayecto == null)
        {
            return null;
        }

        return new TrayectoDetalleDto
        {
            Id = trayecto.Id,
            Origen = trayecto.Origen,
            Destino = trayecto.Destino,
            FechaHoraSalida = trayecto.FechaHoraSalida,
            PlazasTotales = trayecto.PlazasTotales,
            PlazasDisponibles = trayecto.PlazasDisponibles,
            TipoTrayecto = trayecto.TipoTrayecto,
            Modalidad = trayecto.Modalidad,
            DiasSemana = trayecto.DiasSemana,
            Turno = trayecto.Turno,
            Estado = trayecto.Estado,
            EventoId = trayecto.EventoId,
            EventoTitulo = trayecto.Evento?.Titulo,
            FechaCreacion = trayecto.FechaCreacion,
            Conductor = new ConductorResumenDto
            {
                IdPerfil = trayecto.Conductor.Id,
                IdUsuario = trayecto.Conductor.UsuarioTajamar.IdUsuario,
                Nombre = trayecto.Conductor.UsuarioTajamar.Nombre,
                Apellidos = trayecto.Conductor.UsuarioTajamar.Apellidos,
                Imagen = trayecto.Conductor.UsuarioTajamar.Imagen,
                Telefono = trayecto.Conductor.Telefono,
                MarcaCoche = trayecto.Conductor.MarcaCoche,
                ModeloCoche = trayecto.Conductor.ModeloCoche,
                ColorCoche = trayecto.Conductor.ColorCoche,
                Matricula = trayecto.Conductor.Matricula,
                Bio = trayecto.Conductor.Bio
            }
        };
    }

    public async Task<IEnumerable<TrayectoResumenDto>> GetMisTrayectosAsync(int idUsuario)
    {
        var perfil = await _context.PerfilesApp
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdUsuarioTajamar == idUsuario);

        if (perfil == null)
        {
            return Enumerable.Empty<TrayectoResumenDto>();
        }

        var trayectos = await _context.Trayectos
            .AsNoTracking()
            .Include(t => t.Conductor)
                .ThenInclude(c => c.UsuarioTajamar)
            .Where(t => t.ConductorPerfilId == perfil.Id)
            .OrderByDescending(t => t.FechaHoraSalida)
            .Select(t => new TrayectoResumenDto
            {
                Id = t.Id,
                ConductorNombre = $"{t.Conductor.UsuarioTajamar.Nombre} {t.Conductor.UsuarioTajamar.Apellidos}".Trim(),
                ConductorImagen = t.Conductor.UsuarioTajamar.Imagen,
                Coche = $"{t.Conductor.MarcaCoche} {t.Conductor.ModeloCoche}".Trim(),
                Origen = t.Origen,
                Destino = t.Destino,
                FechaHoraSalida = t.FechaHoraSalida,
                PlazasDisponibles = t.PlazasDisponibles,
                PlazasTotales = t.PlazasTotales,
                TipoTrayecto = t.TipoTrayecto,
                Modalidad = t.Modalidad,
                Turno = t.Turno,
                DiasSemana = t.DiasSemana,
                Estado = t.Estado,
                EventoId = t.EventoId
            })
            .ToListAsync();

        return trayectos;
    }

    public async Task<TrayectoDetalleDto?> EditarTrayectoAsync(int idUsuario, int idTrayecto, EditarTrayectoDto dto)
    {
        var perfil = await _context.PerfilesApp
            .FirstOrDefaultAsync(p => p.IdUsuarioTajamar == idUsuario);

        if (perfil == null)
        {
            throw new UnauthorizedAccessException("Perfil de conductor no encontrado.");
        }

        var trayecto = await _context.Trayectos
            .Include(t => t.Conductor)
                .ThenInclude(c => c.UsuarioTajamar)
            .Include(t => t.Evento)
            .FirstOrDefaultAsync(t => t.Id == idTrayecto);

        if (trayecto == null)
        {
            return null;
        }

        if (trayecto.ConductorPerfilId != perfil.Id)
        {
            throw new UnauthorizedAccessException("No tienes permiso para modificar un trayecto que no es tuyo.");
        }

        if (dto.PlazasTotales > perfil.PlazasCoche)
        {
            throw new ArgumentException($"No puedes ofrecer {dto.PlazasTotales} plazas. La capacidad máxima de tu vehículo es {perfil.PlazasCoche}.");
        }

        // Calcular plazas reservadas actuales
        var plazasReservadas = trayecto.PlazasTotales - trayecto.PlazasDisponibles;
        if (dto.PlazasTotales < plazasReservadas)
        {
            throw new ArgumentException($"No puedes reducir las plazas a {dto.PlazasTotales} porque ya tienes {plazasReservadas} plaza(s) reservada(s).");
        }

        trayecto.Origen = dto.Origen.Trim();
        trayecto.Destino = dto.Destino.Trim();
        trayecto.FechaHoraSalida = dto.FechaHoraSalida;
        trayecto.PlazasTotales = dto.PlazasTotales;
        trayecto.PlazasDisponibles = dto.PlazasTotales - plazasReservadas;
        trayecto.Modalidad = dto.Modalidad;
        trayecto.DiasSemana = dto.DiasSemana;
        trayecto.Turno = dto.Turno;

        await _context.SaveChangesAsync();

        return new TrayectoDetalleDto
        {
            Id = trayecto.Id,
            Origen = trayecto.Origen,
            Destino = trayecto.Destino,
            FechaHoraSalida = trayecto.FechaHoraSalida,
            PlazasTotales = trayecto.PlazasTotales,
            PlazasDisponibles = trayecto.PlazasDisponibles,
            TipoTrayecto = trayecto.TipoTrayecto,
            Modalidad = trayecto.Modalidad,
            DiasSemana = trayecto.DiasSemana,
            Turno = trayecto.Turno,
            Estado = trayecto.Estado,
            EventoId = trayecto.EventoId,
            EventoTitulo = trayecto.Evento?.Titulo,
            FechaCreacion = trayecto.FechaCreacion,
            Conductor = new ConductorResumenDto
            {
                IdPerfil = perfil.Id,
                IdUsuario = perfil.UsuarioTajamar.IdUsuario,
                Nombre = perfil.UsuarioTajamar.Nombre,
                Apellidos = perfil.UsuarioTajamar.Apellidos,
                Imagen = perfil.UsuarioTajamar.Imagen,
                Telefono = perfil.Telefono,
                MarcaCoche = perfil.MarcaCoche,
                ModeloCoche = perfil.ModeloCoche,
                ColorCoche = perfil.ColorCoche,
                Matricula = perfil.Matricula,
                Bio = perfil.Bio
            }
        };
    }

    public async Task<bool> CancelarTrayectoAsync(int idUsuario, int idTrayecto)
    {
        var perfil = await _context.PerfilesApp
            .FirstOrDefaultAsync(p => p.IdUsuarioTajamar == idUsuario);

        if (perfil == null)
        {
            throw new UnauthorizedAccessException("Perfil de conductor no encontrado.");
        }

        var trayecto = await _context.Trayectos.FindAsync(idTrayecto);
        if (trayecto == null)
        {
            return false;
        }

        if (trayecto.ConductorPerfilId != perfil.Id)
        {
            throw new UnauthorizedAccessException("No tienes permiso para cancelar un trayecto que no es tuyo.");
        }

        trayecto.Estado = "Cancelado";
        await _context.SaveChangesAsync();

        return true;
    }
}
