using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Utils;
using Wanvi.ModelViews.BookingModelViews;
using Wanvi.ModelViews.UserModelViews;

namespace Wanvi.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _configuration = configuration;
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
        public async Task<IEnumerable<NearbyResponseModelView>> GetNearbyLocalGuides(NearbyRequestModelView model)
        {
            const double radiusInKm = 5.0;
            var localGuides = await _unitOfWork.GetRepository<ApplicationUser>()
                .Entities
                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "LocalGuide"))
                .ToListAsync();

            var nearbyLocalGuides = new List<NearbyResponseModelView>();

            foreach (var localGuide in localGuides)
            {
                var (latitude, longitude) = await GeocodeAddressAsync(localGuide.Address);

                var distance = GeoHelper.GetDistance(model.Latitude, model.Longitude, latitude, longitude);

                if (distance <= radiusInKm)
                {
                    nearbyLocalGuides.Add(new NearbyResponseModelView
                    {
                        Id = localGuide.Id,
                        Name = localGuide.FullName,
                        Address = localGuide.Address,
                        Distance = distance
                    });
                }
            }

            return _mapper.Map<IEnumerable<NearbyResponseModelView>>(nearbyLocalGuides);
        }
        #endregion
    }
}
