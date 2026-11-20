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
