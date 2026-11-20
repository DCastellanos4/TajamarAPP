# 🛠️ BE-D01: Setup de Arquitectura .NET 10 y Base de Datos (Actualizado)

## 📌 Resumen de la Tarea
- **Responsable:** David (Back)
- **Estado:** ✅ Completada y actualizada con datos oficiales de Paco
- **Framework:** .NET 10 (SDK 10.0.301)
- **ORM:** Entity Framework Core 10.0.0
- **Base de Datos:** Azure SQL Database (`sqldavidc3597.database.windows.net / AZURETAJAMAR`)
- **Arquitectura:** 100% Autónoma y autosuficiente (sin dependencias de la API externa de Paco).

---

## 🏛️ Estructura de Proyectos y Capas (Clean Architecture)

```
backend/
├── TajamarApp.slnx                     # Solución .NET 10
└── src/
    ├── TajamarApp.Core/                # Dominio y Entidades
    │   └── Entities/
    │       ├── RolTajamar.cs           # Tabla ROLESCHARLASTAJAMAR (Oficial Paco)
    │       ├── UsuarioTajamar.cs       # Tabla USUARIOSTAJAMAR (Oficial Paco)
    │       ├── CursoTajamar.cs         # Tabla CURSOSTAJAMAR (Oficial Paco)
    │       ├── CursoUsuarioTajamar.cs  # Tabla CURSOSUSUARIOSTAJAMAR (Oficial Paco)
    │       ├── PerfilApp.cs            # Tabla PERFIL_APP (Extensión conductor/vehículo)
    │       ├── EventoApp.cs            # Tabla EVENTOS_APP (Charlas y eventos Tajamar)
    │       ├── EventoAsistente.cs      # Tabla EVENTO_ASISTENTES (Inscripción alumnos)
    │       ├── Trayecto.cs             # Tabla TRAYECTOS (Viajes habituales y de eventos)
    │       ├── ReservaTrayecto.cs      # Tabla RESERVAS_TRAYECTO (Plazas reservadas)
    │       └── ColaEspera.cs           # Tabla COLA_ESPERA (Lista de espera FIFO)
    │
    ├── TajamarApp.Infrastructure/      # Acceso a datos y EF Core
    │   └── Data/
    │       └── TajamarAppContext.cs    # Mapeo Fluent API, índices y relaciones
    │
    └── TajamarApp.API/                 # ASP.NET Core Web API
        ├── Controllers/
        │   ├── CursosController.cs     # Endpoints de cursos oficiales
        │   ├── UsuariosController.cs   # Listado y detalle de alumnos/profesores
        │   └── HealthController.cs     # Chequeo de conexión y estado Azure SQL
        ├── appsettings.json            # Conexión Azure SQL
        └── Program.cs                  # Pipeline HTTP, CORS y Swagger
```

---

## 🗄️ Datos Oficiales Cargados en Azure SQL (`AZURETAJAMAR`)

La base de datos cuenta con el seed oficial provisto por Paco:
1. **Roles:** `1: PROFESOR`, `2: ALUMNO`, `3: ADMINISTRADOR`.
2. **Cursos:** ID `3597` (*Master Desarrollo 2026-2027*).
3. **Usuarios Oficiales (29 alumnos/profesores):**
   * David Castellanos (ID 212)
   * Mario Mingorance (ID 199)
   * Mónica Carmena (ID 220)
   * Harry Floresca (ID 206)
   * *Y los 25 compañeros restantes con sus avatares oficiales y emails @tajamar365.com.*
   * Clave por defecto para todos: `12345` (`OFi9W3uf5kGsaxyTqF1ZFQ==`).
4. **Matrículas:** Los 29 alumnos vinculados al curso 3597.

---

## 🌐 Endpoints Disponibles en Swagger (`/swagger`)

| Controlador | Método | Ruta | Descripción |
|:---|:---:|:---|:---|
| **Cursos** | `GET` | `/api/cursos` | Lista el curso oficial (Master Desarrollo 2026-2027) |
| **Cursos** | `GET` | `/api/cursos/{id}` | Detalle del curso por ID |
| **Usuarios** | `GET` | `/api/usuarios` | Lista los 29 alumnos/profesores de Tajamar con sus roles |
| **Usuarios** | `GET` | `/api/usuarios/{id}` | Detalle de un usuario específico y su `PerfilApp` |
| **Usuarios** | `GET` | `/api/usuarios/curso/{idCurso}` | Lista los alumnos matriculados en un curso |
| **Health** | `GET` | `/api/health` | Estado de conexión y conteo de tablas en Azure SQL |
