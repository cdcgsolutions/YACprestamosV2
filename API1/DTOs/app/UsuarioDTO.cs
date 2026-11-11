using API1.Models;
using AutoMapper;

namespace API1.DTOs.app
{
    public class UsuarioDTO
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public string Rol { get; set; } = "Operador";
        public bool EstaHabilitado { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

    public class UsuarioCreacionDTO
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public string Rol { get; set; } = "Operador";
    }

    public class UsuarioEdicionDTO
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;
        public string Rol { get; set; } = "Operador";
        public bool EstaHabilitado { get; set; }
        public string? NuevaContrasena { get; set; }
    }

    public class UsuarioMap : Profile
    {
        public UsuarioMap()
        {
            CreateMap<Usuario, UsuarioDTO>()
                .ForMember(dest => dest.Contrasena, opt => opt.MapFrom(src => src.ContrasenaHash));

            CreateMap<UsuarioDTO, Usuario>()
                .ForMember(dest => dest.ContrasenaHash, opt => opt.MapFrom(src => src.Contrasena))
                .ForMember(dest => dest.PrestamosRegistrados, opt => opt.Ignore());

            CreateMap<UsuarioCreacionDTO, Usuario>()
                .ForMember(dest => dest.IdUsuario, opt => opt.Ignore())
                .ForMember(dest => dest.ContrasenaHash, opt => opt.MapFrom(src => src.Contrasena))
                .ForMember(dest => dest.EstaHabilitado, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
                .ForMember(dest => dest.PrestamosRegistrados, opt => opt.Ignore());
        }
    }
}
