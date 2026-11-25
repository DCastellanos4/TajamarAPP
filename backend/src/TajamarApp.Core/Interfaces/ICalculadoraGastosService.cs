using TajamarApp.Core.DTOs.Trayectos;

namespace TajamarApp.Core.Interfaces;

public interface ICalculadoraGastosService
{
    EstimacionCosteDto CalcularCoste(string? origen, string? destino, int plazasOcupadas, DateTime? fecha = null, string? telefonoBizum = null);
    double ObtenerDistanciaKm(string? origen, string? destino);
}
