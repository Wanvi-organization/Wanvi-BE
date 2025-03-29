using AutoMapper;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.ModelViews.ScheduleModelViews;

namespace Wanvi.Services.MapperProfile
{
    public class ScheduleProfile : Profile
    {
        public ScheduleProfile()
        {
            CreateMap<Schedule, ResponseScheduleModel>()
                .ForMember(dest => dest.Day, opt => opt.MapFrom(src => ConvertDayToVietnameseString((ResponseScheduleModel.DayOfWeek)src.Day)))
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime))
                .ForMember(dest => dest.MaxTraveler, opt => opt.MapFrom(src => src.MaxTraveler))
                .ForMember(dest => dest.BookedTraveler, opt => opt.MapFrom(src => src.BookedTraveler))
                .ForMember(dest => dest.MinDeposit, opt => opt.MapFrom(src => src.MinDeposit));
            CreateMap<Schedule, UpdateScheduleModel>().ReverseMap();
        }

        private string ConvertDayToVietnameseString(ResponseScheduleModel.DayOfWeek day)
        {
            return day switch
            {
                ResponseScheduleModel.DayOfWeek.Monday => "Thứ Hai",
                ResponseScheduleModel.DayOfWeek.Tuesday => "Thứ Ba",
                ResponseScheduleModel.DayOfWeek.Wednesday => "Thứ Tư",
                ResponseScheduleModel.DayOfWeek.Thursday => "Thứ Năm",
                ResponseScheduleModel.DayOfWeek.Friday => "Thứ Sáu",
                ResponseScheduleModel.DayOfWeek.Saturday => "Thứ Bảy",
                ResponseScheduleModel.DayOfWeek.Sunday => "Chủ Nhật",
                _ => "Không xác định"
            };
        }
    }
}
