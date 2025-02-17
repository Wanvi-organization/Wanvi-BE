using AutoMapper;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.ModelViews.DashboardModelViews;

namespace Wanvi.Services.MapperProfile
{
    public class DashboardProfile : Profile
    {
        public DashboardProfile()
        {
            CreateMap<Dashboard, ResponseDashboardModel>().ReverseMap();
        }
    }
}
