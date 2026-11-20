# 🛠️ Backend - Tajamar App (.NET 8/9 Web API)

## 📌 Responsables
- **David (Back)**
- **Mario (Back)**

## 🚀 Tecnologías
- **Framework:** .NET 8 / 9 Web API
- **ORM:** Entity Framework Core
- **Base de Datos:** Microsoft SQL Server
- **Autenticación:** JWT + Validación contra API de Paco (`apicharlasalumnostajamartesting`)
- **Documentación:** Swagger / OpenAPI + Markdown

## 📁 Estructura Recomendada
```
backend/
├── src/
│   ├── TajamarApp.API/            # Controladores, Program.cs, Middleware
│   ├── TajamarApp.Core/           # Entidades de dominio, Interfaces, DTOs
│   └── TajamarApp.Infrastructure/ # DbContext, Repositorios, Migraciones EF Core
└── tests/
    └── TajamarApp.Tests/          # Pruebas unitarias y de integración
```

Consulta el backlog completo de tareas en [docs/KANBAN_TASKS.md](../docs/KANBAN_TASKS.md).

Consulta [implementación de eventos, asistencia, reservas y cola](README-MARIO.md) para configurar la conexión de BE-M01 a BE-M05 y ejecutar sus pruebas.
