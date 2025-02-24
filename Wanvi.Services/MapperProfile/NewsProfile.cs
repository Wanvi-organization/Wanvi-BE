using AutoMapper;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.ModelViews.NewsModelViews;

namespace Wanvi.Services.MapperProfile
{
    public class NewsProfile : Profile
    {
        public NewsProfile()
        {
            CreateMap<News, ResponseNewsModel>().ReverseMap();
            CreateMap<News, CreateNewsModel>().ReverseMap();
            CreateMap<News, UpdateNewsModel>().ReverseMap();
        }
    }
}
