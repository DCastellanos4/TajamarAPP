# ⭐ BE-D09: Módulo de Valoraciones, Puntuación Media y Reseñas (1 a 5 ⭐)

## 📌 Resumen de la Tarea
- **Responsables:** David (Back) y Mario (Back)
- **Estado:** ✅ Completada y Verificada en Vivo
- **GitHub Issue:** [#35](https://github.com/DCastellanos4/TajamarAPP/issues/35)
- **Objetivo:** Implementar un sistema de reputación comunitaria y feedback seguro para Tajamar App, permitiendo a los alumnos que han viajado como pasajeros con reserva confirmada calificar a sus conductores con una puntuación entre 1 y 5 estrellas, comentarios detallados y etiquetas descriptivas (*tags* como *"Puntual"*, *"ConducciónSegura"*, *"BuenaMúsica"*). Además, expone endpoints para consultar el perfil de reseñas del conductor con desglose por estrellas y enriquecer las tarjetas de trayectos con el promedio visible.

---

## 🗄️ Modelo de Datos y Persistencia

### 1. Script SQL Azure (`dbo.VALORACIONES_APP`)
Se definió y ejecutó el script `backend/sql/003-valoraciones-app.sql` sobre Azure SQL Server (`AZURETAJAMAR`), aplicando integridad referencial, índices únicos para evitar valoraciones duplicadas y restricciones de comprobación:

```sql
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'VALORACIONES_APP' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.VALORACIONES_APP (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        TrayectoId INT NOT NULL,
        ConductorPerfilId INT NOT NULL,
        PasajeroUsuarioId INT NOT NULL,
        Puntuacion INT NOT NULL CHECK (Puntuacion BETWEEN 1 AND 5),
        Comentario NVARCHAR(500) NULL,
        Tags NVARCHAR(250) NULL,
        FechaCreacion DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_Valoracion_Trayecto_Pasajero UNIQUE (TrayectoId, PasajeroUsuarioId),
        CONSTRAINT FK_VALORACIONES_TRAYECTO FOREIGN KEY (TrayectoId) REFERENCES dbo.TRAYECTOS(Id) ON DELETE CASCADE,
        CONSTRAINT FK_VALORACIONES_CONDUCTOR FOREIGN KEY (ConductorPerfilId) REFERENCES dbo.PERFIL_APP(Id),
        CONSTRAINT FK_VALORACIONES_PASAJERO FOREIGN KEY (PasajeroUsuarioId) REFERENCES dbo.USUARIOSTAJAMAR(IDUSUARIO)
    );
END
GO

IF EXISTS (SELECT * FROM sys.database_principals WHERE name = 'tajamar_mario_api')
BEGIN
    GRANT SELECT, INSERT, UPDATE, DELETE ON OBJECT::dbo.VALORACIONES_APP TO [tajamar_mario_api];
END
GO
```

### 2. Entidad de Dominio C# (`TajamarApp.Core/Domain/ValoracionApp.cs`)
```csharp
using TajamarApp.Core.Entities;

namespace TajamarApp.Core.Domain;

public class ValoracionApp
{
    public int Id { get; set; }
    public int TrayectoId { get; set; }
    public int ConductorPerfilId { get; set; }
    public int PasajeroUsuarioId { get; set; }
    public int Puntuacion { get; set; }
    public string? Comentario { get; set; }
    public string? Tags { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Relaciones de navegación
    public Trayecto Trayecto { get; set; } = null!;
    public PerfilApp Conductor { get; set; } = null!;
    public UsuarioTajamar Pasajero { get; set; } = null!;
}
```

---

## 🌐 Endpoints REST Implementados

El controlador `ValoracionesController` se encuentra bajo la etiqueta Swagger `"Valoraciones"`.

### 1. Emitir Valoración (`POST /api/valoraciones`)
* **Ruta:** `POST /api/valoraciones`
* **Autenticación:** Requiere `Bearer Token` (`[Authorize]`)
* **Descripción:** Permite a un alumno autenticado calificar al conductor del viaje tras confirmarse su plaza. El ID del pasajero se extrae directamente de los claims seguros del JWT (`idUsuarioTajamar` o `NameIdentifier`).

#### Contrato de Entrada (`CrearValoracionDto`):
```json
{
  "trayectoId": 4,
  "puntuacion": 5,
  "comentario": "Excelente viaje con David, muy puntual y conducción impecable.",
  "tags": "Puntual,ConducciónSegura,BuenaMúsica"
}
```

#### Contrato de Salida `201 Created` (`ResenaDto`):
```json
{
  "id": 5,
  "trayectoId": 4,
  "conductorPerfilId": 1,
  "pasajeroUsuarioId": 195,
  "pasajeroNombre": "Sara Ulgazi El Yaghmouri",
  "pasajeroFoto": "https://apicharlasalumnotajamar.azurewebsites.net/images/users/195_user.jpg",
  "puntuacion": 5,
  "comentario": "Excelente viaje con David, muy puntual y conducción impecable.",
  "tags": "Puntual,ConducciónSegura,BuenaMúsica",
  "fechaCreacion": "2026-11-24T10:28:14.7439135Z",
  "origenTrayecto": "Vallecas Central",
  "destinoTrayecto": "Tajamar"
}
```

#### Respuestas de Error:
* **`400 Bad Request`**:
  * Puntuación fuera del rango: `"La puntuación debe ser un número entero entre 1 y 5."`
  * Autovaloración de conductor: `"No puedes valorar tu propio trayecto como conductor."`
  * Sin plaza confirmada: `"Solo los pasajeros con reserva confirmada en este trayecto pueden emitir una valoración."`
  * Duplicado: `"Ya has emitido una valoración para este trayecto."`
* **`401 Unauthorized`**: Token JWT ausente o inválido.
* **`404 Not Found`**: El trayecto especificado no existe.

---

### 2. Consultar Resumen y Reseñas de un Conductor (`GET /api/conductores/{id}/resenas`)
* **Ruta:** `GET /api/conductores/{id}/resenas`
* **Acceso:** Público / Autenticado
* **Parámetros:** `{id}` (identificador numérico; el servicio resuelve tanto `IdUsuarioTajamar` como `IdPerfilApp`).
* **Descripción:** Retorna el perfil del conductor, la puntuación media redondeada a 1 decimal, el total acumulado de reseñas, el desglose por niveles de estrellas (1★ a 5★) y el listado cronológico de reseñas con fotos y nombres de los pasajeros.

#### Contrato de Salida `200 OK` (`ResumenResenasConductorDto`):
```json
{
  "conductorIdUsuario": 212,
  "conductorPerfilId": 1,
  "conductorNombre": "David Castellanos",
  "conductorFoto": "https://apicharlasalumnotajamar.azurewebsites.net/images/users/212_user.jpg",
  "puntuacionMedia": 4.5,
  "totalResenas": 2,
  "desgloseEstrellas": {
    "1": 0,
    "2": 0,
    "3": 0,
    "4": 1,
    "5": 1
  },
  "resenas": [
    {
      "id": 6,
      "trayectoId": 4,
      "conductorPerfilId": 1,
      "pasajeroUsuarioId": 194,
      "pasajeroNombre": "Matheus Ferreira De Carvalho",
      "pasajeroFoto": "https://apicharlasalumnotajamar.azurewebsites.net/images/users/194_user.jpg",
      "puntuacion": 4,
      "comentario": "Muy buen trayecto, puntual y buen conductor.",
      "tags": "Puntual,Recomendable",
      "fechaCreacion": "2026-11-24T10:28:15.2979096Z",
      "origenTrayecto": "Vallecas Central",
      "destinoTrayecto": "Tajamar"
    },
    {
      "id": 5,
      "trayectoId": 4,
      "conductorPerfilId": 1,
      "pasajeroUsuarioId": 195,
      "pasajeroNombre": "Sara Ulgazi El Yaghmouri",
      "pasajeroFoto": "https://apicharlasalumnotajamar.azurewebsites.net/images/users/195_user.jpg",
      "puntuacion": 5,
      "comentario": "Excelente viaje con David, muy puntual y conducción impecable.",
      "tags": "Puntual,ConducciónSegura,BuenaMúsica",
      "fechaCreacion": "2026-11-24T10:28:14.7439135Z",
      "origenTrayecto": "Vallecas Central",
      "destinoTrayecto": "Tajamar"
    }
  ]
}
```

---

## 🌟 Enriquecimiento en Tarjetas y Detalle de Trayectos

Para optimizar la experiencia de usuario en el frontend de Angular (desarrollado por Mónica y Harry) sin requerir peticiones HTTP adicionales por cada tarjeta:

1. **`TrayectoResumenDto` (`GET /api/trayectos`, `GET /api/trayectos/mis-viajes`):**
   - Se añadieron los campos:
     - `ConductorPuntuacionMedia` (`decimal?`, ej. `4.5` o `null` si aún no tiene reseñas).
     - `ConductorTotalResenas` (`int`, ej. `2`).
   - Permite renderizar directamente en el feed de búsqueda el badge:
     `⭐ 4.5 (2)`

2. **`TrayectoDetalleDto` (`GET /api/trayectos/{id}`):**
   - Incluye igualmente `ConductorPuntuacionMedia` y `ConductorTotalResenas` junto al objeto completo del conductor.

---

## 🛡️ Reglas de Seguridad y Validación de Negocio

| Regla | Comportamiento Backend | Código HTTP |
| :--- | :--- | :--- |
| **Reserva Confirmada Obligatoria** | Se verifica en `RESERVAS_TRAYECTO` que el usuario tenga una reserva con `Estado == 'Confirmada'`. Si está en cola de espera o no participó, se rechaza la operación. | `400 Bad Request` |
| **Antifraude y Autovaloración** | El conductor del trayecto (`trayecto.Conductor.IdUsuarioTajamar == idUsuarioPasajero`) no puede valorarse a sí mismo. | `400 Bad Request` |
| **Anti-Duplicidad** | Control dual: a nivel de servicio mediante consulta LINQ y a nivel de Azure SQL mediante la restricción única `UQ_Valoracion_Trayecto_Pasajero (TrayectoId, PasajeroUsuarioId)`. | `400 Bad Request` |
| **Rango de Estrellas Estricto** | Validado tanto por DataAnnotations `[Range(1, 5)]` en el DTO como por restricción `CHECK (Puntuacion BETWEEN 1 AND 5)` en la base de datos. | `400 Bad Request` |
| **Identidad Auditada** | El ID del pasajero nunca se recibe en el cuerpo de la petición; se extrae obligatoriamente del token JWT firmado. | `401 Unauthorized` si no hay token |

---

## 🧪 Tabla de Pruebas REST en Vivo Certificadas por QA

Las pruebas fueron ejecutadas contra Azure SQL Server por el equipo de QA (@qa-verifier) arrojando un 100% de éxito:

| Caso de Prueba | Usuario | Método / Endpoint | Payload / Parámetros | Resultado Esperado | Resultado Certificado |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Creación 1ª Reseña (5⭐)** | Sara (195) | `POST /api/valoraciones` | Trayecto 4, 5⭐, "Excelente viaje..." | `201 Created` con DTO de reseña | ✅ Superado (Id 5 generado) |
| **Creación 2ª Reseña (4⭐)** | Matheus (194) | `POST /api/valoraciones` | Trayecto 4, 4⭐, "Muy buen trayecto..." | `201 Created` con DTO de reseña | ✅ Superado (Id 6 generado) |
| **Control de Duplicados** | Sara (195) | `POST /api/valoraciones` | Trayecto 4, intento de re-valorar | `400 Bad Request` ("Ya has emitido una valoración...") | ✅ Superado (Rechazo inmediato) |
| **Bloqueo Autovaloración** | David (212 - Conductor) | `POST /api/valoraciones` | Trayecto 4, intento de calificar su viaje | `400 Bad Request` ("No puedes valorar tu propio trayecto...") | ✅ Superado (Bloqueo exitoso) |
| **Sin Reserva Confirmada** | Bryan (196) | `POST /api/valoraciones` | Trayecto 4 (no fue pasajero confirmado) | `400 Bad Request` ("Solo los pasajeros con reserva...") | ✅ Superado (Bloqueo exitoso) |
| **Resumen y Desglose Conductor** | Cualquiera | `GET /api/conductores/212/resenas` | ID Conductor: 212 | `200 OK`, Media 4.5⭐, Total 2, Desglose {4★:1, 5★:1} | ✅ Superado al 100% |
| **Enriquecimiento Buscador** | Cualquiera | `GET /api/trayectos` | Consulta general | `200 OK`, Trayecto 4 incluye `conductorPuntuacionMedia: 4.5` | ✅ Superado al 100% |
