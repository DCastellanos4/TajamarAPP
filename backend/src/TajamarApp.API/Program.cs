using Microsoft.EntityFrameworkCore;
using TajamarApp.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Cadena de conexión y DbContext (Azure SQL Server)
var connectionString = builder.Configuration.GetConnectionString("AzureSqlServer")
    ?? throw new InvalidOperationException("Cadena de conexión 'AzureSqlServer' no encontrada.");

builder.Services.AddDbContext<TajamarAppContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));

// 2. Controladores
builder.Services.AddControllers();

// 3. CORS para Angular
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

// 4. Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 5. Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAngular");

app.UseAuthorization();

app.MapControllers();

app.Run();
