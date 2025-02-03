using AutoMapper;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.ModelViews.CityModelViews;

namespace Wanvi.Services.MapperProfile
{
    public class CityProfile : Profile
    {
        public CityProfile()
        {
            CreateMap<City, ResponseCityModel>().ReverseMap();
        }
    }
}
