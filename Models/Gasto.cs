// Models/Gasto.cs
public class Gasto
{
    public Guid Id { get; set; }
    public Guid? TurnoId { get; set; }
    public Guid? TerceroId { get; set; }
    public Guid? UsuarioId { get; set; }
    public string Categoria { get; set; } = "otro";
    public string Concepto { get; set; } = default!;
    public decimal Monto { get; set; }
    public string Metodo { get; set; } = "efectivo";
    public DateOnly Fecha { get; set; }
    public string Datos { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }
}