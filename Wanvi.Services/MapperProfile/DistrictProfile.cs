using AutoMapper;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.ModelViews.CityModelViews;
using Wanvi.ModelViews.DistrictModelViews;

namespace Wanvi.Services.MapperProfile
{
    public class DistrictProfile : Profile
    {
        public DistrictProfile()
        {
            CreateMap<District, ResponseDistrictModel>().ReverseMap();
        }
    }
}
