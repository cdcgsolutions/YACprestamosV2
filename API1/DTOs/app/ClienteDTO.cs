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
                .ForMember(dest => dest.Prestamos, opt => opt.Ignore());

            CreateMap<Cliente, ClienteDTO>();
        }
    }
}

