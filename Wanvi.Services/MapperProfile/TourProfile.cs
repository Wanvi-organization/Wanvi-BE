using AutoMapper;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.ModelViews.ReviewModelViews;
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
            .ForMember(dest => dest.Medias, opt => opt.MapFrom(src => src.Medias))
            .ForMember(dest => dest.Reviews, opt => opt.MapFrom(src => src.Reviews != null ? src.Reviews
            .Select(r => new ResponseTourReviewModel
        {
            Id = r.Id,
            Rating = r.Rating,
            Content = r.Content,
            TravelerId = (Guid)r.TravelerId,
            TravelerName = r.Traveler.FullName,
            CreatedTime = r.CreatedTime,
            MediaUrls = r.Medias != null ? r.Medias.Select(m => m.Url).ToList() : new List<string>()
        }).ToList()
        : new List<ResponseTourReviewModel>()
)).ReverseMap();

            CreateMap<Tour, ResponseTourWithIdModel>()
                .ForMember(dest => dest.PickupAddressId, opt => opt.MapFrom(src => src.PickupAddress.Id))
    .ForMember(dest => dest.DropoffAddressId, opt => opt.MapFrom(src => src.DropoffAddress.Id))
    .ForMember(dest => dest.PickupAddress, opt => opt.MapFrom(src => src.PickupAddress.Street))
    .ForMember(dest => dest.DropoffAddress, opt => opt.MapFrom(src => src.DropoffAddress.Street))
    .ForMember(dest => dest.LocalGuideId, opt => opt.MapFrom(src => src.UserId))
    .ForMember(dest => dest.LocalGuideName, opt => opt.MapFrom(src => src.ApplicationUser.FullName))
    .ForMember(dest => dest.TourAddresses, opt => opt.MapFrom(src =>
        src.TourAddresses != null
            ? src.TourAddresses.Select(ta => new ResponseTourAddressModel
            {
                Id = ta.AddressId,
                Street = ta.Address.Street
            }).ToList()
            : new List<ResponseTourAddressModel>()
    ))
    .ForMember(dest => dest.TourActivities, opt => opt.MapFrom(src =>
        src.TourActivities != null
            ? src.TourActivities.Select(ta => new ResponseTourActivityModel
            {
                Id = ta.ActivityId,
                Name = ta.Activity.Name
            }).ToList()
            : new List<ResponseTourActivityModel>()
    ))
    .ForMember(dest => dest.Schedules, opt => opt.MapFrom(src => src.Schedules))
    .ForMember(dest => dest.Medias, opt => opt.MapFrom(src => src.Medias))
    .ForMember(dest => dest.Reviews, opt => opt.MapFrom(src =>
        src.Reviews != null
            ? src.Reviews.Select(r => new ResponseTourReviewModel
            {
                Id = r.Id,
                Rating = r.Rating,
                Content = r.Content,
                TravelerId = (Guid)r.TravelerId,
                TravelerName = r.Traveler.FullName,
                CreatedTime = r.CreatedTime,
                MediaUrls = r.Medias != null ? r.Medias.Select(m => m.Url).ToList() : new List<string>()
            }).ToList()
            : new List<ResponseTourReviewModel>()
    ));

            CreateMap<Tour, CreateTourModel>().ReverseMap();
            CreateMap<Tour, UpdateTourModel>().ReverseMap()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
