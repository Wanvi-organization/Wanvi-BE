using AutoMapper;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.ModelViews.RequestModelViews;

namespace Wanvi.Services.MapperProfile
{
    public class RequestProfile : Profile
    {
        public RequestProfile()
        {
            CreateMap<Request, ResponseRequestModel>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName));
        }
    }
}
