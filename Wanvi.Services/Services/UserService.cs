using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.Core.Utils;
using Wanvi.ModelViews.UserModelViews;
using Wanvi.ModelViews.VietMapModelViews;
using Wanvi.Services.Services.Infrastructure;

namespace Wanvi.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly string _apiKey;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration, IHttpContextAccessor contextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _configuration = configuration;
            _contextAccessor = contextAccessor;
            _apiKey = configuration["VietMap:ApiKey"] ?? throw new Exception("API key is missing from configuration.");
        }

        #region Private Service
        //private async Task<(double Latitude, double Longitude)> GeocodeAddressAsync(string address)
        //{
        //    var httpClient = new HttpClient();
        //    httpClient.DefaultRequestHeaders.Add("User-Agent", "Wanvi");
        //    var requestUrl = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json";

        //    var response = await httpClient.GetAsync(requestUrl);
        //    if (!response.IsSuccessStatusCode)
        //        throw new Exception("Failed to fetch geocoding data.");

        //    var json = await response.Content.ReadAsStringAsync();
        //    var data = JsonSerializer.Deserialize<List<NominatimResponseModelView>>(json);
        //    var location = data?.FirstOrDefault();

        //    if (location == null)
        //        throw new Exception($"Unable to geocode address {address}.");

        //    return (double.Parse(location.Lat), double.Parse(location.Lon));
        //}

        private async Task<(double Latitude, double Longitude)> GeocodeAddressAsync(string address)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Wanvi");

            var geocodeUrl = $"https://maps.vietmap.vn/api/search/v3?apikey={_apiKey}&text={Uri.EscapeDataString(address)}";
            var geocodeResponse = await httpClient.GetAsync(geocodeUrl);

            if (!geocodeResponse.IsSuccessStatusCode)
                throw new Exception("Failed to fetch geocoding data from VietMap.");

            var geocodeJson = await geocodeResponse.Content.ReadAsStringAsync();
            var geocodeData = JsonSerializer.Deserialize<List<ResponseGeocodeModel>>(geocodeJson);

            var location = geocodeData?.FirstOrDefault();
            if (location == null || string.IsNullOrEmpty(location.RefId))
                throw new Exception($"Unable to geocode address {address}.");

            var placeUrl = $"https://maps.vietmap.vn/api/place/v3?apikey={_apiKey}&refid={location.RefId}";
            var placeResponse = await httpClient.GetAsync(placeUrl);

            if (!placeResponse.IsSuccessStatusCode)
                throw new Exception("Failed to fetch place data from VietMap.");

            var placeJson = await placeResponse.Content.ReadAsStringAsync();
            var placeData = JsonSerializer.Deserialize<ResponsePlaceModel>(placeJson);

            return (placeData.Lat, placeData.Lng);
        }
        #endregion

        #region Implementation Interface
        public async Task<IEnumerable<ResponseLocalGuideModel>> GetLocalGuidesAsync(double latitude, double longitude, string? name = null, string? city = null, string? district = null, double? minPrice = null, double? maxPrice = null, double? minRating = null, double? maxRating = null, bool? isVerified = null, bool? sortByPrice = null, bool? sortByNearest = null)
        {
            var query = _unitOfWork.GetRepository<ApplicationUser>()
                .Entities
                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "LocalGuide") && !u.DeletedTime.HasValue);

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(lg => lg.FullName.Contains(name));

            if (!string.IsNullOrWhiteSpace(city))
                query = query.Where(lg => lg.Address.Contains(city));

            if (!string.IsNullOrWhiteSpace(district))
                query = query.Where(lg => lg.Address.Contains(district));

            if (minPrice.HasValue)
                query = query.Where(lg => lg.MinHourlyRate >= minPrice);

            if (maxPrice.HasValue)
                query = query.Where(lg => lg.MinHourlyRate <= maxPrice);

            if (minRating.HasValue)
                query = query.Where(lg => lg.AvgRating >= minRating);

            if (maxRating.HasValue)
                query = query.Where(lg => lg.AvgRating <= maxRating);

            if (isVerified.HasValue)
                query = query.Where(lg => lg.IsVerified == isVerified);

            var localGuides = await query
                .Select(lg => new
                {
                    lg.Id,
                    lg.FullName,
                    lg.Gender,
                    lg.ProfileImageUrl,
                    lg.Address,
                    lg.AvgRating,
                    lg.MinHourlyRate,
                    lg.IsPremium,
                    lg.IsVerified,
                    ReviewCount = lg.Reviews.Count
                })
                .ToListAsync();

            var tasks = localGuides.Select(async lg =>
            {
                var (lat, lon) = await GeocodeAddressAsync(lg.Address);
                var distance = GeoHelper.GetDistance(latitude, longitude, lat, lon);

                return new ResponseLocalGuideModel
                {
                    Id = lg.Id,
                    FullName = lg.FullName,
                    Gender = lg.Gender,
                    ProfileImageUrl = lg.ProfileImageUrl,
                    Address = lg.Address,
                    AvgRating = lg.AvgRating,
                    ReviewCount = lg.ReviewCount,
                    MinHourlyRate = lg.MinHourlyRate,
                    Distance = distance,
                    IsPremium = lg.IsPremium,
                    IsVerified = lg.IsVerified
                };
            });

            var nearbyLocalGuides = (await Task.WhenAll(tasks)).ToList();

            if (sortByNearest == true)
            {
                nearbyLocalGuides = nearbyLocalGuides.OrderBy(lg => lg.Distance).ToList();
            }

            if (sortByPrice.HasValue)
            {
                nearbyLocalGuides = sortByPrice.Value
                    ? nearbyLocalGuides.OrderBy(lg => lg.MinHourlyRate).ToList()
                    : nearbyLocalGuides.OrderByDescending(lg => lg.MinHourlyRate).ToList();
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
                ProfileImageUrl = user.ProfileImageUrl
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
                ProfileImageUrl = user.ProfileImageUrl
            };
            return inforModel;
        }

        public async Task ChangePassword(ChangePasswordModel model)
        {
            // Lấy userId từ HttpContext
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);

            Guid.TryParse(userId, out Guid cb);

            // Kiểm tra xác nhận mật khẩu
            if (model.OldPassword == model.NewPassword)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Mật khẩu mới trùng mật khẩu cũ!");
            }

            // Kiểm tra xác nhận mật khẩu
            if (model.NewPassword != model.ConfirmPassword)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Xác nhận mật khẩu không đúng!");
            }

            ApplicationUser user = await _unitOfWork.GetRepository<ApplicationUser>()
         .Entities.FirstOrDefaultAsync(x => x.Id == cb && !x.DeletedTime.HasValue) ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Tài khoản không tồn tại!");

            // Sử dụng PasswordHasher để băm mật khẩu
            var passwordHasher = new FixedSaltPasswordHasher<ApplicationUser>(Options.Create(new PasswordHasherOptions()));
            user.PasswordHash = passwordHasher.HashPassword(null, model.NewPassword);

            await _unitOfWork.GetRepository<ApplicationUser>().UpdateAsync(user);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateProfiel(UpdateProfileModel model)
        {
            // Lấy userId từ HttpContext
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);

            Guid.TryParse(userId, out Guid cb);


            ApplicationUser user = await _unitOfWork.GetRepository<ApplicationUser>()
         .Entities.FirstOrDefaultAsync(x => x.Id == cb && !x.DeletedTime.HasValue) ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Tài khoản không tồn tại!");

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            await _unitOfWork.GetRepository<ApplicationUser>().UpdateAsync(user);
            await _unitOfWork.SaveAsync();
        }
        #endregion
    }
}
