namespace GestionInventario.Dtos
{
    public class VentaDto
    {
        public int? Id { get; set; }
        public int? ProductoId { get; set; }
        public int? Cantidad { get; set; }
        public DateTime FechaVenta { get; set; }
        public decimal? Total { get; set; }
        public int? ClienteId { get; set; }
    }
}