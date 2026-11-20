# 🚗 BE-D03: Módulo CRUD de Trayectos (Recurrentes de Clase y Eventos)

## 📌 Resumen de la Tarea
- **Responsable:** David (Back)
- **Estado:** ✅ Completada y Verificada
- **Tipos de Trayecto:**
  - `Recurrente`: Trayectos diarios habituales de clase (L-V, turno mañana o tarde).
  - `Puntual`: Trayectos específicos con fecha/hora fija, asociados opcionalmente a un `EventoId`.
- **Reglas de Negocio:**
  - Solo usuarios con `EsConductor == true` pueden publicar viajes.
  - No se pueden ofrecer más plazas que la capacidad máxima del coche del conductor (`PlazasCoche`).
  - Solo el conductor propietario puede editar o cancelar un viaje.

---

## 🌐 Endpoints Implementados

### 1. Publicar Trayecto (Protegido con `Bearer Token`)
* **Ruta:** `POST /api/trayectos`
* **Headers:** `Authorization: Bearer <token>`
* **Request Body (Ejemplo Recurrente a Clase):**
  ```json
  {
    "origen": "Vallecas",
    "destino": "Tajamar",
    "fechaHoraSalida": "2026-11-23T08:00:00",
    "plazasTotales": 3,
    "tipoTrayecto": "Recurrente",
    "modalidad": "IdaYVuelta",
    "diasSemana": "L,M,X,J,V",
    "turno": "Manana"
  }
  ```
* **Request Body (Ejemplo Puntual de Evento):**
  ```json
  {
    "origen": "Móstoles Central",
    "destino": "Tajamar",
    "fechaHoraSalida": "2026-11-28T17:00:00",
    "plazasTotales": 4,
    "tipoTrayecto": "Puntual",
    "modalidad": "SoloIda",
    "eventoId": 1
  }
  ```
* **Response Body (201 Created):**
  ```json
  {
    "id": 2,
    "origen": "Vallecas",
    "destino": "Tajamar",
    "fechaHoraSalida": "2026-11-23T08:00:00",
    "plazasTotales": 3,
    "plazasDisponibles": 3,
    "tipoTrayecto": "Recurrente",
    "modalidad": "IdaYVuelta",
    "diasSemana": "L,M,X,J,V",
    "turno": "Manana",
    "estado": "Activo",
    "eventoId": null,
    "conductor": {
      "idPerfil": 1,
      "idUsuario": 212,
      "nombre": "David",
      "apellidos": "Castellanos",
      "telefono": "611223344",
      "marcaCoche": "Seat",
      "modeloCoche": "Ibiza",
      "matricula": "1234XYZ"
    }
  }
  ```

---

### 2. Consultar Detalle de Trayecto (Público)
* **Ruta:** `GET /api/trayectos/{id}`
* **Response Body (200 OK):** Detalle completo con información del conductor, plazas disponibles y vehículo.

---

### 3. Consultar Mis Viajes Publicados (Protegido con `Bearer Token`)
* **Ruta:** `GET /api/trayectos/mis-viajes`
* **Headers:** `Authorization: Bearer <token>`
* **Response Body (200 OK):** Lista de viajes publicados por el conductor autenticado.

---

### 4. Modificar un Trayecto Propio (Protegido con `Bearer Token`)
* **Ruta:** `PUT /api/trayectos/{id}`
* **Headers:** `Authorization: Bearer <token>`
* **Validación:** No permite reducir las plazas por debajo del número de reservas ya ocupadas.

---

### 5. Cancelar un Trayecto Propio (Protegido con `Bearer Token`)
* **Ruta:** `DELETE /api/trayectos/{id}`
* **Headers:** `Authorization: Bearer <token>`
* **Acción:** Cambia el `Estado` a `"Cancelado"`.
