using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Globalization;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.Core.Utils;
using Wanvi.ModelViews.ScheduleModelViews;
using Wanvi.ModelViews.TourModelViews;
using Wanvi.Services.Configurations;
using Wanvi.Services.Services.Infrastructure;
using DayOfWeek = Wanvi.Contract.Repositories.Entities.DayOfWeek;

namespace Wanvi.Services.Services
{
    public class TourService : ITourService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IAddressService _addressService;
        private readonly UploadSettings _uploadSettings;

        public TourService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor, IAddressService addressService, IOptions<UploadSettings> uploadSettings)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _contextAccessor = httpContextAccessor;
            _addressService = addressService;
            _uploadSettings = uploadSettings.Value;
        }

        public async Task<IEnumerable<ResponseTourModel>> GetAllAsync()
        {
            var tours = await _unitOfWork.GetRepository<Tour>().FindAllAsync(a => !a.DeletedTime.HasValue);
            return _mapper.Map<IEnumerable<ResponseTourModel>>(tours);
        }

        public async Task<IEnumerable<ResponseTourModel>> GetAllByLocalGuideId(Guid userId)
        {
            var tours = await _unitOfWork.GetRepository<Tour>()
                .FindAllAsync(a => a.UserId == userId && !a.DeletedTime.HasValue);

            var user = await _unitOfWork.GetRepository<ApplicationUser>().GetByIdAsync(userId);
            double remainingBalance = user.Balance;

            var filteredTours = new List<Tour>();

            foreach (var tour in tours)
            {
                var schedules = tour.Schedules
                    .OrderBy(s => s.MinDeposit)
                    .ToList();

                var selectedSchedules = new List<Schedule>();

                foreach (var schedule in schedules)
                {
                    if (remainingBalance >= schedule.MinDeposit)
                    {
                        selectedSchedules.Add(schedule);
                        remainingBalance -= schedule.MinDeposit;
                    }
                }

                if (selectedSchedules.Any())
                {
                    filteredTours.Add(tour);
                }
            }

            var responseTours = new List<ResponseTourModel>();
            foreach (var tour in filteredTours)
            {
                var responseTour = _mapper.Map<ResponseTourModel>(tour);
                var availableSchedules = new List<ResponseScheduleModel>();

                foreach (var schedule in tour.Schedules)
                {
                    var existingBookings = await _unitOfWork.GetRepository<Booking>().Entities
                        .Include(p => p.Payments)
                        .Where(x => x.ScheduleId == schedule.Id &&
                                    x.Status != BookingStatus.Cancelled &&
                                    x.Status != BookingStatus.Refunded &&
                                    x.Status != BookingStatus.Completed &&
                                    x.Status != BookingStatus.DepositAll &&
                                    x.Status != BookingStatus.DepositHaft &&
                                    x.RentalDate.Date == DateTime.Now.Date &&
                                    !x.DeletedTime.HasValue)
                        .ToListAsync();

                    int totalBooked = existingBookings.Sum(b => b.TotalTravelers);

                    int remainingTraveler = schedule.MaxTraveler - totalBooked;

                    if (remainingTraveler > 0)
                    {
                        var responseSchedule = _mapper.Map<ResponseScheduleModel>(schedule);
                        responseSchedule.RemainingTraveler = remainingTraveler;
                        availableSchedules.Add(responseSchedule);
                    }
                }

                if (availableSchedules.Any())
                {
                    responseTour.Schedules = availableSchedules;
                    responseTours.Add(responseTour);
                }
            }

            return responseTours;
        }

        public async Task<ResponseTourModel> GetByIdAsync(string id)
        {
            var tour = await _unitOfWork.GetRepository<Tour>().GetByIdAsync(id.Trim())
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Tour không tồn tại.");

            if (tour.DeletedTime.HasValue)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Tour đã bị xóa.");
            }

            return _mapper.Map<ResponseTourModel>(tour); ;
        }

        public async Task CreateAsync(CreateTourModel model)
        {
            string strUserId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);
            Guid.TryParse(strUserId, out Guid userId);
            model.TrimAllStrings();

            var mediaRepo = _unitOfWork.GetRepository<Media>();
            List<Media> medias = new();

            try
            {
                var existingSchedules = await _unitOfWork.GetRepository<Schedule>()
                    .FindAllAsync(s => s.Tour.UserId == userId && !s.DeletedTime.HasValue);

                var newTour = new Tour
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = model.Name,
                    Description = model.Description,
                    HourlyRate = model.HourlyRate,
                    PickupAddressId = model.PickupAddressId,
                    DropoffAddressId = model.DropoffAddressId,
                    Note = model.Note,
                    UserId = userId,
                    TourAddresses = new List<TourAddress>(),
                    Schedules = new List<Schedule>(),
                    Medias = new List<Media>(),
                    TourActivities = new List<TourActivity>()
                };

                newTour.TourAddresses = model.TourAddressIds.Select(id => new TourAddress
                {
                    TourId = newTour.Id,
                    AddressId = id
                }).ToList();

                var daysInNewTour = new HashSet<int>();

                foreach (var schedule in model.Schedules)
                {
                    if (!TimeSpan.TryParse(schedule.StartTime, out TimeSpan startTime) ||
                        !TimeSpan.TryParse(schedule.EndTime, out TimeSpan endTime) ||
                        startTime >= endTime ||
                        (endTime - startTime).TotalMinutes < 30)
                    {
                        throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.INVALID_INPUT, "Lịch trình không hợp lệ.");
                    }

                    if (!daysInNewTour.Add(schedule.Day))
                    {
                        throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.INVALID_INPUT, $"Tour không thể có nhiều lịch trình vào cùng một ngày ({(DayOfWeek)schedule.Day}).");
                    }

                    newTour.Schedules.Add(new Schedule
                    {
                        Day = (DayOfWeek)schedule.Day,
                        StartTime = startTime,
                        EndTime = endTime,
                        MaxTraveler = schedule.MaxTraveler,
                        BookedTraveler = 0,
                        MinDeposit = (endTime - startTime).TotalHours * model.HourlyRate * 0.2,
                        TourId = newTour.Id
                    });
                }

                if (model.MediaIds.Count > 10)
                {
                    throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.INVALID_INPUT, "Không thể tải lên quá 10 ảnh.");
                }

                medias = await mediaRepo.FindAllAsync(m => model.MediaIds.Contains(m.Id));

                foreach (var media in medias)
                {
                    media.TourId = newTour.Id;
                    newTour.Medias.Add(media);
                }

                newTour.TourActivities = model.TourActivityIds.Select(id => new TourActivity
                {
                    TourId = newTour.Id,
                    ActivityId = id
                }).ToList();

                newTour.CreatedBy = userId.ToString();
                newTour.LastUpdatedBy = userId.ToString();

                await _unitOfWork.GetRepository<Tour>().InsertAsync(newTour);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                var unusedMedias = await mediaRepo.FindAllAsync(m => m.TourId == null);

                foreach (var media in unusedMedias)
                {
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), _uploadSettings.UploadPath, Path.GetFileName(media.Url));
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                    await mediaRepo.DeleteAsync(media.Id);
                }

                await _unitOfWork.SaveAsync();
                throw new ErrorException(StatusCodes.Status500InternalServerError, ResponseCodeConstants.INTERNAL_SERVER_ERROR, "Đã xảy ra lỗi khi tạo tour.");
            }

        }

        public async Task UpdateAsync(string id, UpdateTourModel model)
        {
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);
            model.TrimAllStrings();

            var tour = await _unitOfWork.GetRepository<Tour>().GetByIdAsync(id.Trim())
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Tour không tồn tại.");

            if (tour.DeletedTime.HasValue)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Tour đã bị xóa.");
            }

            if (model.Name != null && string.IsNullOrWhiteSpace(model.Name))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.INVALID_INPUT, "Tên tour không hợp lệ.");
            }

            if (model.Description != null && string.IsNullOrWhiteSpace(model.Description))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.INVALID_INPUT, "Mô tả tour không hợp lệ.");
            }

            if (model.HourlyRate.HasValue && model.HourlyRate <= 0)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.INVALID_INPUT, "Giá theo giờ phải lớn hơn 0.");
            }

            if (model.PickupAddress != null)
            {
                var pickupAddress = await _addressService.GetOrCreateAddressAsync(model.PickupAddress.Latitude, model.PickupAddress.Longitude);
                tour.PickupAddressId = pickupAddress.Id;
            }

            if (model.DropoffAddress != null)
            {
                var dropoffAddress = await _addressService.GetOrCreateAddressAsync(model.DropoffAddress.Latitude, model.DropoffAddress.Longitude);
                tour.DropoffAddressId = dropoffAddress.Id;
            }

            if (model.TourAddresses != null)
            {
                tour.TourAddresses.Clear();
                foreach (var addressModel in model.TourAddresses)
                {
                    var address = await _addressService.GetOrCreateAddressAsync(addressModel.Latitude, addressModel.Longitude);
                    tour.TourAddresses.Add(new TourAddress
                    {
                        TourId = tour.Id.ToString(),
                        AddressId = address.Id
                    });
                }
            }

            if (model.Schedules != null)
            {
                tour.Schedules.Clear();
                foreach (var schedule in model.Schedules)
                {
                    if (!TimeSpan.TryParse(schedule.StartTime, out TimeSpan startTime))
                    {
                        throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.INVALID_INPUT, "Thời gian bắt đầu không hợp lệ.");
                    }

                    if (!TimeSpan.TryParse(schedule.EndTime, out TimeSpan endTime))
                    {
                        throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.INVALID_INPUT, "Thời gian kết thúc không hợp lệ.");
                    }

                    if (startTime >= endTime)
                    {
                        throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.INVALID_INPUT, "Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc.");
                    }

                    if ((endTime - startTime).TotalMinutes < 30)
                    {
                        throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.INVALID_INPUT, "Thời gian bắt đầu và kết thúc phải cách nhau ít nhất 30 phút.");
                    }

                    tour.Schedules.Add(new Schedule
                    {
                        Day = (Contract.Repositories.Entities.DayOfWeek)schedule.Day,
                        StartTime = startTime,
                        EndTime = endTime,
                        MaxTraveler = schedule.MaxTraveler,
                        BookedTraveler = 0,
                        MinDeposit = (double)((endTime - startTime).TotalHours * model.HourlyRate * 0.2),
                        TourId = tour.Id.ToString()
                    });
                }
            }

            if (model.Medias != null)
            {
                tour.Medias.Clear();
                foreach (var media in model.Medias)
                {
                    tour.Medias.Add(new Media
                    {
                        Url = media.Url,
                        Type = (MediaType)media.Type,
                        AltText = media.AltText,
                        TourId = tour.Id.ToString()
                    });
                }
            }

            if (model.TourActivityIds != null)
            {
                var existingActivityIds = await _unitOfWork.GetRepository<Activity>()
                    .GetQueryable()
                    .Where(a => model.TourActivityIds.Contains(a.Id.ToString()))
                    .Select(a => a.Id.ToString())
                    .ToListAsync();

                var invalidActivityIds = model.TourActivityIds.Except(existingActivityIds).ToList();

                if (invalidActivityIds.Any())
                {
                    throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.INVALID_INPUT, $"Các hoạt động sau không tồn tại: {string.Join(", ", invalidActivityIds)}");
                }

                tour.TourActivities.Clear();
                foreach (var activityId in model.TourActivityIds)
                {
                    tour.TourActivities.Add(new TourActivity
                    {
                        TourId = tour.Id.ToString(),
                        ActivityId = activityId
                    });
                }
            }

            _mapper.Map(model, tour);
            tour.LastUpdatedTime = CoreHelper.SystemTimeNow;
            tour.LastUpdatedBy = userId;

            await _unitOfWork.GetRepository<Tour>().UpdateAsync(tour);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteAsync(string id)
        {
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);

            var tour = await _unitOfWork.GetRepository<Tour>().GetByIdAsync(id.Trim())
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Tour không tồn tại.");

            if (tour.DeletedTime.HasValue)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Tour đã bị xóa.");
            }

            var hasBookedSchedule = await _unitOfWork.GetRepository<Booking>()
                .GetQueryable()
                .AnyAsync(b => b.Schedule.TourId == id && !b.DeletedTime.HasValue);

            if (hasBookedSchedule)
            {
                throw new ErrorException(StatusCodes.Status409Conflict, ResponseCodeConstants.FAILED, "Không thể xóa vì tour có lịch trình đã được đặt.");
            }

            var schedules = await _unitOfWork.GetRepository<Schedule>()
                .FindAllAsync(s => s.TourId == id && !s.DeletedTime.HasValue);

            foreach (var schedule in schedules)
            {
                schedule.LastUpdatedTime = CoreHelper.SystemTimeNow;
                schedule.LastUpdatedBy = userId;
                schedule.DeletedTime = CoreHelper.SystemTimeNow;
                schedule.DeletedBy = userId;
                await _unitOfWork.GetRepository<Schedule>().UpdateAsync(schedule);
            }

            tour.LastUpdatedTime = CoreHelper.SystemTimeNow;
            tour.LastUpdatedBy = userId;
            tour.DeletedTime = CoreHelper.SystemTimeNow;
            tour.DeletedBy = userId;

            await _unitOfWork.GetRepository<Tour>().UpdateAsync(tour);
            await _unitOfWork.SaveAsync();
        }

        //public async Task<TourStatisticsModel> GetTourStatistics(string? day, string? month, int? year)
        //{
        //    if (day != null)
        //    {
        //        // Lọc theo ngày (format: dd/MM/yyyy)
        //        if (!DateTime.TryParseExact(day, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
        //        {
        //            throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Chuỗi nhập vào không hợp lệ. Định dạng đúng là 'ngày/tháng/năm' (vd: '26/01/2023').");
        //        }

        //        var tours = await _unitOfWork.GetRepository<Tour>().Entities
        //            .Where(t => !t.DeletedTime.HasValue)
        //            .ToListAsync();

        //        var bookings = await _unitOfWork.GetRepository<Booking>().Entities
        //            .Where(b => !b.DeletedTime.HasValue && b.CreatedTime.Date == parsedDate.Date)
        //            .ToListAsync();

        //        var tourStatistics = new TourStatisticsModel
        //        {
        //            TimePeriod = day,
        //            TotalTours = tours.Count,
        //            TotalByArea = tours.Count(t => t.PickupAddress.Area == "Khu vực cụ thể"),  // Giả sử khu vực được lưu trong Address
        //            PopularTours = tours.Select(tour => new PopularTourModel
        //            {
        //                TourName = tour.Name,
        //                BookingRate = bookings.Count(b => b.TourId == tour.Id) / (double)bookings.Count(),
        //                CancelRate = bookings.Count(b => b.TourId == tour.Id && b.Status == BookingStatus.Cancelled) / (double)bookings.Count()
        //            }).OrderByDescending(t => t.BookingRate).ThenBy(t => t.CancelRate).ToList()
        //        };

        //        return tourStatistics;
        //    }

        //    // Lọc theo tháng (format: MM/yyyy)
        //    if (!string.IsNullOrWhiteSpace(month))
        //    {
        //        if (!DateTime.TryParseExact(month, "MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
        //        {
        //            throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Chuỗi nhập vào không hợp lệ. Định dạng đúng là 'tháng/năm' (vd: '01/2023').");
        //        }

        //        var tours = await _unitOfWork.GetRepository<Tour>().Entities
        //            .Where(t => !t.DeletedTime.HasValue)
        //            .ToListAsync();

        //        var bookings = await _unitOfWork.GetRepository<Booking>().Entities
        //            .Where(b => !b.DeletedTime.HasValue && b.CreatedTime.Month == parsedDate.Month && b.CreatedTime.Year == parsedDate.Year)
        //            .ToListAsync();

        //        var tourStatistics = new TourStatisticsModel
        //        {
        //            TimePeriod = month,
        //            TotalTours = tours.Count,
        //            TotalByArea = tours.Count(t => t.PickupAddress.Area == "Khu vực cụ thể"),  // Giả sử khu vực được lưu trong Address
        //            PopularTours = tours.Select(tour => new PopularTourModel
        //            {
        //                TourName = tour.Name,
        //                BookingRate = bookings.Count(b => b.TourId == tour.Id) / (double)bookings.Count(),
        //                CancelRate = bookings.Count(b => b.TourId == tour.Id && b.Status == BookingStatus.Cancelled) / (double)bookings.Count()
        //            }).OrderByDescending(t => t.BookingRate).ThenBy(t => t.CancelRate).ToList()
        //        };

        //        return tourStatistics;
        //    }

        //    // Lọc theo năm (format: yyyy)
        //    if (year != null)
        //    {
        //        var tours = await _unitOfWork.GetRepository<Tour>().Entities
        //            .Where(t => !t.DeletedTime.HasValue)
        //            .ToListAsync();

        //        var bookings = await _unitOfWork.GetRepository<Booking>().Entities
        //            .Where(b => !b.DeletedTime.HasValue && b.CreatedTime.Year == year)
        //            .ToListAsync();

        //        var tourStatistics = new TourStatisticsModel
        //        {
        //            TimePeriod = year.ToString(),
        //            TotalTours = tours.Count,
        //            TotalByArea = tours.Count(t => t.PickupAddress.Area == "Khu vực cụ thể"),  // Giả sử khu vực được lưu trong Address
        //            PopularTours = tours.Select(tour => new PopularTourModel
        //            {
        //                TourName = tour.Name,
        //                BookingRate = bookings.Count(b => b.TourId == tour.Id) / (double)bookings.Count(),
        //                CancelRate = bookings.Count(b => b.TourId == tour.Id && b.Status == BookingStatus.Cancelled) / (double)bookings.Count()
        //            }).OrderByDescending(t => t.BookingRate).ThenBy(t => t.CancelRate).ToList()
        //        };

        //        return tourStatistics;
        //    }

        //    // Nếu không có tham số nào, lấy theo năm hiện tại
        //    int currentYear = DateTime.Now.Year;
        //    var toursDefault = await _unitOfWork.GetRepository<Tour>().Entities
        //        .Where(t => !t.DeletedTime.HasValue)
        //        .ToListAsync();

        //    var bookingsDefault = await _unitOfWork.GetRepository<Booking>().Entities
        //        .Where(b => !b.DeletedTime.HasValue && b.CreatedTime.Year == currentYear)
        //        .ToListAsync();

        //    var defaultStatistics = new TourStatisticsModel
        //    {
        //        TimePeriod = currentYear.ToString(),
        //        TotalTours = toursDefault.Count,
        //        TotalByArea = toursDefault.Count(t => t.PickupAddress.Area == "Khu vực cụ thể"),  // Giả sử khu vực được lưu trong Address
        //        PopularTours = toursDefault.Select(tour => new PopularTourModel
        //        {
        //            TourName = tour.Name,
        //            BookingRate = bookingsDefault.Count(b => b.TourId == tour.Id) / (double)bookingsDefault.Count(),
        //            CancelRate = bookingsDefault.Count(b => b.TourId == tour.Id && b.Status == BookingStatus.Cancelled) / (double)bookingsDefault.Count()
        //        }).OrderByDescending(t => t.BookingRate).ThenBy(t => t.CancelRate).ToList()
        //    };

        //    return defaultStatistics;
        //}

    }
}
