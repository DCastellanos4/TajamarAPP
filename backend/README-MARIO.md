# Backend de eventos, asistencia y reservas

La implementación de BE-M01 a BE-M05 forma parte de `backend/TajamarApp.slnx`. La solución usa ASP.NET Core en .NET 10 y el modelo común de TajamarApp en EF Core. Las operaciones de movilidad que actualizan el inventario usan transacciones de SQL Server mediante `Microsoft.Data.SqlClient` y comparten la conexión configurada para el `DbContext`.

## Ejecutar con AZURETAJAMAR

La API lee `ConnectionStrings:AzureSqlServer`. En este equipo ya está configurado en Secret Manager para la cuenta de base de datos `tajamar_mario_api`. Ejecuta desde la raíz del repositorio:

```powershell
$api = 'backend/src/TajamarApp.Api/TajamarApp.Api.csproj'
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet run --project $api --no-launch-profile --urls http://localhost:5070
```

Comprueba la conexión en `GET http://localhost:5070/api/health`. Swagger está en `http://localhost:5070/swagger`. En Angular, apunta el proxy de desarrollo a ese puerto o usa el origen local autorizado `http://localhost:4200`.

En otro equipo, guarda una cadena con el formato siguiente en el almacén de secretos del proyecto o configura `ConnectionStrings__AzureSqlServer` en el gestor de secretos del entorno.

```text
Server=tcp:sqldavidc3597.database.windows.net,1433;Database=AZURETAJAMAR;User ID=<usuario de aplicación>;Password=<secreto>;Encrypt=True;TrustServerCertificate=False;
```

La cuenta contenida puede leer el catálogo y escribir en las tablas de eventos, asistencia, reservas y cola. En trayectos, solo puede actualizar disponibilidad y estado. [001-permisos-api.sql](sql/001-permisos-api.sql) documenta estos permisos. Las herramientas de administración y migraciones requieren sus propias credenciales.

## Autenticación para Angular

Las escrituras requieren `Authorization: Bearer <JWT del backend>`. Configura `Jwt:SigningKey`, `Jwt:Issuer` y `Jwt:Audience` con los valores del módulo de autenticación del equipo. Esta API valida HS256 con una clave de al menos 32 bytes y el claim numérico `idUsuarioTajamar`. El claim `role` asigna los permisos. `GestorEventos` crea eventos y edita los propios. `Administrador` puede editar cualquiera y consultar los endpoints de usuarios. Ambos roles ven el listado de asistentes en el detalle de un evento.

| Ruta | Resultado principal |
| --- | --- |
| `GET /api/eventos` | `200` con los eventos futuros en orden de inicio. Como `EVENTOS_APP` no tiene columna de estado, un evento se considera activo hasta su inicio. |
| `GET /api/eventos/{id}` | `200` con organizador, imagen si existe, descripción y `totalAsistentes`. |
| `POST /api/eventos` | Envía `nombre`, `inicio` ISO 8601 con offset y `lugar`. Admite `descripcion` e `imagen` opcionales. Devuelve `201` con `id` y cabecera `Location`. |
| `PUT /api/eventos/{id}` | Mismo cuerpo. Devuelve `204`. Se admiten fechas a partir de hoy en Madrid. La fecha se guarda en UTC. |
| `POST /api/eventos/{id}/asistir` | `201` o `409` si ya existe una inscripción. |
| `DELETE /api/eventos/{id}/asistir` | `204` y se actualiza el contador. |
| `POST /api/trayectos/{id}/reservar` | `201` con ID de reserva. Si se agota, devuelve `409`, `code: PLAZAS_AGOTADAS` y `listaEsperaDisponible: true`. |
| `POST /api/trayectos/{id}/cola` | `201` con `posicion`, o `409` si hay plazas o el usuario ya participa. |
| `DELETE /api/reservas/{id}` | `204` para el titular. Promociona al primero si la salida sigue abierta y hay cola. En otro caso devuelve la plaza al inventario. |

La API trabaja con `dbo.EVENTOS_APP`, `dbo.EVENTO_ASISTENTES`, `dbo.TRAYECTOS`, `dbo.RESERVAS_TRAYECTO` y `dbo.COLA_ESPERA`. Bloquea la fila de `TRAYECTOS` mientras cambia las plazas o la cola. Cuando se agota la última plaza, el trayecto pasa a `Completo`. Cuando vuelve a quedar una plaza, pasa a `Activo` si seguía abierto.

## Datos de prueba y verificación

[002-datos-demo.sql](sql/002-datos-demo.sql) inserta dos cursos, cuatro usuarios sin contraseña ni acceso activo, tres eventos y un perfil ficticio de conductor. Se puede repetir sin duplicarlos. Los registros quedan identificados como `DEMO BE-M05`.

```powershell
dotnet build backend/TajamarApp.slnx
dotnet run --project backend/tests/TajamarApp.Tests
```

Para ejecutar la integración contra Azure, usa `TAJAMAR_TEST_SQL` con una cuenta autorizada para crear y retirar registros de prueba. Usa `TAJAMAR_TEST_API_SQL` con la conexión de la cuenta limitada. Ejecuta `dotnet run --project backend/tests/TajamarApp.Tests -- --sql`. El ejecutable usa los usuarios ficticios del seeder, crea un trayecto y un evento propios y elimina los registros temporales cuyos identificadores creó. Verifica la carrera por la última plaza, la promoción FIFO, la restitución de plaza y la asistencia.
