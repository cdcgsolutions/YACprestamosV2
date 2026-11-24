using API1.Models;
using AutoMapper;

namespace API1.DTOs.app
{
    public class InstitucionDTO
    {
        public int IdInstitucion { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Direccion { get; set; }
        public bool EstaHabilitado { get; set; } = true;
        public DateTime FechaRegistro { get; set; }
    }

    public class InstitucionMap : Profile
    {
        public InstitucionMap()
        {
            CreateMap<InstitucionDTO, Institucion>()
                .ForMember(dest => dest.IdInstitucion, opt => opt.Ignore())
                .ForMember(dest => dest.FechaRegistro, opt => opt.Ignore())
                .ForMember(dest => dest.Clientes, opt => opt.Ignore());

            CreateMap<Institucion, InstitucionDTO>();
        }
    }
}
