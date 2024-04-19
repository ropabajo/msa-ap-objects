using AutoMapper;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Departments.Queries.GetDepartments;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Districts.Queries.GetDistricts;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Provinces.Queries.GetProvinces;
using Ropabajo.Church.Sanluis.Objects.Domain;

namespace Ropabajo.Church.Sanluis.Objects.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Departament, DepartmentsVm>()
             .ReverseMap();

            CreateMap<Province, ProvincesVm>()
             .ReverseMap();

            CreateMap<District, DistrictsVm>()
             .ReverseMap();
        }
    }
}
