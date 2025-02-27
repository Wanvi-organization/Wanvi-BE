using AutoMapper;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.ModelViews.NewsDetailModelViews;

namespace Wanvi.Services.MapperProfile
{
    public class NewsDetailProfile : Profile
    {
        public NewsDetailProfile()
        {
            CreateMap<NewsDetail, ResponseNewsDetailModel>().ReverseMap();
            CreateMap<NewsDetail, CreateNewsDetailModel>()
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<NewsDetail, UpdateNewsDetailModel>()
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Id)))
                .ForMember(dest => dest.NewsId, opt => opt.Ignore());
        }
    }
}
