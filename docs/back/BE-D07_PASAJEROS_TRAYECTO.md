# 👥 BE-D07: Consulta de Pasajeros de Trayecto para el Conductor

## 📌 Resumen de la Tarea
- **Responsable:** David (Back)
- **Estado:** ✅ Completada y Verificada en Vivo en Azure SQL
- **GitHub Issue:** [#31](https://github.com/DCastellanos4/TajamarAPP/issues/31)
- **Objetivo:** Permitir al conductor visualizar los pasajeros confirmados de sus trayectos publicados, incluyendo datos de contacto y perfil (nombre, apellidos, email, foto, bio, teléfono), así como la lista de espera ordenada cronológicamente con su posición en la cola, garantizando la seguridad mediante validación estricta de propiedad (403 Forbidden si el usuario no es el conductor del trayecto).

---

## 🌐 Endpoints Implementados

### 1. Consultar Pasajeros y Cola de un Trayecto (`GET /api/trayectos/{id}/pasajeros`)
* **Ruta:** `GET /api/trayectos/{id}/pasajeros`
* **Autenticación:** Requiere `Bearer Token` (`[Authorize]`)
* **Autorización:** Estricta. Comprueba que el usuario autenticado sea el conductor propietario del trayecto (`IdConductor` coincide con el del trayecto).
* **Códigos HTTP:**
  - `200 OK`: Devuelve el resumen del trayecto, lista de pasajeros confirmados y alumnos en cola.
  - `401 Unauthorized`: Token ausente o inválido.
  - `403 Forbidden`: El usuario autenticado no es el conductor del trayecto o no tiene perfil de conductor.
  - `404 NotFound`: El trayecto no existe.

#### Ejemplo de Respuesta `200 OK`:
```json
{
  "trayectoId": 4,
  "origen": "Vallecas Central",
  "destino": "Tajamar",
  "fechaHoraSalida": "2026-11-28T08:00:00",
  "plazasTotales": 3,
  "plazasDisponibles": 0,
  "plazasOcupadas": 3,
  "estado": "Completo",
  "pasajerosConfirmados": [
    {
      "idReserva": 8,
      "idUsuario": 195,
      "nombre": "Sara",
      "apellidos": "Ulgazi El Yaghmouri",
      "email": "sara.ulgazi@tajamar365.com",
      "foto": "https://apicharlasalumnotajamar.azurewebsites.net/images/users/195_user.jpg",
      "telefono": null,
      "bio": null,
      "fechaReserva": "2026-11-24T08:10:38.116Z",
      "estado": "Confirmada"
    },
    {
      "idReserva": 9,
      "idUsuario": 194,
      "nombre": "Matheus",
      "apellidos": "Ferreira De Carvalho",
      "email": "matheus.ferreira@tajamar365.com",
      "foto": "https://apicharlasalumnotajamar.azurewebsites.net/images/users/194_user.jpg",
      "telefono": null,
      "bio": null,
      "fechaReserva": "2026-11-24T08:10:38.278Z",
      "estado": "Confirmada"
    },
    {
      "idReserva": 10,
      "idUsuario": 193,
      "nombre": "Hugo",
      "apellidos": "Solis Torrijos",
      "email": "hugo.solis@tajamar365.com",
      "foto": "https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png",
      "telefono": null,
      "bio": null,
      "fechaReserva": "2026-11-24T09:16:52.874Z",
      "estado": "Confirmada"
    }
  ],
  "alumnosEnCola": [
    {
      "idCola": 6,
      "idUsuario": 196,
      "posicion": 1,
      "nombre": "Bryan Jefferson",
      "apellidos": "Rocabado Corrales",
      "foto": "https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png",
      "fechaRegistro": "2026-11-24T09:18:45.386Z"
    }
  ]
}
```

---

## 🏗️ Arquitectura y Componentes Desarrollados

### 1. DTOs Creados (`TajamarApp.Core/DTOs/Trayectos/PasajerosTrayectoDto.cs`)
- `PasajerosTrayectoDto`: Contiene la cabecera del trayecto (`TrayectoId`, `Origen`, `Destino`, `FechaHoraSalida`, `PlazasTotales`, `PlazasDisponibles`, `PlazasOcupadas`, `Estado`), la lista `PasajerosConfirmados` y la lista `AlumnosEnCola`.
- `PasajeroConfirmadoDto`: Información completa del pasajero (`IdReserva`, `IdUsuario`, `Nombre`, `Apellidos`, `Email`, `Foto`, `Telefono`, `Bio`, `FechaReserva`, `Estado`).
- `AlumnoEnColaDto`: Información del aspirante en lista de espera (`IdCola`, `IdUsuario`, `Posicion`, `Nombre`, `Apellidos`, `Foto`, `FechaRegistro`).

### 2. Capa de Servicios (`ITrayectoService` y `TrayectoService`)
- Método `GetPasajerosTrayectoAsync(int idUsuarioConductor, int trayectoId)`:
  1. Localiza el perfil de conductor del usuario autenticado en `CONDUCTORES`.
  2. Obtiene el trayecto verificando que `ConductorId == conductor.IdConductor` (si no pertenece al usuario, lanza `UnauthorizedAccessException` que el controlador traduce a `403 Forbidden`).
  3. Consulta `RESERVAS_TRAYECTO` con estado `'Confirmada'`, haciendo join con `USUARIOSTAJAMAR` y `PERFILES_APP` para mapear los datos de perfil y contacto.
  4. Consulta `COLA_ESPERA` con estado `'EnEspera'`, ordenando por `Posicion ASC` y cruzando con `USUARIOSTAJAMAR` y `PERFILES_APP`.
  5. Calcula en tiempo real las plazas ocupadas (`PlazasTotales - PlazasDisponibles`).

### 3. Controlador API (`TrayectosController.cs`)
- Endpoint `[HttpGet("{id:int}/pasajeros")]` con `[Authorize]`.
- Manejo estandarizado de `401 Unauthorized`, `403 Forbidden` y `404 NotFound`.

---

## 🧪 Pruebas en Vivo (Live Verification)

| Escenario de Prueba | Entrada | Resultado Esperado | Resultado Obtenido |
| :--- | :--- | :--- | :--- |
| **Consulta Conductor Propietario** | David (ID 212) -> Trayecto 4 | 200 OK con 3 confirmados y 1 en cola | ✅ 200 OK - Pasajeros y cola completos |
| **Pasajeros Confirmados** | Trayecto 4 lleno (3/3) | Sara, Matheus y Hugo | ✅ 3 pasajeros con nombres, fotos y estado |
| **Lista de Espera FIFO** | Bryan (ID 196) solicita plaza | Aparece en cola con Posición 1 | ✅ Posición 1 en cola con datos de usuario |
| **Intento de Acceso no Autorizado** | Hugo (ID 193) -> Trayecto 4 | 403 Forbidden | ✅ 403 Forbidden ("No tienes permiso...") |
| **Consulta Anónima (sin Token)** | Sin cabecera Authorization | 401 Unauthorized | ✅ 401 Unauthorized |
