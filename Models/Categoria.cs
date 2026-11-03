public class Categoria
{
    public Guid Id { get; set; }
    public string Tipo { get; set; } = "producto";
    public string Nombre { get; set; } = default!;
    public int Orden { get; set; }
    public string[] VisibleEn { get; set; } = Array.Empty<string>(); // text[]
    public bool Activo { get; set; } = true;
}