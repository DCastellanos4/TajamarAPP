# 💶 BE-D10: Calculadora Automática de Coste de Combustible, Bizum y Ahorro

## 📌 Resumen de la Tarea
- **Responsables:** David (Back) y Mario (Back)
- **Estado:** ✅ Completada y Verificada en Vivo
- **GitHub Issue:** [#36](https://github.com/DCastellanos4/TajamarAPP/issues/36)
- **Objetivo:** Implementar un servicio desacoplado y reactivo de estimación económica y ecológica para los trayectos de Tajamar App. El módulo calcula automáticamente el gasto de carburante según la distancia real al campus de Tajamar, divide equitativamente el coste entre el conductor y los ocupantes, genera un concepto estandarizado para pagos vía Bizum (incluyendo el teléfono del conductor), cuantifica el ahorro frente a servicios comerciales como taxis tradicionales de Madrid y proyecta la reducción en huella de carbono (kg de CO2 no emitidos).

---

## 🧮 Algoritmo y Fórmulas Matemáticas

La lógica reside en el servicio `CalculadoraGastosService` (`TajamarApp.Infrastructure/Services/CalculadoraGastosService.cs`) bajo la interfaz `ICalculadoraGastosService`:

### 1. Parámetros de Referencia
* **Consumo medio estimado:** $6.5\text{ Litros} / 100\text{ km}$ ($0.065\text{ L/km}$).
* **Precio medio de carburante:** $1.55\text{ € / Litro}$.
* **Factor de emisión CO2 evitado:** $0.120\text{ kg CO}_2 / \text{km}$ ($120\text{ g/km}$) por cada vehículo que se retira de la carretera al compartir viaje.
* **Distancia por defecto en la Comunidad de Madrid:** $20.0\text{ km}$ (para orígenes no catalogados en la matriz).

### 2. Matriz Inteligente de Distancias al Campus Tajamar
El sistema cuenta con una matriz de distancias en kilómetros normalizada frente a acentos y mayúsculas/minúsculas para los principales municipios y distritos de Madrid:

| Localización / Distrito | Distancia al Campus Tajamar |
| :--- | :--- |
| **Vallecas / Campus Tajamar** | $0.0\text{ km}$ a $4.0\text{ km}$ |
| **Moratalaz / Vicálvaro** | $4.5\text{ km}$ - $7.0\text{ km}$ |
| **Retiro / Atocha** | $6.5\text{ km}$ - $7.5\text{ km}$ |
| **Centro / Sol / Salamanca** | $8.5\text{ km}$ - $9.5\text{ km}$ |
| **Chamberí / Usera / Ciudad Lineal** | $9.0\text{ km}$ - $11.0\text{ km}$ |
| **Carabanchel / Latina / Chamartín** | $12.0\text{ km}$ - $13.5\text{ km}$ |
| **Rivas-Vaciamadrid / Coslada** | $13.0\text{ km}$ - $15.0\text{ km}$ |
| **Getafe / Leganés / San Fernando** | $16.0\text{ km}$ - $17.0\text{ km}$ |
| **Alcorcón / Móstoles / Fuenlabrada** | $21.0\text{ km}$ - $24.0\text{ km}$ |
| **Torrejón / Alcobendas / Pozuelo** | $19.0\text{ km}$ - $22.0\text{ km}$ |
| **Arganda del Rey / Boadilla / Las Rozas** | $24.0\text{ km}$ - $27.0\text{ km}$ |
| **Alcalá de Henares / Collado Villalba** | $32.0\text{ km}$ - $45.0\text{ km}$ |

### 3. Fórmulas de Cálculo

1. **Gasto Total del Trayecto:**
   $$\text{CosteTotal} = \text{Round}\left(\text{DistanciaKm} \times 0.065 \times 1.55, 2\right)$$

2. **Reparto Equitativo por Persona (Conductor + Pasajeros Ocupantes):**
   $$\text{PersonasTotales} = \max(1, \text{PlazasOcupadas} + 1)$$
   $$\text{CostePorPlaza} = \text{Round}\left(\frac{\text{CosteTotal}}{\text{PersonasTotales}}, 2\right)$$

3. **Formato del Concepto Bizum:**
   $$\text{ConceptoBizum} = \text{"TajamarApp [Origen] - [dd/MM]"}$$
   *(Ejemplo: `"TajamarApp Vallecas Central - 28/11"`)*

4. **Comparativa de Ahorro frente al Taxi de Madrid:**
   Calcula la tarifa oficial de taxi ($4.00\text{ € base} + 1.25\text{ €/km}$, con suelo mínimo habitual de $18.00\text{ €}$):
   $$\text{CosteTaxi} = \max(18.00, 4.00 + (\text{DistanciaKm} \times 1.25))$$
   $$\text{AhorroVsTaxi} = \max(0.00, \text{CosteTaxi} - \text{CostePorPlaza})$$

5. **Impacto Ecológico (Reducción de CO2):**
   $$\text{Co2AhorradoKg} = \text{Round}\left(\text{DistanciaKm} \times 0.120 \times \max(1, \text{PlazasOcupadas}), 2\right)$$

---

## 📦 Data Transfer Object (`EstimacionCosteDto`)

Ubicado en `TajamarApp.Core/DTOs/Trayectos/EstimacionCosteDto.cs`:

```csharp
namespace TajamarApp.Core.DTOs.Trayectos;

public class EstimacionCosteDto
{
    public double DistanciaKm { get; set; }
    public decimal CosteTotalTrayecto { get; set; }
    public int PlazasOcupadas { get; set; }
    public decimal CostePorPlaza { get; set; }
    public string ConceptoBizum { get; set; } = string.Empty;
    public string? TelefonoBizum { get; set; }
    public decimal AhorroEstimadoVsTaxi { get; set; }
    public decimal Co2AhorradoKg { get; set; }
}
```

---

## 🌐 Endpoints y Exposición en la API

### 1. Endpoint Dinámico Independiente (`GET /api/trayectos/calcular-coste`)
Permite a cualquier usuario o pantalla del frontend (por ejemplo, al publicar un trayecto o antes de reservar) simular el coste y ahorro sin necesidad de que el viaje esté persistido en base de datos.

* **Método:** `GET`
* **Ruta:** `/api/trayectos/calcular-coste`
* **Acceso:** Público / Autenticado
* **Parámetros Query:**
  - `origen` *(string, opcional)*: Ciudad, barrio o distrito de origen (ej. `Vallecas`).
  - `destino` *(string, opcional)*: Destino (por defecto Tajamar).
  - `plazas` *(int, opcional, por defecto 1)*: Número de pasajeros ocupantes para repartir el gasto.

#### Ejemplo de Petición:
```http
GET /api/trayectos/calcular-coste?origen=Vallecas&destino=Tajamar&plazas=3 HTTP/1.1
Host: localhost:5050
```

#### Ejemplo de Respuesta `200 OK`:
```json
{
  "distanciaKm": 4.0,
  "costeTotalTrayecto": 0.40,
  "plazasOcupadas": 3,
  "costePorPlaza": 0.10,
  "conceptoBizum": "TajamarApp Vallecas - 24/11",
  "telefonoBizum": null,
  "ahorroEstimadoVsTaxi": 17.90,
  "co2AhorradoKg": 1.44
}
```

---

### 2. Nodo Integrado en Detalle de Trayecto (`GET /api/trayectos/{id}`)
Al consultar el detalle de un viaje específico (`GET /api/trayectos/{id}`), el backend inyecta automáticamente el objeto `estimacionCoste` calculado en tiempo real con las plazas actualmente ocupadas (`PlazasTotales - PlazasDisponibles`), la fecha programada de salida y el teléfono Bizum del conductor (`conductor.Telefono`).

#### Ejemplo de Respuesta Real `200 OK` (Trayecto 4):
```json
{
  "id": 4,
  "conductor": {
    "idPerfil": 1,
    "idUsuario": 212,
    "nombre": "David",
    "apellidos": "Castellanos",
    "imagen": "https://apicharlasalumnotajamar.azurewebsites.net/images/users/212_user.jpg",
    "telefono": "611223344",
    "marcaCoche": "Seat",
    "modeloCoche": "Ibiza",
    "colorCoche": "Negro",
    "matricula": "1234XYZ",
    "bio": "Estudiante",
    "puntuacionMedia": 4.5,
    "totalResenas": 2
  },
  "origen": "Vallecas Central",
  "destino": "Tajamar",
  "fechaHoraSalida": "2026-11-28T08:00:00",
  "plazasTotales": 3,
  "plazasDisponibles": 0,
  "tipoTrayecto": "Recurrente",
  "modalidad": "IdaYVuelta",
  "diasSemana": "L,M,X,J,V",
  "turno": "Manana",
  "estado": "Completo",
  "eventoId": null,
  "eventoTitulo": null,
  "fechaCreacion": "2026-11-24T08:10:05.0073082",
  "conductorPuntuacionMedia": 4.5,
  "conductorTotalResenas": 2,
  "estimacionCoste": {
    "distanciaKm": 4.0,
    "costeTotalTrayecto": 0.40,
    "plazasOcupadas": 3,
    "costePorPlaza": 0.10,
    "conceptoBizum": "TajamarApp Vallecas Central - 28/11",
    "telefonoBizum": "611223344",
    "ahorroEstimadoVsTaxi": 17.90,
    "co2AhorradoKg": 1.44
  }
}
```

---

## 🎨 Integración y UI Sugerida para Frontend (Angular 18+)

El equipo de Frontend (Mónica y Harry) puede explotar directamente esta información en la vista de detalle del viaje:

1. **Card de Gastos Compartidos:**
   - Muestra el precio por plaza sugerido: `0,10 € / persona`.
   - Explica el cálculo transparente: *(0,40 € total entre 4 personas: conductor + 3 pasajeros)*.

2. **Acción de Pago Rápido (Bizum):**
   - Botón interactivo `"Pagar con Bizum"` que al pulsar copie al portapapeles el concepto oficial (`TajamarApp Vallecas Central - 28/11`) y el número de teléfono del conductor (`611223344`).

3. **Insignias Gamificadas de Sostenibilidad:**
   - Badge Verde Ecológico: `🌱 1,44 kg de CO₂ ahorrados`.
   - Badge Económico: `💰 Ahorras 17,90 € frente a un taxi`.

---

## 🧪 Pruebas Automatizadas y Validación de Calidad

1. **Pruebas Unitarias (`TajamarApp.Tests`):**
   - Verificación de la matriz de distancias con y sin tildes (*Vallecas*, *Canillejas*, *Alcalá*).
   - Cálculo matemático exacto de combustible con decimales redondeados.
   - Manejo de casos límite: 0 plazas ocupadas (solo viaja el conductor, el coste no divide por cero).
2. **Pruebas en Vivo contra Azure SQL Server:**
   - Comprobación de que `GET /api/trayectos/4` recupera el número de teléfono del perfil del conductor y formatea el concepto Bizum con la fecha del viaje (`28/11`).
   - Consulta dinámica a `GET /api/trayectos/calcular-coste?origen=Vallecas&destino=Tajamar&plazas=3` validada con código `200 OK`.
