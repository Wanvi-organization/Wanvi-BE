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
        private readonly IEmailService _emailService;

        public BookingService(IMapper mapper, UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, IHttpContextAccessor contextAccessor, IPaymentService paymentService, IEmailService emailService)
        {
            _mapper = mapper;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _contextAccessor = contextAccessor;
            _paymentService = paymentService;
            _emailService = emailService;
        }

        public async Task<List<GetBookingUsermodel>> GetBookingAdmin(
            string? searchNote = null,
            string? sortBy = null,
            bool isAscending = false,
            string? status = null)
        {
            //string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);
            //Guid.TryParse(userId, out Guid cb);

            var query = _unitOfWork.GetRepository<Booking>()
                .Entities
                .OrderByDescending(x => x.CreatedTime)
                .Where(x => !x.DeletedTime.HasValue);

            // Lọc theo `Note` (tìm kiếm gần đúng, không phân biệt hoa thường)
            if (!string.IsNullOrWhiteSpace(searchNote))
            {
                query = query.Where(x => x.Note != null && x.Note.ToLower().Contains(searchNote.ToLower()));
            }

            // Lọc theo `Status`
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (BookingStatus.TryParse(status, out BookingStatus statusEnum))
                {
                    query = query.Where(x => x.Status == statusEnum);
                }
            }

            // Sắp xếp theo yêu cầu
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                query = sortBy.ToLower() switch
                {
                    "rentaldate" => isAscending ? query.OrderBy(x => x.RentalDate) : query.OrderByDescending(x => x.RentalDate),
                    "totalprice" => isAscending ? query.OrderBy(x => x.TotalPrice) : query.OrderByDescending(x => x.TotalPrice),
                    _ => query.OrderByDescending(x => x.CreatedTime) // Mặc định sắp xếp theo thời gian tạo (mới nhất lên đầu)
                };
            }

            var bookings = await query.ToListAsync();

            var bookingModels = bookings.Select(b => new GetBookingUsermodel
            {
                Id = b.Id.ToString(),
                TotalTravelers = b.TotalTravelers,
                TotalPrice = b.TotalPrice,
                Note = b.Note,
                RentalDate = b.RentalDate.ToString("dd/MM/yyyy"), // Chỉ lấy ngày/tháng/năm
                Status = ConvertStatusToString(b.Status),
                StartTime = b.Schedule?.StartTime.ToString(@"hh\:mm") ?? "00:00", // Chỉ lấy giờ:phút
                EndTime = b.Schedule?.EndTime.ToString(@"hh\:mm") ?? "00:00"
            }).ToList();

            return bookingModels;
        }

        public async Task<List<GetBookingUsermodel>> GetBookingUser(
            string? searchNote = null,
            string? sortBy = null,
            bool isAscending = false,
            string? status = null)
        {
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);
            Guid.TryParse(userId, out Guid cb);

            var query = _unitOfWork.GetRepository<Booking>()
                .Entities
                .OrderByDescending(x => x.CreatedTime)
                .Where(x => x.UserId == cb && !x.DeletedTime.HasValue);

            // Lọc theo `Note` (tìm kiếm gần đúng, không phân biệt hoa thường)
            if (!string.IsNullOrWhiteSpace(searchNote))
            {
                query = query.Where(x => x.Note != null && x.Note.ToLower().Contains(searchNote.ToLower()));
            }

            // Lọc theo `Status`
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (BookingStatus.TryParse(status, out BookingStatus statusEnum))
                {
                    query = query.Where(x => x.Status == statusEnum);
                }
            }

            // Sắp xếp theo yêu cầu
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                query = sortBy.ToLower() switch
                {
                    "rentaldate" => isAscending ? query.OrderBy(x => x.RentalDate) : query.OrderByDescending(x => x.RentalDate),
                    "totalprice" => isAscending ? query.OrderBy(x => x.TotalPrice) : query.OrderByDescending(x => x.TotalPrice),
                    _ => query.OrderByDescending(x => x.CreatedTime) // Mặc định sắp xếp theo thời gian tạo (mới nhất lên đầu)
                };
            }

            var bookings = await query.ToListAsync();

            var bookingModels = bookings.Select(b => new GetBookingUsermodel
            {
                Id = b.Id.ToString(),
                TotalTravelers = b.TotalTravelers,
                TotalPrice = b.TotalPrice,
                Note = b.Note,
                RentalDate = b.RentalDate.ToString("dd/MM/yyyy"), // Chỉ lấy ngày/tháng/năm
                Status = ConvertStatusToString(b.Status),
                StartTime = b.Schedule?.StartTime.ToString(@"hh\:mm") ?? "00:00", // Chỉ lấy giờ:phút
                EndTime = b.Schedule?.EndTime.ToString(@"hh\:mm") ?? "00:00"
            }).ToList();

            return bookingModels;
        }

        // Hàm chuyển đổi Status từ enum sang chữ
        private string ConvertStatusToString(BookingStatus status)
        {
            return status switch
            {
                BookingStatus.DepositHaft => "Đặt cọc 50%",
                BookingStatus.DepositAll => "Đặt cọc toàn bộ",
                BookingStatus.DepositedHaft => "Đã đặt cọc một phần",
                //BookingStatus.DepositHaftEnd => "Đặt cọc 50% còn lại",
                BookingStatus.Paid => "Đã thanh toán",
                BookingStatus.Completed => "Hoàn thành",
                BookingStatus.Cancelled => "Đã hủy",
                BookingStatus.Refunded => "Đã hoàn tiền",
                _ => "Không xác định"
            };
        }

        public async Task<List<GetBookingGuideModel>> GetBookingsByTourGuide(
            string? rentalDate = null,
            string? status = null,
            string? scheduleId = null,
            int? minTravelers = null,
            int? maxTravelers = null,
            string? sortBy = "RentalDate",
            bool ascending = false)
        {
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);
            Guid.TryParse(userId, out Guid guideId);

            // Lấy danh sách Tour của hướng dẫn viên
            var tours = await _unitOfWork.GetRepository<Tour>()
                .Entities.Where(t => t.UserId == guideId)
                .ToListAsync();

            var tourIds = tours.Select(t => t.Id).ToList();

            var excludedStatuses = new[] { BookingStatus.Completed, BookingStatus.Cancelled, BookingStatus.Refunded };

            // Lấy danh sách Booking
            var bookings = await _unitOfWork.GetRepository<Booking>()
                .Entities
                .Where(b => tourIds.Contains(b.Schedule.TourId)
                            && !excludedStatuses.Contains(b.Status))
                .Include(b => b.Schedule)
                .ToListAsync();

            // Nhóm Booking theo ScheduleId + RentalDate
            var groupedBookings = bookings
                .GroupBy(b => new { b.ScheduleId, b.RentalDate })
                .Select(group =>
                {
                    var bookingsList = group.ToList();
                    var allBookingDetailsCompleted = bookingsList
                        .All(d => excludedStatuses.Contains(d.Status));

                    return new GetBookingGuideModel
                    {

                        RentalDate = group.Key.RentalDate.ToString("dd/MM/yyyy"),
                        TotalTravelers = group.Sum(b => b.TotalTravelers),
                        MaxTraveler = group.First().Schedule.MaxTraveler,
                        BookedTraveler = bookingsList.Where(b => !excludedStatuses.Contains(b.Status)).Sum(b => b.TotalTravelers),
                        //Status = allBookingDetailsCompleted ? "Hoàn thành" : "Chưa hoàn thành",

                        Bookings = bookingsList.Select(b => new GetBookingUsermodel
                        {
                            Id = b.Id.ToString(),
                            TotalTravelers = b.TotalTravelers,
                            TotalPrice = b.TotalPrice,
                            Note = b.Note,
                            RentalDate = b.RentalDate.ToString("dd/MM/yyyy"),
                            Status = ConvertStatusToString(b.Status),
                            StartTime = b.Schedule?.StartTime.ToString(@"hh\:mm") ?? "00:00",
                            EndTime = b.Schedule?.EndTime.ToString(@"hh\:mm") ?? "00:00"
                        }).ToList()
                    };
                })
                .ToList();

            // **💡 BƯỚC 1: Tìm kiếm**
            if (!string.IsNullOrEmpty(rentalDate))
            {
                groupedBookings = groupedBookings
                    .Where(b => b.RentalDate == rentalDate)
                    .ToList();
            }

            //if (!string.IsNullOrEmpty(status))
            //{
            //    groupedBookings = groupedBookings
            //        .Where(b => b.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
            //        .ToList();
            //}

            if (minTravelers.HasValue)
            {
                groupedBookings = groupedBookings
                    .Where(b => b.TotalTravelers >= minTravelers.Value)
                    .ToList();
            }

            if (maxTravelers.HasValue)
            {
                groupedBookings = groupedBookings
                    .Where(b => b.TotalTravelers <= maxTravelers.Value)
                    .ToList();
            }

            // **💡 BƯỚC 2: Sắp xếp**
            groupedBookings = sortBy switch
            {
                "RentalDate" => ascending
                    ? groupedBookings.OrderBy(b => DateTime.ParseExact(b.RentalDate, "dd/MM/yyyy", null)).ToList()
                    : groupedBookings.OrderByDescending(b => DateTime.ParseExact(b.RentalDate, "dd/MM/yyyy", null)).ToList(),

                "TotalTravelers" => ascending
                    ? groupedBookings.OrderBy(b => b.TotalTravelers).ToList()
                    : groupedBookings.OrderByDescending(b => b.TotalTravelers).ToList(),

                "BookedTraveler" => ascending
                    ? groupedBookings.OrderBy(b => b.BookedTraveler).ToList()
                    : groupedBookings.OrderByDescending(b => b.BookedTraveler).ToList(),

                _ => groupedBookings
            };

            return groupedBookings;
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

            // Danh sách tên thứ trong tuần theo ENUM của bạn (Thứ 2 = 0)
            string[] daysInVietnamese = { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "Chủ nhật" };

            // Lấy thứ trong tuần của ngày đặt
            int rentalDayIndex = ((int)model.RentalDate.DayOfWeek + 6) % 7; // Chuyển đổi để Thứ 2 = 0
            string rentalDay = daysInVietnamese[rentalDayIndex];

            // Lấy thứ trong tuần của Schedule
            string scheduleDay = daysInVietnamese[(int)schedule.Day];

            // Kiểm tra ngày đặt có đúng với lịch trình không
            if (rentalDayIndex != (int)schedule.Day)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest,
                    $"Ngày đặt: ({rentalDay}) không phù hợp với lịch trình của hướng dẫn viên: ({scheduleDay})!");
            }

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
            // Lấy ngày tháng năm của DateOfArrival và ngày hiện tại để so sánh, điều kiện phải đặt trước 2 ngày
            if (model.RentalDate.ToUniversalTime().Date < DateTime.UtcNow.AddDays(2).Date)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Bạn chỉ có thể đặt tour trước 2 ngày!");
            }

            // Lấy danh sách booking hợp lệ (cùng Schedule, cùng ngày, trạng thái hợp lệ)
            var existingBookings = await _unitOfWork.GetRepository<Booking>().Entities
                .Include(p => p.Payments)
                .Where(x => x.ScheduleId == model.ScheduleId
                            && x.Status != BookingStatus.Cancelled
                            && x.Status != BookingStatus.Refunded
                            && x.Status != BookingStatus.Completed
                            && x.Status != BookingStatus.DepositAll
                            && x.Status != BookingStatus.DepositHaft
                            && x.RentalDate.Date == model.RentalDate.Date
                            && !x.DeletedTime.HasValue) // Chỉ lấy booking có ngày đặt trùng với model
                .ToListAsync();

            // Tính tổng số người đã đặt trước đó trong ngày
            int totalBooked = existingBookings.Sum(b => b.TotalTravelers);

            // Tính số chỗ còn trống
            int availableSlots = schedule.MaxTraveler - totalBooked;

            if (model.NumberOfParticipants > availableSlots)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest,
                    $"Số người đăng ký ({model.NumberOfParticipants}) vượt quá số slot trống ({availableSlots}) trong ngày {model.RentalDate:dd/MM/yyyy}!");
            }

            //Tìm người dùng đặt và kt số tiền có đủ để thanh toán không
            var user = await _unitOfWork.GetRepository<ApplicationUser>().Entities.FirstOrDefaultAsync(x => x.Id == cb && !x.DeletedTime.HasValue) ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Không tìm thấy người dùng!");
            int Total = (int)(model.NumberOfParticipants * schedule.Tour.HourlyRate * countHour);
            //if (user.Balance < Total)
            //{
            //    throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Số tiền của quý khách không đủ thực hiện giao dịch này!");

            //}

            var booking = new Booking
            {
                Id = Guid.NewGuid().ToString("N"),
                ScheduleId = model.ScheduleId,
                Note = model.Note,
                OrderCode = await GenerateUniqueOrderCodeAsync(),
                CreatedBy = userId,
                UserId = cb,
                CreatedTime = DateTime.UtcNow,
                LastUpdatedTime = DateTime.UtcNow,
                LastUpdatedBy = userId,
                RentalDate = model.RentalDate,
                TotalPrice = model.NumberOfParticipants * schedule.Tour.HourlyRate * countHour,
                TotalTravelers = model.NumberOfParticipants,
                Status = BookingStatus.DepositAll,
            };
            await _unitOfWork.GetRepository<Booking>().InsertAsync(booking);
            //await _unitOfWork.SaveAsync();

            var bookingDetail = new BookingDetail
            {
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

            await _unitOfWork.SaveAsync();

            // Call PaymentService to generate payment link
            //string checkoutUrl = await _paymentService.CreatePayOSPaymentLink(payOSRequest);
            return "Tạo đơn hàng thành công";
        }
        public async Task<string> CreateBookingHaft(CreateBookingModel model)
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

            // Danh sách tên thứ trong tuần theo ENUM của bạn (Thứ 2 = 0)
            string[] daysInVietnamese = { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "Chủ nhật" };

            // Lấy thứ trong tuần của ngày đặt
            int rentalDayIndex = ((int)model.RentalDate.DayOfWeek + 6) % 7; // Chuyển đổi để Thứ 2 = 0
            string rentalDay = daysInVietnamese[rentalDayIndex];

            // Lấy thứ trong tuần của Schedule
            string scheduleDay = daysInVietnamese[(int)schedule.Day];

            // Kiểm tra ngày đặt có đúng với lịch trình không
            if (rentalDayIndex != (int)schedule.Day)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest,
                    $"Ngày đặt: ({rentalDay}) không phù hợp với lịch trình của hướng dẫn viên: ({scheduleDay})!");
            }

            // Lấy ngày tháng năm của DateOfArrival và ngày hiện tại để so sánh, điều kiện phải đặt trước 2 ngày
            if (model.RentalDate.Date < DateTime.Now.AddDays(2).Date)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Bạn chỉ có thể đạt tour trước 2 ngày!");
            }

            // Lấy danh sách booking hợp lệ (cùng Schedule, cùng ngày, trạng thái hợp lệ)
            var existingBookings = await _unitOfWork.GetRepository<Booking>().Entities
                .Where(x => x.ScheduleId == model.ScheduleId
                            && x.Status != BookingStatus.Cancelled
                            && x.Status != BookingStatus.Refunded
                            && x.RentalDate.Date == model.RentalDate.Date
                            && !x.DeletedTime.HasValue) // Chỉ lấy booking có ngày đặt trùng với model
                .ToListAsync();

            // Tính tổng số người đã đặt trước đó trong ngày
            int totalBooked = existingBookings.Sum(b => b.TotalTravelers);

            // Tính số chỗ còn trống
            int availableSlots = schedule.MaxTraveler - totalBooked;

            if (model.NumberOfParticipants > availableSlots)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest,
                    $"Số người đăng ký ({model.NumberOfParticipants}) vượt quá số slot trống ({availableSlots}) trong ngày {model.RentalDate:dd/MM/yyyy}!");
            }

            //Tìm người dùng đặt và kt số tiền có đủ để thanh toán không
            var user = await _unitOfWork.GetRepository<ApplicationUser>().Entities.FirstOrDefaultAsync(x => x.Id == cb && !x.DeletedTime.HasValue) ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Không tìm thấy người dùng!");
            //Số tiền tour phải cọc
            int Total = (int)(model.NumberOfParticipants * schedule.Tour.HourlyRate * countHour * 0.5);
            //if (user.Balance < Total)
            //{
            //    throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Số tiền của quý khách không đủ thực hiện giao dịch này!");

            //}

            var booking = new Booking
            {
                Id = Guid.NewGuid().ToString("N"),
                ScheduleId = model.ScheduleId,
                Note = model.Note,
                CreatedBy = userId,
                UserId = cb,
                OrderCode = await GenerateUniqueOrderCodeAsync(),
                RentalDate = model.RentalDate,
                CreatedTime = DateTime.UtcNow,
                LastUpdatedTime = DateTime.UtcNow,
                LastUpdatedBy = userId,
                TotalPrice = model.NumberOfParticipants * schedule.Tour.HourlyRate * countHour,
                TotalTravelers = model.NumberOfParticipants,
                Status = BookingStatus.DepositHaft,
            };
            await _unitOfWork.GetRepository<Booking>().InsertAsync(booking);
            //await _unitOfWork.SaveAsync();

            var bookingDetail = new BookingDetail
            {
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

            await _unitOfWork.SaveAsync();

            // Call PaymentService to generate payment link
            //string checkoutUrl = await _paymentService.CreatePayOSPaymentLink(payOSRequest);
            return "Tạo đơn hàng thành công";
        }

        public async Task<string> ChangeBookingToUser(ChangeBookingToUserModel model)
        {
            var existingBookings = await _unitOfWork.GetRepository<Booking>().Entities
                .FirstOrDefaultAsync(x => x.Id == model.BookingId
                            && x.Status == BookingStatus.Completed
                            && x.Request == false
                            && !x.DeletedTime.HasValue) ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Không tìm thấy đơn hàng!");
            //Cập nhật đơn hàng
            existingBookings.Status = BookingStatus.Refunded;
            existingBookings.Request = true;

            //Tìm HDV
            var tourGuide = await _unitOfWork.GetRepository<ApplicationUser>().Entities.FirstOrDefaultAsync(x => x.Id == existingBookings.Schedule.Tour.UserId && !x.DeletedTime.HasValue) ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Không tìm thấy hướng dẫn viên!");
            //Cập nhật tiền của HDV
            tourGuide.Balance += existingBookings.TotalTravelers;//Chuyển vào tiền lấy làm cọc
            tourGuide.Deposit -= existingBookings.TotalTravelers;//trừ tiền đã chuyển vào cọc

            Request request = new Request()
            {
                Balance = tourGuide.Balance,
                CreatedTime = DateTime.UtcNow,
                LastUpdatedTime = DateTime.UtcNow,
                LastUpdatedBy = tourGuide.Id.ToString(),
                CreatedBy = tourGuide.Id.ToString(),
                OrderCode = existingBookings.OrderCode,
                Status = Core.Constants.Enum.RequestStatus.Confirmed,
                Note = model.Note,
                UserId = tourGuide.Id,
                //Bank = tourGuide.Bank,
                //BankAccount = tourGuide.BankAccount,
                //BankAccountName = tourGuide.BankAccountName,               
            };
            //add request
            await _unitOfWork.GetRepository<Request>().InsertAsync(request);

            await _unitOfWork.SaveAsync();
            return "Chuyển tiền thành công!";
        }

        public async Task<string> WithdrawMoneyFromBooking(WithdrawMoneyFromBookingModel model)
        {
            var existingBookings = await _unitOfWork.GetRepository<Booking>().Entities
            .FirstOrDefaultAsync(x => x.Id == model.BookingId
                && x.Status == BookingStatus.Completed
                && x.Request == false
                && !x.DeletedTime.HasValue) ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Không tìm thấy đơn hàng!");
            //Cập nhật đơn hàng
            existingBookings.Status = BookingStatus.Refunded;
            existingBookings.Request = true;
            //Tìm HDV
            var tourGuide = await _unitOfWork.GetRepository<ApplicationUser>().Entities.FirstOrDefaultAsync(x => x.Id == existingBookings.Schedule.Tour.UserId && !x.DeletedTime.HasValue) ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Không tìm thấy hướng dẫn viên!");
            ////Cập nhật tiền của HDV
            //tourGuide.Balance += existingBookings.TotalTravelers;//Chuyển vào tiền lấy làm cọc
            //tourGuide.Deposit -= existingBookings.TotalTravelers;//trừ tiền đã chuyển vào cọc

            Request request = new Request()
            {
                Balance = tourGuide.Balance,
                CreatedTime = DateTime.UtcNow,
                LastUpdatedTime = DateTime.UtcNow,
                LastUpdatedBy = tourGuide.Id.ToString(),
                CreatedBy = tourGuide.Id.ToString(),
                OrderCode = existingBookings.OrderCode,
                Status = Core.Constants.Enum.RequestStatus.Pending,
                Note = model.Note,
                UserId = tourGuide.Id,
                Bank = tourGuide.Bank,
                BankAccount = tourGuide.BankAccount,
                BankAccountName = tourGuide.BankAccountName,
            };
            //add request
            await _unitOfWork.GetRepository<Request>().InsertAsync(request);

            await _emailService.SendEmailAsync(
                tourGuide.Email,
                "Yêu cầu rút tiền từ đơn hàng",
                $@"
                <html>
                <head>
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            background-color: #f4f4f4;
                            margin: 0;
                            padding: 0;
                        }}
                        .container {{
                            width: 100%;
                            max-width: 600px;
                            margin: 20px auto;
                            background: #ffffff;
                            padding: 20px;
                            border-radius: 8px;
                            box-shadow: 0px 0px 10px rgba(0, 0, 0, 0.1);
                        }}
                        h2 {{
                            color: #333;
                        }}
                        p {{
                            font-size: 16px;
                            line-height: 1.6;
                            color: #555;
                        }}
                        .footer {{
                            margin-top: 20px;
                            font-size: 14px;
                            color: #777;
                            text-align: center;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h2>Xin chào {tourGuide.FullName},</h2>
                        <p>Chúng tôi đã nhận được yêu cầu rút tiền từ đơn hàng của bạn.</p>
                        <p>Việc hoàn tiền sẽ được xử lý theo quy định của chính sách và có thể mất một khoảng thời gian nhất định.</p>
                        <p>Nếu bạn có bất kỳ thắc mắc nào, vui lòng liên hệ với bộ phận hỗ trợ khách hàng.</p>
                        <div class='footer'>
                            <p>Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!</p>
                        </div>
                    </div>
                </body>
                </html>"
            );

            await _unitOfWork.SaveAsync();
            return "Gửi yêu cầu thành công!";
        }

        private async Task<long> GenerateUniqueOrderCodeAsync()
        {
            Random random = new Random();
            long orderCode;
            bool exists;

            do
            {
                orderCode = random.NextInt64(10000000, 9999999999); // Sinh số ngẫu nhiên 8 chữ số
                exists = await _unitOfWork.GetRepository<Booking>().Entities
                    .AnyAsync(x => x.OrderCode == orderCode && !x.DeletedTime.HasValue);
            }
            while (exists);

            return orderCode;
        }

    }
}
