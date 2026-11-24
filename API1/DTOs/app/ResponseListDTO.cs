namespace API1.DTOs.app
{
    public class ResponseListDTO<T>
    {
        public int page { get; set; }
        public int total { get; set; }
        public int quanty { get; set; }
        public List<T>? value { get; set; }
    }
}
