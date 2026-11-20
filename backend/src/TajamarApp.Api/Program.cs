using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using TajamarApp.Infrastructure.Data;

await TajamarApplication.Build(args).RunAsync();

public static class TajamarApplication
{
    public static WebApplication Build(string[] args, Action<WebApplicationBuilder>? configure = null)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            ApplicationName = typeof(TajamarApplication).Assembly.GetName().Name
        });
        configure?.Invoke(builder);

        var connectionString = builder.Configuration.GetConnectionString(Database.ConnectionName);
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException($"Configura ConnectionStrings:{Database.ConnectionName}.");

        builder.Services.AddDbContext<TajamarAppContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton<Database>();
        builder.Services.AddProblemDetails();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddCors(options => options.AddPolicy("AllowAngular", policy =>
            policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));
        builder.Services.AddAuthentication("Bearer")
            .AddScheme<AuthenticationSchemeOptions, TeamJwt>("Bearer", null);
        builder.Services.AddAuthorization(options =>
            options.AddPolicy("GestionEventos", policy => policy.RequireRole("GestorEventos", "Administrador")));

        var app = builder.Build();
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseCors("AllowAngular");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapGroup("/api").MapEvents();
        app.MapGroup("/api").RequireAuthorization().MapReservations();
        return app;
    }
}
