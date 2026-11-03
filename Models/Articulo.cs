using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendGeneral.Api.Models;

public class Articulo
{
    public Guid Id { get; set; }
    public Guid? CategoriaId { get; set; }
    public string Tipo { get; set; } = "producto";
    public string? Codigo { get; set; }
    public string Nombre { get; set; } = default!;
    public string? Descripcion { get; set; }
    public string Unidad { get; set; } = "unidad";
    public decimal PrecioVenta { get; set; }
    public decimal CostoActual { get; set; }
    public bool ControlaStock { get; set; }
    public decimal StockMinimo { get; set; }
    public string Opciones { get; set; } = "[]";     // jsonb
    public string Atributos { get; set; } = "{}";    // jsonb
    public string? ImagenUrl { get; set; }
    public bool Disponible { get; set; } = true;
    public string[] VisibleEn { get; set; } = { "pos", "web", "whatsapp" };
    public bool Activo { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // 👇 AGREGA ESTA LÍNEA AL FINAL
    public Categoria? Categoria { get; set; }
}