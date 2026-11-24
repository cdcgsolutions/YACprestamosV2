using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API1.Models;

[Table("Instituciones")]
public class Institucion
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdInstitucion { get; set; }

    [Required]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Direccion { get; set; }

    public bool EstaHabilitado { get; set; } = true;

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
}
