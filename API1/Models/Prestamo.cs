using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API1.Models;

[Table("Prestamos")]
public class Prestamo
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdPrestamo { get; set; }

    [Required]
    public int IdCliente { get; set; }

    public int? IdUsuarioRegistro { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Monto { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal PorcentajeInteres { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoTotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal SaldoPendiente { get; set; }

    [Required]
    [MaxLength(20)]
    public string ModalidadCobro { get; set; } = "Mensual";

    [Required]
    [MaxLength(20)]
    public string Estado { get; set; } = "Activo";

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(IdCliente))]
    public virtual Cliente? Cliente { get; set; }

    [ForeignKey(nameof(IdUsuarioRegistro))]
    public virtual Usuario? UsuarioRegistro { get; set; }

    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
}
