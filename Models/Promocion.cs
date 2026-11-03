// Models/Promocion.cs
public class Promocion
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = default!;
    public string Tipo { get; set; } = "porcentaje";
    public decimal Valor { get; set; }
    public string Reglas { get; set; } = "{}";
    public DateTime? VigenteDesde { get; set; }
    public DateTime? VigenteHasta { get; set; }
    public bool RequiereAutorizacion { get; set; }
    public bool Activo { get; set; } = true;
}