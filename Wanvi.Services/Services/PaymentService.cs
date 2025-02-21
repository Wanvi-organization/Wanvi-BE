using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Wanvi.Contract.Repositories.Base;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.BookingModelViews;
using Wanvi.ModelViews.PaymentModelViews;
using Wanvi.Services.Services.Infrastructure;

namespace Wanvi.Services.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly HttpClient _httpClient;
        private string _payOSApiUrl;
        private string _apiKey;
        private string _checksumKey;
        private string _clientKey; // Thêm biến lưu ClientKey
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaymentService> _logger;
        private readonly IHttpContextAccessor _contextAccessor;

        public PaymentService(HttpClient httpClient, IConfiguration configuration, IUnitOfWork unitOfWork, ILogger<PaymentService> logger, IHttpContextAccessor contextAccessor)
        {
            _httpClient = httpClient;
            _payOSApiUrl = configuration["PayOS:ApiUrl"];
            _apiKey = configuration["PayOS:ApiKey"];
            _checksumKey = configuration["PayOS:ChecksumKey"];
            _clientKey = configuration["PayOS:ClientKey"]; // Lấy ClientKey từ appsettings.json
            _unitOfWork = unitOfWork;
            _logger = logger;
            _contextAccessor = contextAccessor;
        }

        public async Task<string> CreatePayOSPaymentLink(CreatePayOSPaymentRequest request)
        {
            // 1. Lấy thông tin booking từ database dựa trên BookingId
            var booking = await _unitOfWork.GetRepository<Booking>().Entities.FirstOrDefaultAsync(x => x.Id == request.BookingId && !x.DeletedTime.HasValue); // _context là DbContext của bạn
            if (booking == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NotFound, "Không tìm thấy booking");
            }
            var buyer = await _unitOfWork.GetRepository<ApplicationUser>().Entities.FirstOrDefaultAsync(x => x.Id.ToString() == booking.CreatedBy && !x.DeletedTime.HasValue) ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NotFound, "Không tìm thấy người mua!");

            // 2. Tạo PayOSPaymentRequest từ thông tin booking
            var payOSRequest = new PayOSPaymentRequest
            {
                orderCode = GenerateRandomOrderCode(),
                amount = (long)booking.TotalPrice, // Chuyển đổi TotalPrice sang long
                description = $"Thanh toán!!!",
                buyerName = buyer.FullName, // Lấy tên người dùng từ booking.User
                buyerEmail = buyer.Email,   // Lấy email người dùng từ booking.User
                buyerPhone = buyer.PhoneNumber, // Lấy số điện thoại từ booking.User
                buyerAddress = buyer.Address,  // Lấy địa chỉ từ booking.User
                /*items = GetBookingItems(booking.Id), */// Hàm này sẽ lấy danh sách sản phẩm từ booking (xem bên dưới)
                cancelUrl = "https://wanvi-landing-page.vercel.app/", // Thay thế bằng URL của bạn
                returnUrl = "https://wanvi-landing-page.vercel.app/",  // Thay thế bằng URL của bạn
                expiredAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 1800,
                // ... các trường khác 
            };

            // 3. Tạo chữ ký
            payOSRequest.signature = CalculateSignature(payOSRequest);

            // 4. Gọi API PayOS
            string checkoutUrl = await CallPayOSApi(payOSRequest);

            // 5. Trả về checkout URL
            return checkoutUrl;
        }

        private long GenerateRandomOrderCode()
        {
            Random random = new Random();
            return (long)(random.NextInt64(11111111, 99999999));
        }
        // Hàm lấy danh sách sản phẩm từ booking (bạn cần điều chỉnh theo cấu trúc database của bạn)
        private List<PayOSItem> GetBookingItems(string bookingId)
        {
            // Ví dụ: Lấy danh sách sản phẩm từ bảng BookingDetails
            var bookingDetails = _unitOfWork.GetRepository<BookingDetail>().Entities.Where(bd => bd.BookingId == bookingId).ToList();

            var items = bookingDetails.Select(bd => new PayOSItem
            {
                Name = "Tour Booking",
                TravelerName = bd.TravelerName,
                Age = bd.Age,
                Email = bd.Email,
                IdentityCard = bd.IdentityCard,
                PassportNumber = bd.PassportNumber,
                PhoneNumber = bd.PhoneNumber
            }).ToList();

            foreach (var item in items)
            {
                _logger.LogInformation("PayOSItem: {Item}", JsonConvert.SerializeObject(item));
            }

            return items;
        }

        private string CalculateSignature(PayOSPaymentRequest request)
        {
            // Chắc chắn `amount` là số nguyên
            int amount = (int)request.amount;

            // Sắp xếp và format dữ liệu chính xác
            string data = $"amount={amount}&cancelUrl={request.cancelUrl}&description={request.description}" +
                          $"&orderCode={request.orderCode}&returnUrl={request.returnUrl}";

            Console.WriteLine($"Data to sign: {data}");

            // Tạo HMAC-SHA256 signature
            byte[] keyBytes = Encoding.UTF8.GetBytes(_checksumKey);
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            using (HMACSHA256 hmac = new HMACSHA256(keyBytes))
            {
                byte[] hash = hmac.ComputeHash(dataBytes);
                string signature = BitConverter.ToString(hash).Replace("-", "").ToLower();

                Console.WriteLine($"Generated signature: {signature}");
                return signature;
            }
        }

        private async Task<string> CallPayOSApi(PayOSPaymentRequest payOSRequest)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey); // Đổi từ "Authorization: Bearer" sang "x-api-key"
            _httpClient.DefaultRequestHeaders.Add("x-client-id", _clientKey);

            string json = JsonConvert.SerializeObject(payOSRequest);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            foreach (var header in _httpClient.DefaultRequestHeaders)
            {
                Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }

            using (HttpResponseMessage response = await _httpClient.PostAsync(_payOSApiUrl, content))
            {
                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    PayOSResponse payOSResponse = JsonConvert.DeserializeObject<PayOSResponse>(responseJson);

                    if (payOSResponse != null && payOSResponse.data != null && !string.IsNullOrEmpty(payOSResponse.data.checkoutUrl))
                    {
                        return payOSResponse.data.checkoutUrl;
                    }
                    else
                    {
                        throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.ServerError, "Invalid PayOS response: " + responseJson);
                    }
                }
                else
                {
                    string errorJson = await response.Content.ReadAsStringAsync();
                    throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.ServerError, $"Error calling PayOS API: {response.StatusCode} - {errorJson}");
                }
            }
        }
        public bool VerifyPayOSSignature(PayOSWebhookRequest request, string signature)
        {
            // 1. Lấy dữ liệu từ webhook (cần sắp xếp theo thứ tự alphabet)
            string data = $"amount={request.amount}&orderCode={request.orderCode}&status={request.status}"; //...

            // 2. Tạo chữ ký bằng HMAC-SHA256
            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_checksumKey)))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                string computedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();

                // 3. So sánh chữ ký
                return computedSignature == signature;
            }
        }
        public async Task PayOSCallback(PayOSWebhookRequest request, string signature)
        {
            // 1. Xác thực chữ ký
            if (!VerifyPayOSSignature(request, signature))
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.ServerError, "Invalid signature");
            }

            // 2. Tìm Payment trong database
            var payment = await _unitOfWork.GetRepository<Payment>().Entities
                .FirstOrDefaultAsync(x => x.Id == request.paymentId && !x.DeletedTime.HasValue)
                ?? throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.ServerError, "Không tìm thấy thanh toán!");

            // 3. Cập nhật trạng thái Payment
            switch (request.status)
            {
                case "success":
                    payment.Status = PaymentStatus.Paid;
                    await _unitOfWork.SaveAsync();

                    // 4. Tìm Booking và tính tổng tiền đã thanh toán
                    var booking = await _unitOfWork.GetRepository<Booking>().Entities
                        .FirstOrDefaultAsync(x => x.Id == payment.BookingId && !x.DeletedTime.HasValue)
                        ?? throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.ServerError, "Không tìm thấy đơn hàng!");

                    double totalPaid = booking.Payments
                        .Where(x => x.Status == PaymentStatus.Paid && !x.DeletedTime.HasValue)
                        .Sum(x => x.Amount);

                    // 5. Xác định trạng thái mới của Booking
                    if (totalPaid >= booking.TotalPrice)
                    {
                        booking.Status = BookingStatus.Paid; // Đã thanh toán đủ 100%
                    }
                    else if (totalPaid >= booking.TotalPrice * 0.5)
                    {
                        if (booking.Status == BookingStatus.DepositHaft)
                            booking.Status = BookingStatus.DepositedHaft;
                        else if (booking.Status == BookingStatus.DepositHaftEnd)
                            booking.Status = BookingStatus.Paid;
                    }

                    var schedule = await _unitOfWork.GetRepository<Schedule>().Entities.FirstOrDefaultAsync(x => x.Id == booking.ScheduleId && !x.DeletedTime.HasValue) ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NotFound, "Không tìm thấy lịch");

                    //Tìm người HDV để cộng tiền
                    var tourGuide = await _unitOfWork.GetRepository<ApplicationUser>().Entities.FirstOrDefaultAsync(x => x.Id == schedule.Tour.UserId && !x.DeletedTime.HasValue) ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Không tìm thấy hướng dẫn viên!");
                    tourGuide.Balance += (int)(payment.Amount);
                    await _unitOfWork.GetRepository<ApplicationUser>().UpdateAsync(tourGuide);
                    ////Tìm người dùng để trừ tiền
                    //var user = await _unitOfWork.GetRepository<ApplicationUser>().Entities.FirstOrDefaultAsync(x => x.Id == booking.UserId && !x.DeletedTime.HasValue) ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Không tìm thấy người dùng!");
                    //user.Balance -= (int)(payment.Amount);
                    //await _unitOfWork.GetRepository<ApplicationUser>().UpdateAsync(user);

                    break;
                case "failed":
                    payment.Status = PaymentStatus.Unpaid;
                    break;
                case "canceled":
                    payment.Status = PaymentStatus.Canceled;
                    break;
            }
            await _unitOfWork.SaveAsync();
        }

        public async Task<string> CreateBookingHaftEnd(CreateBookingEndModel model)
        {
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);
            Guid.TryParse(userId, out Guid cb);

            // Lấy danh sách booking hợp lệ (cùng Schedule, cùng ngày, trạng thái hợp lệ)
            var existingBookings = await _unitOfWork.GetRepository<Booking>().Entities
                .Include(bp => bp.Payments)
                .FirstOrDefaultAsync(x => x.Id == model.BookingId
                                    && !x.DeletedTime.HasValue
                                    && x.Status == BookingStatus.DepositedHaft);
            //Tìm người dùng đặt và kt số tiền có đủ để thanh toán không
            var user = await _unitOfWork.GetRepository<ApplicationUser>().Entities.FirstOrDefaultAsync(x => x.Id == cb && !x.DeletedTime.HasValue) ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Không tìm thấy người dùng!");
            //Số tiền tour phải trả còn lại
            int Total = (int)(existingBookings.TotalPrice * 0.5);
            if (user.Balance < Total)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Số tiền của quý khách không đủ thực hiện giao dịch này!");
            }
            user.Balance -= Total;
            await _unitOfWork.GetRepository<ApplicationUser>().UpdateAsync(user);

            // 7. Tạo bản ghi Payment mới
            var payment = new Payment
            {
                Id = Guid.NewGuid().ToString("N"),
                Method = PaymentMethod.Banking, // Hoặc PaymentMethod phù hợp với PayOS
                Status = PaymentStatus.Unpaid,
                Amount = existingBookings.TotalPrice * 0.5,
                CreatedBy = userId,
                LastUpdatedBy = userId,
                CreatedTime = DateTime.UtcNow,
                LastUpdatedTime = DateTime.UtcNow,
                BookingId = existingBookings.Id,
                //... các thông tin khác (nếu cần)...
            };
            await _unitOfWork.GetRepository<Payment>().InsertAsync(payment);

            existingBookings.Status = BookingStatus.DepositHaftEnd;
            await _unitOfWork.GetRepository<Booking>().UpdateAsync(existingBookings);
            await _unitOfWork.SaveAsync();

            var payOSRequest = new CreatePayOSPaymentRequest { BookingId = existingBookings.Id };

            // Call PaymentService to generate payment link
            string checkoutUrl = await  CreatePayOSPaymentLink(payOSRequest);


            return checkoutUrl;
        }

    }
}

