using AutoMapper;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.ModelViews.TourModelViews;

namespace Wanvi.Services.MapperProfile
{
    public class TourProfile : Profile
    {
        public TourProfile()
        {
            CreateMap<Tour, ResponseTourModel>()
            .ForMember(dest => dest.PickupAddress, opt => opt.MapFrom(src => src.PickupAddress.Street))
            .ForMember(dest => dest.DropoffAddress, opt => opt.MapFrom(src => src.DropoffAddress.Street))
            .ForMember(dest => dest.LocalGuideId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.LocalGuideName, opt => opt.MapFrom(src => src.ApplicationUser.FullName))
            .ForMember(dest => dest.TourAddresses, opt => opt.MapFrom(src => src.TourAddresses.Select(ta => ta.Address.Street).ToList()))
            .ForMember(dest => dest.TourActivities, opt => opt.MapFrom(src =>
        src.TourActivities != null
        ? src.TourActivities
            .Where(ta => ta.Activity != null)
            .Select(ta => ta.Activity.Name)
            .ToList()
        : new List<string>()))
            .ForMember(dest => dest.Schedules, opt => opt.MapFrom(src => src.Schedules))
            .ForMember(dest => dest.Medias, opt => opt.MapFrom(src => src.Medias)).ReverseMap();
            CreateMap<Tour, CreateTourModel>().ReverseMap();
            CreateMap<Tour, UpdateTourModel>().ReverseMap()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
