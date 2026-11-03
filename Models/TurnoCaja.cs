// Models/TurnoCaja.cs
public class TurnoCaja
{
    public Guid Id { get; set; }
    public Guid? UsuarioId { get; set; }
    public DateTime AbiertoEn { get; set; }
    public DateTime? CerradoEn { get; set; }
    public decimal FondoInicial { get; set; }
    public decimal? EfectivoEsperado { get; set; }
    public decimal? EfectivoContado { get; set; }
    public string? Nota { get; set; }
}