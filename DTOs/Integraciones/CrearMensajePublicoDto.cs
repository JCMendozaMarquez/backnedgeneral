namespace BackendGeneral.Api.DTOs.Integraciones;

public record CrearMensajePublicoDto(
    string Telefono,
    string Contenido,
    string? Nombre,
    string? Datos);