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

    [Required]
    [MaxLength(30)]
    public string NumeroTarjetaCredito { get; set; } = string.Empty;

    public DateOnly? FechaInicioContrato { get; set; }

    public DateOnly? FechaFinContrato { get; set; }

    public int? IdInstitucion { get; set; }

    [ForeignKey(nameof(IdInstitucion))]
    public virtual Institucion? Institucion { get; set; }

    public bool EstaHabilitado { get; set; } = true;

    public DateTime FechaRegistro { get; set; } = API1.Helpers.FechaHelper.AhoraBolivia;

    [JsonIgnore]
    public virtual ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
}
