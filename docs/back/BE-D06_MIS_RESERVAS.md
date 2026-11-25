# 🎟️ BE-D06: Mis Reservas del Pasajero y Gestión de Cancelación

## 📌 Resumen de la Tarea
- **Responsable:** David (Back)
- **Estado:** ✅ Completada y Verificada en Vivo
- **GitHub Issue:** [#30](https://github.com/DCastellanos4/TajamarAPP/issues/30)
- **Objetivo:** Proveer un endpoint unificado para que el alumno consulte su panel de actividad como pasajero (`GET /api/reservas/mis-reservas`), viendo tanto los viajes donde tiene plaza confirmada como aquellos en los que está en lista de espera (con su posición actual en la cola FIFO), además de permitir cancelar reservas y salir voluntariamente de las colas.

---

## 🌐 Endpoints Implementados

### 1. Consultar Mis Reservas y Colas (`GET /api/reservas/mis-reservas`)
* **Ruta:** `GET /api/reservas/mis-reservas`
* **Autenticación:** Requiere `Bearer Token` (`[Authorize]`)
* **Descripción:** Devuelve un listado ordenado cronológicamente por `FechaHoraSalida ASC` con todas las reservas activas/históricas y solicitudes en lista de espera del alumno.

#### Ejemplo de Respuesta `200 OK` (Reserva Confirmada):
```json
[
  {
    "id": 7,
    "tipoEntrada": "Reserva",
    "idReserva": 7,
    "idCola": null,
    "estado": "Confirmada",
    "posicionCola": null,
    "fechaRegistro": "2026-11-24T08:10:26.449Z",
    "trayectoId": 4,
    "origen": "Vallecas Central",
    "destino": "Tajamar",
    "fechaHoraSalida": "2026-11-28T08:00:00",
    "tipoTrayecto": "Recurrente",
    "modalidad": "IdaYVuelta",
    "turno": "Manana",
    "diasSemana": "L,M,X,J,V",
    "plazasTotales": 3,
    "plazasDisponibles": 2,
    "estadoTrayecto": "Activo",
    "eventoId": null,
    "eventoTitulo": null,
    "conductorIdUsuario": 212,
    "conductorNombre": "David Castellanos",
    "conductorFoto": "https://apicharlasalumnotajamar.azurewebsites.net/images/users/212_user.jpg",
    "conductorTelefono": "611223344",
    "coche": "Seat Ibiza",
    "colorCoche": "Negro",
    "matricula": "1234XYZ"
  }
]
```

#### Ejemplo de Respuesta `200 OK` (En Lista de Espera FIFO):
```json
[
  {
    "id": 5,
    "tipoEntrada": "ColaEspera",
    "idReserva": null,
    "idCola": 5,
    "estado": "EnEspera",
    "posicionCola": 1,
    "fechaRegistro": "2026-11-24T08:10:38.423Z",
    "trayectoId": 4,
    "origen": "Vallecas Central",
    "destino": "Tajamar",
    "fechaHoraSalida": "2026-11-28T08:00:00",
    "tipoTrayecto": "Recurrente",
    "modalidad": "IdaYVuelta",
    "turno": "Manana",
    "diasSemana": "L,M,X,J,V",
    "plazasTotales": 3,
    "plazasDisponibles": 0,
    "estadoTrayecto": "Completo",
    "conductorIdUsuario": 212,
    "conductorNombre": "David Castellanos",
    "coche": "Seat Ibiza",
    "matricula": "1234XYZ"
  }
]
```

---

### 2. Salir de la Lista de Espera (`DELETE /api/reservas/cola/{idCola}`)
* **Ruta:** `DELETE /api/reservas/cola/{idCola}`
* **Autenticación:** Requiere `Bearer Token` (`[Authorize]`)
* **Descripción:** Permite al alumno renunciar a su puesto en la lista de espera de un trayecto lleno.
* **Comportamiento FIFO:** Al salir, el sistema busca a todos los alumnos que estaban detrás en la cola para ese mismo trayecto y decrementa su posición (`Posicion = Posicion - 1`), asegurando la integridad del orden de llegada.
* **Respuesta `200 OK`:**
```json
{
  "mensaje": "Has salido de la lista de espera correctamente."
}
```

---

### 3. Cancelar Reserva Confirmada (`DELETE /api/reservas/{id}`)
* **Ruta:** `DELETE /api/reservas/{id}`
* **Autenticación:** Requiere `Bearer Token` (`[Authorize]`)
* **Descripción:** Cancela una plaza confirmada del pasajero. Si hay alguien en la cola de espera FIFO, esa persona es promocionada automáticamente a reserva `Confirmada`. Si no hay nadie, la plaza se reincorpora al inventario (`PlazasDisponibles + 1`) y el viaje vuelve a estado `Activo`.

---

## ⚡ Reglas de Negocio Verificadas
1. **Unificación para Frontend:** Mónica y Harry pueden pintar en un único componente de pestañas o listado tanto las plazas confirmadas como el número de turno en cola.
2. **Seguridad Multi-Tenant:** Cada alumno solo puede consultar y cancelar sus propias reservas y entradas en cola (`UsuarioTajamarId == idUsuario`).
3. **Mantenimiento del Histórico:** Al cancelar una reserva confirmada, su estado cambia a `"Cancelada"` en base de datos para preservar la trazabilidad.
