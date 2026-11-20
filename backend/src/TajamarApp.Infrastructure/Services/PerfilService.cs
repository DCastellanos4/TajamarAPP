using Microsoft.EntityFrameworkCore;
using TajamarApp.Core.DTOs.Auth;
using TajamarApp.Core.DTOs.Perfil;
using TajamarApp.Core.Entities;
using TajamarApp.Core.Interfaces;
using TajamarApp.Infrastructure.Data;

namespace TajamarApp.Infrastructure.Services;

public class PerfilService : IPerfilService
{
    private readonly TajamarAppContext _context;

    public PerfilService(TajamarAppContext context)
    {
        _context = context;
    }

    public async Task<PerfilMeResponseDto?> GetMiPerfilAsync(int idUsuario)
    {
        var usuario = await _context.UsuariosTajamar
            .AsNoTracking()
            .Include(u => u.Rol)
            .Include(u => u.PerfilApp)
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

        if (usuario == null)
        {
            return null;
        }

        var rolNombre = usuario.Rol?.Role ?? "ALUMNO";
        var perfilApp = usuario.PerfilApp;

        return new PerfilMeResponseDto
        {
            Usuario = new UsuarioBasicoDto
            {
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                Apellidos = usuario.Apellidos,
                Email = usuario.Email,
                Imagen = usuario.Imagen,
                Rol = rolNombre
            },
            PerfilCompletado = perfilApp != null,
            PerfilApp = perfilApp == null ? null : new PerfilAppDto
            {
                Telefono = perfilApp.Telefono,
                EsConductor = perfilApp.EsConductor,
                MarcaCoche = perfilApp.MarcaCoche,
                ModeloCoche = perfilApp.ModeloCoche,
                ColorCoche = perfilApp.ColorCoche,
                Matricula = perfilApp.Matricula,
                PlazasCoche = perfilApp.PlazasCoche,
                Bio = perfilApp.Bio
            }
        };
    }

    public async Task<PerfilAppDto> GuardarPerfilAsync(int idUsuario, PerfilAppDto dto)
    {
        var perfil = await _context.PerfilesApp
            .FirstOrDefaultAsync(p => p.IdUsuarioTajamar == idUsuario);

        if (perfil == null)
        {
            // Alta de nuevo perfil extendido (Onboarding)
            perfil = new PerfilApp
            {
                IdUsuarioTajamar = idUsuario,
                Telefono = dto.Telefono,
                EsConductor = dto.EsConductor,
                MarcaCoche = dto.EsConductor ? dto.MarcaCoche : null,
                ModeloCoche = dto.EsConductor ? dto.ModeloCoche : null,
                ColorCoche = dto.EsConductor ? dto.ColorCoche : null,
                Matricula = dto.EsConductor ? dto.Matricula : null,
                PlazasCoche = dto.EsConductor ? dto.PlazasCoche : 0,
                Bio = dto.Bio,
                FechaCreacion = DateTime.UtcNow
            };

            await _context.PerfilesApp.AddAsync(perfil);
        }
        else
        {
            // Actualización de perfil existente
            perfil.Telefono = dto.Telefono;
            perfil.EsConductor = dto.EsConductor;
            perfil.MarcaCoche = dto.EsConductor ? dto.MarcaCoche : null;
            perfil.ModeloCoche = dto.EsConductor ? dto.ModeloCoche : null;
            perfil.ColorCoche = dto.EsConductor ? dto.ColorCoche : null;
            perfil.Matricula = dto.EsConductor ? dto.Matricula : null;
            perfil.PlazasCoche = dto.EsConductor ? dto.PlazasCoche : 0;
            perfil.Bio = dto.Bio;
        }

        await _context.SaveChangesAsync();

        return new PerfilAppDto
        {
            Telefono = perfil.Telefono,
            EsConductor = perfil.EsConductor,
            MarcaCoche = perfil.MarcaCoche,
            ModeloCoche = perfil.ModeloCoche,
            ColorCoche = perfil.ColorCoche,
            Matricula = perfil.Matricula,
            PlazasCoche = perfil.PlazasCoche,
            Bio = perfil.Bio
        };
    }
}
