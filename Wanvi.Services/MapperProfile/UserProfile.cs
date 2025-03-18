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
            .ForMember(dest => dest.TourCount, opt => opt.MapFrom(src =>
                src.Tours.SelectMany(t => t.Schedules)
                        .SelectMany(s => s.Bookings)
                        .Count(b => b.Status == BookingStatus.Completed || b.Status == BookingStatus.Refunded)
            ))
            .ForMember(dest => dest.ReviewCount, opt => opt.MapFrom(src => src.LocalGuideReviews.Count))
            .ReverseMap();
        }
    }
}
