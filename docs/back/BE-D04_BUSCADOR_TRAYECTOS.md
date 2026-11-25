# 🔍 BE-D04: Motor de Búsqueda y Filtrado Paginado de Trayectos

## 📌 Resumen de la Tarea
- **Responsable:** David (Back)
- **Estado:** ✅ Completada y Verificada
- **Objetivo:** Proveer un endpoint robusto y flexible para que pasajeros y estudiantes busquen trayectos activos con filtrado multicriterio y paginación en servidor.

---

## 🌐 Endpoint de Búsqueda

### `GET /api/trayectos`
- **Autenticación:** Pública (no requiere token, pero accesible con o sin él).
- **Parámetros de Query:**

| Parámetro | Tipo | Descripción | Ejemplo |
| :--- | :--- | :--- | :--- |
| `origen` | string | Filtro parcial por punto de partida (LIKE insensitive) | `?origen=Vallecas` |
| `destino` | string | Filtro parcial por destino | `?destino=Tajamar` |
| `tipoTrayecto` | string | `Recurrente` o `Puntual` | `?tipoTrayecto=Recurrente` |
| `modalidad` | string | `SoloIda`, `SoloVuelta` o `IdaYVuelta` | `?modalidad=IdaYVuelta` |
| `turno` | string | `Manana` o `Tarde` (para trayectos de clase) | `?turno=Manana` |
| `eventoId` | int? | Filtrar trayectos asignados a un evento de Tajamar | `?eventoId=1` |
| `fecha` | DateTime? | Fecha exacta del trayecto (compara por día) | `?fecha=2026-11-23` |
| `soloConPlazas` | bool? | Si es `true`, excluye trayectos con 0 plazas libres | `?soloConPlazas=true` |
| `pagina` | int | Número de página (default: 1, mín: 1) | `?pagina=1` |
| `tamanoPagina` | int | Registros por página (default: 10, mín: 1, máx: 50) | `?tamanoPagina=5` |

---

## 📦 Formato de Respuesta (`ResultadoPaginadoDto<TrayectoResumenDto>`)

```json
{
  "elementos": [
    {
      "id": 2,
      "conductorNombre": "David Castellanos",
      "conductorImagen": "https://apicharlasalumnotajamar.azurewebsites.net/images/users/212_user.jpg",
      "coche": "Seat Ibiza",
      "origen": "Vallecas",
      "destino": "Tajamar",
      "fechaHoraSalida": "2026-11-23T08:00:00",
      "plazasDisponibles": 3,
      "plazasTotales": 3,
      "tipoTrayecto": "Recurrente",
      "modalidad": "IdaYVuelta",
      "turno": "Manana",
      "diasSemana": "L,M,X,J,V",
      "estado": "Activo",
      "eventoId": null
    }
  ],
  "totalRegistros": 1,
  "paginaActual": 1,
  "totalPaginas": 1,
  "tienePaginaAnterior": false,
  "tienePaginaSiguiente": false
}
```

---

## ⚡ Comportamiento Técnico
1. **Filtro de Estado:** Solo retorna trayectos con `Estado == "Activo"`.
2. **Búsqueda Parcial:** Los campos de texto (`Origen`, `Destino`) emplean `EF.Functions.Like(..., "%valor%")`.
3. **Optimización EF Core:** Consulta con `AsNoTracking()` para máxima velocidad de lectura y menor consumo de memoria.
4. **Ordenación:** Los resultados se ordenan cronológicamente por `FechaHoraSalida ASC` (los más próximos primero).
5. **Cálculo de Paginación en Servidor:**
   - Realiza un `CountAsync()` antes del particionado para obtener el total exacto.
   - Aplica `.Skip((pagina - 1) * tamanoPagina).Take(tamanoPagina)`.
