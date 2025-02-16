using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.Core.Utils;
using Wanvi.ModelViews.TourModelViews;
using Wanvi.Services.Services.Infrastructure;

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
            var tours = await _unitOfWork.GetRepository<Tour>().GetAllAsync();
            return _mapper.Map<IEnumerable<ResponseTourModel>>(tours);
        }

        public async Task<ResponseTourModel> CreateTourAsync(CreateTourModel model)
        {
            string strUserId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);
            Guid.TryParse(strUserId, out Guid userId);

            var pickupAddress = await _addressService.GetOrCreateAddressAsync(model.PickupAddress.Latitude, model.PickupAddress.Longitude);
            var dropoffAddress = await _addressService.GetOrCreateAddressAsync(model.DropoffAddress.Latitude, model.DropoffAddress.Longitude);

            var newTour = new Tour
            {
                Name = model.Name,
                Description = model.Description,
                HourlyRate = model.HourlyRate,
                PickupAddressId = pickupAddress.Id,
                DropoffAddressId = dropoffAddress.Id,
                UserId = userId,
                TourAddresses = new List<TourAddress>(),
                Schedules = new List<Schedule>(),
                TourActivities = new List<TourActivity>(),
                Medias = new List<Media>()
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

            foreach (var activityId in model.TourActivityIds)
            {
                newTour.TourActivities.Add(new TourActivity
                {
                    TourId = newTour.Id.ToString(),
                    ActivityId = activityId
                });
            }

            foreach (var schedule in model.Schedules)
            {
                if (!TimeSpan.TryParse(schedule.StartTime, out TimeSpan startTime))
                {
                    throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.INVALID_INPUT, "StartTime không hợp lệ.");
                }

                if (!TimeSpan.TryParse(schedule.EndTime, out TimeSpan endTime))
                {
                    throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.INVALID_INPUT, "EndTime không hợp lệ.");
                }

                newTour.Schedules.Add(new Schedule
                {
                    Day = (Schedule.DayOfWeek)schedule.Day,
                    StartTime = startTime,
                    EndTime = endTime,
                    MaxTraveler = schedule.MaxTraveler,
                    BookedTraveler = 0,
                    TourId = newTour.Id.ToString()
                });
            }

            foreach (var media in model.Medias)
            {
                newTour.Medias.Add(new Media
                {
                    Url = media.Url,
                    Type = (Media.MediaType)media.Type,
                    AltText = media.AltText,
                    TourId = newTour.Id.ToString()
                });
            }

            newTour.CreatedBy = userId.ToString();
            newTour.LastUpdatedBy = userId.ToString();

            await _unitOfWork.GetRepository<Tour>().InsertAsync(newTour);
            await _unitOfWork.SaveAsync();

            var createdTour = await _unitOfWork.GetRepository<Tour>().Entities
            .Where(t => t.Id == newTour.Id)
            .Include(t => t.TourActivities)
            .ThenInclude(t => t.Activity)
            .FirstOrDefaultAsync();

            return _mapper.Map<ResponseTourModel>(createdTour);
        }
    }
}
