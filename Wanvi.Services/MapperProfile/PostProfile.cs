using AutoMapper;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.ModelViews.PostModelViews;

namespace Wanvi.Services.MapperProfile
{
    public class PostProfile : Profile
    {
        public PostProfile()
        {
            CreateMap<Post, ResponsePostModel>().ReverseMap();
            CreateMap<Post, CreatePostModel>().ReverseMap();
            CreateMap<Post, UpdatePostModel>().ReverseMap()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
