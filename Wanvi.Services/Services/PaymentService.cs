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
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.PaymentModelViews;

namespace Wanvi.Services.Services
{
    public class PaymentService: IPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly string _payOSApiUrl;
        private readonly string _apiKey;
        private readonly string _checksumKey;
        private readonly IUnitOfWork _unitOfWork;

        public PaymentService(HttpClient httpClient, IConfiguration configuration, IUnitOfWork unitOfWork)
        {
            _httpClient = httpClient;
            _payOSApiUrl = configuration["PayOS:ApiUrl"];
            _apiKey = configuration["PayOS:ApiKey"];
            _checksumKey = configuration["PayOS:ChecksumKey"];
            _unitOfWork = unitOfWork;
        }

        public async Task<string> CreatePayOSPaymentLink(CreatePayOSPaymentRequest request)
        {
            // 1. Lấy thông tin booking từ database dựa trên BookingId
            var booking = await _unitOfWork.GetRepository<Booking>().Entities.FirstOrDefaultAsync(x=>x.Id == request.Id && !x.DeletedTime.HasValue); // _context là DbContext của bạn
            if (booking == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NotFound, "Không tìm thấy booking");
            }

            // 2. Tạo PayOSPaymentRequest từ thông tin booking
            var payOSRequest = new PayOSPaymentRequest
            {
                orderCode = booking.Id, // hoặc booking.BookingCode nếu bạn có trường này
                amount = (long)booking.TotalPrice, // Chuyển đổi TotalPrice sang long
                description = $"Thanh toán booking {booking.Id}",
                buyerName = booking.User.FullName, // Lấy tên người dùng từ booking.User
                buyerEmail = booking.User.Email,   // Lấy email người dùng từ booking.User
                buyerPhone = booking.User.PhoneNumber, // Lấy số điện thoại từ booking.User
                buyerAddress = booking.User.Address,  // Lấy địa chỉ từ booking.User
                items = GetBookingItems(booking.Id), // Hàm này sẽ lấy danh sách sản phẩm từ booking (xem bên dưới)
                cancelUrl = "YOUR_CANCEL_URL", // Thay thế bằng URL của bạn
                returnUrl = "YOUR_RETURN_URL"  // Thay thế bằng URL của bạn
                                               // ... các trường khác 
            };

            // 3. Tạo chữ ký
            payOSRequest.signature = CalculateSignature(payOSRequest);

            // 4. Gọi API PayOS
            string checkoutUrl = await CallPayOSApi(payOSRequest);

            // 5. Trả về checkout URL
            return checkoutUrl;
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
            // Sắp xếp các tham số theo thứ tự alphabet
            string data = $"amount={request.amount}&cancelUrl={request.cancelUrl}&description={request.description}&orderCode={request.orderCode}&returnUrl={request.returnUrl}";

            // Tạo chữ ký HMAC-SHA256
            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_checksumKey)))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hash); // Hoặc Convert.ToHexString(hash) nếu PayOS yêu cầu
            }
        }

        private async Task<string> CallPayOSApi(PayOSPaymentRequest payOSRequest)
        {
            // Cấu hình header request
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("x-client-idx-api-key", _apiKey);

            // Chuyển đổi request object sang JSON string
            string json = JsonConvert.SerializeObject(payOSRequest);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            // Gửi request đến PayOS API
            using (HttpResponseMessage response = await _httpClient.PostAsync(_payOSApiUrl, content))
            {
                // Kiểm tra response thành công
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
                        // Xử lý lỗi: PayOS response không hợp lệ
                        throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.ServerError, "Invalid PayOS response: " + responseJson);
                    }
                }
                else
                {
                    // Xử lý lỗi: Gọi PayOS API thất bại
                    string errorJson = await response.Content.ReadAsStringAsync();
                    throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.ServerError, $"Error calling PayOS API: {response.StatusCode} - {errorJson}");
                }
            }
        }
    }
}

