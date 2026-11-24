using System.Globalization;
using System.Text;
using TajamarApp.Core.DTOs.Trayectos;
using TajamarApp.Core.Interfaces;

namespace TajamarApp.Infrastructure.Services;

public class CalculadoraGastosService : ICalculadoraGastosService
{
    private const decimal ConsumoLitrosPor100Km = 6.5m;
    private const decimal PrecioLitroCombustible = 1.55m;
    private const decimal EmisionCo2KgPorKm = 0.120m; // 120 g CO2 / km
    private const decimal DistanciaPorDefectoKm = 20.0m; // Rango 15-25 km por defecto

    // Matriz de distancias estimadas (en Km) entre municipios/distritos de Madrid y el Campus Tajamar (Vallecas)
    private static readonly Dictionary<string, double> DistanciasTajamarKm = new(StringComparer.OrdinalIgnoreCase)
    {
        { "tajamar", 0.0 },
        { "vallecas", 4.0 },
        { "puente de vallecas", 3.5 },
        { "villa de vallecas", 5.0 },
        { "moratalaz", 4.5 },
        { "vicalvaro", 7.0 },
        { "san blas", 10.0 },
        { "canillejas", 11.0 },
        { "retiro", 6.5 },
        { "atocha", 7.5 },
        { "centro", 9.5 },
        { "sol", 9.5 },
        { "chamberi", 11.0 },
        { "salamanca", 8.5 },
        { "chamartin", 13.0 },
        { "tetuan", 13.5 },
        { "moncloa", 14.0 },
        { "ciudad lineal", 9.0 },
        { "hortaleza", 15.0 },
        { "barajas", 16.0 },
        { "fuencarral", 18.0 },
        { "carabanchel", 12.0 },
        { "usera", 9.0 },
        { "villaverde", 11.5 },
        { "latina", 13.0 },
        { "coslada", 13.0 },
        { "san fernando", 16.0 },
        { "san fernando de henares", 16.0 },
        { "rivas", 15.0 },
        { "rivas-vaciamadrid", 15.0 },
        { "arganda", 25.0 },
        { "arganda del rey", 25.0 },
        { "getafe", 16.0 },
        { "leganes", 17.0 },
        { "alcorcon", 21.0 },
        { "mostoles", 24.0 },
        { "fuenlabrada", 23.0 },
        { "parla", 26.0 },
        { "pinto", 22.0 },
        { "valdemoro", 28.0 },
        { "torrejon", 22.0 },
        { "torrejon de ardoz", 22.0 },
        { "alcala", 32.0 },
        { "alcala de henares", 32.0 },
        { "alcobendas", 22.0 },
        { "san sebastian de los reyes", 24.0 },
        { "pozuelo", 19.0 },
        { "pozuelo de alarcon", 19.0 },
        { "boadilla", 24.0 },
        { "boadilla del monte", 24.0 },
        { "majadahonda", 25.0 },
        { "las rozas", 27.0 },
        { "collado villalba", 45.0 }
    };

    public EstimacionCosteDto CalcularCoste(
        string? origen,
        string? destino,
        int plazasOcupadas,
        DateTime? fecha = null,
        string? telefonoBizum = null)
    {
        var distanciaKm = ObtenerDistanciaKm(origen, destino);
        var distDec = (decimal)distanciaKm;

        // Consumo 6.5 L / 100km * 1.55 €/L
        var costeTotal = Math.Round(distDec * (ConsumoLitrosPor100Km / 100m) * PrecioLitroCombustible, 2);

        // Reparto equitativo entre conductor y ocupantes: (PlazasOcupadas + 1)
        var totalPersonas = Math.Max(1, plazasOcupadas + 1);
        var costePorPlaza = Math.Round(costeTotal / totalPersonas, 2);

        // Formato de concepto Bizum: 'TajamarApp [Origen] - [Fecha]'
        var origenTexto = !string.IsNullOrWhiteSpace(origen) ? origen.Trim() : "Tajamar";
        var fechaReferencia = fecha ?? DateTime.UtcNow;
        var conceptoBizum = $"TajamarApp {origenTexto} - {fechaReferencia:dd/MM}";

        // Ahorro estimado vs Taxi tradicional en Madrid (~18 € mínimo o 4.00€ base + 1.25€/km)
        var costeTaxi = Math.Max(18.0m, Math.Round(4.0m + (distDec * 1.25m), 2));
        var ahorroVsTaxi = Math.Max(0m, Math.Round(costeTaxi - costePorPlaza, 2));

        // CO2 ahorrado: ~120 g/km por cada pasajero que no utiliza vehículo propio
        var pasajerosAhorradores = Math.Max(1, plazasOcupadas);
        var co2AhorradoKg = Math.Round(distDec * EmisionCo2KgPorKm * pasajerosAhorradores, 2);

        return new EstimacionCosteDto
        {
            DistanciaKm = Math.Round(distanciaKm, 1),
            CosteTotalTrayecto = costeTotal,
            PlazasOcupadas = plazasOcupadas,
            CostePorPlaza = costePorPlaza,
            ConceptoBizum = conceptoBizum,
            TelefonoBizum = telefonoBizum,
            AhorroEstimadoVsTaxi = ahorroVsTaxi,
            Co2AhorradoKg = co2AhorradoKg
        };
    }

    public double ObtenerDistanciaKm(string? origen, string? destino)
    {
        var origenNorm = NormalizarTexto(origen);
        var destinoNorm = NormalizarTexto(destino);

        // Si uno es Tajamar y el otro un lugar conocido
        if (EsTajamar(destinoNorm))
        {
            var dist = BuscarDistancia(origenNorm);
            if (dist.HasValue) return dist.Value;
        }

        if (EsTajamar(origenNorm))
        {
            var dist = BuscarDistancia(destinoNorm);
            if (dist.HasValue) return dist.Value;
        }

        // Si ninguno es explícitamente Tajamar pero uno coincide con la matriz respecto a Tajamar
        var distOrigen = BuscarDistancia(origenNorm);
        if (distOrigen.HasValue && distOrigen.Value > 0)
        {
            return distOrigen.Value;
        }

        var distDestino = BuscarDistancia(destinoNorm);
        if (distDestino.HasValue && distDestino.Value > 0)
        {
            return distDestino.Value;
        }

        return (double)DistanciaPorDefectoKm;
    }

    private static double? BuscarDistancia(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;

        foreach (var (clave, distancia) in DistanciasTajamarKm)
        {
            if (texto.Contains(clave, StringComparison.OrdinalIgnoreCase))
            {
                return distancia;
            }
        }
        return null;
    }

    private static bool EsTajamar(string texto)
    {
        return string.IsNullOrWhiteSpace(texto) ||
               texto.Contains("tajamar", StringComparison.OrdinalIgnoreCase) ||
               texto.Contains("vallecas", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizarTexto(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return string.Empty;

        var normalizedString = texto.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC).Trim().ToLowerInvariant();
    }
}
