using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.AuthModelViews;
using Wanvi.ModelViews.BookingModelViews;
using Wanvi.ModelViews.PaymentModelViews;
using Wanvi.Services.Services.Infrastructure;

namespace Wanvi.Services.Services
{
    public class BookingService: IBookingService
    {
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IPaymentService _paymentService;
        public BookingService(IMapper mapper, UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, IHttpContextAccessor contextAccessor, IPaymentService paymentService)
        {
            _mapper = mapper;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _contextAccessor = contextAccessor;
            _paymentService = paymentService;
        }
        public async Task<string> CreateBooking(CreateBookingModel model)
        {
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);

            Guid.TryParse(userId, out Guid cb);
            if (model.Email == null)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Vui lòng email không để trống!");
            }
            if (model.PhoneNumber == null)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Vui lòng số điện thoại không để trống!");
            }
            if (model.NumberOfParticipants <= 0)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Vui lòng điền số người tham gia!");
            }
            var schedule = await _unitOfWork.GetRepository<Schedule>().Entities.FirstOrDefaultAsync(x => x.Id == model.ScheduleId && !x.DeletedTime.HasValue) ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NotFound, "Không tìm thấy lịch");
            int countHour = (schedule.EndTime.Hours - schedule.StartTime.Hours);
            if (countHour < 0)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Lỗi tính lịch");
            }
            if(model.NumberOfParticipants > schedule.MaxTraveler)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, $"Số người đăng kí lớn hơn số người mặc định({schedule.MaxTraveler} người)!");
            }
            var booking = new Booking
            {
                ScheduleId = model.ScheduleId,
                Note = model.Note,
                CreatedBy = userId,
                UserId = model.UserId,
                CreatedTime = DateTime.UtcNow,
                LastUpdatedTime = DateTime.UtcNow,
                LastUpdatedBy = userId,
                TotalPrice = model.NumberOfParticipants * schedule.Tour.HourlyRate * countHour,
                TotalTravelers = model.NumberOfParticipants,
            };
            await _unitOfWork.GetRepository<Booking>().InsertAsync(booking);
            await _unitOfWork.SaveAsync();

            // Create PayOS payment request
            var payOSRequest = new CreatePayOSPaymentRequest
            {
                Id = booking.Id,
                //... other necessary information for PayOS...
            };

            // Call PaymentService to generate payment link
            string checkoutUrl = await _paymentService.CreatePayOSPaymentLink(payOSRequest);
            return checkoutUrl;
        }
    }
}
