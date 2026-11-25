namespace TajamarApp.Core.DTOs.Common;

public class ResultadoPaginadoDto<T>
{
    public IEnumerable<T> Elementos { get; set; } = Enumerable.Empty<T>();
    public int TotalRegistros { get; set; }
    public int PaginaActual { get; set; }
    public int TotalPaginas { get; set; }
    public bool TienePaginaAnterior => PaginaActual > 1;
    public bool TienePaginaSiguiente => PaginaActual < TotalPaginas;

    public ResultadoPaginadoDto() { }

    public ResultadoPaginadoDto(IEnumerable<T> elementos, int totalRegistros, int pagina, int tamanoPagina)
    {
        Elementos = elementos;
        TotalRegistros = totalRegistros;
        PaginaActual = pagina;
        TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanoPagina);
    }
}
