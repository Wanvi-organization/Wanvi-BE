using AutoMapper;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.UserModelViews;

namespace Wanvi.Services.Services
{
    public class AddressService : IAddressService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AddressService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
    }
}
