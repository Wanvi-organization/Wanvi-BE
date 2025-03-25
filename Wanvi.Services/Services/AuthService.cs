using AutoMapper;
using Google.Apis.Auth;
using MailKit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
using Wanvi.Repositories.UOW;
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

        public async Task RegisterByEmail(RegisterByEmailModel model)
        {
            // Kiểm tra user co tồn tại
            var applicationUser = await _unitOfWork.GetRepository<ApplicationUser>().Entities
                .FirstOrDefaultAsync(x => x.Id == model.Id && !x.DeletedTime.HasValue) ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Tài khoản không tồn tại!");

            // Kiểm tra xác nhận mật khẩu
            if (model.Password != model.ConfirmPassword)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Xác nhận mật khẩu không đúng!");
            }
            if (!Regex.IsMatch(model.PhoneNumber, @"^\d{10,11}$"))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Định dạng số điện thoại không hợp lệ. Số điện thoại phải có 10-11 chữ số.");
            }
            if (!Regex.IsMatch(model.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&.])[A-Za-z\d@$!%*?&.]{8,16}$"))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest,
                    "Mật khẩu phải có ít nhất 8 ký tự, 1 chữ hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt.");
            }
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Tên không được để trống.");
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
            applicationUser.PhoneNumber = model.PhoneNumber;
            applicationUser.UserName = applicationUser.Email;
            applicationUser.Address = model.PlaceOfBirth;
            applicationUser.DateOfBirth = model.DateOfBirth;
            applicationUser.Gender = model.Gender;
            applicationUser.NormalizedEmail = applicationUser.Email.ToUpper();
            applicationUser.NormalizedUserName = applicationUser.Email.ToUpper();
            applicationUser.SecurityStamp = Guid.NewGuid().ToString();
            applicationUser.PasswordHash = passwordHasher.HashPassword(null, model.Password); // Băm mật khẩu tại đây


            // Cập nhậtk người dùng  vào cơ sở dữ liệu
            await _unitOfWork.GetRepository<ApplicationUser>().UpdateAsync(applicationUser);

            ApplicationUserRole applicationUserRole = new ApplicationUserRole()
            {
                UserId = applicationUser.Id,
                RoleId = applicationRole.Id,
            };

            await _unitOfWork.GetRepository<ApplicationUserRole>().InsertAsync(applicationUserRole);

            await _unitOfWork.SaveAsync();

        }

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

        public async Task ResendConfirmationEmail(EmailModelView emailModelView)
        {
            if (string.IsNullOrWhiteSpace(emailModelView.Email))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Email không được để trống!");
            }
            var user = await _unitOfWork.GetRepository<ApplicationUser>().Entities.FirstOrDefaultAsync(x => x.Email == emailModelView.Email && !x.DeletedTime.HasValue) ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Không tìm thấy Email");

            int OTP = Int32.Parse(GenerateOtp());
            user.EmailCode = OTP;
            user.CodeGeneratedTime = DateTime.Now;
            await _emailService.SendEmailAsync(emailModelView.Email, "Xác nhận tài khoản",
           $"Vui lòng xác nhận tài khoản của bạn, OTP của bạn là:  <div class='otp'>{OTP}</div>");
            await _unitOfWork.GetRepository<ApplicationUser>().UpdateAsync(user);
            await _unitOfWork.SaveAsync();

        }
        public async Task<ResponseEmailModel> CreateUserByEmail(CreateUserByEmailModel model)
        {
            //... (Your existing code for phone number validation and user creation)
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Email không được để trống!");
            }

            if (!Regex.IsMatch(model.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Định dạng email không hợp lệ.");
            }

            // Kiểm tra số điện thoại đã tồn tại
            var userExists = await _unitOfWork.GetRepository<ApplicationUser>().Entities.FirstOrDefaultAsync(x => x.Email == model.Email && !x.DeletedTime.HasValue);
            if (userExists != null)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Email đã được đăng kí.");
            }
            int OTP = Int32.Parse(GenerateOtp());

            // Thực hiện logic tạo user
            var user = new ApplicationUser
            {
                Email = model.Email,
                CreatedTime = DateTime.Now,
                EmailConfirmed = false,
                PhoneNumberConfirmed = true,
                CodeGeneratedTime = DateTime.Now,
                NormalizedEmail = model.Email.ToUpper(),
                EmailCode = OTP,
            };
            await _emailService.SendEmailAsync(model.Email, "Xác nhận tài khoản",
                       $"Vui lòng xác nhận tài khoản của bạn, OTP của bạn là:  <div class='otp'>{OTP}</div>");
            await _unitOfWork.GetRepository<ApplicationUser>().InsertAsync(user);
            await _unitOfWork.SaveAsync();
            return new ResponseEmailModel() 
            {
                Email = model.Email,
                Otp = OTP
            };
        }
        public async Task<Guid> CheckEmail(CheckEmailModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Email không được để trống!");
            }
            if (string.IsNullOrWhiteSpace(model.Otp.ToString()))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Otp không được để trống!");
            }
            ApplicationUser user = await _unitOfWork.GetRepository<ApplicationUser>().Entities.FirstOrDefaultAsync(x => x.PhoneNumber == model.Email && !x.DeletedTime.HasValue) ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Email không tồn tại!");
            if (user.EmailCode.ToString() != model.Otp.ToString())
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
            if (user.Violate == true)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Tài khoản đã bị khóa");
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

        public async Task<LoginResponse> RefreshToken(RefreshTokenModel refreshTokenModel)
        {
            ApplicationUser? user = await CheckRefreshToken(refreshTokenModel.RefreshToken);

            ApplicationUserRole roleUser = _unitOfWork.GetRepository<ApplicationUserRole>().Entities
                .FirstOrDefault(x => x.UserId == user.Id)
                ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Không tìm thấy tài khoản");

            string roleName = _unitOfWork.GetRepository<ApplicationRole>().Entities
                .Where(x => x.Id == roleUser.RoleId)
                .Select(x => x.Name)
                .FirstOrDefault() ?? "unknown";

            var tokenResponse = _tokenService.GenerateTokens(user, roleName);

            return new LoginResponse
            {
                TokenResponse = tokenResponse,
                Role = roleName
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

        public async Task<LoginResponse> CheckGoogle(CheckGoogleModel model)
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
            if (user.Violate == true)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Tài khoản đã bị khóa");
            }

            ApplicationUserRole roleUser = _unitOfWork.GetRepository<ApplicationUserRole>().Entities
                .FirstOrDefault(x => x.UserId == user.Id)
                ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Không tìm thấy tài khoản");

            string roleName = _unitOfWork.GetRepository<ApplicationRole>().Entities
                .Where(x => x.Id == roleUser.RoleId)
                .Select(x => x.Name)
                .FirstOrDefault() ?? "unknown";

            var tokenResponse = _tokenService.GenerateTokens(user, roleName);

            return new LoginResponse
            {
                TokenResponse = tokenResponse,
                Role = roleName
            };
        }

        public async Task<LoginResponse> LoginGoogle(TokenModelView model)
        {
            GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(model.Token);
            string email = payload.Email;

            ApplicationUser? user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Tài khoản chưa được tạo. Vui lòng tạo tài khoản trước khi đăng nhập.");
            }

            if (user.DeletedTime.HasValue)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Tài khoản đã bị xóa");
            }
            if (user.Violate == true)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Tài khoản đã bị khóa");
            }

            ApplicationUserRole roleUser = _unitOfWork.GetRepository<ApplicationUserRole>().Entities
                .FirstOrDefault(x => x.UserId == user.Id)
                ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Không tìm thấy tài khoản");

            string roleName = _unitOfWork.GetRepository<ApplicationRole>().Entities
                .Where(x => x.Id == roleUser.RoleId)
                .Select(x => x.Name)
                .FirstOrDefault() ?? "unknown";

            var tokenResponse = _tokenService.GenerateTokens(user, roleName);

            return new LoginResponse
            {
                TokenResponse = tokenResponse,
                Role = roleName
            };
        }

        public async Task<LoginResponse> LoginFacebook(TokenModelView model)
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

            ApplicationUser? user = await _userManager.FindByEmailAsync(facebookData.Email);

            if (user == null)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Tài khoản chưa được tạo. Vui lòng tạo tài khoản trước khi đăng nhập.");
            }

            if (user.DeletedTime.HasValue)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Tài khoản đã bị xóa");
            }
            if (user.Violate == true)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Tài khoản đã bị khóa");
            }

            ApplicationUserRole roleUser = _unitOfWork.GetRepository<ApplicationUserRole>().Entities
                .FirstOrDefault(x => x.UserId == user.Id)
                ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Không tìm thấy tài khoản");

            string roleName = _unitOfWork.GetRepository<ApplicationRole>().Entities
                .Where(x => x.Id == roleUser.RoleId)
                .Select(x => x.Name)
                .FirstOrDefault() ?? "unknown";

            var tokenResponse = _tokenService.GenerateTokens(user, roleName);

            return new LoginResponse
            {
                TokenResponse = tokenResponse,
                Role = roleName
            };
        }

        public async Task LogoutAsync(RefreshTokenModel model)
        {
            // Tải toàn bộ user (hoặc tối ưu hơn nếu bạn có userId từ JWT)
            var users = await _userManager.Users.ToListAsync();

            // Tìm user có refresh token trùng khớp
            ApplicationUser? user = null;
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
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Refresh token không hợp lệ.");
            }

            // Kiểm tra token có hết hạn không
            var expirationString = await _userManager.GetAuthenticationTokenAsync(user, "Default", "RefreshTokenExpiration");
            if (!string.IsNullOrEmpty(expirationString) && DateTime.UtcNow > DateTime.Parse(expirationString))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Refresh token đã hết hạn.");
            }

            // Xóa Refresh Token
            var result = await _userManager.RemoveAuthenticationTokenAsync(user, "Default", "RefreshToken");
            if (!result.Succeeded)
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.ServerError, "Không thể logout, vui lòng thử lại.");
            }
        }

        #endregion
    }
}
