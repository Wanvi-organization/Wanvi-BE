using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.Core.Utils;
using Wanvi.ModelViews.ScheduleModelViews;
using Wanvi.ModelViews.TourModelViews;
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

        public TourService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor, IAddressService addressService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _contextAccessor = httpContextAccessor;
            _addressService = addressService;
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

        //public async Task<IEnumerable<ResponseTourModel>> GetAllByLocalGuideId(Guid userId)
        //{
        //    var tours = await _unitOfWork.GetRepository<Tour>()
        //        .FindAllAsync(a => a.UserId == userId && !a.DeletedTime.HasValue);

        //    var user = await _unitOfWork.GetRepository<ApplicationUser>().GetByIdAsync(userId);
        //    double remainingBalance = user.Balance;

        //    var filteredTours = new List<Tour>();

        //    foreach (var tour in tours)
        //    {
        //        var schedules = tour.Schedules
        //            .OrderBy(s => s.MinDeposit)
        //            .ToList();

        //        var selectedSchedules = new List<Schedule>();

        //        foreach (var schedule in schedules)
        //        {
        //            if (remainingBalance >= schedule.MinDeposit)
        //            {
        //                selectedSchedules.Add(schedule);
        //                remainingBalance -= schedule.MinDeposit;
        //            }
        //        }

        //        if (selectedSchedules.Any())
        //        {
        //            filteredTours.Add(tour);
        //        }
        //    }

        //    //var existingBookings = await _unitOfWork.GetRepository<Booking>().Entities
        //    //    .Include(b => b.Payments)
        //    //    .Where(p => p.ScheduleId == filteredTours.;

        //    return _mapper.Map<IEnumerable<ResponseTourModel>>(filteredTours);
        //}

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

            var existingSchedules = await _unitOfWork.GetRepository<Schedule>()
                .FindAllAsync(s => s.Tour.UserId == userId && !s.DeletedTime.HasValue);

            var newTour = new Tour
            {
                Name = model.Name,
                Description = model.Description,
                HourlyRate = model.HourlyRate,
                PickupAddressId = (await _addressService.GetOrCreateAddressAsync(model.PickupAddress.Latitude, model.PickupAddress.Longitude)).Id,
                DropoffAddressId = (await _addressService.GetOrCreateAddressAsync(model.DropoffAddress.Latitude, model.DropoffAddress.Longitude)).Id,
                UserId = userId,
                TourAddresses = new List<TourAddress>(),
                Schedules = new List<Schedule>(),
                Medias = new List<Media>(),
                TourActivities = new List<TourActivity>()
            };

            foreach (var addressModel in model.TourAddresses)
            {
                var address = await _addressService.GetOrCreateAddressAsync(addressModel.Latitude, addressModel.Longitude);
                newTour.TourAddresses.Add(new TourAddress
                {
                    TourId = newTour.Id.ToString(),
                    AddressId = address.Id
                });
            }

            var daysInNewTour = new HashSet<int>();

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

                if (!daysInNewTour.Add(schedule.Day))
                {
                    throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.INVALID_INPUT, $"Tour không thể có nhiều lịch trình vào cùng một ngày ({(DayOfWeek)schedule.Day}).");
                }

                // Kiểm tra trùng lịch trình của user
                foreach (var existingSchedule in existingSchedules)
                {
                    if ((int)existingSchedule.Day == schedule.Day)
                    {
                        TimeSpan existingStart = existingSchedule.StartTime;
                        TimeSpan existingEnd = existingSchedule.EndTime;

                        // Kiểm tra trùng giờ hoặc không cách nhau ít nhất 1 tiếng
                        if ((startTime >= existingStart && startTime < existingEnd) ||
                            (endTime > existingStart && endTime <= existingEnd) ||
                            (Math.Abs((startTime - existingEnd).TotalMinutes) < 60) ||
                            (Math.Abs((endTime - existingStart).TotalMinutes) < 60))
                        {
                            throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.INVALID_INPUT,
                                $"Lịch trình vào {((DayOfWeek)schedule.Day)} {startTime:hh\\:mm} - {endTime:hh\\:mm} trùng với lịch trình đã tồn tại ({existingStart:hh\\:mm} - {existingEnd:hh\\:mm}).");
                        }
                    }
                }

                newTour.Schedules.Add(new Schedule
                {
                    Day = (Contract.Repositories.Entities.DayOfWeek)schedule.Day,
                    StartTime = startTime,
                    EndTime = endTime,
                    MaxTraveler = schedule.MaxTraveler,
                    BookedTraveler = 0,
                    MinDeposit = (endTime - startTime).TotalHours * model.HourlyRate * 0.2,
                    TourId = newTour.Id.ToString()
                });
            }

            foreach (var media in model.Medias)
            {
                newTour.Medias.Add(new Media
                {
                    Url = media.Url,
                    Type = (Contract.Repositories.Entities.MediaType)media.Type,
                    AltText = media.AltText,
                    TourId = newTour.Id.ToString()
                });
            }

            foreach (var activityId in model.TourActivityIds)
            {
                newTour.TourActivities.Add(new TourActivity
                {
                    TourId = newTour.Id.ToString(),
                    ActivityId = activityId
                });
            }

            newTour.CreatedBy = userId.ToString();
            newTour.LastUpdatedBy = userId.ToString();

            await _unitOfWork.GetRepository<Tour>().InsertAsync(newTour);
            await _unitOfWork.SaveAsync();
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

    }
}
