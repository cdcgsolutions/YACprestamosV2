using API1.Models;
using AutoMapper;

namespace API1.DTOs.app
{
    public class ClienteDTO
    {
        public int IdCliente { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Celular { get; set; } = string.Empty;
        public string? Correo { get; set; }
        public string EstadoCivil { get; set; } = "Soltero(a)";
        public DateOnly? FechaNacimiento { get; set; }
        public string? Direccion { get; set; }

        // Nuevos atributos
        public string NumeroTarjetaCredito { get; set; } = string.Empty;
        public DateOnly? FechaInicioContrato { get; set; }
        public DateOnly? FechaFinContrato { get; set; }
        public int? IdInstitucion { get; set; }
        public string InstitucionNombre { get; set; } = "Sin Institución";

        public bool EstaHabilitado { get; set; }
        public DateTime FechaRegistro { get; set; }
    }

    public class ClienteMap : Profile
    {
        public ClienteMap()
        {
            CreateMap<ClienteDTO, Cliente>()
                .ForMember(dest => dest.IdCliente, opt => opt.Ignore())
                .ForMember(dest => dest.FechaRegistro, opt => opt.Ignore())
                .ForMember(dest => dest.Prestamos, opt => opt.Ignore())
                .ForMember(dest => dest.Institucion, opt => opt.Ignore());

            CreateMap<Cliente, ClienteDTO>()
                .ForMember(dest => dest.InstitucionNombre,
                           opt => opt.MapFrom(src => src.Institucion != null ? src.Institucion.Nombre : "Sin Institución"));
        }
    }
}
