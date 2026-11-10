using AutoMapper;

namespace API1.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            /*  MAPEOS AUTOMAPPER ENTRE ENTIDADES Y DTOS
             * 
             * CreateMap<Producto, ProductoDTO>().ReverseMap();
             * 
             * // Mapeos de creación y edición
             * CreateMap<CreateProductoDTO, Producto>();
             * CreateMap<UpdateProductoDTO, Producto>();
             */
        }
    }
}
