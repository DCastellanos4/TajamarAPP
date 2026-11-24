using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using TajamarApp.API.Features.Events;
using TajamarApp.API.Features.Reservations;
using TajamarApp.API.Infrastructure;
using TajamarApp.Core.Interfaces;
using TajamarApp.Infrastructure.Data;
using TajamarApp.Infrastructure.Services;

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

        // 1. Cadena de conexión y DbContext (Azure SQL Server)
        var connectionString = builder.Configuration.GetConnectionString(Database.ConnectionName)
            ?? throw new InvalidOperationException($"Configura ConnectionStrings:{Database.ConnectionName}.");

        builder.Services.AddDbContext<TajamarAppContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            }));

        // 2. Servicios de Utilidad e Infraestructura
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton<Database>();
        builder.Services.AddProblemDetails();

        // 3. Inyección de Dependencias de Servicios de Negocio
        builder.Services.AddScoped<IJwtService, JwtService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IPerfilService, PerfilService>();
        builder.Services.AddScoped<ITrayectoService, TrayectoService>();
        builder.Services.AddScoped<IReservaService, ReservaService>();

        // 4. Autenticación JWT Bearer
        var jwtSecret = builder.Configuration["JwtSettings:Secret"] 
            ?? builder.Configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException("Clave JWT no configurada (JwtSettings:Secret o Jwt:SigningKey).");
        var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] 
            ?? builder.Configuration["Jwt:Issuer"] 
            ?? "TajamarAppAPI";
        var jwtAudience = builder.Configuration["JwtSettings:Audience"] 
            ?? builder.Configuration["Jwt:Audience"] 
            ?? "TajamarAppClients";

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ClockSkew = TimeSpan.Zero
            };
        });

        // 5. Políticas de Autorización
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("GestionEventos", policy => 
                policy.RequireRole("GestorEventos", "Administrador"));
        });

        // 6. Controladores y Enrutamiento (URLs en minúsculas estándar para frontend)
        builder.Services.AddControllers();
        builder.Services.Configure<RouteOptions>(options =>
        {
            options.LowercaseUrls = true;
        });

        // 7. CORS para Angular
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAngular", policy =>
            {
                policy.WithOrigins("http://localhost:4200")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        // 8. Swagger / OpenAPI con soporte para Token Bearer
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Autenticación JWT. Introduce 'Bearer {token}'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT"
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer", document),
                    new List<string>()
                }
            });
        });

        var app = builder.Build();

        // 9. Pipeline HTTP
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

        // Mapeo de Controladores y Minimal API Route Groups
        app.MapControllers();
        app.MapGroup("/api").MapEvents();
        app.MapGroup("/api").RequireAuthorization().MapReservations();

        return app;
    }
}
