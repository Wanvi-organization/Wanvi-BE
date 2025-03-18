using AutoMapper;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.ModelViews.ReviewModelViews;

namespace Wanvi.Services.MapperProfile
{
    public class ReviewProfile : Profile
    {
        public ReviewProfile()
        {
            CreateMap<Review, ResponseReviewModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ConvertReviewTypeToVietnameseString((ResponseReviewModel.ReviewType)src.ReviewType)))
            .ForMember(dest => dest.TravelerId, opt => opt.MapFrom(src => src.TravelerId))
            .ForMember(dest => dest.TravelerName, opt => opt.MapFrom(src => src.Traveler.FullName))
            .ForMember(dest => dest.LocalGuideId, opt => opt.MapFrom(src => src.LocalGuideId))
            .ForMember(dest => dest.LocalGuideName, opt => opt.MapFrom(src => src.LocalGuide.FullName))
            .ForMember(dest => dest.CreatedTime, opt => opt.MapFrom(src => src.CreatedTime))
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
            .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Rating))
            .ForMember(dest => dest.TourId, opt => opt.MapFrom((src, dest) => src.ReviewType == ReviewType.TourReview ? src.TourId : null))
            .ForMember(dest => dest.TourName, opt => opt.MapFrom((src, dest) => src.ReviewType == ReviewType.TourReview ? src.Tour.Name : null))
            .ForMember(dest => dest.BookingId, opt => opt.MapFrom((src, dest) => src.ReviewType == ReviewType.TourReview ? src.BookingId : null))
            .ForMember(dest => dest.MediaUrls, opt => opt.MapFrom(src => src.Medias.Select(m => m.Url).ToList()));
            CreateMap<Review, CreateReviewModel>().ReverseMap();
        }

        private static string ConvertReviewTypeToVietnameseString(ResponseReviewModel.ReviewType reviewType)
        {
            return reviewType switch
            {
                ResponseReviewModel.ReviewType.TourReview => "Đánh giá Tour",
                ResponseReviewModel.ReviewType.LocalGuideReview => "Đánh giá Hướng Dẫn Viên",
                _ => "Loại Đánh Giá Không Xác Định"
            };
        }
    }
}
