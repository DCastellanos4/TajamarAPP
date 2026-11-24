using Microsoft.EntityFrameworkCore;
using TajamarApp.Core.Domain;
using TajamarApp.Core.Entities;

namespace TajamarApp.Infrastructure.Data;

public class TajamarAppContext : DbContext
{
    public TajamarAppContext(DbContextOptions<TajamarAppContext> options) : base(options)
    {
    }

    // --- TABLAS TAJAMAR (ESQUEMA PACO) ---
    public DbSet<RolTajamar> RolesTajamar => Set<RolTajamar>();
    public DbSet<UsuarioTajamar> UsuariosTajamar => Set<UsuarioTajamar>();
    public DbSet<CursoTajamar> CursosTajamar => Set<CursoTajamar>();
    public DbSet<CursoUsuarioTajamar> CursosUsuariosTajamar => Set<CursoUsuarioTajamar>();

    // --- TABLAS PROPIAS DE LA APP ---
    public DbSet<PerfilApp> PerfilesApp => Set<PerfilApp>();
    public DbSet<EventoApp> EventosApp => Set<EventoApp>();
    public DbSet<EventoAsistente> EventosAsistentes => Set<EventoAsistente>();
    public DbSet<Trayecto> Trayectos => Set<Trayecto>();
    public DbSet<ReservaTrayecto> ReservasTrayecto => Set<ReservaTrayecto>();
    public DbSet<ColaEspera> ColasEspera => Set<ColaEspera>();
    public DbSet<ValoracionApp> ValoracionesApp => Set<ValoracionApp>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- TABLAS TAJAMAR (ESQUEMA PACO) ---
        modelBuilder.Entity<RolTajamar>(entity =>
        {
            entity.ToTable("ROLESCHARLASTAJAMAR");
            entity.HasKey(e => e.IdRole);
            entity.Property(e => e.IdRole).HasColumnName("IDROLE").ValueGeneratedNever();
            entity.Property(e => e.Role).HasColumnName("ROLE").HasMaxLength(50);
        });

        modelBuilder.Entity<UsuarioTajamar>(entity =>
        {
            entity.ToTable("USUARIOSTAJAMAR");
            entity.HasKey(e => e.IdUsuario);
            entity.HasIndex(e => e.Email).IsUnique().HasDatabaseName("u_mailuser");

            entity.Property(e => e.IdUsuario).HasColumnName("IDUSUARIO");
            entity.Property(e => e.Nombre).HasColumnName("NOMBRE").HasMaxLength(70);
            entity.Property(e => e.Apellidos).HasColumnName("APELLIDOS").HasMaxLength(70);
            entity.Property(e => e.Email).HasColumnName("EMAIL").HasMaxLength(70);
            entity.Property(e => e.Estado).HasColumnName("ESTADO");
            entity.Property(e => e.Imagen).HasColumnName("IMAGEN").HasMaxLength(600);
            entity.Property(e => e.Password).HasColumnName("PASSWORD").HasMaxLength(100);
            entity.Property(e => e.IdRole).HasColumnName("IDROLE");

            entity.HasOne(d => d.Rol)
                .WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRole)
                .HasConstraintName("FK_USUARIOSTAJAMAR_ROLESCHARLASTAJAMAR");
        });

        modelBuilder.Entity<CursoTajamar>(entity =>
        {
            entity.ToTable("CURSOSTAJAMAR");
            entity.HasKey(e => e.IdCurso);
            entity.Property(e => e.IdCurso).HasColumnName("IDCURSO");
            entity.Property(e => e.Nombre).HasColumnName("NOMBRE").HasMaxLength(150);
            entity.Property(e => e.FechaInicio).HasColumnName("FECHAINICIO");
            entity.Property(e => e.FechaFin).HasColumnName("FECHAFIN");
            entity.Property(e => e.Activo).HasColumnName("ACTIVO");
        });

        modelBuilder.Entity<CursoUsuarioTajamar>(entity =>
        {
            entity.ToTable("CURSOSUSUARIOSTAJAMAR");
            entity.HasKey(e => e.IdCursosUsuarios);
            entity.HasIndex(e => new { e.IdCurso, e.IdUsuario }).IsUnique().HasDatabaseName("u_idcurso_idusuario");

            entity.Property(e => e.IdCursosUsuarios).HasColumnName("IDCURSOSUSUARIOS");
            entity.Property(e => e.IdCurso).HasColumnName("IDCURSO");
            entity.Property(e => e.IdUsuario).HasColumnName("IDUSUARIO");

            entity.HasOne(d => d.Curso)
                .WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdCurso)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_CURSOSUSUARIOSTAJAMAR_CURSOSTAJAMAR");

            entity.HasOne(d => d.Usuario)
                .WithMany(p => p.Cursos)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_CURSOSUSUARIOSTAJAMAR_USUARIOSTAJAMAR");
        });

        // --- TABLAS PROPIAS DE LA APP ---
        modelBuilder.Entity<PerfilApp>(entity =>
        {
            entity.ToTable("PERFIL_APP");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.IdUsuarioTajamar).IsUnique();

            entity.Property(e => e.Telefono).HasMaxLength(20);
            entity.Property(e => e.MarcaCoche).HasMaxLength(50);
            entity.Property(e => e.ModeloCoche).HasMaxLength(50);
            entity.Property(e => e.ColorCoche).HasMaxLength(30);
            entity.Property(e => e.Matricula).HasMaxLength(15);
            entity.Property(e => e.Bio).HasMaxLength(500);

            entity.HasOne(d => d.UsuarioTajamar)
                .WithOne(p => p.PerfilApp)
                .HasForeignKey<PerfilApp>(d => d.IdUsuarioTajamar)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EventoApp>(entity =>
        {
            entity.ToTable("EVENTOS_APP");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Titulo).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Descripcion).HasMaxLength(2000);
            entity.Property(e => e.Ubicacion).HasMaxLength(200);
            entity.Property(e => e.Imagen).HasMaxLength(600);

            entity.HasOne(d => d.Creador)
                .WithMany()
                .HasForeignKey(d => d.CreadorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EventoAsistente>(entity =>
        {
            entity.ToTable("EVENTO_ASISTENTES");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EventoId, e.UsuarioTajamarId }).IsUnique();

            entity.HasOne(d => d.Evento)
                .WithMany(p => p.Asistentes)
                .HasForeignKey(d => d.EventoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Usuario)
                .WithMany(p => p.AsistenciasEventos)
                .HasForeignKey(d => d.UsuarioTajamarId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Trayecto>(entity =>
        {
            entity.ToTable("TRAYECTOS");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Origen).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Destino).HasMaxLength(150).IsRequired();
            entity.Property(e => e.TipoTrayecto).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Modalidad).HasMaxLength(20).IsRequired();
            entity.Property(e => e.DiasSemana).HasMaxLength(50);
            entity.Property(e => e.Turno).HasMaxLength(20);
            entity.Property(e => e.Estado).HasMaxLength(20).IsRequired();

            entity.HasOne(d => d.Conductor)
                .WithMany(p => p.TrayectosPublicados)
                .HasForeignKey(d => d.ConductorPerfilId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Evento)
                .WithMany(p => p.Trayectos)
                .HasForeignKey(d => d.EventoId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ReservaTrayecto>(entity =>
        {
            entity.ToTable("RESERVAS_TRAYECTO");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Estado).HasMaxLength(20).IsRequired();

            entity.HasOne(d => d.Trayecto)
                .WithMany(p => p.Reservas)
                .HasForeignKey(d => d.TrayectoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Pasajero)
                .WithMany(p => p.Reservas)
                .HasForeignKey(d => d.PasajeroUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ColaEspera>(entity =>
        {
            entity.ToTable("COLA_ESPERA");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Estado).HasMaxLength(20).IsRequired();

            entity.HasOne(d => d.Trayecto)
                .WithMany(p => p.ColaEspera)
                .HasForeignKey(d => d.TrayectoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Usuario)
                .WithMany(p => p.ColasEspera)
                .HasForeignKey(d => d.UsuarioTajamarId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ValoracionApp>(entity =>
        {
            entity.ToTable("VALORACIONES_APP");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TrayectoId, e.PasajeroUsuarioId }).IsUnique().HasDatabaseName("UQ_Valoracion_Trayecto_Pasajero");

            entity.Property(e => e.Puntuacion).IsRequired();
            entity.Property(e => e.Comentario).HasMaxLength(500);
            entity.Property(e => e.Tags).HasMaxLength(250);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasOne(d => d.Trayecto)
                .WithMany()
                .HasForeignKey(d => d.TrayectoId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_VALORACIONES_TRAYECTO");

            entity.HasOne(d => d.Conductor)
                .WithMany()
                .HasForeignKey(d => d.ConductorPerfilId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_VALORACIONES_CONDUCTOR");

            entity.HasOne(d => d.Pasajero)
                .WithMany()
                .HasForeignKey(d => d.PasajeroUsuarioId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_VALORACIONES_PASAJERO");
        });
    }
}
