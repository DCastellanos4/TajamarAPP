# 🎨 Frontend - Tajamar App (Angular 18+ Standalone)

## 📌 Responsables
- **Mónica (Front)**
- **Harry (Front)**

## 🚀 Tecnologías
- **Framework:** Angular 18+ (Standalone Components & Signals)
- **Estilos:** Tailwind CSS o Angular Material
- **Comunicación:** HttpClient con Interceptor JWT
- **Estado/Gestión:** Angular Signals / RxJS

## 📁 Estructura Recomendada
```
frontend/
└── src/
    └── app/
        ├── core/            # Servicios singleton, Interceptores, Guards, Modelos
        ├── shared/          # Componentes reutilizables (Navbar, Cards, Badges, Modales)
        └── features/
            ├── auth/        # Login y Onboarding de Perfil App
            ├── eventos/     # Catálogo y detalle de eventos Tajamar
            ├── trayectos/   # Buscador, detalle y publicar trayectos
            └── perfil/      # Panel 'Mis Viajes' y datos personales
```

Consulta el backlog completo de tareas en [docs/KANBAN_TASKS.md](../docs/KANBAN_TASKS.md).
