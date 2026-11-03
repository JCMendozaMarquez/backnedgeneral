// Models/Receta.cs
public class Receta
{
    public Guid Id { get; set; }
    public Guid ArticuloId { get; set; }
    public Guid MaterialId { get; set; }
    public decimal Cantidad { get; set; }
}