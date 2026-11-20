public readonly record struct EntradaCola(long Id, int UsuarioId, int Posicion);

public static class ColaFifo
{
    public static EntradaCola? Primera(IEnumerable<EntradaCola> activas) =>
        activas.OrderBy(entrada => entrada.Posicion).ThenBy(entrada => entrada.Id)
            .Select(entrada => (EntradaCola?)entrada).FirstOrDefault();
}
