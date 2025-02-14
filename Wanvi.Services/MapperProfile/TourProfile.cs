using AutoMapper;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.ModelViews.TourModelViews;

namespace Wanvi.Services.MapperProfile
{
    public class TourProfile : Profile
    {
        public TourProfile()
        {
            CreateMap<Tour, CreateTourModel>().ReverseMap();
        }
    }
}
