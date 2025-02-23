using AutoMapper;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.ModelViews.UserModelViews;

namespace Wanvi.Services.MapperProfile
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<ApplicationUser, ResponseLocalGuideProfileModel>()
            .ForMember(dest => dest.TourCount, opt => opt.MapFrom(src => src.Tours.Count))
            .ForMember(dest => dest.ReviewCount, opt => opt.MapFrom(src => src.Reviews.Count))
            .ReverseMap();
        }
    }
}
