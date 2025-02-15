using AutoMapper;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.ModelViews.MediaModelViews;

namespace Wanvi.Services.MapperProfile
{
    public class MediaProfile : Profile
    {
        public MediaProfile()
        {
            CreateMap<Media, ResponseMediaModel>()
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (ResponseMediaModel.MediaType)src.Type))
                .ForMember(dest => dest.AltText, opt => opt.MapFrom(src => src.AltText));
        }
    }
}
