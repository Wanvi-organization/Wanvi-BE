using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Utils;
using Wanvi.ModelViews.UserModelViews;
using Wanvi.Services.Services.Infrastructure;

namespace Wanvi.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _contextAccessor;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration, IHttpContextAccessor contextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _configuration = configuration;
            _contextAccessor = contextAccessor;
        }

        #region Private Service
        //private async Task<(double Latitude, double Longitude)> GeocodeAddressAsync(string address)
        //{
        //    var httpClient = new HttpClient();
        //    var apiKey = _configuration["GoogleMaps:ApiKey"] ?? throw new Exception("Google Map API key is not set");
        //    var requestUrl = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={apiKey}";

        //    var response = await httpClient.GetAsync(requestUrl);
        //    if (!response.IsSuccessStatusCode) throw new Exception("Failed to fetch geocoding data.");

        //    var json = await response.Content.ReadAsStringAsync();
        //    var data = JsonSerializer.Deserialize<GoogleGeocodingModelView>(json);

        //    var location = data?.Results?.FirstOrDefault()?.Geometry?.Location;
        //    if (location == null) throw new Exception("Unable to geocode address.");

        //    return (location.Lat, location.Lng);
        //}
        private async Task<(double Latitude, double Longitude)> GeocodeAddressAsync(string address)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Wanvi");
            var requestUrl = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json";

            var response = await httpClient.GetAsync(requestUrl);
            if (!response.IsSuccessStatusCode)
                throw new Exception("Failed to fetch geocoding data.");

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<List<NominatimResponseModelView>>(json);
            var location = data?.FirstOrDefault();

            if (location == null)
                throw new Exception("Unable to geocode address.");

            return (double.Parse(location.Lat), double.Parse(location.Lon));
        }
        #endregion

        #region Implementation Interface
        public async Task<IEnumerable<ResponseLocalGuideModel>> GetLocalGuidesAsync(double latitude, double longitude, string? name = null, string ? city = null, string? district = null, double? minPrice = null, double? maxPrice = null, double? minRating = null, double? maxRating = null, bool? isVerified = null, bool? sortByPriceAsc = null, bool? sortByPriceDesc = null, bool? sortByNearest = null)
        {
            const double radiusInKm = 10.0;

            var localGuides = await _unitOfWork.GetRepository<ApplicationUser>()
                .Entities
                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "LocalGuide") && !u.DeletedTime.HasValue)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(name))
            {
                localGuides = localGuides.Where(lg => lg.FullName.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(city))
            {
                localGuides = localGuides.Where(lg => lg.Address.Contains(city)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(district))
            {
                localGuides = localGuides.Where(lg => lg.Address.Contains(district)).ToList();
            }

            if (minPrice.HasValue && maxPrice.HasValue)
            {
                localGuides = localGuides.Where(lg => lg.MinHourlyRate >= minPrice && lg.MinHourlyRate <= maxPrice).ToList();
            }

            if (minRating.HasValue && maxRating.HasValue)
            {
                localGuides = localGuides.Where(lg => lg.AvgRating >= minRating && lg.AvgRating <= maxRating).ToList();
            }

            if (isVerified.HasValue)
            {
                localGuides = localGuides.Where(lg => lg.IsVerified == isVerified).ToList();
            }

            var nearbyLocalGuides = new List<ResponseLocalGuideModel>();

            foreach (var localGuide in localGuides)
            {
                var (lat, lon) = await GeocodeAddressAsync(localGuide.Address);
                var distance = GeoHelper.GetDistance(latitude, longitude, lat, lon);

                if (distance <= radiusInKm)
                {
                    nearbyLocalGuides.Add(new ResponseLocalGuideModel
                    {
                        Id = localGuide.Id,
                        FullName = localGuide.FullName,
                        Address = localGuide.Address,
                        AvgRating = localGuide.AvgRating,
                        ReviewCount = localGuide.Reviews != null ? localGuide.Reviews.Count : 0,
                        MinHourlyRate = localGuide.MinHourlyRate,
                        Distance = distance,
                        IsPremium = localGuide.IsPremium,
                        IsVerified = localGuide.IsVerified
                    });
                }
            }

            if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(city) && string.IsNullOrWhiteSpace(district) &&
        !minPrice.HasValue && !maxPrice.HasValue && !minRating.HasValue && !maxRating.HasValue && !isVerified.HasValue)
            {
                nearbyLocalGuides = nearbyLocalGuides
                    .OrderByDescending(lg => lg.IsVerified && lg.IsPremium)
                    .ThenByDescending(lg => lg.IsPremium)
                    .ThenByDescending(lg => lg.IsVerified)
                    .ThenBy(lg => lg.Distance)
                    .ToList();
            }
            else
            {
                if (sortByPriceAsc.HasValue && sortByPriceAsc.Value)
                {
                    nearbyLocalGuides = nearbyLocalGuides.OrderBy(lg => lg.MinHourlyRate).ToList();
                }
                else if (sortByPriceDesc.HasValue && sortByPriceDesc.Value)
                {
                    nearbyLocalGuides = nearbyLocalGuides.OrderByDescending(lg => lg.MinHourlyRate).ToList();
                }

                if (sortByNearest.HasValue && sortByNearest.Value)
                {
                    nearbyLocalGuides = nearbyLocalGuides.OrderBy(lg => lg.Distance).ToList();
                }
            }

            return nearbyLocalGuides;
        }

        public async Task<UserInforModel> GetInfor()
        {
            // Lấy userId từ HttpContext
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);

            Guid.TryParse(userId, out Guid cb);


            // Lấy thông tin người dùng
            ApplicationUser user = await _unitOfWork.GetRepository<ApplicationUser>()
                .Entities.FirstOrDefaultAsync(x => x.Id == cb);
            UserInforModel inforModel = new UserInforModel
            {
                Id = user.Id,
                Address = user.Address,
                Balance = user.Balance,
                DateOfBirth = user.DateOfBirth,
                Email = user.Email,
                FullName = user.FullName,
                Gender = user.Gender,
                PhoneNumber = user.PhoneNumber,  
            };
            return inforModel;
        }

        public async Task<UserInforModel> GetTravelerBaseId(Guid Id)
        {
            // Lấy thông tin người dùng
            ApplicationUser user = await _unitOfWork.GetRepository<ApplicationUser>()
                .Entities.FirstOrDefaultAsync(x => x.Id == Id && !x.DeletedTime.HasValue);
            UserInforModel inforModel = new UserInforModel
            {
                Id = user.Id,
                Address = user.Address,
                Balance = user.Balance,
                DateOfBirth = user.DateOfBirth,
                Email = user.Email,
                FullName = user.FullName,
                Gender = user.Gender,
                PhoneNumber = user.PhoneNumber,
            };
            return inforModel;
        }
        #endregion
    }
}
