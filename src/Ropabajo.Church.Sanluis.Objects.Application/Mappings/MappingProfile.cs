using AutoMapper;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Departments.Queries.GetObjects;

namespace Ropabajo.Church.Sanluis.Objects.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Object, ObjectsVm>()
             .ReverseMap();
        }
    }
}
