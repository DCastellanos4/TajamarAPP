using Microsoft.AspNetCore.Authentication;

await TajamarApplication.Build(args).RunAsync();

public static class TajamarApplication
{
    public static WebApplication Build(string[] args, Action<WebApplicationBuilder>? configure = null)
    {
        var builder = WebApplication.CreateBuilder(args);
        configure?.Invoke(builder);
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton<Database>();
        builder.Services.AddProblemDetails();
        builder.Services.AddAuthentication("Bearer")
            .AddScheme<AuthenticationSchemeOptions, TeamJwt>("Bearer", null);
        builder.Services.AddAuthorization(options =>
            options.AddPolicy("GestionEventos", policy => policy.RequireRole("GestorEventos", "Administrador")));

        var app = builder.Build();
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapGroup("/api").MapEvents();
        app.MapGroup("/api").RequireAuthorization().MapReservations();
        return app;
    }
}
