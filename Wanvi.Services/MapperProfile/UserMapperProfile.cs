using AutoMapper;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.ModelViews.UserModelViews;

namespace Wanvi.Services.MapperProfile
{
    public class UserMapperProfile : Profile
    {
        public UserMapperProfile()
        {
            CreateMap<ApplicationUser, NearbyResponseModelView>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.Distance, opt => opt.Ignore());
        }
    }
}
