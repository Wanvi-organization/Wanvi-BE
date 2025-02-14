using AutoMapper;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.ModelViews.ScheduleModelViews;

namespace Wanvi.Services.MapperProfile
{
    public class ScheduleMProfile : Profile
    {
        public ScheduleMProfile()
        {
            CreateMap<Schedule, ResponseScheduleModel>()
                .ForMember(dest => dest.Day, opt => opt.MapFrom(src => (ResponseScheduleModel.DayOfWeek)src.Day))
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime))
                .ForMember(dest => dest.MaxTraveler, opt => opt.MapFrom(src => src.MaxTraveler))
                .ForMember(dest => dest.BookedTraveler, opt => opt.MapFrom(src => src.BookedTraveler));
        }
    }
}
