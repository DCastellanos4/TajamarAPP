using Microsoft.EntityFrameworkCore;
using TajamarApp.Core.Domain;
using TajamarApp.Core.DTOs.Valoraciones;
using TajamarApp.Core.Interfaces;
using TajamarApp.Infrastructure.Data;

namespace TajamarApp.Infrastructure.Services;

public class ValoracionService : IValoracionService
{
    private readonly TajamarAppContext _context;

    public ValoracionService(TajamarAppContext context)
    {
        _context = context;
    }

    public async Task<ResenaDto> CrearValoracionAsync(int idUsuarioPasajero, CrearValoracionDto dto)
    {
        if (dto.Puntuacion < 1 || dto.Puntuacion > 5)
        {
            throw new ArgumentException("La puntuación debe ser un número entero entre 1 y 5.");
        }

        var trayecto = await _context.Trayectos
            .Include(t => t.Conductor)
                .ThenInclude(c => c.UsuarioTajamar)
            .FirstOrDefaultAsync(t => t.Id == dto.TrayectoId);

        if (trayecto == null)
        {
            throw new KeyNotFoundException($"El trayecto con ID {dto.TrayectoId} no existe.");
        }

        if (trayecto.Conductor.IdUsuarioTajamar == idUsuarioPasajero)
        {
            throw new InvalidOperationException("No puedes valorar tu propio trayecto como conductor.");
        }

        var tieneReservaConfirmada = await _context.ReservasTrayecto
            .AnyAsync(r => r.TrayectoId == dto.TrayectoId &&
                           r.PasajeroUsuarioId == idUsuarioPasajero &&
                           r.Estado == "Confirmada");

        if (!tieneReservaConfirmada)
        {
            throw new InvalidOperationException("Solo los pasajeros con reserva confirmada en este trayecto pueden emitir una valoración.");
        }

        var yaValoro = await _context.ValoracionesApp
            .AnyAsync(v => v.TrayectoId == dto.TrayectoId && v.PasajeroUsuarioId == idUsuarioPasajero);

        if (yaValoro)
        {
            throw new InvalidOperationException("Ya has emitido una valoración para este trayecto.");
        }

        var valoracion = new ValoracionApp
        {
            TrayectoId = dto.TrayectoId,
            ConductorPerfilId = trayecto.ConductorPerfilId,
            PasajeroUsuarioId = idUsuarioPasajero,
            Puntuacion = dto.Puntuacion,
            Comentario = string.IsNullOrWhiteSpace(dto.Comentario) ? null : dto.Comentario.Trim(),
            Tags = string.IsNullOrWhiteSpace(dto.Tags) ? null : dto.Tags.Trim(),
            FechaCreacion = DateTime.UtcNow
        };

        await _context.ValoracionesApp.AddAsync(valoracion);
        await _context.SaveChangesAsync();

        var pasajero = await _context.UsuariosTajamar.FindAsync(idUsuarioPasajero);

        return new ResenaDto
        {
            Id = valoracion.Id,
            TrayectoId = valoracion.TrayectoId,
            ConductorPerfilId = valoracion.ConductorPerfilId,
            PasajeroUsuarioId = idUsuarioPasajero,
            PasajeroNombre = pasajero != null ? $"{pasajero.Nombre} {pasajero.Apellidos}".Trim() : string.Empty,
            PasajeroFoto = pasajero?.Imagen,
            Puntuacion = valoracion.Puntuacion,
            Comentario = valoracion.Comentario,
            Tags = valoracion.Tags,
            FechaCreacion = valoracion.FechaCreacion,
            OrigenTrayecto = trayecto.Origen,
            DestinoTrayecto = trayecto.Destino
        };
    }

    public async Task<ResumenResenasConductorDto> GetResenasConductorAsync(int idUsuarioConductor)
    {
        var perfil = await _context.PerfilesApp
            .Include(p => p.UsuarioTajamar)
            .FirstOrDefaultAsync(p => p.IdUsuarioTajamar == idUsuarioConductor || p.Id == idUsuarioConductor);

        if (perfil == null)
        {
            throw new KeyNotFoundException($"No se encontró perfil de conductor asociado al identificador {idUsuarioConductor}.");
        }

        var valoraciones = await _context.ValoracionesApp
            .AsNoTracking()
            .Include(v => v.Pasajero)
            .Include(v => v.Trayecto)
            .Where(v => v.ConductorPerfilId == perfil.Id)
            .OrderByDescending(v => v.FechaCreacion)
            .ToListAsync();

        var desglose = new Dictionary<int, int>
        {
            { 1, 0 },
            { 2, 0 },
            { 3, 0 },
            { 4, 0 },
            { 5, 0 }
        };

        foreach (var v in valoraciones)
        {
            if (desglose.ContainsKey(v.Puntuacion))
            {
                desglose[v.Puntuacion]++;
            }
        }

        var total = valoraciones.Count;
        var media = total > 0 ? Math.Round((decimal)valoraciones.Average(v => v.Puntuacion), 1) : 0.0m;

        var resenas = valoraciones.Select(v => new ResenaDto
        {
            Id = v.Id,
            TrayectoId = v.TrayectoId,
            ConductorPerfilId = v.ConductorPerfilId,
            PasajeroUsuarioId = v.PasajeroUsuarioId,
            PasajeroNombre = v.Pasajero != null ? $"{v.Pasajero.Nombre} {v.Pasajero.Apellidos}".Trim() : string.Empty,
            PasajeroFoto = v.Pasajero?.Imagen,
            Puntuacion = v.Puntuacion,
            Comentario = v.Comentario,
            Tags = v.Tags,
            FechaCreacion = v.FechaCreacion,
            OrigenTrayecto = v.Trayecto?.Origen,
            DestinoTrayecto = v.Trayecto?.Destino
        }).ToList();

        return new ResumenResenasConductorDto
        {
            ConductorIdUsuario = perfil.UsuarioTajamar.IdUsuario,
            ConductorPerfilId = perfil.Id,
            ConductorNombre = $"{perfil.UsuarioTajamar.Nombre} {perfil.UsuarioTajamar.Apellidos}".Trim(),
            ConductorFoto = perfil.UsuarioTajamar.Imagen,
            PuntuacionMedia = media,
            TotalResenas = total,
            DesgloseEstrellas = desglose,
            Resenas = resenas
        };
    }
}
