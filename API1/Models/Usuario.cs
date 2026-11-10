using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API1.Models;

[Table("Usuarios")]
public class Usuario
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdUsuario { get; set; }

    [Required]
    [MaxLength(100)]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string CorreoElectronico { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [JsonIgnore]
    public string ContrasenaHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Rol { get; set; } = "Operador";

    public bool EstaHabilitado { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public virtual ICollection<Prestamo> PrestamosRegistrados { get; set; } = new List<Prestamo>();
}
