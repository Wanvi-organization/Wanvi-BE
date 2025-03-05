using AutoMapper;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Wanvi.Contract.Repositories.Base;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.Core.Utils;
using Wanvi.ModelViews.AuthModelViews;
using Wanvi.ModelViews.UserModelViews;
using Wanvi.Services.Services.Infrastructure;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Twilio.Types;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
namespace Wanvi.Services.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly JwtSettings _jwtSettings;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;

        public AuthService(UserManager<ApplicationUser> userManager, IEmailService emailService, IConfiguration configuration, IUnitOfWork unitOfWork, ITokenService tokenService, JwtSettings jwtSettings, IHttpContextAccessor contextAccessor, IMapper mapper, ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _tokenService = tokenService;
            _jwtSettings = jwtSettings;
            _contextAccessor = contextAccessor;
            _mapper = mapper;
            _logger = logger;

        }

        #region Private Service
        private (string token, IEnumerable<string> roles) GenerateJwtToken(ApplicationUser user)
        {
            byte[] key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"] ?? throw new Exception("JWT_KEY is not set"));
            List<Claim> claims = new List<Claim> {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };
            IEnumerable<string> roles = _userManager.GetRolesAsync(user).Result;
            foreach (string role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            string issuer = _configuration["JwtSettings:Issuer"] ?? throw new Exception("JWT_ISSUER is not set");
            string audience = _configuration["JwtSettings:Audience"] ?? throw new Exception("JWT_AUDIENCE is not set");

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddHours(1),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return (tokenHandler.WriteToken(token), roles);
        }

        private async Task<string> GenerateRefreshToken(ApplicationUser user)
        {
            string? refreshToken = Guid.NewGuid().ToString();

            string? initToken = await _userManager.GetAuthenticationTokenAsync(user, "Default", "RefreshToken");
            if (initToken != null)
            {

                await _userManager.RemoveAuthenticationTokenAsync(user, "Default", "RefreshToken");

            }

            await _userManager.SetAuthenticationTokenAsync(user, "Default", "RefreshToken", refreshToken);
            return refreshToken;
        }

        private string GenerateOtp()
        {
            Random random = new Random();
            string otp = random.Next(100000, 999999).ToString();
            return otp;
        }
        #endregion


        #region Implementation Interface
        public async Task ForgotPassword(EmailModelView model)
        {
            ApplicationUser? user = await _userManager.FindByEmailAsync(model.Email)
                ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Vui lòng kiểm tra email của bạn");

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Vui lòng kiểm tra email của bạn");
            }

            string OTP = GenerateOtp();
            user.EmailCode = int.Parse(OTP);
            user.CodeGeneratedTime = DateTime.Now;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Không thể lưu OTP, vui lòng thử lại sau.");
            }

            await _emailService.SendEmailAsync(model.Email, "Đặt lại mật khẩu", $"Vui lòng xác nhận tài khoản của bạn, OTP của bạn là: <div class='otp'>{OTP}</div>");
        }

        public async Task VerifyOtp(ConfirmOTPModelView model, bool isResetPassword)
        {
            ApplicationUser? user = await _userManager.FindByEmailAsync(model.Email)
                ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Vui lòng kiểm tra email của bạn");

            if (user.EmailCode == null || user.EmailCode.ToString() != model.OTP)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "OTP không hợp lệ");
            }

            if (!user.CodeGeneratedTime.HasValue || DateTime.Now > user.CodeGeneratedTime.Value.AddMinutes(5))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "OTP đã hết hạn");
            }

            if (!isResetPassword)
            {
                string? token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                await _userManager.ConfirmEmailAsync(user, token);
            }

            user.EmailCode = null;
            user.CodeGeneratedTime = null;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Không thể cập nhật thông tin người dùng");
            }
        }

        public async Task ResetPassword(ResetPasswordModelView model)
        {
            ApplicationUser? user = await _userManager.FindByEmailAsync(model.Email)
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NotFound, "Không tìm thấy user");

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Vui lòng kiểm tra email của bạn");
            }

            string? token = await _userManager.GeneratePasswordResetTokenAsync(user);
            IdentityResult? result = await _userManager.ResetPasswordAsync(user, token, model.Password);

            if (!result.Succeeded)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, result.Errors.FirstOrDefault()?.Description);
            }
        }

        public async Task Register(RegisterModel model)
        {
            // Kiểm tra user co tồn tại
            var applicationUser = await _unitOfWork.GetRepository<ApplicationUser>().Entities
                .FirstOrDefaultAsync(x => x.Id == model.Id && !x.DeletedTime.HasValue) ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Tài khoản không tồn tại!");

            // Kiểm tra xác nhận mật khẩu
            if (model.Password != model.ConfirmPassword)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Xác nhận mật khẩu không đúng!");
            }

            // Xác định vai trò
            var roleName = model.RoleName ? "Traveler" : "LocalGuide";
            var applicationRole = await _unitOfWork.GetRepository<ApplicationRole>().Entities
                .FirstOrDefaultAsync(x => x.Name == roleName);

            if (applicationRole == null)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Vai trò không tồn tại!");
            }

            // Sử dụng PasswordHasher để băm mật khẩu
            var passwordHasher = new FixedSaltPasswordHasher<ApplicationUser>(Options.Create(new PasswordHasherOptions()));

            applicationUser.FullName = model.Name;
            applicationUser.Email = model.Email;
            applicationUser.UserName = model.Email;
            applicationUser.Address = model.PlaceOfBirth;
            applicationUser.DateOfBirth = model.DateOfBirth;
            applicationUser.Gender = model.Gender;
            applicationUser.NormalizedEmail = model.Email.ToUpper();
            applicationUser.NormalizedUserName = model.Email.ToUpper();
            applicationUser.SecurityStamp = Guid.NewGuid().ToString();
            applicationUser.PasswordHash = passwordHasher.HashPassword(null, model.Password); // Băm mật khẩu tại đây


            // Cập nhậtk người dùng  vào cơ sở dữ liệu
            await _unitOfWork.GetRepository<ApplicationUser>().UpdateAsync(applicationUser);

            ApplicationUserRole applicationUserRole = new ApplicationUserRole()
            {
                UserId = applicationUser.Id,
                RoleId = applicationRole.Id,
            };
            //string OTP = GenerateOtp();
            //newUser.EmailCode = int.Parse(OTP);
            //newUser.CodeGeneratedTime = DateTime.Now;
            //await _emailService.SendEmailAsync(model.Email, "Xác nhận tài khoản", $"Vui lòng xác nhận tài khoản của bạn, OTP của bạn là: <div class='otp'>{OTP}</div>");
            await _unitOfWork.GetRepository<ApplicationUserRole>().InsertAsync(applicationUserRole);

            await _unitOfWork.SaveAsync();

        }

        //public async Task<ResponsePhoneModel> CreateUserByPhone(CreateUseByPhoneModel model)
        //{
        //    if (string.IsNullOrWhiteSpace(model.PhoneNumber))
        //    {
        //        throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Số điên thoại không được để trống!");
        //    }

        //    if (!Regex.IsMatch(model.PhoneNumber, @"^\d{10,11}$"))
        //    {

        //        throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Định dạng số điện thoại không hợp lệ. Số điện thoại phải có 10-11 chữ số.");

        //    }

        //    // Kiểm tra số điện thoại đã tồn tại
        //    var userExists = await _unitOfWork.GetRepository<ApplicationUser>().Entities.FirstOrDefaultAsync(x => x.PhoneNumber == model.PhoneNumber && !x.DeletedTime.HasValue);
        //    if (userExists != null)
        //    {
        //        throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Số điện thoại đã được đăng kí.");
        //    }

        //    // Thực hiện logic tạo user
        //    var user = new ApplicationUser
        //    {
        //        PhoneNumber = model.PhoneNumber,
        //        CreatedTime = DateTime.Now,
        //        EmailConfirmed = true,
        //        PhoneNumberConfirmed = false,
        //        EmailCode = Int32.Parse(GenerateOtp()),
        //    };



        //    try
        //    {
        //        string accountSid = _configuration["Twilio:AccountSid"];
        //        string authToken = _configuration["Twilio:AuthToken"];
        //        string fromPhoneNumber = _configuration["Twilio:FromPhoneNumber"]; // Your Twilio number (purchased)

        //        TwilioClient.Init(accountSid, authToken);

        //        string toPhoneNumber = model.PhoneNumber; // Get the phone number from your model

        //        // 1. Remove leading zero (if present)
        //        if (toPhoneNumber.StartsWith("0"))
        //        {
        //            toPhoneNumber = toPhoneNumber.Substring(1);
        //        }

        //        // 2. Add the country code (+84 for Vietnam)
        //        toPhoneNumber = "+84" + toPhoneNumber;

        //        // Now toPhoneNumber is in E.164 format

        //        string message = $"Mã OTP của bạn là: {user.EmailCode}";

        //        var messageResource = await MessageResource.CreateAsync(
        //            body: message,
        //            from: new PhoneNumber(fromPhoneNumber), // Your Twilio number
        //            to: new PhoneNumber(toPhoneNumber) // Correctly formatted recipient number
        //        );

        //        _logger.LogInformation($"SMS sent successfully. SID: {messageResource.Sid}");

        //        return new ResponsePhoneModel // Or whatever your success response is
        //        {
        //            PhoneNumber = model.PhoneNumber,
        //            Otp = user.EmailCode.ToString()
        //        };

        //    }
        //    catch (Twilio.Exceptions.ApiException ex) // Catch Twilio specific exceptions
        //    {
        //        _logger.LogError(ex, $"Twilio API Error: {ex.Message}");
        //        throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.ServerError, $"Lỗi gửi OTP: {ex.Message}"); // More specific error message
        //    }
        //    catch (Exception ex)  // Catch other exceptions
        //    {
        //        _logger.LogError(ex, "General Error sending SMS with Twilio.");
        //        throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.ServerError, "Lỗi gửi OTP: " + ex.Message);
        //    }
        //}

        public async Task<ResponsePhoneModel> CreateUserByPhone(CreateUseByPhoneModel model)
        {
            //... (Your existing code for phone number validation and user creation)
            if (string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Số điên thoại không được để trống!");
            }

            if (!Regex.IsMatch(model.PhoneNumber, @"^\d{10,11}$"))
            {

                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Định dạng số điện thoại không hợp lệ. Số điện thoại phải có 10-11 chữ số.");

            }

            // Kiểm tra số điện thoại đã tồn tại
            var userExists = await _unitOfWork.GetRepository<ApplicationUser>().Entities.FirstOrDefaultAsync(x => x.PhoneNumber == model.PhoneNumber && !x.DeletedTime.HasValue);
            if (userExists != null)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Số điện thoại đã được đăng kí.");
            }

            // Thực hiện logic tạo user
            var user = new ApplicationUser
            {
                PhoneNumber = model.PhoneNumber,
                CreatedTime = DateTime.Now,
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                EmailCode = Int32.Parse(GenerateOtp()),
            };
            try
            {
                string apiKey = "CAFC9A6326BEEBF876041A29BE0FCD"; // Your eSMS API Key
                string secretKey = "DFC0F1ABF05B46912B7D153AE04144"; // Your eSMS Secret Key
                string phoneNumber = model.PhoneNumber;
                int smsType = 8; // Tin nhắn đầu số cố định (chăm sóc khách hàng) - MUST REGISTER TEMPLATE!
                string content = $"Mã OTP của bạn là: {user.EmailCode}";

                // 1. Remove leading zero (if present)
                //if (phoneNumber.StartsWith("0"))
                //{
                //    phoneNumber = phoneNumber.Substring(1);
                //}

                // 2. Add the country code (+84 for Vietnam)
                phoneNumber = /*"+84" + */phoneNumber;


                var requestBody = new Dictionary<string, string>
        {
            { "ApiKey", apiKey },
            { "SecretKey", secretKey },
            { "Phone", phoneNumber },
            { "Content", content },
            { "SmsType", smsType.ToString() }
        };

                string json = JsonConvert.SerializeObject(requestBody);

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://rest.esms.vn/MainService.svc/json/");
                    var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                    using (HttpResponseMessage response = await client.PostAsync("SendMultipleMessage_V4_post_json/", httpContent))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string responseJson = await response.Content.ReadAsStringAsync();
                            var esmsResponse = JsonConvert.DeserializeObject<EsmsResponse>(responseJson);

                            if (esmsResponse.CodeResult == "100")
                            {
                                _logger.LogInformation($"eSMS sent successfully. SMSID: {esmsResponse.SMSID}");

                                // ***SAVE USER TO DATABASE HERE***
                                _unitOfWork.GetRepository<ApplicationUser>().Insert(user);
                                await _unitOfWork.SaveAsync();

                                return new ResponsePhoneModel
                                {
                                    PhoneNumber = model.PhoneNumber,
                                    Otp = user.EmailCode.ToString()
                                };
                            }
                            else
                            {
                                _logger.LogError($"eSMS API Error: CodeResult: {esmsResponse.CodeResult} - {GetErrorMessage(esmsResponse.CodeResult)}");
                                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.ServerError, $"Lỗi gửi OTP qua eSMS: {GetErrorMessage(esmsResponse.CodeResult)}");
                            }
                        }
                        else
                        {
                            string errorContent = await response.Content.ReadAsStringAsync();
                            _logger.LogError($"eSMS API Error: {response.StatusCode} - {errorContent}");

                            // More specific HTTP error handling (example)
                            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                            {
                                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, $"Lỗi gửi OTP qua eSMS: {response.StatusCode} - {errorContent}");
                            }
                            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) // Check for authentication issues
                            {
                                throw new ErrorException(StatusCodes.Status401Unauthorized, ErrorCode.Unauthorized, $"Lỗi gửi OTP qua eSMS: {response.StatusCode} - {errorContent}");
                            }
                            else
                            {
                                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.ServerError, $"Lỗi gửi OTP qua eSMS: {response.StatusCode} - {errorContent}");
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "General Error sending SMS with eSMS.");
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.ServerError, "Lỗi gửi OTP: " + ex.Message);
            }
        }


        // Class to represent the eSMS response (important for deserialization)
        public class EsmsResponse
        {
            public string CodeResult { get; set; }
            public string CountRegenerate { get; set; }
            public string SMSID { get; set; }
        }

        private string GetErrorMessage(string code)
        {
            switch (code)
            {
                case "100": return "Request đã được nhận và xử lý thành công.";
                case "104": return "Brandname không tồn tại hoặc đã bị hủy";
                case "118": return "Loại tin nhắn không hợp lệ";
                case "119": return "Brandname quảng cáo phải gửi ít nhất 20 số điện thoại";
                case "131": return "Tin nhắn brandname quảng cáo độ dài tối đa 422 kí tự";
                case "132": return "Không có quyền gửi tin nhắn đầu số cố định";
                case "875599": return "Lỗi không xác định";
                case "177": return "Brandname không có hướng";
                case "159": return "RequestId quá 120 ký tự";
                case "145": return "Sai template mạng xã hội";
                default: return "Lỗi không xác định";
            }
        }

        public async Task<Guid> CheckPhone(CheckPhoneModel model)
        {
            if (string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Số điên thoại không được để trống!");
            }
            if (string.IsNullOrWhiteSpace(model.Otp))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Otp không được để trống!");
            }
            ApplicationUser user = await _unitOfWork.GetRepository<ApplicationUser>().Entities.FirstOrDefaultAsync(x => x.PhoneNumber == model.PhoneNumber && !x.DeletedTime.HasValue) ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Số điên thoại không tồn tại!");
            if (user.EmailCode.ToString() != model.Otp)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Otp sai, vui lòng nhập lại!");
            }
            user.PhoneNumberConfirmed = true;
            user.EmailConfirmed = true;
            await _unitOfWork.GetRepository<ApplicationUser>().UpdateAsync(user);
            await _unitOfWork.SaveAsync();
            return user.Id;
        }
        public async Task CreateRole(RoleModel model)
        {
            ApplicationRole applicationRole = await _unitOfWork.GetRepository<ApplicationRole>().Entities.FirstOrDefaultAsync(x => x.Name == model.RoleName);
            if (applicationRole != null)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Vai trò đã tồn tại!");
            }
            ApplicationRole role = new ApplicationRole();
            role.Name = model.RoleName;
            await _unitOfWork.GetRepository<ApplicationRole>().InsertAsync(role);
            await _unitOfWork.SaveAsync();
        }

        public async Task<LoginResponse> LoginAsync(LoginRequestModel request)
        {
            var user = _unitOfWork.GetRepository<ApplicationUser>().Entities
                .Where(u => !u.DeletedTime.HasValue && u.UserName == request.Username)
                .FirstOrDefault()
                ?? throw new ErrorException(StatusCodes.Status401Unauthorized, ResponseCodeConstants.BADREQUEST, "Không tìm thấy tài khoản");
            if (user.EmailConfirmed == false || user.PhoneNumberConfirmed == false)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST, "Tài khoản chưa được xác thực!");
            }
            // create hash
            var passwordHasher = new FixedSaltPasswordHasher<ApplicationUser>(Options.Create(new PasswordHasherOptions()));

            var hashedInputPassword = passwordHasher.HashPassword(null, request.Password);

            if (hashedInputPassword != user.PasswordHash)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Không tìm thấy tài khoản");
            }

            //// Get the user's role
            //var roles = await _userManager.GetRolesAsync(user);
            //var role = roles.FirstOrDefault(); // Assuming a single role for simplicity
            ApplicationUserRole roleUser = _unitOfWork.GetRepository<ApplicationUserRole>().Entities.Where(x => x.UserId == user.Id).FirstOrDefault()
                                ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Không tìm thấy tài khoản");
            string roleName = _unitOfWork.GetRepository<ApplicationRole>().Entities.Where(x => x.Id == roleUser.RoleId).Select(x => x.Name).FirstOrDefault()
             ?? "unknow";
            var tokenResponse = _tokenService.GenerateTokens(user, roleName);
            //var token = Authentication.CreateToken(user.Id.ToString(), _jwtSettings);
            var loginResponse = new LoginResponse
            {
                TokenResponse = tokenResponse,
                Role = roleName,
            };
            return loginResponse;

        }

        public async Task<AuthResponseModelView> RefreshToken(RefreshTokenModel refreshTokenModel)
        {
            ApplicationUser? user = await CheckRefreshToken(refreshTokenModel.RefreshToken);
            (string token, IEnumerable<string> roles) = GenerateJwtToken(user);
            string refreshToken = await GenerateRefreshToken(user);
            return new AuthResponseModelView
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                TokenType = "JWT",
                AuthType = "Bearer",
                ExpiresIn = DateTime.Now.AddMinutes(5),
                User = new UserInfo
                {
                    Email = user.Email,
                    Roles = roles.ToList()
                }
            };
        }

        private async Task<ApplicationUser> CheckRefreshToken(string refreshToken)
        {

            List<ApplicationUser>? users = await _userManager.Users.ToListAsync();
            foreach (ApplicationUser user in users)
            {
                string? storedToken = await _userManager.GetAuthenticationTokenAsync(user, "Default", "RefreshToken");

                if (storedToken == refreshToken)
                {
                    return user;
                }
            }
            throw new ErrorException(StatusCodes.Status401Unauthorized, ErrorCode.Unauthorized, "Token không hợp lệ");
        }

        public async Task<AuthResponseModelView> CheckGoogle(CheckGoogleModel model)
        {
            ApplicationUser? user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Tài khoản chưa được tạo. Vui lòng tạo tài khoản trước khi đăng nhập.");
            }

            if (user.DeletedTime.HasValue)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Tài khoản đã bị xóa");
            }

            (string token, IEnumerable<string> roles) = GenerateJwtToken(user);
            string refreshToken = await GenerateRefreshToken(user);

            return new AuthResponseModelView
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                TokenType = "JWT",
                AuthType = "Bearer",
                ExpiresIn = DateTime.Now.AddHours(1),
                User = new UserInfo
                {
                    Email = user.Email,
                    Roles = roles.ToList()
                }
            };
        }

        public async Task<AuthResponseModelView> LoginGoogle(TokenModelView model)
        {
            GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(model.Token);
            string email = payload.Email;
            string providerKey = payload.Subject;
            ApplicationUser? user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Tài khoản chưa được tạo. Vui lòng tạo tài khoản trước khi đăng nhập.");
            }

            if (user.DeletedTime.HasValue)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Tài khoản đã bị xóa");
            }

            (string token, IEnumerable<string> roles) = GenerateJwtToken(user);
            string refreshToken = await GenerateRefreshToken(user);

            return new AuthResponseModelView
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                TokenType = "JWT",
                AuthType = "Bearer",
                ExpiresIn = DateTime.Now.AddHours(1),
                User = new UserInfo
                {
                    Email = user.Email,
                    Roles = roles.ToList()
                }
            };
        }

        public async Task<AuthResponseModelView> LoginFacebook(TokenModelView model)
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"https://graph.facebook.com/me?fields=id,email&access_token={model.Token}");

            if (!response.IsSuccessStatusCode)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Token không hợp lệ hoặc đã hết hạn.");
            }

            var content = await response.Content.ReadAsStringAsync();
            var facebookData = JsonConvert.DeserializeObject<FacebookPayloadModelView>(content);

            if (facebookData == null || string.IsNullOrEmpty(facebookData.Email))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Không thể lấy thông tin người dùng từ Facebook.");
            }

            string email = facebookData.Email;
            string providerKey = facebookData.Id;

            ApplicationUser? user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Tài khoản chưa được tạo. Vui lòng tạo tài khoản trước khi đăng nhập.");
            }

            if (user.DeletedTime.HasValue)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Tài khoản đã bị xóa");
            }

            (string token, IEnumerable<string> roles) = GenerateJwtToken(user);
            string refreshToken = await GenerateRefreshToken(user);

            return new AuthResponseModelView
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                TokenType = "JWT",
                AuthType = "Bearer",
                ExpiresIn = DateTime.Now.AddHours(1),
                User = new UserInfo
                {
                    Email = user.Email,
                    Roles = roles.ToList()
                }
            };
        }

        public async Task LogoutAsync(RefreshTokenModel model)
        {
            var users = await _userManager.Users.ToListAsync();

            ApplicationUser user = null;
            foreach (var u in users)
            {
                var token = await _userManager.GetAuthenticationTokenAsync(u, "Default", "RefreshToken");
                if (token == model.RefreshToken)
                {
                    user = u;
                    break;
                }
            }

            if (user == null)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Refresh token không hợp lệ");
            }

            var result = await _userManager.RemoveAuthenticationTokenAsync(user, "Default", "RefreshToken");

            if (!result.Succeeded)
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.ServerError, "Không thể logout, vui lòng thử lại.");
            }
        }

        #endregion
    }
}
