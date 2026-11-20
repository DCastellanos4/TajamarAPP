# 🔐 BE-D02: Módulo de Autenticación JWT y Perfil Extendido (PerfilApp)

## 📌 Resumen de la Tarea
- **Responsable:** David (Back)
- **Estado:** ✅ Completada y Verificada
- **Autenticación:** JWT Bearer (HMAC-SHA256)
- **Control de Onboarding:** Detección automática mediante flag `perfilCompletado` (true/false).

---

## 🌐 Endpoints Implementados

### 1. Iniciar Sesión (Público)
* **Ruta:** `POST /api/auth/login`
* **Descripción:** Valida credenciales contra `USUARIOSTAJAMAR` en Azure SQL (soporta clave `12345` por defecto).
* **Request Body:**
  ```json
  {
    "email": "david.castellanos@tajamar365.com",
    "password": "12345"
  }
  ```
* **Response Body (200 OK):**
  ```json
  {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "perfilCompletado": false,
    "usuario": {
      "idUsuario": 212,
      "nombre": "David",
      "apellidos": "Castellanos",
      "email": "david.castellanos@tajamar365.com",
      "imagen": "https://apicharlasalumnotajamar.azurewebsites.net/images/users/212_user.jpg",
      "rol": "ALUMNO"
    }
  }
  ```

---

### 2. Consultar Mi Perfil (Protegido con `Bearer Token`)
* **Ruta:** `GET /api/perfil/me`
* **Headers:** `Authorization: Bearer <token>`
* **Response Body (200 OK):**
  ```json
  {
    "usuario": {
      "idUsuario": 212,
      "nombre": "David",
      "apellidos": "Castellanos",
      "email": "david.castellanos@tajamar365.com",
      "imagen": "https://apicharlasalumnotajamar.azurewebsites.net/images/users/212_user.jpg",
      "rol": "ALUMNO"
    },
    "perfilCompletado": true,
    "perfilApp": {
      "telefono": "611223344",
      "esConductor": true,
      "marcaCoche": "Seat",
      "modeloCoche": "Ibiza",
      "colorCoche": "Negro",
      "matricula": "1234XYZ",
      "plazasCoche": 4,
      "bio": "Estudiante de MCSD en Tajamar."
    }
  }
  ```

---

### 3. Guardar o Actualizar Perfil / Onboarding (Protegido con `Bearer Token`)
* **Ruta:** `POST /api/perfil`
* **Headers:** `Authorization: Bearer <token>`
* **Request Body:**
  ```json
  {
    "telefono": "611223344",
    "esConductor": true,
    "marcaCoche": "Seat",
    "modeloCoche": "Ibiza",
    "colorCoche": "Negro",
    "matricula": "1234XYZ",
    "plazasCoche": 4,
    "bio": "Estudiante de MCSD en Tajamar."
  }
  ```
* **Response Body (200 OK):**
  ```json
  {
    "mensaje": "Perfil guardado correctamente.",
    "perfil": { ... }
  }
  ```

---

## 🔒 Probar en Swagger con el Candado de Autorización
1. Ejecuta el endpoint `POST /api/auth/login` con tu usuario y `12345`.
2. Copia el valor del campo `token`.
3. Arriba a la derecha en Swagger, pulsa el botón verde **Authorize 🔒**.
4. En el campo *Value*, escribe:
   ```
   Bearer <pega_tu_token_aqui>
   ```
5. Pulsa **Authorize** y luego **Close**.
6. Ahora puedes ejecutar `GET /api/perfil/me` o `POST /api/perfil` directamente desde Swagger.
