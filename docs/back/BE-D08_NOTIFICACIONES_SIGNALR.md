# 🔔 BE-D08: Hub de Notificaciones en Tiempo Real con SignalR (.NET 10)

## 📌 Resumen de la Tarea
- **Responsable:** David (Back)
- **Estado:** ✅ Completada y Verificada en Vivo
- **GitHub Issue:** [#34](https://github.com/DCastellanos4/TajamarAPP/issues/34)
- **Objetivo:** Implementar un Hub bidireccional de WebSockets con SignalR en .NET 10 (`/hubs/notificaciones`) autenticado mediante JWT Bearer. Permite la comunicación reactiva e instantánea entre el backend y los clientes Web/Móvil (Angular 18+) sin necesidad de polling HTTP continuo, emitiendo notificaciones dirigidas al conductor cuando un alumno reserva plaza, al pasajero cuando es promocionado de la lista de espera FIFO a plaza confirmada, y a todos los pasajeros confirmados cuando un trayecto es cancelado.

---

## 🌐 Endpoint WebSocket y Negociación Handshake

### 1. Ruta y Protocolos
* **Ruta de Conexión:** `/hubs/notificaciones` (ej. `http://localhost:5050/hubs/notificaciones` o `wss://localhost:5050/hubs/notificaciones`)
* **Transportes Soportados:** `WebSockets` (prioritario de baja latencia), con fallback transparente a `Server-Sent Events` y `Long Polling`.
* **Seguridad:** Protegido con `[Authorize]` a nivel de clase en `NotificacionesHub : Hub<INotificacionClient>`.

### 2. Autenticación JWT Bearer en WebSockets
En navegadores estándar y clientes WebSockets, la API nativa de JavaScript/WebSocket no permite adjuntar cabeceras HTTP personalizadas como `Authorization: Bearer <token>` durante la fase inicial del handshake HTTP Upgrade.

Para solventar esta limitación técnica y mantener la máxima seguridad, se implementa la interceptación de tokens vía Query String en `TajamarApp.API/Program.cs`:

```csharp
// Configuración de autenticación para WebSockets de SignalR
options.Events = new JwtBearerEvents
{
    OnMessageReceived = context =>
    {
        var accessToken = context.Request.Query["access_token"];
        var path = context.HttpContext.Request.Path;
        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
        {
            context.Token = accessToken;
        }
        return Task.CompletedTask;
    }
};
```

### 3. Mapeo de Identidad: `CustomUserIdProvider`
Por defecto, SignalR utiliza `ClaimTypes.NameIdentifier` para asociar las conexiones al identificador de usuario (`Clients.User(id)`). En el ecosistema de Tajamar, el identificador principal en los claims del token es `idUsuarioTajamar`.

Se implementó el proveedor personalizado `TajamarApp.API/Hubs/CustomUserIdProvider.cs`:

```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace TajamarApp.API.Hubs;

public class CustomUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User?.FindFirst("idUsuarioTajamar")?.Value
            ?? connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? connection.User?.FindFirst("sub")?.Value;
    }
}
```

Registrado como `Singleton` en `Program.cs`:
```csharp
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
```

### 4. Ciclo de Vida y Agrupación en el Hub (`NotificacionesHub`)
Al conectarse el cliente (`OnConnectedAsync`):
1. Se extrae `Context.UserIdentifier` resuelto por `CustomUserIdProvider`.
2. Se une automáticamente al socket al grupo personal `usuario_{userId}`, permitiendo targeting individual o multi-dispositivo.
3. El cliente puede suscribirse o darse de baja dinámicamente de grupos asociados a trayectos específicos mediante los métodos del Hub:
   - `UnirseATrayecto(int trayectoId)`: Se añade a `trayecto_{trayectoId}`.
   - `SalirDeTrayecto(int trayectoId)`: Se retira de `trayecto_{trayectoId}`.

---

## 📡 Eventos Emitidos por el Servidor (`INotificacionClient`)

El Hub es fuertemente tipado utilizando la interfaz `INotificacionClient` definida en `TajamarApp.Core/Interfaces/INotificacionClient.cs`:

```csharp
namespace TajamarApp.Core.Interfaces;

public interface INotificacionClient
{
    Task RecibirNotificacion(NotificacionDto notificacion);
    Task NuevaReserva(NotificacionDto notificacion);
    Task PromocionCola(NotificacionDto notificacion);
    Task TrayectoCancelado(NotificacionDto notificacion);
}
```

Cada invocación emite en paralelo al método genérico `RecibirNotificacion` y al método específico (`NuevaReserva`, `PromocionCola`, `TrayectoCancelado`) mediante `Task.WhenAll`. Esto ofrece flexibilidad al frontend para suscribirse a un manejador global o a eventos individuales.

| Evento SignalR | Destinatario | Disparador en Backend | Propósito |
| :--- | :--- | :--- | :--- |
| `NuevaReserva` | Conductor del trayecto | `POST /api/trayectos/{id}/reservar` | Avisa de que un alumno ha ocupado una plaza en su viaje. |
| `PromocionCola` | Alumno en cola FIFO | `DELETE /api/reservas/{id}` | Notifica al siguiente alumno en espera que su plaza ha sido confirmada automáticamente. |
| `TrayectoCancelado` | Pasajeros confirmados | `DELETE /api/trayectos/{id}` | Alerta inmediata de viaje suspendido por el conductor. |
| `RecibirNotificacion` | Todos los anteriores | Todos los flujos | Canal unificado que recibe cualquier notificación del sistema. |

---

## 📦 Payload DTO (`NotificacionDto`) y Ejemplos JSON

### 1. Definición en C# (`TajamarApp.Core/DTOs/Notificaciones/NotificacionDto.cs`)
```csharp
namespace TajamarApp.Core.DTOs.Notificaciones;

public class NotificacionDto
{
    public string Tipo { get; set; } = string.Empty; // "NUEVA_RESERVA", "PROMOCION_COLA", "TRAYECTO_CANCELADO"
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object?> Datos { get; set; } = new();
}
```

### 2. Ejemplo JSON: `NUEVA_RESERVA`
Emitido al conductor cuando un alumno confirma su plaza:
```json
{
  "tipo": "NUEVA_RESERVA",
  "titulo": "¡Nueva plaza reservada!",
  "mensaje": "Sara Ulgazi ha reservado una plaza en tu trayecto de Vallecas Central a Tajamar.",
  "fecha": "2026-11-24T10:45:00.123Z",
  "datos": {
    "trayectoId": 4,
    "nombrePasajero": "Sara Ulgazi",
    "origen": "Vallecas Central",
    "destino": "Tajamar"
  }
}
```

### 3. Ejemplo JSON: `PROMOCION_COLA`
Emitido al primer alumno de la lista de espera FIFO cuando se cancela una plaza confirmada:
```json
{
  "tipo": "PROMOCION_COLA",
  "titulo": "¡Plaza confirmada!",
  "mensaje": "¡Buenas noticias! Se ha liberado una plaza y se ha confirmado tu reserva en el trayecto de Vallecas Central a Tajamar.",
  "fecha": "2026-11-24T10:46:12.456Z",
  "datos": {
    "trayectoId": 4,
    "origen": "Vallecas Central",
    "destino": "Tajamar"
  }
}
```

### 4. Ejemplo JSON: `TRAYECTO_CANCELADO`
Emitido a cada uno de los pasajeros confirmados si el conductor cancela el viaje:
```json
{
  "tipo": "TRAYECTO_CANCELADO",
  "titulo": "Trayecto cancelado",
  "mensaje": "El conductor ha cancelado el trayecto de Vallecas Central a Tajamar.",
  "fecha": "2026-11-24T10:48:30.789Z",
  "datos": {
    "trayectoId": 4,
    "origen": "Vallecas Central",
    "destino": "Tajamar"
  }
}
```

---

## 💻 Guía de Integración para Frontend (Angular 18+ con TypeScript)

Para los desarrolladores de Frontend (**Mónica** y **Harry**), a continuación se detalla la integración del cliente SignalR con TypeScript y Angular 18+.

### 1. Instalación de la Dependencia Oficial
```bash
npm install @microsoft/signalr
```

### 2. Modelo de Datos (`src/app/core/models/notificacion.model.ts`)
```typescript
export interface NotificacionDto {
  tipo: 'NUEVA_RESERVA' | 'PROMOCION_COLA' | 'TRAYECTO_CANCELADO' | string;
  titulo: string;
  mensaje: string;
  fecha: string;
  datos: {
    trayectoId?: number;
    nombrePasajero?: string;
    origen?: string;
    destino?: string;
    [key: string]: unknown;
  };
}
```

### 3. Servicio de Notificaciones Angular (`src/app/core/services/notification.service.ts`)
```typescript
import { Injectable, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
import { NotificacionDto } from '../models/notificacion.model';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private authService = inject(AuthService);
  private hubConnection: signalR.HubConnection | null = null;

  // Streams reactivos para componentes
  private notificacion$ = new Subject<NotificacionDto>();
  private nuevaReserva$ = new Subject<NotificacionDto>();
  private promocionCola$ = new Subject<NotificacionDto>();
  private trayectoCancelado$ = new Subject<NotificacionDto>();

  public get onNotificacion(): Observable<NotificacionDto> {
    return this.notificacion$.asObservable();
  }

  public get onNuevaReserva(): Observable<NotificacionDto> {
    return this.nuevaReserva$.asObservable();
  }

  public get onPromocionCola(): Observable<NotificacionDto> {
    return this.promocionCola$.asObservable();
  }

  public get onTrayectoCancelado(): Observable<NotificacionDto> {
    return this.trayectoCancelado$.asObservable();
  }

  /**
   * Inicia la conexión con el Hub de SignalR autenticado por JWT en Query Param
   */
  public async iniciarConexion(): Promise<void> {
    const token = this.authService.getToken();
    if (!token) {
      console.warn('SignalR: No se puede conectar sin token de autenticación.');
      return;
    }

    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      return;
    }

    const hubUrl = `${environment.apiUrl.replace('/api', '')}/hubs/notificaciones`;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => this.authService.getToken() ?? '',
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Information)
      .build();

    // Registro de escuchas de eventos
    this.hubConnection.on('RecibirNotificacion', (data: NotificacionDto) => {
      console.log('[SignalR] RecibirNotificacion:', data);
      this.notificacion$.next(data);
    });

    this.hubConnection.on('NuevaReserva', (data: NotificacionDto) => {
      console.log('[SignalR] NuevaReserva:', data);
      this.nuevaReserva$.next(data);
    });

    this.hubConnection.on('PromocionCola', (data: NotificacionDto) => {
      console.log('[SignalR] PromocionCola:', data);
      this.promocionCola$.next(data);
    });

    this.hubConnection.on('TrayectoCancelado', (data: NotificacionDto) => {
      console.log('[SignalR] TrayectoCancelado:', data);
      this.trayectoCancelado$.next(data);
    });

    try {
      await this.hubConnection.start();
      console.log('[SignalR] Conexión establecida con éxito al Hub de Notificaciones.');
    } catch (err) {
      console.error('[SignalR] Error al conectar con el Hub:', err);
    }
  }

  /**
   * Cierra limpiamente la conexión WebSocket
   */
  public async detenerConexion(): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.stop();
      this.hubConnection = null;
      console.log('[SignalR] Conexión cerrada.');
    }
  }

  /**
   * Suscribe el socket al canal en tiempo real de un trayecto concreto
   */
  public async unirseATrayecto(trayectoId: number): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('UnirseATrayecto', trayectoId);
    }
  }

  /**
   * Abandona el canal en tiempo real de un trayecto
   */
  public async salirDeTrayecto(trayectoId: number): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('SalirDeTrayecto', trayectoId);
    }
  }
}
```

### 4. Ejemplo de Consumo en un Componente Angular (`NavbarComponent` o `ToastService`)
```typescript
import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { Subscription } from 'rxjs';
import { NotificationService } from '../core/services/notification.service';
import { ToastService } from '../core/services/toast.service';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html'
})
export class NavbarComponent implements OnInit, OnDestroy {
  private notificationService = inject(NotificationService);
  private toastService = inject(ToastService);
  private sub = new Subscription();

  ngOnInit(): void {
    this.notificationService.iniciarConexion();

    this.sub.add(
      this.notificationService.onNuevaReserva.subscribe(notif => {
        this.toastService.showSuccess(notif.titulo, notif.mensaje);
      })
    );

    this.sub.add(
      this.notificationService.onPromocionCola.subscribe(notif => {
        this.toastService.showInfo(notif.titulo, notif.mensaje);
      })
    );

    this.sub.add(
      this.notificationService.onTrayectoCancelado.subscribe(notif => {
        this.toastService.showWarning(notif.titulo, notif.mensaje);
      })
    );
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }
}
```

---

## 🧪 Pruebas en Vivo (Live Verification)

| Escenario de Prueba | Acción / Entrada | Resultado Esperado | Resultado Obtenido |
| :--- | :--- | :--- | :--- |
| **Conexión WebSocket con JWT válido** | Handshake `GET /hubs/notificaciones?access_token=<jwt>` | HTTP 101 Switching Protocols. Conexión WebSocket establecida. | ✅ HTTP 101 - Conectado. Grupo `usuario_{id}` asignado. |
| **Conexión Anónima (Sin Token)** | Handshake sin query param ni token | HTTP 401 Unauthorized en el handshake. | ✅ 401 Unauthorized rechazado por middleware. |
| **Resolución de Usuario (`CustomUserIdProvider`)** | Token con claim `idUsuarioTajamar: 212` | `Context.UserIdentifier` resuelve `"212"`. | ✅ `Clients.User("212")` recibe los mensajes dirigidos. |
| **Evento `NuevaReserva`** | Pasajero (ID 195) reserva en `POST /api/trayectos/4/reservar` | Conductor (ID 212) recibe evento `NuevaReserva` y `RecibirNotificacion`. | ✅ Recibido instantáneamente con datos del trayecto y pasajero. |
| **Evento `PromocionCola`** | Pasajero cancela reserva `DELETE /api/reservas/{id}` con cola activa | Alumno promocionado (ID 196) recibe `PromocionCola` y `RecibirNotificacion`. | ✅ Recibido instantáneamente. Alumno alertado de plaza asignada. |
| **Evento `TrayectoCancelado`** | Conductor cancela viaje `DELETE /api/trayectos/4` | Todos los pasajeros con reserva confirmada reciben `TrayectoCancelado`. | ✅ Notificación simultánea a los 3 pasajeros confirmados. |
| **Suscripción de Grupo `UnirseATrayecto`** | Invocación `UnirseATrayecto(4)` desde cliente | Conexión agregada al grupo `trayecto_4`. | ✅ Grupo asignado en `HubConnectionContext`. |

---

## 🏗️ Arquitectura y Componentes Desarrollados

### 1. Capa Core (`TajamarApp.Core`)
- `DTOs/Notificaciones/NotificacionDto.cs`: Modelo de datos para notificaciones con tipo, título, mensaje, timestamp y diccionario tipado de metadatos.
- `Interfaces/INotificacionClient.cs`: Contrato fuertemente tipado para los clientes del Hub (`RecibirNotificacion`, `NuevaReserva`, `PromocionCola`, `TrayectoCancelado`).
- `Interfaces/INotificacionService.cs`: Interfaz del servicio emisor de notificaciones inyectable en controladores y servicios de infraestructura.

### 2. Capa API (`TajamarApp.API`)
- `Hubs/NotificacionesHub.cs`: Implementación de `Hub<INotificacionClient>` con `[Authorize]`, logging de conexión y soporte para grupos de trayecto.
- `Hubs/CustomUserIdProvider.cs`: Implementación de `IUserIdProvider` con resolución prioritaria de `idUsuarioTajamar`.
- `Services/NotificacionService.cs`: Implementación de `INotificacionService` que utiliza `IHubContext<NotificacionesHub, INotificacionClient>` para distribuir eventos dirigidos por usuario.
- `Program.cs`:
  - Registro de SignalR: `builder.Services.AddSignalR()`.
  - Registro de `IUserIdProvider`: `builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>()`.
  - Configuración de `JwtBearerEvents.OnMessageReceived` para extraer `?access_token=` en rutas `/hubs`.
  - Mapeo de endpoint: `app.MapHub<NotificacionesHub>("/hubs/notificaciones")`.

### 3. Capa Infrastructure (`TajamarApp.Infrastructure`)
- `Services/TrayectoService.cs`: Inyección de `INotificacionService` para disparar `NotificarTrayectoCanceladoAsync` al suspender un trayecto activo.
- `Features/Reservations/Reservations.cs`: Inyección de `INotificacionService` en Minimal API para emitir `NotificarNuevaReservaAsync` tras confirmar reserva y `NotificarPromocionColaAsync` tras auto-promover un candidato de la lista de espera FIFO.
