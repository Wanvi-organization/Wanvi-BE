using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.UserModelViews;
using Wanvi.ModelViews.VietMapModelViews;

namespace Wanvi.Services.Services
{
    public class AddressService : IAddressService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly string _vietmapApiKey;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public AddressService(IUnitOfWork unitOfWork, IMapper mapper, IMemoryCache cache, IConfiguration configuration, HttpClient httpClient)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cache = cache;
            _configuration = configuration;
            _vietmapApiKey = configuration["VietMap:ApiKey"] ?? throw new Exception("API key is missing from configuration.");
            _httpClient = httpClient;
        }

        public async Task<string> GetStreetFromCoordinatesAsync(double latitude, double longitude)
        {
            string requestUrl = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={latitude}&lon={longitude}";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; MyApp/1.0)");

                HttpResponseMessage response = await client.GetAsync(requestUrl);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var jsonObject = JsonSerializer.Deserialize<NominatimResponseModelView>(jsonResponse);

                    return jsonObject?.DisplayName ?? "Unknown Street";
                }
            }
            return "Unknown Street";
        }

        public async Task<Address> GetOrCreateAddressAsync(double latitude, double longitude)
        {
            double tolerance = 0.0001; // Sai số ~10m (1 độ ~ 111km, nên 0.0001 ~ 11m)

            var existingAddress = await _unitOfWork.GetRepository<Address>()
                .FindAsync(a => Math.Abs(a.Latitude - latitude) < tolerance
                             && Math.Abs(a.Longitude - longitude) < tolerance);

            if (existingAddress != null)
            {
                return existingAddress;
            }

            var street = await GetStreetFromCoordinatesAsync(latitude, longitude);
            if (string.IsNullOrEmpty(street))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.INVALID_INPUT, $"Không thể tìm thấy địa chỉ từ tọa độ: {latitude}, {longitude}.");
            }

            var city = await _unitOfWork.GetRepository<City>()
                .FindAsync(c => street.Contains(c.Name));
            if (city == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, $"Không tìm thấy thành phố phù hợp với tọa độ: {latitude}, {longitude}.");
            }

            var district = await _unitOfWork.GetRepository<District>()
                .FindAsync(d => street.Contains(d.Name) && d.CityId == city.Id);
            if (district == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, $"Không tìm thấy quận/huyện phù hợp với tọa độ: {latitude}, {longitude}.");
            }

            var newAddress = new Address
            {
                Id = Guid.NewGuid().ToString(),
                Street = street,
                Latitude = latitude,
                Longitude = longitude,
                DistrictId = district.Id
            };

            await _unitOfWork.GetRepository<Address>().InsertAsync(newAddress);
            await _unitOfWork.SaveAsync();
            return newAddress;
        }

        public async Task<IEnumerable<ResponseAutocompleteModel>> SearchAsync(string query)
        {
            if (_cache.TryGetValue(query, out IEnumerable<ResponseAutocompleteModel> cachedResult))
            {
                return cachedResult;
            }

            string apiUrl = $"https://maps.vietmap.vn/api/search/v3?apikey={_vietmapApiKey}&text={query}";
            var response = await _httpClient.GetFromJsonAsync<List<ResponseAutocompleteModel>>(apiUrl);

            if (response != null)
            {
                _cache.Set(query, response, TimeSpan.FromMinutes(10));
            }

            return response ?? new List<ResponseAutocompleteModel>();
        }

        public async Task<ResponsePlaceModel> GetOrCreateAddressByRefIdAsync(string refId)
        {
            string apiUrl = $"https://maps.vietmap.vn/api/place/v3?apikey={_vietmapApiKey}&refid={refId}";
            var response = await _httpClient.GetFromJsonAsync<ResponsePlaceModel>(apiUrl);

            if (response == null)
            {
                throw new Exception("Không tìm thấy thông tin địa chỉ từ API VietMap.");
            }

            var city = await _unitOfWork.GetRepository<City>().FindAsync(c => response.Display.Contains(c.Name));
            var district = city != null
                ? await _unitOfWork.GetRepository<District>().FindAsync(d => response.Display.Contains(d.Name) && d.CityId == city.Id)
                : null;

            var existingAddress = await _unitOfWork.GetRepository<Address>()
                .FindAsync(a => a.Latitude == response.Lat && a.Longitude == response.Lng);

            if (existingAddress == null)
            {
                var newAddress = new Address
                {
                    RefId = refId,
                    Street = response.Display,
                    Latitude = response.Lat,
                    Longitude = response.Lng,
                    DistrictId = district?.Id
                };
                await _unitOfWork.GetRepository<Address>().InsertAsync(newAddress);
                await _unitOfWork.SaveAsync();
                response.AddressId = newAddress.Id;
            }
            else
            {
                response.AddressId = existingAddress.Id;
            }

            return response;
        }
    }
}
