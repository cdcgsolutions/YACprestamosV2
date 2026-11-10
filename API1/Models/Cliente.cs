using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API1.Models;

[Table("Clientes")]
public class Cliente
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdCliente { get; set; }

    [Required]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Celular { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(150)]
    public string? Correo { get; set; }

    [Required]
    [MaxLength(20)]
    public string EstadoCivil { get; set; } = "Soltero(a)";

    public DateOnly? FechaNacimiento { get; set; }

    [MaxLength(250)]
    public string? Direccion { get; set; }

    public bool EstaHabilitado { get; set; } = true;

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public virtual ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
}
