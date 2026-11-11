using API1.Models;
using AutoMapper;

namespace API1.DTOs.app
{
    public class PrestamoDTO
    {
        public int IdPrestamo { get; set; }
        public int IdCliente { get; set; }
        public string? NombreCliente { get; set; }
        public int? IdUsuarioRegistro { get; set; }
        public decimal Monto { get; set; }
        public decimal PorcentajeInteres { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal SaldoPendiente { get; set; }
        public string ModalidadCobro { get; set; } = "Mensual";
        public string Estado { get; set; } = "Activo";
        public DateTime FechaRegistro { get; set; }
    }

    public class PrestamoCreacionDTO
    {
        public int IdCliente { get; set; }
        public int? IdUsuarioRegistro { get; set; }
        public decimal Monto { get; set; }
        public decimal PorcentajeInteres { get; set; } = 10;
        public string ModalidadCobro { get; set; } = "Mensual";
    }

    public class PrestamoMap : Profile
    {
        public PrestamoMap()
        {
            CreateMap<Prestamo, PrestamoDTO>()
                .ForMember(dest => dest.NombreCliente, opt => opt.MapFrom(src => src.Cliente != null ? src.Cliente.Nombre : null));

            CreateMap<PrestamoDTO, Prestamo>()
                .ForMember(dest => dest.IdPrestamo, opt => opt.Ignore())
                .ForMember(dest => dest.Cliente, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioRegistro, opt => opt.Ignore())
                .ForMember(dest => dest.Pagos, opt => opt.Ignore());

            CreateMap<PrestamoCreacionDTO, Prestamo>()
                .ForMember(dest => dest.IdPrestamo, opt => opt.Ignore())
                .ForMember(dest => dest.MontoTotal, opt => opt.Ignore())
                .ForMember(dest => dest.SaldoPendiente, opt => opt.Ignore())
                .ForMember(dest => dest.Estado, opt => opt.Ignore())
                .ForMember(dest => dest.FechaRegistro, opt => opt.Ignore())
                .ForMember(dest => dest.Cliente, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioRegistro, opt => opt.Ignore())
                .ForMember(dest => dest.Pagos, opt => opt.Ignore());
        }
    }
}

