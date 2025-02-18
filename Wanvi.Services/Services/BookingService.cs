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
    public class BookingService : IBookingService
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
        public async Task<string> CreateBookingAll(CreateBookingModel model)
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
            //Số giờ của dịch vụ
            int countHour = (schedule.EndTime.Hours - schedule.StartTime.Hours);
            if (countHour < 0)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Lỗi tính lịch");
            }
            if (model.NumberOfParticipants > schedule.MaxTraveler)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, $"Số người đăng kí lớn hơn số người mặc định({schedule.MaxTraveler} người)!");
            }
            // Lấy ngày tháng năm của DateOfArrival và ngày hiện tại để so sánh
            if (model.RentalDate.Date < DateTime.Now.Date)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Ngày đến phải lớn hơn hoặc bằng ngày hiện tại!");
            }

            // Lấy danh sách booking hợp lệ (trạng thái không phải Cancelled)
            var existingBookings = await _unitOfWork.GetRepository<Booking>().Entities
                .Where(x => x.ScheduleId == model.ScheduleId
                            && x.Status != BookingStatus.Cancelled
                            && x.Status != BookingStatus.Refunded) // Loại bỏ booking bị hủy hoặc hoàn tiền
                .ToListAsync();

            // Tính tổng số người đã đặt trước đó
            int totalBooked = existingBookings.Sum(b => b.TotalTravelers);

            // Tính số chỗ còn trống
            int availableSlots = schedule.MaxTraveler - totalBooked;

            if (model.NumberOfParticipants > availableSlots)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest,
                    $"Số người đăng ký ({model.NumberOfParticipants}) vượt quá số slot trống ({availableSlots})!");
            }

            //Tìm người dùng đặt và kt số tiền có đủ để thanh toán không
            var user = await _unitOfWork.GetRepository<ApplicationUser>().Entities.FirstOrDefaultAsync(x => x.Id == cb && !x.DeletedTime.HasValue) ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Không tìm thấy người dùng!");
            int Total = (int)(model.NumberOfParticipants * schedule.Tour.HourlyRate * countHour);
            if (user.Balance < Total)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Số tiền của quý khách không đủ thực hiện giao dịch này!");

            }
            user.Balance -= Total;
            await _unitOfWork.GetRepository<ApplicationUser>().UpdateAsync(user);

            // Lấy giờ, phút của StartTime và so sánh với giờ phút hiện tại
            TimeSpan currentTime = DateTime.Now.TimeOfDay;

            if (schedule.StartTime.Add(TimeSpan.FromHours(1)) < currentTime)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Vui lòng đặt tour trước 1 tiếng!");
            }

            var booking = new Booking
            {
                Id = Guid.NewGuid().ToString("N"),
                ScheduleId = model.ScheduleId,
                Note = model.Note,
                CreatedBy = userId,
                UserId = model.UserId,
                CreatedTime = DateTime.UtcNow,
                LastUpdatedTime = DateTime.UtcNow,
                LastUpdatedBy = userId,
                TotalPrice = model.NumberOfParticipants * schedule.Tour.HourlyRate * countHour,
                TotalTravelers = model.NumberOfParticipants,
                Status = BookingStatus.DepositedAll,
            };
            await _unitOfWork.GetRepository<Booking>().InsertAsync(booking);
            //await _unitOfWork.SaveAsync();

            var bookingDetail = new BookingDetail
            {
                Age = model.Age,
                BookingId = booking.Id,
                CreatedBy = userId,
                Email = model.Email,
                CreatedTime = DateTime.Now,
                LastUpdatedTime = DateTime.UtcNow,
                LastUpdatedBy = userId,
                PassportNumber = user.IdentificationNumber,
                IdentityCard = user.IdentificationNumber,
                PhoneNumber = model.PhoneNumber,
                TravelerName = model.TravelerName,           
            };

            await _unitOfWork.GetRepository<BookingDetail>().InsertAsync(bookingDetail);


            // Create PayOS payment request
            var payOSRequest = new CreatePayOSPaymentRequest
            {
                Id = booking.Id,
                //... other necessary information for PayOS...
            };

            // 7. Tạo bản ghi Payment mới
            var payment = new Payment
            {
                Id = Guid.NewGuid().ToString("N"),
                Method = PaymentMethod.Banking, // Hoặc PaymentMethod phù hợp với PayOS
                Status = PaymentStatus.Unpaid,
                Amount = booking.TotalPrice,
                CreatedBy = userId,
                LastUpdatedBy = userId,
                CreatedTime = DateTime.UtcNow,
                LastUpdatedTime = DateTime.UtcNow,
                 
                //... các thông tin khác (nếu cần)...
            };
            await _unitOfWork.GetRepository<Payment>().InsertAsync(payment);

            // 8. Tạo bản ghi BookingPayment để liên kết Booking và Payment
            var bookingPayment = new BookingPayment
            {
                BookingId = booking.Id,
                PaymentId = payment.Id
            };
            await _unitOfWork.GetRepository<BookingPayment>().InsertAsync(bookingPayment);

            await _unitOfWork.SaveAsync();

            // Call PaymentService to generate payment link
            string checkoutUrl = await _paymentService.CreatePayOSPaymentLink(payOSRequest);
            return checkoutUrl;
        }
    }
}
