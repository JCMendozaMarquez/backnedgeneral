public class Tercero
{
    public Guid Id { get; set; }
    public string Tipo { get; set; } = "cliente";
    public string Nombre { get; set; } = default!;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Documento { get; set; }
    public string? Direccion { get; set; }
    public string Datos { get; set; } = "{}";      // jsonb
    public bool Activo { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}