using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        public async Task<List<GetBookingUserModel>> GetBookingAdmin(
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

            var bookingModels = bookings.Select(b => new GetBookingUserModel
            {
                Id = b.Id.ToString(),
                TotalTravelers = b.TotalTravelers,
                TotalPrice = b.TotalPrice,
                Note = b.Note,
                RentalDate = b.RentalDate.ToString("dd/MM/yyyy"), // Chỉ lấy ngày/tháng/năm
                Status = ConvertStatusToString(b.Status),
                StartTime = b.Schedule?.StartTime.ToString(@"hh\:mm") ?? "00:00", // Chỉ lấy giờ:phút
                EndTime = b.Schedule?.EndTime.ToString(@"hh\:mm") ?? "00:00",
                TourName = b.Schedule.Tour.Name
            }).ToList();

            return bookingModels;
        }

        public async Task<List<GetBookingUserModel>> GetBookingUser(
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

            var bookingModels = bookings.Select(b => new GetBookingUserModel
            {
                Id = b.Id.ToString(),
                TotalTravelers = b.TotalTravelers,
                TotalPrice = b.TotalPrice,
                Note = b.Note,
                RentalDate = b.RentalDate.ToString("dd/MM/yyyy"), // Chỉ lấy ngày/tháng/năm
                Status = ConvertStatusToString(b.Status),
                StartTime = b.Schedule?.StartTime.ToString(@"hh\:mm") ?? "00:00", // Chỉ lấy giờ:phút
                EndTime = b.Schedule?.EndTime.ToString(@"hh\:mm") ?? "00:00",
                TourName = b.Schedule.Tour.Name
            }).ToList();

            return bookingModels;
        }

        // Hàm chuyển đổi Status từ enum sang chữ
        private string ConvertStatusToString(BookingStatus status)
        {
            return status switch
            {
                BookingStatus.DepositHaft => "Đặt cọc 50%",
                BookingStatus.DepositAll => "Đặt cọc 100%",
                BookingStatus.DepositedHaft => "Đã đặt cọc 50%",
                //BookingStatus.DepositHaftEnd => "Đặt cọc 50% còn lại",
                BookingStatus.Paid => "Đã thanh toán",
                BookingStatus.Completed => "Hoàn thành",
                BookingStatus.Cancelled => "Đã hủy",
                BookingStatus.Refunded => "Đã hoàn tiền",
                _ => "Không xác định"
            };
        }

        public async Task<List<GetBookingUserDetailModel>> GetBookingsByTourGuide(
    string? rentalDate = null,
    string? status = null,
    string? scheduleId = null,
    int? minTravelers = null,
    int? maxTravelers = null)
        {
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);
            if (!Guid.TryParse(userId, out Guid guideId))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "UserId không hợp lệ.");
            }

            var excludedStatuses = new[]
            {
        BookingStatus.Completed, BookingStatus.Cancelled, BookingStatus.Refunded,
        BookingStatus.DepositAll, BookingStatus.DepositHaft
    };

            // **💡 Lấy danh sách Booking của hướng dẫn viên theo Tour.UserId**
            var bookings = await _unitOfWork.GetRepository<Booking>()
                .Entities
                .Where(b => b.Schedule.Tour.UserId == guideId
                            && !excludedStatuses.Contains(b.Status))
                .OrderBy(x => x.RentalDate)
                .Include(b => b.Schedule)
                .ThenInclude(s => s.Tour)
                .ToListAsync();

            if (!bookings.Any())
            {
                return new List<GetBookingUserDetailModel>(); // Không có dữ liệu, trả về danh sách rỗng
            }

            // **💡 Gom nhóm theo `ScheduleId` + `RentalDate` để loại bỏ trùng ngày**
            var groupedBookings = bookings
                .GroupBy(b => new { b.ScheduleId, b.RentalDate }) // Nhóm theo ScheduleId + Ngày đặt
                .Select(g => new GetBookingUserDetailModel
                {
                    ScheduleId = g.Key.ScheduleId.ToString(),
                    RentalDate = g.Key.RentalDate.ToString("dddd, dd/MM/yyyy", new CultureInfo("vi-VN")),
                    TotalTravelers = g.Sum(b => b.TotalTravelers), // **Cộng tổng số khách**
                    TotalTravelersOfTour = g.First().Schedule.MaxTraveler, // **Số khách tối đa của tour**
                    StartTime = g.First().Schedule.StartTime.ToString(@"hh\:mm"),
                    EndTime = g.First().Schedule.EndTime.ToString(@"hh\:mm"),
                    TourName = g.First().Schedule.Tour?.Name ?? "Không có dữ liệu"
                })
                //.OrderBy(g => Math.Abs((DateTime.ParseExact(g.RentalDate.Split(", ")[1], "dd/MM/yyyy", new CultureInfo("vi-VN")) - DateTime.Now).TotalDays)) // **Sắp xếp ngày gần nhất lên đầu**
                .ToList();

            // **💡 BƯỚC 1: Lọc theo điều kiện nếu có**
            if (!string.IsNullOrEmpty(rentalDate))
            {
                if (DateTime.TryParseExact(rentalDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    groupedBookings = groupedBookings
                        .Where(s => s.RentalDate.Contains(parsedDate.ToString("dd/MM/yyyy")))
                        .ToList();
                }
            }

            if (minTravelers.HasValue)
            {
                groupedBookings = groupedBookings
                    .Where(s => s.TotalTravelers >= minTravelers.Value)
                    .ToList();
            }

            if (maxTravelers.HasValue)
            {
                groupedBookings = groupedBookings
                    .Where(s => s.TotalTravelers <= maxTravelers.Value)
                    .ToList();
            }

            return groupedBookings;
        }

        public async Task<GetBookingGuideModel> GetBookingSummaryBySchedule(
    string scheduleId,
    string rentalDate,
    string? status = null,
    int? minPrice = null,
    int? maxPrice = null,
    string sortBy = "CustomerName",
    bool ascending = true)
        {
            var excludedStatuses = new[]
            {
        BookingStatus.Completed, BookingStatus.Cancelled, BookingStatus.Refunded,
        BookingStatus.DepositAll, BookingStatus.DepositHaft
            };

            // **💡 Lấy Schedule & Tour theo ScheduleId**
            var schedule = await _unitOfWork.GetRepository<Schedule>()
                .Entities
                .Where(s => s.Id == scheduleId)
                .Include(s => s.Tour)
                .Include(s => s.Bookings)
                .ThenInclude(b => b.User) // Lấy thông tin khách hàng
                .FirstOrDefaultAsync()
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NotFound, "Không tìm thấy lịch trình.");

            // **💡 Lọc Booking hợp lệ**
            var validBookings = schedule.Bookings
                .Where(b => !excludedStatuses.Contains(b.Status) && b.RentalDate.ToString("dddd, dd/MM/yyyy", new CultureInfo("vi-VN")) == rentalDate)
                .ToList();

            if (!validBookings.Any())
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NotFound, "Không có đơn đặt chỗ hợp lệ.");
            }

            // **💡 Chuyển danh sách `Booking` thành `GetBookingUserByTourGuideModel`**
            var bookings = validBookings
                .Select(b => new GetBookingUserByTourGuideModel
                {
                    BookingId = b.Id.ToString(),
                    CustomerName = b.User?.FullName ?? "Không có dữ liệu",
                    Price = (int)b.TotalPrice,
                    Status = ConvertStatusToString(b.Status)
                })
                .ToList();

            // **💡 BƯỚC 1: Lọc dữ liệu**
            if (!string.IsNullOrEmpty(status))
            {
                bookings = bookings
                    .Where(b => b.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (minPrice.HasValue)
            {
                bookings = bookings
                    .Where(b => b.Price >= minPrice.Value)
                    .ToList();
            }

            if (maxPrice.HasValue)
            {
                bookings = bookings
                    .Where(b => b.Price <= maxPrice.Value)
                    .ToList();
            }

            // **💡 BƯỚC 2: Sắp xếp danh sách `Bookings`**
            bookings = sortBy switch
            {
                "CustomerName" => ascending
                    ? bookings.OrderBy(b => b.CustomerName).ToList()
                    : bookings.OrderByDescending(b => b.CustomerName).ToList(),

                "Price" => ascending
                    ? bookings.OrderBy(b => b.Price).ToList()
                    : bookings.OrderByDescending(b => b.Price).ToList(),

                _ => bookings
            };

            // **💡 Tạo `GetBookingGuideModel` duy nhất**
            return new GetBookingGuideModel
            {
                TourName = schedule.Tour?.Name ?? "Không có dữ liệu",
                RentalDate = rentalDate,
                TotalBooking = validBookings.Count, // **Tổng số đơn**
                TotailPrice = validBookings.Sum(b => (long)b.TotalPrice), // **Tổng doanh thu**
                Bookings = bookings // **Danh sách Booking đã lọc & sắp xếp**
            };
        }

        public async Task<GetBookingGuideScreen3Model> GetBookingDetailsById(string bookingId)
        {

            // **💡 Lấy thông tin Booking**
            var booking = await _unitOfWork.GetRepository<Booking>()
                .Entities
                .Where(b => b.Id == bookingId)
                .Include(b => b.User) // Thông tin khách hàng
                .Include(b => b.Schedule)
                .ThenInclude(s => s.Tour) // Lấy thông tin tour
                .FirstOrDefaultAsync()
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NotFound, "Không tìm thấy đơn hàng.");
            var pickupAddress = await _unitOfWork.GetRepository<Address>().Entities.FirstOrDefaultAsync(x => x.Id == booking.Schedule.Tour.PickupAddressId && !x.DeletedTime.HasValue) ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NotFound, "Không tìm thấy đại chỉ đón.");

            var dropoffAddress = await _unitOfWork.GetRepository<Address>().Entities.FirstOrDefaultAsync(x => x.Id == booking.Schedule.Tour.DropoffAddressId && !x.DeletedTime.HasValue) ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NotFound, "Không tìm thấy đại chỉ trả.");

            // **💡 Tạo `GetBookingGuideScreen3Model`**
            return new GetBookingGuideScreen3Model
            {
                TourName = booking.Schedule?.Tour?.Name ?? "Không có dữ liệu",
                TotailPrice = (long)booking.TotalPrice,
                RentalDate = booking.RentalDate.ToString("dddd, dd/MM/yyyy", new CultureInfo("vi-VN")),
                TotalCustomer = booking.TotalTravelers,
                CustomerName = booking.User?.FullName ?? "Không có dữ liệu",
                DropoffAddress = pickupAddress.Street,
                PickupAddress = dropoffAddress.Street,
                Status = ConvertStatusToString(booking.Status),
                Note = booking.Note ?? "Không có ghi chú",
                Email = booking.User.Email,
                Phone = booking.User.PhoneNumber,
                Gender = booking.User.Gender ? "Nam" : "Nữ",
            };
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
            // Lấy ngày tháng năm của DateOfArrival và ngày hiện tại để so sánh, điều kiện phải đặt trước 8 ngày
            if (model.RentalDate.Date < DateTime.Now.AddDays(8).Date)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Bạn chỉ có thể đặt tour trước 8 ngày!");
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
                CreatedTime = DateTime.Now,
                LastUpdatedTime = DateTime.Now,
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
                LastUpdatedTime = DateTime.Now,
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
            return booking.Id;
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

            // Lấy ngày tháng năm của DateOfArrival và ngày hiện tại để so sánh, điều kiện phải đặt trước 8 ngày
            if (model.RentalDate.Date < DateTime.Now.AddDays(8).Date)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Bạn chỉ có thể đạt tour trước 8 ngày!");
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
                CreatedTime = DateTime.Now,
                LastUpdatedTime = DateTime.Now,
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
                LastUpdatedTime = DateTime.Now,
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
            return booking.Id;
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
            tourGuide.Balance += (int)(existingBookings.TotalTravelers * 0.8);//Chuyển vào tiền lấy làm cọc
            tourGuide.Deposit -= existingBookings.TotalTravelers;//trừ tiền đã chuyển vào cọc

            Request request = new Request()
            {
                Balance = tourGuide.Balance,
                CreatedTime = DateTime.Now,
                LastUpdatedTime = DateTime.Now,
                LastUpdatedBy = tourGuide.Id.ToString(),
                CreatedBy = tourGuide.Id.ToString(),
                OrderCode = existingBookings.OrderCode,
                Status = RequestStatus.Confirmed,
                Note = $"Bạn nhận {existingBookings.TotalTravelers * 0.8:N0} đ là 80% tiền hóa đơn vì {existingBookings.TotalTravelers * 0.2:N0} đ là tiền khấu trừ hoa hồng!",
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
                CreatedTime = DateTime.Now,
                LastUpdatedTime = DateTime.Now,
                LastUpdatedBy = tourGuide.Id.ToString(),
                CreatedBy = tourGuide.Id.ToString(),
                OrderCode = existingBookings.OrderCode,
                Status = RequestStatus.Pending,
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

        public async Task<string> CancelBookingForCustomer(CancelBookingForCustomerModel model)
        {
            var booking = await _unitOfWork.GetRepository<Booking>().Entities.FirstOrDefaultAsync(x => x.Id == model.bookingId && !x.DeletedTime.HasValue) ??
            throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Không tìm thấy đơn hàng!");

            var tourGuide = await _unitOfWork.GetRepository<ApplicationUser>().Entities.FirstOrDefaultAsync(x => x.Id == booking.Schedule.Tour.UserId && !x.DeletedTime.HasValue) ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Không tìm thấy hướng dẫn viên!");

            var customer = await _unitOfWork.GetRepository<ApplicationUser>().Entities.FirstOrDefaultAsync(x => x.Id == booking.UserId && !x.DeletedTime.HasValue) ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Không tìm thấy khách hàng!");

            var daysBeforeStart = (booking.RentalDate - DateTime.UtcNow).TotalDays;
            double customerAmount = 0;
            double tourGuideAmount = 0;
            double multiplier = booking.Status == BookingStatus.DepositedHaft ? 0.5 : 1.0;

            if (daysBeforeStart > 7)
            {
                customerAmount = booking.TotalPrice * 0.1 * multiplier;
            }
            else if (daysBeforeStart >= 3)
            {
                customerAmount = booking.TotalPrice * 0.6 * multiplier;
                tourGuideAmount = booking.TotalPrice * 0.2 * multiplier;
            }
            else if (daysBeforeStart >= 2)
            {
                tourGuideAmount = booking.TotalPrice * 0.7 * multiplier;
            }
            else
            {
                tourGuideAmount = booking.TotalPrice * 0.8 * multiplier;
            }
            //Hoàn tiền cho khách hàng
            if (customerAmount > 0) customer.Balance += (int)customerAmount;
            //Hoàn tiền cho HDV
            if (tourGuideAmount > 0) tourGuide.Balance += (int)tourGuideAmount;
            //Đổi trạng thái của đơn
            booking.Status = BookingStatus.Cancelled;
            await _unitOfWork.GetRepository<Booking>().UpdateAsync(booking);
            await _unitOfWork.GetRepository<ApplicationUser>().UpdateAsync(tourGuide);
            await _unitOfWork.GetRepository<ApplicationUser>().UpdateAsync(customer);
            await _unitOfWork.SaveAsync();

            // Gửi email cho hướng dẫn viên
            await SendMailCancelToTourGuide(tourGuide, customer, booking);

            return "Hủy đơn thành công!";
        }

        public async Task<string> CancelBookingForGuide(CancelBookingForGuideModel model)
        {
            var booking = await _unitOfWork.GetRepository<Booking>().Entities.FirstOrDefaultAsync(x => x.Id == model.bookingId && !x.DeletedTime.HasValue) ??
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Không tìm thấy đơn hàng!");

            var tourGuide = await _unitOfWork.GetRepository<ApplicationUser>().Entities.FirstOrDefaultAsync(x => x.Id == booking.Schedule.Tour.UserId && !x.DeletedTime.HasValue) ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Không tìm thấy hướng dẫn viên!");

            var customer = await _unitOfWork.GetRepository<ApplicationUser>().Entities.FirstOrDefaultAsync(x => x.Id == booking.UserId && !x.DeletedTime.HasValue) ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Không tìm thấy khách hàng!");


            var daysBeforeStart = (booking.RentalDate - DateTime.Now).TotalDays;
            double penalty = 0;
            double refundToCustomer = 0;
            double multiplier = booking.Status == BookingStatus.DepositedHaft ? 0.5 : 1.0;

            if (daysBeforeStart >= 7)
            {
                refundToCustomer = booking.TotalPrice * 1.0 * multiplier;
            }
            else if (daysBeforeStart >= 3)
            {
                penalty = booking.TotalPrice * 0.2 * multiplier;
                refundToCustomer = booking.TotalPrice * 1.0 * multiplier;
            }
            else if (daysBeforeStart >= 2)
            {
                penalty = booking.TotalPrice * 0.4 * multiplier;
                refundToCustomer = booking.TotalPrice * 1.0 * multiplier;
            }
            else
            {
                penalty = booking.TotalPrice * 0.8 * multiplier;
                refundToCustomer = booking.TotalPrice * 1.0 * multiplier;
            }
            //Trừ tiền vi phạm của HDV
            if (penalty > 0) tourGuide.Balance -= (int)penalty;
            //Trả + tiền bồi thường của khách hàng
            if (refundToCustomer > 0) customer.Balance += (int)refundToCustomer;
            //Cập nhật lại trạng thái của đơn
            booking.Status = BookingStatus.Cancelled;
            //lưu vào DB
            await _unitOfWork.GetRepository<Booking>().UpdateAsync(booking);
            await _unitOfWork.GetRepository<ApplicationUser>().UpdateAsync(tourGuide);
            await _unitOfWork.GetRepository<ApplicationUser>().UpdateAsync(customer);
            await _unitOfWork.SaveAsync();

            // Gửi email cho khách hàng
            await SendMailCancelToCustomer(customer, booking);

            return "Hủy đơn thành công!";
        }

        private async Task SendMailCancelToCustomer(ApplicationUser customer, Booking booking)
        {
            await _emailService.SendEmailAsync(
                customer.Email,
                "Thông Báo Hủy Tour",
                $@"
            <html>
            <body>
                <h2>THÔNG BÁO HỦY TOUR</h2>
                <p>Xin chào {customer.FullName},</p>
                <p>Chúng tôi xin thông báo rằng hướng dẫn viên đã hủy tour của bạn.</p>
                <p><strong>Tên tour:</strong> {booking.Schedule.Tour.Name}</p>
                <p><strong>Mã đơn hàng:</strong> {booking.OrderCode}</p>
                <p><strong>Ngày khởi hành:</strong> {booking.RentalDate:dd/MM/yyyy}</p>
                <p><strong>Giờ:</strong> {booking.Schedule.StartTime:HH:mm} - {booking.Schedule.EndTime:HH:mm}</p>
                <p>Chúng tôi xin lỗi vì sự bất tiện này!</p>
            </body>
            </html>"
            );
        }

        private async Task SendMailCancelToTourGuide(ApplicationUser guide, ApplicationUser customer, Booking booking)
        {
            await _emailService.SendEmailAsync(
                guide.Email,
                "Thông Báo Khách Hủy Tour",
                $@"
            <html>
            <body>
                <h2>THÔNG BÁO HỦY TOUR</h2>
                <p>Khách hàng <strong>{customer.FullName}</strong> đã hủy tour của bạn.</p>
                <p><strong>Mã đơn hàng:</strong> {booking.OrderCode}</p>
                <p><strong>Tên tour:</strong> {booking.Schedule.Tour.Name}</p>
                <p><strong>Ngày khởi hành:</strong> {booking.RentalDate:dd/MM/yyyy}</p>
                <p><strong>Giờ:</strong> {booking.Schedule.StartTime:HH:mm} - {booking.Schedule.EndTime:HH:mm}</p>
            </body>
            </html>"
            );
        }

        public async Task<string> CancelBookingForAdmin(CancelBookingForAdminModel model)
        {
            var tourGuide = await _unitOfWork.GetRepository<ApplicationUser>().Entities
                .FirstOrDefaultAsync(x => x.Id == model.UserId && !x.DeletedTime.HasValue)
                ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Không tìm thấy hướng dẫn viên!");
            //HDV vi phạm đôi trạng thái
            tourGuide.Violate = true;
            var bookingList = await _unitOfWork.GetRepository<Booking>().Entities
                .Where(x => x.Schedule.Tour.UserId == tourGuide.Id
                            && x.Status != BookingStatus.Completed
                            && x.Status != BookingStatus.Cancelled
                            && x.Status != BookingStatus.Refunded
                            && !x.DeletedTime.HasValue)
                .ToListAsync()
                ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Không tìm thấy đơn hàng!");

            // Nhóm khách chưa cọc (DepositAll, DepositHaft)
            var bookingsNoDeposit = bookingList
                .Where(x => x.Status == BookingStatus.DepositAll || x.Status == BookingStatus.DepositHaft)
                .ToList();

            // Nhóm khách đã cọc (DepositedHaft, Paid)
            var bookingsWithDeposit = bookingList
                .Where(x => x.Status == BookingStatus.DepositedHaft || x.Status == BookingStatus.Paid)
                .ToList();

            // Xử lý khách chưa cọc
            foreach (var booking in bookingsNoDeposit)
            {
                booking.Status = BookingStatus.Cancelled;
                await _unitOfWork.GetRepository<Booking>().UpdateAsync(booking);

                // Gửi email thông báo hủy nhưng không hoàn tiền
                await SendTourCancellationEmailNoDeposit(booking.User, booking);
            }

            // Xử lý khách đã cọc
            foreach (var booking in bookingsWithDeposit)
            {
                if (booking.Status == BookingStatus.DepositedHaft)
                {
                    booking.User.Balance += (int)(booking.TotalPrice * 0.5);
                }
                else
                {
                    booking.User.Balance += (int)(booking.TotalPrice * 1);
                }
                booking.Status = BookingStatus.Cancelled;
                await _unitOfWork.GetRepository<Booking>().UpdateAsync(booking);
                await _unitOfWork.GetRepository<ApplicationUser>().UpdateAsync(booking.User);

                // Gửi email thông báo hủy + hoàn tiền
                await SendTourCancellationEmailWithRefund(booking.User, booking);
                await _unitOfWork.SaveAsync();  // Lưu ngay sau khi cập nhật số dư
            }

            // Lưu vào DB
            await _unitOfWork.GetRepository<ApplicationUser>().UpdateAsync(tourGuide);
            await _unitOfWork.SaveAsync();
            // Gửi email thông báo tài khoản bị khóa
            await SendTourGuideAccountBlockedEmail(tourGuide);
            return "Hủy đơn thành công!";
        }

        public async Task<BookingStatisticsModel> BookingStatistics(string? day, string? month, int? year)
        {
            if (day != null)
            {

                if (!DateTime.TryParseExact(day, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Chuỗi nhập vào không hợp lệ. Định dạng đúng là 'ngày/tháng/năm' (vd: '26/01/2023').");

                }
                // Lọc danh sách payment theo ngày/tháng/năm đã chọn
                var bookingList = await _unitOfWork.GetRepository<Booking>().Entities
                    .Where(p => !p.DeletedTime.HasValue && p.CreatedTime.Date == parsedDate.Date)
                    .ToListAsync();

                var bookingStatistic = new BookingStatisticsModel()
                {
                    TotalBooking = bookingList.Count,
                    TotalCompleted = bookingList.Count(x => x.Status == BookingStatus.Completed && x.Status == BookingStatus.Refunded),
                    TotalCancelled = bookingList.Count(x => x.Status == BookingStatus.Cancelled)
                };

                return await Task.FromResult(bookingStatistic);
            }
            // Lọc theo tháng (format: MM/yyyy)
            if (!string.IsNullOrWhiteSpace(month))
            {
                if (!DateTime.TryParseExact(month, "MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Chuỗi nhập vào không hợp lệ. Định dạng đúng là 'tháng/năm' (vd: '01/2023').");
                }
                // Lọc danh sách payment theo ngày/tháng/năm đã chọn
                var bookingList = await _unitOfWork.GetRepository<Booking>().Entities
                    .Where(p => !p.DeletedTime.HasValue)
                    .Where(p => p.CreatedTime.Month == parsedDate.Month && p.CreatedTime.Year == parsedDate.Year)
                    .ToListAsync();
                var bookingStatistic = new BookingStatisticsModel()
                {
                    TotalBooking = bookingList.Count,
                    TotalCompleted = bookingList.Count(x => x.Status == BookingStatus.Completed && x.Status == BookingStatus.Refunded),
                    TotalCancelled = bookingList.Count(x => x.Status == BookingStatus.Cancelled)
                };
                return await Task.FromResult(bookingStatistic);

            }

            // Lọc theo năm (format: yyyy)
            if (year != null)
            {
                var bookingList = await _unitOfWork.GetRepository<Booking>().Entities
                    .Where(p => !p.DeletedTime.HasValue)
                    .Where(p => p.CreatedTime.Year == year)
                    .ToListAsync();
                var bookingStatistic = new BookingStatisticsModel()
                {
                    TotalBooking = bookingList.Count,
                    TotalCompleted = bookingList.Count(x => x.Status == BookingStatus.Completed && x.Status == BookingStatus.Refunded),
                    TotalCancelled = bookingList.Count(x => x.Status == BookingStatus.Cancelled)
                };
                return await Task.FromResult(bookingStatistic);
            }
            else
            {
                var bookingList = await _unitOfWork.GetRepository<Booking>().Entities
                    .Where(p => !p.DeletedTime.HasValue)
                    .ToListAsync();
                var bookingStatistic = new BookingStatisticsModel()
                {
                    TotalBooking = bookingList.Count,
                    TotalCompleted = bookingList.Count(x => x.Status == BookingStatus.Completed && x.Status == BookingStatus.Refunded),
                    TotalCancelled = bookingList.Count(x => x.Status == BookingStatus.Cancelled)
                };
                return await Task.FromResult(bookingStatistic);
            }
        }
        public async Task<TotalRevenueModel> TotalRevenue(string? day, string? month, int? year)
        {
            if (day != null)
            {

                if (!DateTime.TryParseExact(day, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Chuỗi nhập vào không hợp lệ. Định dạng đúng là 'ngày/tháng/năm' (vd: '26/01/2023').");

                }
                // Lọc danh sách payment theo ngày/tháng/năm đã chọn
                var bookingList = await _unitOfWork.GetRepository<Booking>().Entities
                    .Where(p => !p.DeletedTime.HasValue
                    && p.CreatedTime.Date == parsedDate.Date
                    && p.Status == BookingStatus.Completed && p.Status == BookingStatus.Refunded)
                    .ToListAsync();
                long CommissionRevenue = 0;
                long TourGuideRevenue = 0;
                foreach (var booking in bookingList)
                {
                    CommissionRevenue += (long)(booking.TotalPrice * 0.2);
                    TourGuideRevenue += (long)(booking.TotalPrice * 0.8);
                }
                var totalRevenueModel = new TotalRevenueModel()
                {
                    CommissionRevenue = CommissionRevenue,
                    TourGuideRevenue = TourGuideRevenue,
                };

                return await Task.FromResult(totalRevenueModel);
            }
            // Lọc theo tháng (format: MM/yyyy)
            if (!string.IsNullOrWhiteSpace(month))
            {
                if (!DateTime.TryParseExact(month, "MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Chuỗi nhập vào không hợp lệ. Định dạng đúng là 'tháng/năm' (vd: '01/2023').");
                }
                // Lọc danh sách payment theo ngày/tháng/năm đã chọn
                var bookingList = await _unitOfWork.GetRepository<Booking>().Entities
                    .Where(p => !p.DeletedTime.HasValue)
                    .Where(p => p.CreatedTime.Month == parsedDate.Month && p.CreatedTime.Year == parsedDate.Year
                     && p.Status == BookingStatus.Completed && p.Status == BookingStatus.Refunded)
                    .ToListAsync();
                long CommissionRevenue = 0;
                long TourGuideRevenue = 0;
                foreach (var booking in bookingList)
                {
                    CommissionRevenue += (long)(booking.TotalPrice * 0.2);
                    TourGuideRevenue += (long)(booking.TotalPrice * 0.8);
                }
                var totalRevenueModel = new TotalRevenueModel()
                {
                    CommissionRevenue = CommissionRevenue,
                    TourGuideRevenue = TourGuideRevenue,
                };
                return await Task.FromResult(totalRevenueModel);

            }

            // Lọc theo năm (format: yyyy)
            if (year != null)
            {
                var bookingList = await _unitOfWork.GetRepository<Booking>().Entities
                    .Where(p => !p.DeletedTime.HasValue)
                    .Where(p => p.CreatedTime.Year == year && p.Status == BookingStatus.Completed && p.Status == BookingStatus.Refunded)
                    .ToListAsync();
                long CommissionRevenue = 0;
                long TourGuideRevenue = 0;
                foreach (var booking in bookingList)
                {
                    CommissionRevenue += (long)(booking.TotalPrice * 0.2);
                    TourGuideRevenue += (long)(booking.TotalPrice * 0.8);
                }
                var totalRevenueModel = new TotalRevenueModel()
                {
                    CommissionRevenue = CommissionRevenue,
                    TourGuideRevenue = TourGuideRevenue,
                };
                return await Task.FromResult(totalRevenueModel);
            }
            else
            {
                var bookingList = await _unitOfWork.GetRepository<Booking>().Entities
                    .Where(p => !p.DeletedTime.HasValue && p.Status == BookingStatus.Completed && p.Status == BookingStatus.Refunded)
                    .ToListAsync();
                long CommissionRevenue = 0;
                long TourGuideRevenue = 0;
                foreach (var booking in bookingList)
                {
                    CommissionRevenue += (long)(booking.TotalPrice * 0.2);
                    TourGuideRevenue += (long)(booking.TotalPrice * 0.8);
                }
                var totalRevenueModel = new TotalRevenueModel()
                {
                    CommissionRevenue = CommissionRevenue,
                    TourGuideRevenue = TourGuideRevenue,
                };
                return await Task.FromResult(totalRevenueModel);
            }
        }
        private async Task SendTourCancellationEmailNoDeposit(ApplicationUser customer, Booking booking)
        {
            await _emailService.SendEmailAsync(
                customer.Email,
                "Thông Báo Hủy Tour Do Hướng Dẫn Viên Vi Phạm",
                $@"
                <html>
                <body>
                    <h2>THÔNG BÁO HỦY TOUR</h2>
                    <p>Xin chào {customer.FullName},</p>
                    <p>Chúng tôi rất tiếc phải thông báo rằng tour của bạn đã bị hủy do hướng dẫn viên vi phạm quy định của ứng dụng.</p>
                    <p><strong>Tên tour:</strong> {booking.Schedule.Tour.Name}</p>
                    <p><strong>Mã đơn hàng:</strong> {booking.OrderCode}</p>
                    <p><strong>Ngày khởi hành:</strong> {booking.RentalDate:dd/MM/yyyy}</p>
                    <p><strong>Thời gian:</strong> {booking.Schedule.StartTime.ToString(@"hh\:mm")} - {booking.Schedule.EndTime.ToString(@"hh\:mm")}</p>
                    <p>Chúng tôi thành thật xin lỗi vì sự bất tiện này.</p>
                    <p>Nếu có bất kỳ thắc mắc nào, vui lòng liên hệ với chúng tôi.</p>
                    <p>Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!</p>
                </body>
                </html>"
            );
        }
        private async Task SendTourCancellationEmailWithRefund(ApplicationUser customer, Booking booking)
        {
            await _emailService.SendEmailAsync(
                customer.Email,
                "Thông Báo Hủy Tour & Hoàn Tiền",
                $@"
            <html>
            <body>
                <h2>THÔNG BÁO HỦY TOUR & HOÀN TIỀN</h2>
                <p>Xin chào {customer.FullName},</p>
                <p>Chúng tôi rất tiếc phải thông báo rằng tour của bạn đã bị hủy do hướng dẫn viên vi phạm quy định của ứng dụng.</p>
                <p><strong>Tên tour:</strong> {booking.Schedule.Tour.Name}</p>
                <p><strong>Mã đơn hàng:</strong> {booking.OrderCode}</p>
                <p><strong>Ngày khởi hành:</strong> {booking.RentalDate:dd/MM/yyyy}</p>
                <p><strong>Thời gian:</strong> {booking.Schedule.StartTime.ToString(@"hh\:mm")} - {booking.Schedule.EndTime.ToString(@"hh\:mm")}</p>
                <p>Số tiền cọc của bạn sẽ được hoàn trả vào tài khoản của bạn trong thời gian sớm nhất.</p>
                <p>Nếu có bất kỳ thắc mắc nào, vui lòng liên hệ với chúng tôi.</p>
                <p>Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!</p>
            </body>
            </html>"
            );
        }
        private async Task SendTourGuideAccountBlockedEmail(ApplicationUser guide)
        {
            await _emailService.SendEmailAsync(
                guide.Email,
                "Thông Báo Tài Khoản Bị Khóa",
                $@"
                <html>
                <body>
                    <h2>THÔNG BÁO KHÓA TÀI KHOẢN</h2>
                    <p>Xin chào {guide.FullName},</p>
                    <p>Chúng tôi xin thông báo rằng tài khoản của bạn đã bị khóa do vi phạm quy định của ứng dụng.</p>
                    <p><strong>Trạng thái tài khoản:</strong> Đã bị khóa</p>
                    <p>Bạn không thể nhận hoặc quản lý tour trên nền tảng cho đến khi tài khoản được xem xét lại.</p>
                    <p>Nếu bạn cho rằng đây là sự nhầm lẫn hoặc cần hỗ trợ thêm, vui lòng liên hệ với chúng tôi.</p>
                    <p>Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!</p>
                </body>
                </html>"
            );
        }


    }
}
