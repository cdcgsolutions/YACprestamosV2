namespace API1.DTOs.app
{
    public class DashboardResumenDTO
    {
        public int TotalClientes { get; set; }
        public int ClientesActivos { get; set; }

        public int TotalPrestamos { get; set; }
        public int PrestamosActivos { get; set; }
        public int PrestamosPagados { get; set; }

        public decimal TotalCapitalPrestado { get; set; }
        public decimal TotalInteresGenerado { get; set; }
        public decimal TotalMontoCartera { get; set; }
        public decimal TotalRecaudado { get; set; }
        public decimal SaldoPendienteTotal { get; set; }

        public List<ModalidadEstadisticaDTO> DistribucionModalidades { get; set; } = new();
    }

    public class ModalidadEstadisticaDTO
    {
        public string Modalidad { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal MontoTotal { get; set; }
    }
}

