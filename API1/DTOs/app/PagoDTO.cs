using API1.Models;
using AutoMapper;

namespace API1.DTOs.app
{
    public class PagoDTO
    {
        public int IdPago { get; set; }
        public int IdPrestamo { get; set; }
        public decimal MontoAbonado { get; set; }
        public DateTime FechaPago { get; set; }
    }

    public class RegistrarAbonoDTO
    {
        public int IdPrestamo { get; set; }
        public decimal MontoAbonado { get; set; }
    }

    public class ResumenHistorialPagosDTO
    {
        public int IdPrestamo { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public decimal MontoTotalPrestamo { get; set; }
        public decimal SaldoPendiente { get; set; }
        public decimal TotalAbonado { get; set; }
        public List<PagoDTO> Pagos { get; set; } = new();
    }

    public class PagoMap : Profile
    {
        public PagoMap()
        {
            CreateMap<Pago, PagoDTO>();

            CreateMap<PagoDTO, Pago>()
                .ForMember(dest => dest.IdPago, opt => opt.Ignore())
                .ForMember(dest => dest.Prestamo, opt => opt.Ignore());

            CreateMap<RegistrarAbonoDTO, Pago>()
                .ForMember(dest => dest.IdPago, opt => opt.Ignore())
                .ForMember(dest => dest.FechaPago, opt => opt.Ignore())
                .ForMember(dest => dest.Prestamo, opt => opt.Ignore());
        }
    }
}

