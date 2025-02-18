using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
using Wanvi.ModelViews.PaymentModelViews;

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

        public PaymentService(HttpClient httpClient, IConfiguration configuration, IUnitOfWork unitOfWork)
        {
            _httpClient = httpClient;
            _payOSApiUrl = configuration["PayOS:ApiUrl"];
            _apiKey = configuration["PayOS:ApiKey"];
            _checksumKey = configuration["PayOS:ChecksumKey"];
            _clientKey = configuration["PayOS:ClientKey"]; // Lấy ClientKey từ appsettings.json
            _unitOfWork = unitOfWork;
        }

        public async Task<string> CreatePayOSPaymentLink(CreatePayOSPaymentRequest request)
        {
            // 1. Lấy thông tin booking từ database dựa trên BookingId
            var booking = await _unitOfWork.GetRepository<Booking>().Entities.FirstOrDefaultAsync(x => x.Id == request.Id && !x.DeletedTime.HasValue); // _context là DbContext của bạn
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
                items = GetBookingItems(booking.Id), // Hàm này sẽ lấy danh sách sản phẩm từ booking (xem bên dưới)
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

            return bookingDetails.Select(bd => new PayOSItem
            {
                TravelerName = bd.TravelerName,
                Age = bd.Age,
                Email = bd.Email,
                IdentityCard = bd.IdentityCard,
                PassportNumber = bd.PassportNumber,
                PhoneNumber = bd.PhoneNumber

            }).ToList();
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
            // 1. Xác thực signature (xem tài liệu PayOS)
            if (!VerifyPayOSSignature(request, signature))
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.ServerError, "Invalid signature");
            }

            // 2. Tìm Payment trong database dựa trên thông tin trong request
            var payment = await _unitOfWork.GetRepository<Payment>().Entities.FirstOrDefaultAsync(x => x.Id == request.paymentId && !x.DeletedTime.HasValue);
            if (payment == null)
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.ServerError, "Payment not found");

            }

            // 3. Cập nhật trạng thái Payment
            switch (request.status)
            {
                case "success":
                    payment.Status = PaymentStatus.Paid;
                    break;
                case "failed":
                    payment.Status = PaymentStatus.Unpaid; // Hoặc một trạng thái lỗi khác
                    break;
                case "canceled":
                    payment.Status = PaymentStatus.Canceled; // Hoặc một trạng thái hủy khác
                    break;
                    // ... các trạng thái khác
            }

            await _unitOfWork.SaveAsync();
        }



    }
}

