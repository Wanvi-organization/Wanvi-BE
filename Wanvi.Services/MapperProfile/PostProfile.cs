using AutoMapper;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.ModelViews.PostModelViews;

namespace Wanvi.Services.MapperProfile
{
    public class PostProfile : Profile
    {
        public PostProfile()
        {
            CreateMap<Post, ResponsePostModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.Hashtags, opt => opt.MapFrom(src => src.PostHashtags.Select(ph => ph.Hashtag.Name)))
                .ForMember(dest => dest.MediaUrls, opt => opt.MapFrom(src => src.Medias.Select(m => m.Url)))
                .ForMember(dest => dest.CreatedTime, opt => opt.MapFrom(src => src.CreatedTime))
                .ReverseMap();
            CreateMap<Post, CreatePostModel>().ReverseMap();
            CreateMap<Post, UpdatePostModel>().ReverseMap()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
