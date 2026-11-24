using System.Globalization;
using System.Text.RegularExpressions;
using BackendGeneral.Api.DTOs.Integraciones;

namespace BackendGeneral.Api.Services;

public class PedidoParser
{
    private static readonly Regex LineaRegex = new(
        @"^[•\-\*]\s*(\d+(?:[.,]\d+)?)\s*[×xX]\s*(.+?)\s*[\(\$]?\s*(?:Bs\.?|\$)?\s*([\d.,]+)\s*\)?\s*$",
        RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex TotalRegex = new(
        @"Total(?:\s+referencia)?\s*:?\s*(?:Bs\.?|\$)?\s*([\d.,]+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public (bool Ok, List<ItemPedidoDto> Items, decimal Total) Parse(string? contenido)
    {
        var items = new List<ItemPedidoDto>();
        if (string.IsNullOrWhiteSpace(contenido))
            return (false, items, 0m);

        foreach (Match m in LineaRegex.Matches(contenido))
        {
            var cantidad = ParseDecimal(m.Groups[1].Value);
            var nombre = m.Groups[2].Value.Trim();
            var precio = ParseDecimal(m.Groups[3].Value);
            if (cantidad <= 0 || string.IsNullOrEmpty(nombre)) continue;

            items.Add(new ItemPedidoDto(null, nombre, cantidad, precio));
        }

        var total = items.Sum(i => i.Cantidad * i.PrecioUnitario);
        var tm = TotalRegex.Match(contenido);
        if (tm.Success)
        {
            var t = ParseDecimal(tm.Groups[1].Value);
            if (t > 0) total = t;
        }

        return (items.Count > 0, items, total);
    }

    private static decimal ParseDecimal(string s)
    {
        s = s.Trim();
        if (s.Contains(',') && s.Contains('.'))
            s = s.Replace(".", "").Replace(",", ".");
        else if (s.Contains(','))
            s = s.Replace(",", ".");

        return decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : 0m;
    }
}