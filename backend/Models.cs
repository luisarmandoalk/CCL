using System.ComponentModel.DataAnnotations;

namespace CCL.Backend;

public class Producto
{
    public int Id { get; set; }

    [Required]
    public string Nombre { get; set; } = string.Empty;

    public int Cantidad { get; set; }
}

public class LoginRequest
{
    [Required]
    public string Usuario { get; set; } = string.Empty;

    [Required]
    public string Clave { get; set; } = string.Empty;
}

public class MovimientoRequest
{
    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Cantidad { get; set; }

    [Required]
    public string Tipo { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
}

