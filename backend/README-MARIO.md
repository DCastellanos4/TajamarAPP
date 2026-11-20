# Backend de eventos, asistencia y reservas

La solución de BE-M01 a BE-M05 está en `backend/TajamarApp.slnx`. Usa ASP.NET Core en .NET 10 y transacciones de SQL Server con `Microsoft.Data.SqlClient`. Dentro de la API, `Features` contiene los módulos HTTP, `Auth` valida el JWT del equipo, `Infrastructure` gestiona las conexiones y transacciones, y `Domain` contiene la regla FIFO. El login, la publicación de trayectos y su búsqueda pertenecen a los módulos de David.

## Configurar tu base de datos

Desde la raíz del repositorio, guarda la cadena en Secret Manager de .NET. Sustituye los valores entre ángulos por los que te dieron. En desarrollo la aplicación lee la clave `ConnectionStrings:Tajamar`.

```powershell
$api = 'backend/src/TajamarApp.Api/TajamarApp.Api.csproj'
dotnet user-secrets set 'ConnectionStrings:Tajamar' 'Server=tcp:<servidor>,<puerto>;Database=<base>;User ID=<usuario>;Password=<contraseña>;Encrypt=True;TrustServerCertificate=False;' --project $api
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet run --project $api --urls http://localhost:5070
```

En un despliegue, configura `ConnectionStrings__Tajamar` en el gestor de secretos del entorno. Para una instancia local con autenticación de Windows, puedes usar `Server=localhost\DEVELOPER;Database=<base>;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;`. Usa esa opción de confianza del certificado solo con la instancia local de desarrollo.

La cadena de conexión indica a qué servidor y base conectarse. Para usar las tablas privadas, contrasta primero los nombres, tipos de clave, columnas y estados con [el esquema de referencia](sql/001-esquema-referencia.sql). El código consulta `movilidad.Evento`, `movilidad.Asistencia`, `movilidad.Trayecto`, `movilidad.Reserva` y `movilidad.ColaEspera`. Si tu esquema difiere, comparte el DDL anonimizado u obtenido con «Generar scripts → Solo esquema» de esas tablas y sus relaciones y restricciones. Con esa información se pueden ajustar las consultas de los dos módulos. Los scripts de referencia y los datos ficticios se aplican manualmente a una base separada de desarrollo.

La reserva bloquea la fila del trayecto hasta confirmar el cambio de inventario. La cancelación y la inscripción en la cola usan ese mismo bloqueo. Cada ID corresponde a una salida reservable con un inventario propio. El CRUD de trayectos del equipo debe mantener ese inventario y coordinar los cambios de estado y capacidad con el mismo registro.

## Autenticación para Angular

Las escrituras requieren `Authorization: Bearer <JWT del backend>`. Configura `Jwt:SigningKey`, `Jwt:Issuer` y `Jwt:Audience` con los valores del emisor de David mediante Secret Manager. El módulo espera HS256, una clave de al menos 32 bytes y el claim numérico `idUsuarioTajamar`. Usa `nombre` para el nombre visible y `role` para permisos. Los gestores crean eventos y editan los propios. Administrador puede editar cualquiera. Esos dos roles pueden consultar los asistentes. En Angular, usa las rutas relativas `/api/...` y apunta el proxy de desarrollo al puerto de la API.

| Ruta | Resultado principal |
| --- | --- |
| `GET /api/eventos` | `200` con próximos eventos activos ordenados por inicio. |
| `GET /api/eventos/{id}` | `200` con organizador, imagen si existe y `totalAsistentes`. |
| `POST /api/eventos` | Envía `nombre`, `inicio` ISO 8601 con offset, `lugar` e `imagen` opcional. Devuelve `201` con `id` y cabecera `Location`. |
| `PUT /api/eventos/{id}` | Mismo cuerpo. Devuelve `204`. Se admiten fechas a partir de hoy en Madrid. |
| `POST /api/eventos/{id}/asistir` | `201` o `409` si ya existe una inscripción. |
| `DELETE /api/eventos/{id}/asistir` | `204` y se actualiza el contador. |
| `POST /api/trayectos/{id}/reservar` | `201` con ID de reserva. Si se agota, devuelve `409`, `code: PLAZAS_AGOTADAS` y `listaEsperaDisponible: true`. |
| `POST /api/trayectos/{id}/cola` | `201` con `posicion`, o `409` si hay plazas o ya participa. |
| `DELETE /api/reservas/{id}` | `204` para el titular. Promociona al primero si la salida sigue abierta y hay cola. En otro caso devuelve la plaza al inventario. |

## Datos de prueba y verificación

En una base separada de desarrollo, ejecuta [001-esquema-referencia.sql](sql/001-esquema-referencia.sql) una vez. Ejecuta [002-datos-demo.sql](sql/002-datos-demo.sql) para cargar dos cursos, cuatro usuarios ficticios, tres eventos y un trayecto de una plaza. Los usuarios del seeder son datos de prueba. Las pruebas de integración crean sus propios JWT y sus propios registros temporales.

```powershell
dotnet build backend/TajamarApp.slnx
dotnet run --project backend/tests/TajamarApp.Tests
$env:TAJAMAR_TEST_SQL = '<cadena de la base de pruebas con el esquema de referencia>'
dotnet run --project backend/tests/TajamarApp.Tests -- --sql
Remove-Item Env:TAJAMAR_TEST_SQL
```

La primera ejecución de pruebas comprueba el orden FIFO. Con `--sql`, el ejecutable inicia la API en un puerto local, envía dos peticiones concurrentes por la última plaza y verifica el inventario y las reservas directamente en SQL Server. También comprueba la promoción y renumeración FIFO, la restitución de la plaza, los permisos básicos y la unicidad de la asistencia.
