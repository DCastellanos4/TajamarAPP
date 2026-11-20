<#
.SYNOPSIS
    Script para importar las 20 tareas iniciales del proyecto TajamarApp directamente a GitHub Issues / Projects usando GitHub CLI (gh).

.DESCRIPTION
    Asegúrate de:
    1. Tener instalado GitHub CLI: winget install --id GitHub.cli
    2. Haber iniciado sesión: gh auth login
    3. Ejecutar este script desde la raíz de vuestro repositorio git.
#>

Write-Host "Verificando autenticación con GitHub CLI..." -ForegroundColor Cyan
gh auth status
if ($LASTEXITCODE -ne 0) {
    Write-Host "No has iniciado sesión en GitHub CLI. Ejecuta 'gh auth login' primero." -ForegroundColor Red
    exit 1
}

$issues = @(
    # --- DAVID (BACKEND) ---
    @{
        Title = "[BE-D01] Setup de Arquitectura .NET 8 y Configuración de Base de Datos"
        Labels = "backend,david"
        Body = "### Descripción`nInicialización de la solución de Backend siguiendo arquitectura limpia/en capas (API, Domain/Core, Infrastructure). Configuración del contexto de base de datos con Entity Framework Core apuntando a SQL Server y habilitando Swagger.`n`n### Criterios de Aceptación`n- [ ] Solución `.sln` creada con capas separadas (*API, Core, Infrastructure*).`n- [ ] Cadena de conexión a SQL Server configurada en `appsettings.json`.`n- [ ] `DbContext` configurado con EF Core mapeando tablas base y de la app.`n- [ ] Swagger / OpenAPI configurado y accesible en `/swagger`."
    },
    @{
        Title = "[BE-D02] Autenticación con API Paco y Gestión de Perfil Extendido (PerfilApp)"
        Labels = "backend,david"
        Body = "### Descripción`nIntegración con la API existente de Paco (`apicharlasalumnostajamartesting`) para validar credenciales y emisión de token JWT propio. Gestión de la tabla `PERFIL_APP` vinculada 1:1 a `USUARIOSTAJAMAR`.`n`n### Criterios de Aceptación`n- [ ] Endpoint `POST /api/auth/login` recibe usuario/password, valida contra API de Paco y emite JWT firmado.`n- [ ] El JWT contiene claims esenciales (`idUsuarioTajamar`, `email`, `nombre`).`n- [ ] Endpoint `GET /api/perfil/me` devuelve datos del usuario combinando Tajamar y `PERFIL_APP`.`n- [ ] Si no tiene `PERFIL_APP`, devuelve indicador `perfilCompletado: false`.`n- [ ] Endpoint `POST /api/perfil` permite completar datos de conductor o pasajero."
    },
    @{
        Title = "[BE-D03] CRUD de Trayectos (Recurrentes de Clase y Puntuales de Eventos)"
        Labels = "backend,david"
        Body = "### Descripción`nEndpoints para que los conductores puedan publicar, modificar y cancelar viajes. Soporte para viajes diarios de clase (con días de la semana y horarios) y viajes para eventos específicos.`n`n### Criterios de Aceptación`n- [ ] Endpoint `POST /api/trayectos` permite crear trayectos validando `EsConductor == true`.`n- [ ] Soporte para tipo *Recurrente* (días de semana L-V y turno mañana/tarde).`n- [ ] Soporte para tipo *Puntual* (asociado opcionalmente a un `EventoId` y fecha/hora exacta).`n- [ ] Endpoints `PUT /api/trayectos/{id}` y `DELETE /api/trayectos/{id}` para editar y cancelar trayectos."
    },
    @{
        Title = "[BE-D04] Motor de Búsqueda y Filtrado de Trayectos"
        Labels = "backend,david"
        Body = "### Descripción`nEndpoint para que los pasajeros encuentren viajes disponibles con filtros combinados.`n`n### Criterios de Aceptación`n- [ ] Endpoint `GET /api/trayectos` con paginación y filtros: `origen`, `destino`, `fecha`, `modalidad`.`n- [ ] Parámetro `?eventoId={id}` para filtrar trayectos creados para un evento específico.`n- [ ] Parámetro `?tipo=recurrente` para filtrar trayectos de rutina a clase.`n- [ ] Solo se retornan viajes en estado *Activo* con indicador de plazas restantes."
    },
    @{
        Title = "[BE-D05] Generación de Documentación Técnica en Markdown y Contratos Swagger"
        Labels = "backend,david"
        Body = "### Descripción`nRedacción de la documentación obligatoria del proyecto en ficheros `.md` y exportación de contratos para Frontend.`n`n### Criterios de Aceptación`n- [ ] Fichero `docs/ARQUITECTURA_BACKEND.md` detallando capas y patrones.`n- [ ] Fichero `docs/CONTRATOS_API.md` con ejemplos de payload JSON para cada endpoint.`n- [ ] Exportación de `swagger.json` en la carpeta `docs/`."
    },

    # --- MARIO (BACKEND) ---
    @{
        Title = "[BE-M01] Módulo CRUD de Eventos Tajamar"
        Labels = "backend,mario"
        Body = "### Descripción`nGestión completa de eventos de Tajamar (charlas, eventos de empresa, torneos, graduaciones) a los que se vincularán alumnos y trayectos.`n`n### Criterios de Aceptación`n- [ ] Endpoint `GET /api/eventos` lista eventos activos ordenados cronológicamente.`n- [ ] Endpoint `GET /api/eventos/{id}` devuelve detalle del evento con organizador e imagen.`n- [ ] Endpoints `POST /api/eventos` y `PUT /api/eventos/{id}` para alta y modificación.`n- [ ] Validación: la fecha del evento no puede ser anterior a la actual."
    },
    @{
        Title = "[BE-M02] Sistema de Asistencia e Inscripción a Eventos"
        Labels = "backend,mario"
        Body = "### Descripción`nFuncionalidad para que los alumnos de Tajamar puedan inscribirse o desapuntarse de los eventos organizados.`n`n### Criterios de Aceptación`n- [ ] Endpoint `POST /api/eventos/{id}/asistir` inscribe al alumno autenticado.`n- [ ] Endpoint `DELETE /api/eventos/{id}/asistir` cancela la inscripción.`n- [ ] Validación: control de unicidad (no duplicar inscripciones).`n- [ ] El endpoint de detalle de evento incluye el contador total y listado de inscritos."
    },
    @{
        Title = "[BE-M03] Motor de Reservas de Plazas en Trayectos"
        Labels = "backend,mario"
        Body = "### Descripción`nLógica transaccional para que un alumno reserve una plaza en un coche disponible.`n`n### Criterios de Aceptación`n- [ ] Endpoint `POST /api/trayectos/{id}/reservar` reserva una plaza para el usuario autenticado.`n- [ ] Validación: el conductor no puede reservar en su propio coche.`n- [ ] Validación: un pasajero no puede reservar más de una vez en el mismo viaje.`n- [ ] Si `PlazasDisponibles > 0`, se decrementa la plaza atómicamente y se confirma reserva.`n- [ ] Si `PlazasDisponibles == 0`, retorna HTTP 409 (debe ir a cola de espera)."
    },
    @{
        Title = "[BE-M04] Motor de Cola de Espera (FIFO) y Asignación Automática tras Cancelación"
        Labels = "backend,mario"
        Body = "### Descripción`nSistema de lista de espera para viajes llenos. Cuando un pasajero cancela, el primer usuario en cola recibe la plaza automáticamente.`n`n### Criterios de Aceptación`n- [ ] Endpoint `POST /api/trayectos/{id}/cola` inscribe en lista de espera con orden FIFO (`Posicion`).`n- [ ] Endpoint `DELETE /api/reservas/{id}` permite cancelar una reserva.`n- [ ] Transacción: si existen usuarios en cola, se toma al de `Posicion = 1`, se crea su reserva y se marca como *Promocionado*.`n- [ ] Si la cola está vacía, se repone la plaza disponible (`PlazasDisponibles + 1`)."
    },
    @{
        Title = "[BE-M05] Seeders de Datos de Prueba y Pruebas Unitarias/Integración"
        Labels = "backend,mario"
        Body = "### Descripción`nPreparación de scripts para poblar la base de datos local y tests de concurrencia.`n`n### Criterios de Aceptación`n- [ ] Seeder de BBDD con cursos de Tajamar, 3 eventos de prueba y usuarios simulados.`n- [ ] Pruebas unitarias de la lógica FIFO de la cola de espera.`n- [ ] Prueba de concurrencia para evitar sobreventa simultánea de plazas."
    },

    # --- MÓNICA (FRONTEND) ---
    @{
        Title = "[FE-M01] Setup Inicial de Angular 18+, Enrutamiento e Interceptores HTTP"
        Labels = "frontend,monica"
        Body = "### Descripción`nInicialización del proyecto Angular Standalone, configuración de estilos (Tailwind / Angular Material) y capa HTTP.`n`n### Criterios de Aceptación`n- [ ] Proyecto Angular generado y estructurado por módulos (*auth, eventos, trayectos, perfil*).`n- [ ] `AuthInterceptor` configurado para adjuntar el JWT en cada petición HTTP.`n- [ ] `AuthGuard` implementado para proteger rutas privadas.`n- [ ] Sistema de notificaciones toast para feedback al usuario."
    },
    @{
        Title = "[FE-M02] Vistas de Login con API Tajamar y Onboarding de Perfil App"
        Labels = "frontend,monica"
        Body = "### Descripción`nPantalla de inicio de sesión con credenciales de Tajamar y pantalla de onboarding tras el primer login para completar el perfil extendido.`n`n### Criterios de Aceptación`n- [ ] Formulario reactivo de Login con validación visual.`n- [ ] Al loguear, si `perfilCompletado: false`, redirige automáticamente a `/onboarding`.`n- [ ] Formulario de Onboarding con toggle para conductores (marca, modelo, matrícula, plazas).`n- [ ] Persistencia de sesión en storage local."
    },
    @{
        Title = "[FE-M03] Catálogo y Vista Detallada de Eventos Tajamar"
        Labels = "frontend,monica"
        Body = "### Descripción`nPantallas para explorar los eventos de Tajamar y consultar su información detallada.`n`n### Criterios de Aceptación`n- [ ] Grid responsive de tarjetas de eventos (imagen, fecha, título y lugar).`n- [ ] Filtro y buscador por nombre o fecha del evento.`n- [ ] Vista `/eventos/:id` con descripción extendida, ponente y contador de inscritos."
    },
    @{
        Title = "[FE-M04] Integración de Asistencia a Eventos y Bloque 'Coches Disponibles'"
        Labels = "frontend,monica"
        Body = "### Descripción`nBotón de inscripción al evento y componente que muestra los vehículos que van a dicho evento.`n`n### Criterios de Aceptación`n- [ ] Botón interactivo: **'Apuntarme al evento'** / **'Cancelar asistencia'** con feedback visual.`n- [ ] Sección en detalle del evento: *'Coches que van a este evento'* con plazas libres.`n- [ ] Botón *'¿Vas en coche? Publica tu viaje'* que precarga el evento en el formulario de conductor."
    },
    @{
        Title = "[FE-M05] Panel 'Mi Actividad / Mis Viajes'"
        Labels = "frontend,monica"
        Body = "### Descripción`nVista centralizada donde el alumno gestiona sus viajes y eventos.`n`n### Criterios de Aceptación`n- [ ] Pestaña *'Mis Reservas'*: lista de viajes donde el usuario es pasajero, con botón para cancelar.`n- [ ] Pestaña *'Mis Eventos'*: eventos a los que se ha apuntado.`n- [ ] Pestaña *'Mis Viajes Publicados'* (solo conductores): lista de trayectos y pasajeros confirmados."
    },

    # --- HARRY (FRONTEND) ---
    @{
        Title = "[FE-H01] Design System Base y Componentes Compartidos"
        Labels = "frontend,harry"
        Body = "### Descripción`nCreación de componentes reutilizables transversales a toda la plataforma.`n`n### Criterios de Aceptación`n- [ ] Navbar responsive con logo, navegación principal y avatar del usuario.`n- [ ] Componente `BadgePlazas` con colores semánticos (Verde, Naranja, Rojo).`n- [ ] Modales de confirmación reutilizables y estados de carga (skeletons/spinners)."
    },
    @{
        Title = "[FE-H02] Buscador Principal de Trayectos con Filtros"
        Labels = "frontend,harry"
        Body = "### Descripción`nPágina principal del módulo de movilidad (BlaBlaCar de Tajamar) con buscador y filtros avanzados.`n`n### Criterios de Aceptación`n- [ ] Tabs para alternar entre *'Trayectos diarios de clase'* y *'Trayectos para eventos'*.`n- [ ] Filtros por origen, fecha, horario y modalidad (Ida / Vuelta).`n- [ ] Tarjetas con avatar del conductor, coche, horario y plazas disponibles.`n- [ ] Mensaje informativo amigable cuando no se encuentran resultados."
    },
    @{
        Title = "[FE-H03] Formulario Interactivo para Publicar Trayecto (Conductor)"
        Labels = "frontend,harry"
        Body = "### Descripción`nFlujo para que los conductores publiquen viajes en la plataforma.`n`n### Criterios de Aceptación`n- [ ] Accesible solo para usuarios con perfil de conductor activo.`n- [ ] Selector de modalidad: viaje de clase (días L-V) o puntual de evento (dropdown de eventos).`n- [ ] Inputs de origen, destino (por defecto Tajamar), fecha/hora y número de plazas.`n- [ ] Validación de formulario con toasts de confirmación."
    },
    @{
        Title = "[FE-H04] Vista Detalle del Trayecto y Flujo de Reserva de Plaza"
        Labels = "frontend,harry"
        Body = "### Descripción`nPantalla informativa de un viaje seleccionado y acción para reservar asiento.`n`n### Criterios de Aceptación`n- [ ] Ruta `/trayectos/:id` con itinerario, información del coche y perfil del conductor.`n- [ ] Botón destacado **'Reservar Plaza'** con modal de confirmación.`n- [ ] Feedback visual de reserva confirmada y redirección a 'Mis Viajes'."
    },
    @{
        Title = "[FE-H05] Integración de la Cola de Espera en la UI"
        Labels = "frontend,harry"
        Body = "### Descripción`nExperiencia de usuario para vehículos completos y gestión de turnos de espera.`n`n### Criterios de Aceptación`n- [ ] Si `PlazasDisponibles == 0`, el botón se transforma en **'Unirme a la lista de espera'**.`n- [ ] Modal informativo explicando el sistema automático de promociones al producirse bajas.`n- [ ] Componente visual de turno: *'Estás en la posición #X de la lista de espera'* con opción para salir."
    }
)

Write-Host "Creando $($issues.Count) issues en el repositorio de GitHub..." -ForegroundColor Yellow

foreach ($item in $issues) {
    Write-Host "Creando Issue: $($item.Title)" -ForegroundColor Green
    gh issue create --title $item.Title --body $item.Body --label $item.Labels
}

Write-Host "`n¡Todas las tareas se han creado exitosamente en GitHub!" -ForegroundColor Cyan
