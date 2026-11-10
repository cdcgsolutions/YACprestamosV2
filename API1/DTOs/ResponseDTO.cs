using System.Collections.Generic;

namespace API1.DTOs
{
    public class ResponseListDTO<T>
    {
        public ResponseListDTO()
        {

        }
        public ResponseListDTO(List<T> valores, Dictionary<string, string> datosPaginacion)
        {
            value = valores;

            quanty = int.Parse(datosPaginacion["quanty"]);
            page = int.Parse(datosPaginacion["page"]);
            total = int.Parse(datosPaginacion["total"]);
        }

        public int page { get; set; }
        public int total { get; set; }
        public int quanty { get; set; }
        public List<T>? value { get; set; }

    }
    public class ResponseAnyDTO<T>
    {
        public ResponseAnyDTO()
        {

        }
        public ResponseAnyDTO(T valor, Dictionary<string, string> datosPaginacion)
        {
            value = valor;

            quanty = int.Parse(datosPaginacion["quanty"]);
            page = int.Parse(datosPaginacion["page"]);
            total = int.Parse(datosPaginacion["total"]);
        }

        public int page { get; set; }
        public int total { get; set; }
        public int quanty { get; set; }
        public T? value { get; set; }

    }
}
