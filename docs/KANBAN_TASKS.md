# 🚗 Plataforma de Movilidad Compartida (Tajamar App)

## 📋 Backlog de Tareas y Criterios de Aceptación (Kanban)

Este documento contiene la definición completa de las tareas iniciales repartidas equitativamente entre los integrantes del equipo:
- **Backend (.NET):** David (Back) y Mario (Back)
- **Frontend (Angular):** Mónica (Front) y Harry (Front)

---

## 👥 Equipo Backend (.NET 8/9 + SQL Server)

### 👤 David (Backend)

#### [BE-D01] Setup de Arquitectura .NET 8 y Configuración de Base de Datos
- **Descripción:** Inicialización de la solución de Backend siguiendo una arquitectura limpia/por capas (API, Domain/Core, Infrastructure). Configuración del contexto de base de datos con Entity Framework Core apuntando a SQL Server y habilitando Swagger.
- **Criterios de Aceptación:**
  - [ ] Solución `.sln` creada con capas separadas (*API, Core, Infrastructure*).
  - [ ] Cadena de conexión a SQL Server configurada en `appsettings.json`.
  - [ ] `DbContext` configurado con EF Core mapeando las tablas base de Tajamar y tablas propias de la app.
  - [ ] Swagger / OpenAPI configurado y accesible en `/swagger` en entorno de desarrollo.

#### [BE-D02] Autenticación con API Paco y Gestión de Perfil Extendido (PerfilApp)
- **Descripción:** Integración con la API existente de Paco (`apicharlasalumnostajamartesting`) para validar credenciales y emisión de token JWT propio. Gestión de la tabla `PERFIL_APP` vinculada 1:1 a `USUARIOSTAJAMAR`.
- **Criterios de Aceptación:**
  - [ ] Endpoint `POST /api/auth/login` recibe usuario/password, valida contra la API de Paco y emite un JWT firmado.
  - [ ] El JWT contiene claims esenciales (`idUsuarioTajamar`, `email`, `nombre`).
  - [ ] Endpoint `GET /api/perfil/me` devuelve datos del usuario combinando Tajamar y `PERFIL_APP`.
  - [ ] Si el usuario aún no tiene `PERFIL_APP`, devuelve indicador `perfilCompletado: false`.
  - [ ] Endpoint `POST /api/perfil` permite completar datos de conductor (coche, matrícula, plazas) o pasajero (teléfono, preferencias).

#### [BE-D03] CRUD de Trayectos (Recurrentes de Clase y Puntuales de Eventos)
- **Descripción:** Endpoints para que los conductores puedan publicar, modificar y cancelar viajes. Soporte para viajes diarios de clase (con días de la semana y horarios habituales) y viajes para eventos específicos de Tajamar.
- **Criterios de Aceptación:**
  - [ ] Endpoint `POST /api/trayectos` permite crear trayectos validando que el usuario tenga `EsConductor == true`.
  - [ ] Soporte para tipo *Recurrente* (especificando días de la semana: L, M, X, J, V y turno mañana/tarde).
  - [ ] Soporte para tipo *Puntual* (asociado opcionalmente a un `EventoId` y fecha/hora exacta).
  - [ ] Endpoints `PUT /api/trayectos/{id}` y `DELETE /api/trayectos/{id}` para editar y cancelar trayectos propios.

#### [BE-D04] Motor de Búsqueda y Filtrado de Trayectos
- **Descripción:** Endpoint público/protegido para que los pasajeros encuentren viajes disponibles con filtros combinados.
- **Criterios de Aceptación:**
  - [ ] Endpoint `GET /api/trayectos` con paginación y filtros: `origen`, `destino`, `fecha`, `modalidad` (*SoloIda, SoloVuelta, IdaYVuelta*).
  - [ ] Parámetro `?eventoId={id}` para filtrar trayectos creados específicamente para asistir a un evento.
  - [ ] Parámetro `?tipo=recurrente` para filtrar trayectos de rutina a clase.
  - [ ] Solo se retornan viajes en estado *Activo* con al menos 1 plaza disponible (o todos indicando si están llenos).

#### [BE-D05] Generación de Documentación Técnica en Markdown y Contratos Swagger
- **Descripción:** Redacción de la documentación obligatoria del proyecto en ficheros `.md` y exportación de contratos para Frontend.
- **Criterios de Aceptación:**
  - [ ] Fichero `docs/ARQUITECTURA_BACKEND.md` detallando estructura de capas, inyección de dependencias y patrones.
  - [ ] Fichero `docs/CONTRATOS_API.md` con ejemplos de payload JSON para cada endpoint.
  - [ ] Exportación actualizada de `swagger.json` en la carpeta `docs/`.

---

### 👤 Mario (Backend)

#### [BE-M01] Módulo CRUD de Eventos Tajamar
- **Descripción:** Gestión completa de eventos de Tajamar (charlas, eventos de empresa, torneos, graduaciones) a los que se vincularán los alumnos y trayectos.
- **Criterios de Aceptación:**
  - [ ] Endpoint `GET /api/eventos` lista eventos activos ordenados cronológicamente.
  - [ ] Endpoint `GET /api/eventos/{id}` devuelve detalle del evento con datos del organizador e imagen.
  - [ ] Endpoints `POST /api/eventos` y `PUT /api/eventos/{id}` para alta y modificación de eventos.
  - [ ] Validación: la fecha del evento no puede ser anterior a la fecha actual.

#### [BE-M02] Sistema de Asistencia e Inscripción a Eventos
- **Descripción:** Funcionalidad para que los alumnos de Tajamar puedan inscribirse o desapuntarse de los eventos organizados.
- **Criterios de Aceptación:**
  - [ ] Endpoint `POST /api/eventos/{id}/asistir` inscribe al alumno autenticado en el evento.
  - [ ] Endpoint `DELETE /api/eventos/{id}/asistir` cancela la inscripción del alumno.
  - [ ] Validación: control de unicidad para evitar que un alumno se apunte dos veces al mismo evento.
  - [ ] El endpoint de detalle de evento incluye el contador total y listado de asistentes.

#### [BE-M03] Motor de Reservas de Plazas en Trayectos
- **Descripción:** Lógica transaccional para que un alumno reserve una plaza en un coche disponible.
- **Criterios de Aceptación:**
  - [ ] Endpoint `POST /api/trayectos/{id}/reservar` reserva una plaza para el usuario autenticado.
  - [ ] Validación: el conductor del trayecto no puede reservar plaza en su propio coche.
  - [ ] Validación: un pasajero no puede reservar más de una vez en el mismo trayecto.
  - [ ] Si `PlazasDisponibles > 0`, se decrementa la plaza atómicamente y se genera la reserva confirmada.
  - [ ] Si `PlazasDisponibles == 0`, la petición retorna HTTP 409 indicando que debe apuntarse a la cola de espera.

#### [BE-M04] Motor de Cola de Espera (FIFO) y Asignación Automática tras Cancelación
- **Descripción:** Sistema de lista de espera para viajes llenos. Cuando un pasajero cancela, el primer usuario en cola recibe la plaza automáticamente.
- **Criterios de Aceptación:**
  - [ ] Endpoint `POST /api/trayectos/{id}/cola` inscribe al usuario en la lista de espera con orden FIFO (`Posicion` incremental).
  - [ ] Endpoint `DELETE /api/reservas/{id}` permite a un pasajero cancelar su reserva.
  - [ ] Transacción SQL/EF Core: al cancelar, si existen usuarios en cola, se toma al usuario con `Posicion = 1`, se le crea la reserva confirmada y se actualiza su estado a *Promocionado*.
  - [ ] Si la cola está vacía, la plaza se suma nuevamente (`PlazasDisponibles + 1`).

#### [BE-M05] Seeders de Datos de Prueba y Pruebas Unitarias/Integración
- **Descripción:** Preparación de scripts para poblar la base de datos local y tests de concurrencia.
- **Criterios de Aceptación:**
  - [ ] Seeder de base de datos con cursos de Tajamar, 3 eventos de prueba y varios usuarios simulados.
  - [ ] Pruebas unitarias de la lógica FIFO de la cola de espera.
  - [ ] Prueba de estrés/concurrencia para asegurar que no se sobrevenden plazas al reservar simultáneamente.

---

## 👥 Equipo Frontend (Angular 18+ Standalone)

### 👩 Mónica (Frontend)

#### [FE-M01] Setup Inicial de Angular 18+, Enrutamiento e Interceptores HTTP
- **Descripción:** Inicialización del proyecto Angular con Standalone Components, configuración de estilos (Tailwind CSS / Angular Material), estructura de carpetas y capa HTTP.
- **Criterios de Aceptación:**
  - [ ] Proyecto Angular generado y estructurado por módulos funcionales (*auth, eventos, trayectos, perfil*).
  - [ ] `AuthInterceptor` configurado para adjuntar el JWT en el header `Authorization: Bearer <token>`.
  - [ ] `AuthGuard` implementado para proteger rutas privadas contra accesos no autenticados.
  - [ ] Sistema de notificaciones toast o alerts para feedback de operaciones.

#### [FE-M02] Vistas de Login con API Tajamar y Onboarding de Perfil App
- **Descripción:** Pantalla de inicio de sesión con credenciales de Tajamar y pantalla de onboarding tras el primer login para completar el perfil extendido.
- **Criterios de Aceptación:**
  - [ ] Formulario reactivo de Login con validación visual de campos.
  - [ ] Al loguear, si el backend indica que no tiene perfil de la app, redirige automáticamente a `/onboarding`.
  - [ ] Formulario de Onboarding con toggle *"¿Vas a ofrecer viajes como conductor?"*; si se activa, solicita marca, modelo, matrícula y plazas.
  - [ ] Persistencia del token en `localStorage` o `sessionStorage`.

#### [FE-M03] Catálogo y Vista Detallada de Eventos Tajamar
- **Descripción:** Pantallas para explorar los eventos de Tajamar y consultar su información detallada.
- **Criterios de Aceptación:**
  - [ ] Grid responsive de tarjetas de eventos mostrando imagen, fecha, título y lugar.
  - [ ] Filtro y buscador por nombre o fecha del evento.
  - [ ] Vista de detalle `/eventos/:id` con descripción extendida, ponente y contador de inscritos.

#### [FE-M04] Integración de Asistencia a Eventos y Bloque "Coches Disponibles"
- **Descripción:** Botón de inscripción al evento y componente que muestra los vehículos que van a dicho evento.
- **Criterios de Aceptación:**
  - [ ] Botón interactivo en el detalle: **"Apuntarme al evento"** / **"Cancelar asistencia"** con cambio de estado visual.
  - [ ] Sección integrada en el evento: *"Coches que van a este evento"*.
  - [ ] Muestra tarjetas resumidas con las plazas libres de los conductores que van a ese evento.
  - [ ] Botón *"¿Vas en coche a este evento? Publica tu viaje"* que precarga el evento en el formulario de conductor.

#### [FE-M05] Panel "Mi Actividad / Mis Viajes"
- **Descripción:** Vista centralizada donde el alumno gestiona sus viajes y eventos en los que participa.
- **Criterios de Aceptación:**
  - [ ] Pestaña *"Mis Reservas"*: lista de viajes donde el usuario es pasajero, con botón para cancelar la plaza.
  - [ ] Pestaña *"Mis Eventos"*: eventos a los que se ha apuntado.
  - [ ] Pestaña *"Mis Viajes Publicados"* (visible solo si es conductor): lista de trayectos creados y sus pasajeros confirmados.

---

### 👨 Harry (Frontend)

#### [FE-H01] Design System Base y Componentes Compartidos
- **Descripción:** Creación de componentes reutilizables transversales a toda la plataforma.
- **Criterios de Aceptación:**
  - [ ] Navbar principal responsive con navegación clara (*Eventos*, *Buscar Viaje*, *Publicar Viaje*, *Mi Perfil*).
  - [ ] Componente `BadgePlazas` con colores semánticos (Verde: plazas disponibles, Naranja: última plaza, Rojo: coche lleno).
  - [ ] Modales de confirmación reutilizables y estados de carga (skeletons/spinners).

#### [FE-H02] Buscador Principal de Trayectos con Filtros
- **Descripción:** Página principal del módulo de movilidad (BlaBlaCar de Tajamar) con buscador y filtros avanzados.
- **Criterios de Aceptación:**
  - [ ] Pestañas para alternar entre *"Trayectos diarios de clase"* y *"Trayectos para eventos"*.
  - [ ] Filtros por origen, fecha, horario y tipo de trayecto (*Solo Ida, Solo Vuelta, Ida y Vuelta*).
  - [ ] Tarjetas de resultado con avatar del conductor, coche, horario, precio (0€/compartido) y plazas.
  - [ ] Mensaje informativo amigable cuando no se encuentran trayectos para los filtros seleccionados.

#### [FE-H03] Formulario Interactivo para Publicar Trayecto (Conductor)
- **Descripción:** Flujo para que los conductores publiquen viajes en la plataforma.
- **Criterios de Aceptación:**
  - [ ] Control de acceso: solo accesible para usuarios con perfil de conductor activo.
  - [ ] Selector de modalidad: viaje recurrente de clase (selección de días L-V) o puntual de evento (dropdown de eventos).
  - [ ] Inputs de origen, destino (por defecto Tajamar), fecha/hora y número de plazas disponibles.
  - [ ] Validación de formulario antes de enviar al backend con toasts de éxito.

#### [FE-H04] Vista Detalle del Trayecto y Flujo de Reserva de Plaza
- **Descripción:** Pantalla informativa de un viaje seleccionado y acción para reservar asiento.
- **Criterios de Aceptación:**
  - [ ] Ruta `/trayectos/:id` con detalle de itinerario, información del coche y perfil del conductor.
  - [ ] Botón destacado **"Reservar Plaza"** con modal de confirmación.
  - [ ] Feedback visual de reserva completada y enlace directo para verlo en *"Mis Viajes"*.

#### [FE-H05] Integración de la Cola de Espera en la UI
- **Descripción:** Experiencia de usuario para vehículos completos y gestión de turnos de espera.
- **Criterios de Aceptación:**
  - [ ] Si `PlazasDisponibles == 0`, el botón de reservar se transforma dinámicamente en **"Unirme a la lista de espera"**.
  - [ ] Modal informativo que explica cómo funciona la lista de espera automática al producirse una baja.
  - [ ] Componente visual de turno: *"Estás en la posición #X de la lista de espera"*.
  - [ ] Opción para salir de la lista de espera en cualquier momento.
