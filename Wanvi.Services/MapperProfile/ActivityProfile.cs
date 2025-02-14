using AutoMapper;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.ModelViews.ActivityModelViews;

namespace Wanvi.Services.MapperProfile
{
    public class ActivityProfile : Profile
    {
        public ActivityProfile()
        {
            CreateMap<Activity, ResponseActivityModel>().ReverseMap();
        }
    }
}
