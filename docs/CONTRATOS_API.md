# 📜 Contratos de API REST - Tajamar App

Este documento sirve como referencia oficial para los desarrolladores Frontend (**Mónica** y **Harry**) y Backend (**Mario**), detallando las rutas, cabeceras, modelos de datos de entrada/salida y códigos de estado HTTP implementados en la API.

- **Base URL Local:** `http://localhost:5050`
- **Swagger UI:** `http://localhost:5050/swagger`
- **Contrato OpenAPI JSON:** [swagger.json](file:///C:/Users/Alumnos%20MCSD%20Mañana/Documents/tajamarAPP/docs/swagger.json)

---

## 📑 Índice de Endpoints
1. [Módulo de Autenticación (`/api/auth`)](#1-módulo-de-autenticación-apiauth)
2. [Módulo de Perfil Extendido (`/api/perfil`)](#2-módulo-de-perfil-extendido-apiperfil)
3. [Módulo de Trayectos (`/api/trayectos`)](#3-módulo-de-trayectos-apitrayectos)
4. [Módulo Académico Tajamar (`/api/cursos`, `/api/usuarios`)](#4-módulo-académico-tajamar-apicursos-apiusuarios)
5. [Módulo de Eventos y Asistencia (`/api/eventos`)](#5-módulo-de-eventos-y-asistencia-apieventos)
6. [Módulo de Reservas y Cola FIFO (`/api/trayectos/{id}/reservar`, `/api/trayectos/{id}/cola`, `/api/reservas/{id}`)](#6-módulo-de-reservas-y-cola-fifo)
7. [Health Check (`/api/health`)](#7-health-check-apihealth)

---

## 1. Módulo de Autenticación (`/api/auth`)

### 1.1. Login de Usuario
Valida credenciales contra `USUARIOSTAJAMAR` (con contraseña cifrada en base64/sha256).

- **Método:** `POST`
- **Ruta:** `/api/auth/login`
- **Acceso:** Público (sin token)
- **Request Body:**
```json
{
  "email": "david.castellanos@tajamar365.com",
  "password": "12345"
}
```
- **Respuestas:**
  - **`200 OK`**:
    ```json
    {
      "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
      "idUsuario": 212,
      "nombre": "David",
      "apellidos": "Castellanos",
      "email": "david.castellanos@tajamar365.com",
      "imagen": "https://apicharlasalumnotajamar.azurewebsites.net/images/users/212_user.jpg",
      "idRol": 1,
      "rol": "Alumno",
      "perfilCompletado": true,
      "esConductor": true
    }
    ```
  - **`400 Bad Request`**: Modelo inválido o campos vacíos.
  - **`401 Unauthorized`**:
    ```json
    {
      "mensaje": "Credenciales incorrectas."
    }
    ```

---

## 2. Módulo de Perfil Extendido (`/api/perfil`)

### 2.1. Obtener mi perfil actual
Obtiene los datos del usuario en Tajamar junto a sus datos extendidos de la app (teléfono, coche, bio).

- **Método:** `GET`
- **Ruta:** `/api/perfil/me`
- **Headers:** `Authorization: Bearer <token>`
- **Respuestas:**
  - **`200 OK`**:
    ```json
    {
      "id": 1,
      "idUsuarioTajamar": 212,
      "nombre": "David",
      "apellidos": "Castellanos",
      "email": "david.castellanos@tajamar365.com",
      "imagen": "https://apicharlasalumnotajamar.azurewebsites.net/images/users/212_user.jpg",
      "telefono": "611223344",
      "bio": "Estudiante de desarrollo de Tajamar. Conduzco a diario desde Vallecas.",
      "esConductor": true,
      "marcaCoche": "Seat",
      "modeloCoche": "Ibiza",
      "colorCoche": "Gris",
      "matricula": "1234XYZ",
      "plazasCoche": 4,
      "fechaRegistro": "2026-11-20T10:55:00"
    }
    ```
  - **`401 Unauthorized`**: Token expirado o no enviado.
  - **`404 Not Found`**: El usuario todavía no tiene perfil extendido registrado (necesita onboarding).

### 2.2. Crear o actualizar mi perfil (Onboarding)
- **Método:** `POST`
- **Ruta:** `/api/perfil`
- **Headers:** `Authorization: Bearer <token>`
- **Request Body (Si es conductor):**
```json
{
  "telefono": "611223344",
  "bio": "Estudiante de desarrollo de Tajamar. Conduzco a diario desde Vallecas.",
  "esConductor": true,
  "marcaCoche": "Seat",
  "modeloCoche": "Ibiza",
  "colorCoche": "Gris",
  "matricula": "1234XYZ",
  "plazasCoche": 4
}
```
- **Request Body (Si es solo pasajero):**
```json
{
  "telefono": "699887766",
  "bio": "Busco coche desde Alcorcón para el turno de mañana.",
  "esConductor": false
}
```
- **Respuestas:**
  - **`200 OK`**: Retorna el `PerfilDto` completo guardado.
  - **`400 Bad Request`**: Datos inválidos (por ejemplo, `esConductor: true` pero sin marca/matrícula/plazas).

---

## 3. Módulo de Trayectos (`/api/trayectos`)

### 3.1. Búsqueda y filtrado paginado de trayectos
- **Método:** `GET`
- **Ruta:** `/api/trayectos`
- **Acceso:** Público (con o sin token)
- **Query Parameters:**
  - `origen`: string (ej. `Vallecas`)
  - `destino`: string (ej. `Tajamar`)
  - `tipoTrayecto`: `Recurrente` | `Puntual`
  - `modalidad`: `SoloIda` | `SoloVuelta` | `IdaYVuelta`
  - `turno`: `Manana` | `Tarde`
  - `eventoId`: int (opcional, para trayectos de eventos)
  - `fecha`: ISO Date (ej. `2026-11-23`)
  - `soloConPlazas`: boolean (ej. `true`)
  - `pagina`: int (default: 1)
  - `tamanoPagina`: int (default: 10, max: 50)
- **Respuesta `200 OK`:**
```json
{
  "elementos": [
    {
      "id": 2,
      "conductorNombre": "David Castellanos",
      "conductorImagen": "https://apicharlasalumnotajamar.azurewebsites.net/images/users/212_user.jpg",
      "coche": "Seat Ibiza",
      "origen": "Vallecas",
      "destino": "Tajamar",
      "fechaHoraSalida": "2026-11-23T08:00:00",
      "plazasDisponibles": 3,
      "plazasTotales": 3,
      "tipoTrayecto": "Recurrente",
      "modalidad": "IdaYVuelta",
      "turno": "Manana",
      "diasSemana": "L,M,X,J,V",
      "estado": "Activo",
      "eventoId": null
    }
  ],
  "totalRegistros": 1,
  "paginaActual": 1,
  "totalPaginas": 1,
  "tienePaginaAnterior": false,
  "tienePaginaSiguiente": false
}
```

### 3.2. Publicar nuevo trayecto
- **Método:** `POST`
- **Ruta:** `/api/trayectos`
- **Headers:** `Authorization: Bearer <token>`
- **Request Body (Viaje a Clase):**
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
- **Request Body (Viaje a Evento):**
```json
{
  "origen": "Getafe Central",
  "destino": "Tajamar",
  "fechaHoraSalida": "2026-11-25T16:00:00",
  "plazasTotales": 4,
  "tipoTrayecto": "Puntual",
  "modalidad": "SoloIda",
  "eventoId": 1
}
```
- **Respuestas:**
  - **`201 Created`**: Retorna el objeto `TrayectoDetalleDto`.
  - **`400 Bad Request`**: Si el usuario no es conductor, o las plazas solicitadas superan la capacidad del coche.

### 3.3. Obtener detalle de un trayecto
- **Método:** `GET`
- **Ruta:** `/api/trayectos/{id}`
- **Acceso:** Público
- **Respuesta `200 OK`:**
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
  "eventoTitulo": null,
  "fechaCreacion": "2026-11-20T11:05:00",
  "conductor": {
    "idPerfil": 1,
    "idUsuario": 212,
    "nombre": "David",
    "apellidos": "Castellanos",
    "imagen": "https://apicharlasalumnotajamar.azurewebsites.net/images/users/212_user.jpg",
    "telefono": "611223344",
    "marcaCoche": "Seat",
    "modeloCoche": "Ibiza",
    "colorCoche": "Gris",
    "matricula": "1234XYZ",
    "bio": "Estudiante de desarrollo..."
  }
}
```

### 3.4. Consultar mis trayectos como conductor
- **Método:** `GET`
- **Ruta:** `/api/trayectos/mis-viajes`
- **Headers:** `Authorization: Bearer <token>`
- **Respuesta `200 OK`:** Array de `TrayectoResumenDto` pertenecientes al usuario autenticado.

### 3.5. Modificar trayecto
- **Método:** `PUT`
- **Ruta:** `/api/trayectos/{id}`
- **Headers:** `Authorization: Bearer <token>`
- **Request Body:** Similar a `CrearTrayectoDto`.
- **Respuestas:**
  - **`200 OK`**: Retorna el trayecto actualizado.
  - **`400 Bad Request`**: Validación fallida (ej. reducir plazas por debajo de reservas existentes).
  - **`403 Forbidden`**: Si intentas modificar un trayecto que no es tuyo.

### 3.6. Cancelar trayecto
- **Método:** `DELETE`
- **Ruta:** `/api/trayectos/{id}`
- **Headers:** `Authorization: Bearer <token>`
- **Respuestas:**
  - **`200 OK`**: `{"mensaje": "Trayecto cancelado correctamente."}`
  - **`403 Forbidden`**: Si el trayecto no pertenece al conductor autenticado.

### 3.7. Consultar pasajeros confirmados y lista de espera de un trayecto (Conductor)
- **Método:** `GET`
- **Ruta:** `/api/trayectos/{id}/pasajeros`
- **Headers:** `Authorization: Bearer <token>`
- **Respuestas:**
  - **`200 OK`**:
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
  - **`401 Unauthorized`**: Token ausente o inválido.
  - **`403 Forbidden`**: Si el usuario no es el conductor del trayecto o no tiene perfil de conductor.
  - **`404 NotFound`**: Si el trayecto no existe.

---

## 4. Módulo Académico Tajamar (`/api/cursos`, `/api/usuarios`)

### 4.1. Listar Cursos
- **Método:** `GET`
- **Ruta:** `/api/cursos`
- **Acceso:** Público / Autenticado
- **Respuesta `200 OK`:**
```json
[
  {
    "idCurso": 3597,
    "nombre": "Master Desarrollo 2026-2027"
  }
]
```

### 4.2. Listar Usuarios del Ecosistema
- **Método:** `GET`
- **Ruta:** `/api/usuarios`
- **Acceso:** Público / Autenticado
- **Respuesta `200 OK`:** Listado de usuarios/alumnos/profesores de Tajamar con su rol y avatar.

---

## 5. Módulo de Eventos y Asistencia (`/api/eventos`)

### 5.1. Listar Eventos Futuros
- **Método:** `GET`
- **Ruta:** `/api/eventos`
- **Acceso:** Público
- **Respuesta `200 OK`:**
```json
[
  {
    "id": 1,
    "nombre": "Charla de tecnología",
    "inicio": "2026-11-27T10:04:09.186Z",
    "lugar": "Tajamar",
    "activo": true,
    "totalAsistentes": 5
  }
]
```

### 5.2. Detalle de Evento
- **Método:** `GET`
- **Ruta:** `/api/eventos/{id}`
- **Acceso:** Público (si el usuario tiene rol `GestorEventos` o `Administrador`, incluye array de `asistentes`).
- **Respuesta `200 OK`:**
```json
{
  "id": 1,
  "nombre": "Charla de tecnología",
  "inicio": "2026-11-27T10:04:09.186Z",
  "lugar": "Tajamar",
  "activo": true,
  "descripcion": "Presentación sobre desarrollo cloud.",
  "organizador": {
    "id": 212,
    "nombre": "David Castellanos"
  },
  "totalAsistentes": 5
}
```

### 5.3. Crear Evento
- **Método:** `POST`
- **Ruta:** `/api/eventos`
- **Headers:** `Authorization: Bearer <token>` (requiere rol `GestorEventos` o `Administrador`)
- **Request Body:**
```json
{
  "nombre": "Torneo Tajamar",
  "inicio": "2026-12-04T10:00:00Z",
  "lugar": "Tajamar",
  "imagen": "https://link-a-imagen.jpg",
  "descripcion": "Torneo de pádel de Tajamar"
}
```
- **Respuesta `201 Created`:** `{"id": 4}` con cabecera `Location`.

### 5.4. Inscribirse a un Evento
- **Método:** `POST`
- **Ruta:** `/api/eventos/{id}/asistir`
- **Headers:** `Authorization: Bearer <token>`
- **Respuestas:**
  - **`201 Created`**: Inscripción realizada.
  - **`409 Conflict`**: `{"code": "ASISTENCIA_DUPLICADA"}` si ya estás inscrito, o `{"code": "EVENTO_CERRADO"}` si ya ha comenzado.

### 5.5. Cancelar Asistencia a un Evento
- **Método:** `DELETE`
- **Ruta:** `/api/eventos/{id}/asistir`
- **Headers:** `Authorization: Bearer <token>`
- **Respuesta `204 NoContent`**.

---

## 6. Módulo de Reservas y Cola FIFO

### 6.1. Reservar Plaza en un Trayecto
- **Método:** `POST`
- **Ruta:** `/api/trayectos/{id}/reservar`
- **Headers:** `Authorization: Bearer <token>`
- **Respuestas:**
  - **`201 Created`**:
    ```json
    {
      "id": 10,
      "trayectoId": 2,
      "estado": "Confirmada"
    }
    ```
  - **`409 Conflict` (Sin plazas)**:
    ```json
    {
      "title": "No quedan plazas. Usa la cola de espera.",
      "status": 409,
      "code": "PLAZAS_AGOTADAS",
      "listaEsperaDisponible": true
    }
    ```
  - **`409 Conflict` (Conductor propio / ya reservado)**: `TRAYECTO_PROPIO` o `RESERVA_DUPLICADA`.

### 6.2. Apuntarse a la Cola de Espera (FIFO)
- **Método:** `POST`
- **Ruta:** `/api/trayectos/{id}/cola`
- **Headers:** `Authorization: Bearer <token>`
- **Respuestas:**
  - **`201 Created`**:
    ```json
    {
      "trayectoId": 2,
      "posicion": 1,
      "estado": "EnEspera"
    }
    ```
  - **`409 Conflict`**: `PLAZAS_DISPONIBLES` (si aún quedan plazas libres) o `YA_EN_COLA`.

### 6.3. Cancelar Reserva (con Auto-Promoción FIFO)
- **Método:** `DELETE`
- **Ruta:** `/api/reservas/{id}`
- **Headers:** `Authorization: Bearer <token>`
- **Comportamiento:** Si hay usuarios en cola de espera, el primero es automáticamente promocionado a reserva `Confirmada`. Si no hay nadie en espera, la plaza vuelve a estar disponible (`PlazasDisponibles + 1`).
- **Respuesta `204 NoContent`**.

### 6.4. Consultar Mis Reservas del Pasajero
- **Método:** `GET`
- **Ruta:** `/api/reservas/mis-reservas`
- **Headers:** `Authorization: Bearer <token>`
- **Respuesta `200 OK`:**
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
    "conductorNombre": "David Castellanos",
    "coche": "Seat Ibiza",
    "matricula": "1234XYZ"
  }
]
```

### 6.5. Salir Voluntariamente de la Cola de Espera
- **Método:** `DELETE`
- **Ruta:** `/api/reservas/cola/{idCola}`
- **Headers:** `Authorization: Bearer <token>`
- **Comportamiento:** Marca la entrada como `Cancelado` y reajusta automáticamente las posiciones de los alumnos restantes en la cola FIFO.
- **Respuesta `200 OK`:** `{"mensaje": "Has salido de la lista de espera correctamente."}`

---

## 7. Health Check (`/api/health`)

- **Método:** `GET`
- **Ruta:** `/api/health`
- **Acceso:** Público
- **Respuesta `200 OK`:**
```json
{
  "status": "Online",
  "database": "Azure SQL (AZURETAJAMAR)",
  "canConnect": true,
  "totalCursosRegistrados": 3,
  "totalUsuariosRegistrados": 33,
  "timestamp": "2026-11-20T11:30:54.580Z"
}
```
