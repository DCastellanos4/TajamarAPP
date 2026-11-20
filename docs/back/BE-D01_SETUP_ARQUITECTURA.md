# 🛠️ BE-D01: Setup de Arquitectura .NET 10 y Base de Datos

## 📌 Resumen de la Tarea
- **Responsable:** David (Back)
- **Estado:** ✅ Completada
- **Framework:** .NET 10 (SDK 10.0.301)
- **ORM:** Entity Framework Core 10.0.0
- **Base de Datos:** Azure SQL Database (`sqldavidc3597.database.windows.net / AZURETAJAMAR`)

---

## 🏛️ Estructura de Proyectos y Capas (Clean Architecture)

```
backend/
├── TajamarApp.slnx                     # Solución .NET 10
└── src/
    ├── TajamarApp.Core/                # Dominio y Entidades (sin dependencias externas)
    │   └── Entities/
    │       ├── UsuarioTajamar.cs       # Tabla USUARIOSTAJAMAR (Paco)
    │       ├── CursoTajamar.cs         # Tabla CURSOSTAJAMAR (Paco)
    │       ├── CursoUsuarioTajamar.cs  # Tabla CURSOSUSUARIOSTAJAMAR (Paco)
    │       ├── PerfilApp.cs            # Tabla PERFIL_APP (Extensión conductor/vehículo)
    │       ├── EventoApp.cs            # Tabla EVENTOS_APP (Charlas y eventos Tajamar)
    │       ├── EventoAsistente.cs      # Tabla EVENTO_ASISTENTES (Inscripción alumnos)
    │       ├── Trayecto.cs             # Tabla TRAYECTOS (Viajes habituales y de eventos)
    │       ├── ReservaTrayecto.cs      # Tabla RESERVAS_TRAYECTO (Plazas reservadas)
    │       └── ColaEspera.cs           # Tabla COLA_ESPERA (Lista de espera FIFO)
    │
    ├── TajamarApp.Infrastructure/      # Acceso a datos y EF Core
    │   ├── Data/
    │   │   └── TajamarAppContext.cs    # Configuración Fluent API y DbSets
    │   └── Migrations/                 # Historial de migraciones
    │       └── 20261120093240_InitialCreate.cs # Migración única inicial
    │
    └── TajamarApp.API/                 # ASP.NET Core Web API
        ├── Controllers/                # Controladores REST
        ├── appsettings.json            # Cadena de conexión Azure SQL Server
        └── Program.cs                  # Inyección de dependencias, CORS y Swagger
```

---

## 🗄️ Modelo de Base de Datos y Migración Única

Cumpliendo el requisito de **minimizar al máximo el número de migraciones**, se ha diseñado el modelo completo del sistema en la primera migración `InitialCreate`, creando las siguientes tablas en Azure SQL:

| Tabla | Propósito | Clave Primaria | Relaciones Clave |
|:---|:---|:---|:---|
| `USUARIOSTAJAMAR` | Usuarios y auth base (Paco) | `IDUSUARIO` (Identity) | 1:1 con `PERFIL_APP` |
| `CURSOSTAJAMAR` | Cursos lectivos de Tajamar | `IDCURSO` (Identity) | M:N con `USUARIOSTAJAMAR` |
| `CURSOSUSUARIOSTAJAMAR` | Tabla pivote alumnos-cursos | `IDCURSOSUSUARIOS` | FK a Curso y Usuario |
| `PERFIL_APP` | Extensión perfil de movilidad | `Id` (Identity) | FK única a `USUARIOSTAJAMAR` |
| `EVENTOS_APP` | Eventos y actividades Tajamar | `Id` (Identity) | FK a Creador (`USUARIOSTAJAMAR`) |
| `EVENTO_ASISTENTES` | Alumnos apuntados a eventos | `Id` (Identity) | FK a Evento y Usuario |
| `TRAYECTOS` | Viajes compartidos (rutina/eventos) | `Id` (Identity) | FK a Conductor (`PERFIL_APP`) y Evento (nullable) |
| `RESERVAS_TRAYECTO` | Plazas confirmadas | `Id` (Identity) | FK a Trayecto y Pasajero |
| `COLA_ESPERA` | Lista de espera FIFO | `Id` (Identity) | FK a Trayecto y Usuario |

---

## 🚀 Cómo Ejecutar el Backend en Local

1. Situarse en la carpeta de la API:
   ```bash
   cd backend/src/TajamarApp.API
   ```
2. Ejecutar el proyecto:
   ```bash
   dotnet run
   ```
3. Abrir Swagger en el navegador:
   ```
   https://localhost:7xxx/swagger
   ```
