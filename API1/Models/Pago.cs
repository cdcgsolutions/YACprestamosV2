using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API1.Models;

[Table("Pagos")]
public class Pago
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdPago { get; set; }

    [Required]
    public int IdPrestamo { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoAbonado { get; set; }

    public DateTime FechaPago { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(IdPrestamo))]
    [JsonIgnore]
    public virtual Prestamo? Prestamo { get; set; }
}
