using AutoMapper;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.ModelViews.CommentModelViews;

namespace Wanvi.Services.MapperProfile
{
    public class CommentProfile : Profile
    {
        public CommentProfile()
        {
            CreateMap<Comment, ResponseCommentModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
                .ForMember(dest => dest.IsLikedByUser, opt => opt.MapFrom(src => src.IsLikedByUser))
                .ForMember(dest => dest.LikeCount, opt => opt.MapFrom(src => src.LikeCount))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.Replies, opt => opt.MapFrom(src => src.Replies))
                .ForMember(dest => dest.CreatedTime, opt => opt.MapFrom(src => src.CreatedTime))
                .ForMember(dest => dest.LastUpdatedTime, opt => opt.MapFrom(src => src.LastUpdatedTime));

            CreateMap<Comment, CreateCommentModel>().ReverseMap();
            CreateMap<Comment, UpdateCommentModel>().ReverseMap();
        }
    }
}
