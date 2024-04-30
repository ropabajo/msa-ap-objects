using AutoMapper;
using Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoads.Queries.GetPagedBulkLoads;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Departments.Queries.GetObjects;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Formats.Queries.GetFormats;
using Ropabajo.Church.Sanluis.Objects.Domain.Entities;
using Object = Ropabajo.Church.Sanluis.Objects.Domain.Entities.Object;

namespace Ropabajo.Church.Sanluis.Objects.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Object, ObjectsVm>()
             .ReverseMap();

            CreateMap<Format, FormatsVm>()
               .ForMember(dest => dest.FormatCode, act => act.MapFrom(src => src.Code))
               .ReverseMap();

            CreateMap<BulkLoad, PagedBulkLoadsVm>()
              .ForMember(dest => dest.BulkLoadCode, act => act.MapFrom(src => src.Code))
              .ForMember(dest => dest.PayrollObjectCode, act => act.MapFrom(src => src.ObjectCode))
              .ReverseMap();
        }
    }
}
