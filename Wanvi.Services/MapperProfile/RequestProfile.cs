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
                .ForMember(dest => dest.CreatedTime, opt => opt.MapFrom(src => src.CreatedTime))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ConvertRequestStatusToVietnamese(src.Status)))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ConvertRequestTypeToVietnamese(src.Type)))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName));
        }

        private string ConvertRequestStatusToVietnamese(RequestStatus status)
        {
            return status switch
            {
                RequestStatus.Pending => "Đang chờ",
                RequestStatus.Confirmed => "Đã xác nhận",
                RequestStatus.Cancelled => "Đã hủy",
                _ => "Không xác định"
            };
        }
        private string ConvertRequestTypeToVietnamese(RequestType status)
        {
            return status switch
            {
                RequestType.BookingWithdrawal => "Rút tiền từ booking",
                RequestType.BalanceWithdrawal => "Rút tiền từ ví",
                RequestType.Complaint => "Khiếu nại",
                RequestType.Question => "Câu hỏi",
                _ => "Không xác định"
            };
        }
    }
}
