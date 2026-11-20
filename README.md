# 🚗 Plataforma de Movilidad Compartida (Tajamar App)

Proyecto de aplicación de coche compartido (estilo BlaBlaCar) para la comunidad de Tajamar. Conecta a conductores y pasajeros tanto para trayectos habituales de clase como para eventos puntuales organizados en el centro.

---

## 👥 Integrantes del Equipo
- **David (Back)** - Setup de arquitectura, Autenticación/Perfil extendido, Trayectos y Búsqueda.
- **Mario (Back)** - Módulo CRUD de Eventos, Asistencia, Reservas y Motor de Cola de Espera (FIFO).
- **Mónica (Front)** - Setup Angular, Autenticación/Onboarding, Módulo de Eventos y Panel de Actividad.
- **Harry (Front)** - Design System, Buscador de Trayectos, Publicar Trayecto y Cola de Espera UI.

---

## 📁 Estructura del Repositorio
```
tajamarAPP/
├── backend/               # Proyecto .NET 8/9 Web API + SQL Server
├── frontend/              # Proyecto Angular 18+ (Standalone Components)
└── docs/                  # Documentación técnica obligatoria (.md)
    ├── KANBAN_TASKS.md    # Backlog detallado con criterios de aceptación
    └── import-issues.ps1  # Script para importar todas las tareas de golpe a GitHub Projects
```

---

## ⚡ Inicio Rápido (Importar tareas a GitHub Projects)
1. Instala GitHub CLI si no lo tienes: `winget install --id GitHub.cli`
2. Autentica tu cuenta: `gh auth login`
3. Ejecuta desde este directorio:
   ```powershell
   powershell -ExecutionPolicy Bypass -File .\docs\import-issues.ps1
   ```
