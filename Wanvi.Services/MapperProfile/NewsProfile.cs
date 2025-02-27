using AutoMapper;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.ModelViews.NewsModelViews;

namespace Wanvi.Services.MapperProfile
{
    public class NewsProfile : Profile
    {
        public NewsProfile()
        {
            CreateMap<News, ResponseNewsModel>()
            .ForMember(dest => dest.NewsDetails, opt => opt.MapFrom(src => src.NewsDetails))
            .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.User.FullName))
            .ReverseMap();
            CreateMap<News, CreateNewsModel>().ReverseMap();
            CreateMap<News, UpdateNewsModel>()
                .ForMember(dest => dest.NewsDetails, opt => opt.Ignore())
                .ReverseMap()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
