using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TajamarApp.Core.Interfaces;

namespace TajamarApp.API.Hubs;

[Authorize]
public class NotificacionesHub : Hub<INotificacionClient>
{
    private readonly ILogger<NotificacionesHub> _logger;

    public NotificacionesHub(ILogger<NotificacionesHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        _logger.LogInformation("SignalR: Usuario {UserId} conectado con ConnectionId {ConnectionId}", userId, Context.ConnectionId);

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"usuario_{userId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        _logger.LogInformation("SignalR: Usuario {UserId} desconectado", userId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Permite al cliente suscribirse al canal específico de un trayecto.
    /// </summary>
    public async Task UnirseATrayecto(int trayectoId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"trayecto_{trayectoId}");
        _logger.LogInformation("Connection {ConnectionId} unida a grupo trayecto_{TrayectoId}", Context.ConnectionId, trayectoId);
    }

    /// <summary>
    /// Permite al cliente salir del canal específico de un trayecto.
    /// </summary>
    public async Task SalirDeTrayecto(int trayectoId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"trayecto_{trayectoId}");
    }
}
