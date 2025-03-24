using AutoMapper;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.ModelViews.NewsDetailModelViews;
using Wanvi.ModelViews.NewsModelViews;

namespace Wanvi.Services.MapperProfile
{
    public class NewsProfile : Profile
    {
        public NewsProfile()
        {
            // Mapping cho ResponseNewsModel
            CreateMap<News, ResponseNewsModel>()
                .ForMember(dest => dest.CreatedTime, opt => opt.MapFrom(src => src.CreatedTime))
                .ForMember(dest => dest.NewsDetails, opt => opt.MapFrom(src => src.NewsDetails))
                .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.CoverPhotoUrl, opt => opt.MapFrom(src => src.Media.Url)) // Ánh xạ URL từ Media
                .ForMember(dest => dest.MediaId, opt => opt.MapFrom(src => src.MediaId)) // Nếu cần ánh xạ MediaId
                .ReverseMap();

            // Mapping cho CreateNewsModel và UpdateNewsModel
            CreateMap<News, CreateNewsModel>().ReverseMap();
            CreateMap<News, UpdateNewsModel>()
                .ForMember(dest => dest.NewsDetails, opt => opt.MapFrom(src => src.NewsDetails))
                .ReverseMap()
                .ForMember(dest => dest.NewsDetails, opt => opt.Ignore()) // Không ghi đè `NewsDetails`, xử lý thủ công
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Mapping cho ResponseNewsDetailModel
            CreateMap<NewsDetail, ResponseNewsDetailModel>()
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Media.Url)) // Ánh xạ URL từ Media
                .ForMember(dest => dest.MediaId, opt => opt.MapFrom(src => src.MediaId)) // Ánh xạ MediaId nếu cần
                .ReverseMap();
        }
    }
}
