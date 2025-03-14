using AutoMapper;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.ModelViews.HashtagModelViews;

namespace Wanvi.Services.MapperProfile
{
    public class HashtagProfile : Profile
    {
        public HashtagProfile()
        {
            CreateMap<Hashtag, ResponseHashtagModel>().ReverseMap();
            CreateMap<Hashtag, CreateHashtagModel>().ReverseMap();
            CreateMap<Hashtag, UpdateHashtagModel>().ReverseMap()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
