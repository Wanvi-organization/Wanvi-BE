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
using Wanvi.Core.Utils;
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
        private readonly IEmailService _emailService;

        public PaymentService(HttpClient httpClient, IConfiguration configuration, IUnitOfWork unitOfWork, ILogger<PaymentService> logger, IHttpContextAccessor contextAccessor, IEmailService emailService)
        {
            _httpClient = httpClient;
            _payOSApiUrl = configuration["PayOS:ApiUrl"];
            _apiKey = configuration["PayOS:ApiKey"];
            _checksumKey = configuration["PayOS:ChecksumKey"];
            _clientKey = configuration["PayOS:ClientKey"]; // Lấy ClientKey từ appsettings.json
            _unitOfWork = unitOfWork;
            _logger = logger;
            _contextAccessor = contextAccessor;
            _emailService = emailService;
        }

        public async Task<string> CreatePayOSPaymentAllLink(CreatePayOSPaymentRequest request)
        {
            // 1. Lấy thông tin booking từ database dựa trên BookingId
            var booking = await _unitOfWork.GetRepository<Booking>().Entities
                .FirstOrDefaultAsync(x => x.Id == request.BookingId
                                        && x.Status == BookingStatus.DepositAll
                                        && !x.DeletedTime.HasValue) ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NotFound, "Không tìm thấy booking");

            var buyer = await _unitOfWork.GetRepository<ApplicationUser>().Entities.FirstOrDefaultAsync(x => x.Id.ToString() == booking.CreatedBy && !x.DeletedTime.HasValue) ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NotFound, "Không tìm thấy người mua!");

            //Điều kiện số tiền HDV Đủ cọc không
            if (!CheckGuideDeposit(booking.Schedule.Tour.UserId, booking))
            {
                //Đơn bị hủy do ko đủ slot
                booking.Status = BookingStatus.Cancelled;
                await _unitOfWork.SaveAsync();
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Tour hiện tại không còn đủ chỗ để đặt!");
            }

            //Điều kiện số người hợp lệ
            // Lấy danh sách booking hợp lệ (cùng Schedule, cùng ngày, trạng thái hợp lệ)
            var existingBookings = await _unitOfWork.GetRepository<Booking>().Entities
                .Include(p => p.Payments)
                .Where(x => x.ScheduleId == booking.ScheduleId
                            && x.Status != BookingStatus.Cancelled
                            && x.Status != BookingStatus.Refunded
                            && x.Status != BookingStatus.Completed
                            && x.Status != BookingStatus.DepositAll
                            && x.Status != BookingStatus.DepositHaft
                            && x.RentalDate.Date == booking.RentalDate.Date
                            && !x.DeletedTime.HasValue) // Chỉ lấy booking có ngày đặt trùng với model
                .ToListAsync();

            // Tính tổng số người đã đặt trước đó trong ngày
            int totalBooked = existingBookings.Sum(b => b.TotalTravelers);

            // Tính số chỗ còn trống
            int availableSlots = booking.Schedule.MaxTraveler - totalBooked;

            if (booking.TotalTravelers > availableSlots)
            {
                //Đơn bị hủy do ko đủ slot
                booking.Status = BookingStatus.Cancelled;
                await _unitOfWork.SaveAsync();
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest,
                    $"Số người đăng ký ({booking.TotalTravelers}) vượt quá số slot trống ({availableSlots}) trong ngày {booking.RentalDate:dd/MM/yyyy}!");
            }

            // 2. Tạo PayOSPaymentRequest từ thông tin booking
            var payOSRequest = new PayOSPaymentRequest
            {
                orderCode = await GenerateUniqueOrderCodeAsync(),
                amount = (long)booking.TotalPrice, // Chuyển đổi TotalPrice sang long
                description = $"Thanh toán 100%!!!",
                buyerName = buyer.FullName, // Lấy tên người dùng từ booking.User
                buyerEmail = buyer.Email,   // Lấy email người dùng từ booking.User
                buyerPhone = buyer.PhoneNumber, // Lấy số điện thoại từ booking.User
                buyerAddress = buyer.Address,  // Lấy địa chỉ từ booking.User
                /*items = GetBookingItems(booking.Id), */// Hàm này sẽ lấy danh sách sản phẩm từ booking (xem bên dưới)
                cancelUrl = "https://wanvi-landing-page.vercel.app/", // Thay thế bằng URL của bạn
                returnUrl = "https://wanvi-landing-page.vercel.app/",  // Thay thế bằng URL của bạn
                expiredAt = DateTimeOffset.Now.ToUnixTimeSeconds() + 1800,

                // ... các trường khác 
            };

            // 3. Tạo chữ ký
            payOSRequest.signature = CalculateSignature(payOSRequest);

            // 7. Tạo bản ghi Payment mới
            var payment = new Payment
            {
                Id = Guid.NewGuid().ToString("N"),
                Method = PaymentMethod.Banking, // Hoặc PaymentMethod phù hợp với PayOS
                Status = PaymentStatus.Unpaid,
                Amount = booking.TotalPrice,
                OrderCode = payOSRequest.orderCode,
                BuyerAddress = payOSRequest.buyerAddress,
                Description = payOSRequest.description,
                Signature = payOSRequest.signature,
                BuyerEmail = payOSRequest.buyerEmail,
                BuyerPhone = payOSRequest.buyerPhone,
                BuyerName = payOSRequest.buyerName,
                CreatedBy = buyer.Id.ToString(),
                LastUpdatedBy = buyer.Id.ToString(),
                CreatedTime = DateTime.Now,
                LastUpdatedTime = DateTime.Now,
                BookingId = booking.Id,
                //... các thông tin khác (nếu cần)...
            };

            await _unitOfWork.GetRepository<Payment>().InsertAsync(payment);

            await _unitOfWork.SaveAsync();
            // 4. Gọi API PayOS
            string checkoutUrl = await CallPayOSApi(payOSRequest);

            // 5. Trả về checkout URL
            return checkoutUrl;
        }

        public bool CheckGuideDeposit(Guid guideId, Booking booking)
        {
            // Lấy danh sách Booking mà HDV đã nhận (trừ các trạng thái không cần tính tiền cọc)
            var bookings = _unitOfWork.GetRepository<Booking>().Entities
                .Where(b => b.Schedule.Tour.UserId == guideId)
                .Where(b => !new BookingStatus[]
                {
                    BookingStatus.DepositHaft,
                    BookingStatus.DepositAll,
                    BookingStatus.Completed,
                    BookingStatus.Cancelled,
                    BookingStatus.Refunded
                }.Contains(b.Status) && !b.DeletedTime.HasValue)
                .ToList();

            // Tổng tiền cọc cần thiết = Sum(MinDeposit * TotalTravelers)
            double totalRequiredDeposit = bookings.Sum(b => b.Schedule.MinDeposit * b.TotalTravelers) + booking.TotalPrice * 0.2;

            var user = _unitOfWork.GetRepository<ApplicationUser>().Entities.FirstOrDefault(x => x.Id == guideId && !x.DeletedTime.HasValue) ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NotFound, "Không tìm thấy hướng dãn viên!");

            //// Tổng tiền khả dụng của HDV (đã thanh toán thành công)
            //double totalBalanceBooking = _unitOfWork.GetRepository<Booking>().Entities
            //    .Where(p => p.Schedule.Tour.UserId == guideId && !p.DeletedTime.HasValue)
            //    .Where(p => p.Status == BookingStatus.Completed)
            //    .Sum(p => p.TotalPrice);

            //Tổng số tiền 
            double totalBalance = /*totalBalanceBooking +*/ user.Balance;

            //// Trừ đi tiền đã nhận từ user (DepositedHaft = 50%, Paid = 100%)
            //double deductedAmount = _unitOfWork.GetRepository<Booking>().Entities
            //    .Where(b => b.Schedule.Tour.UserId == guideId && !b.DeletedTime.HasValue)
            //    .Where(b => b.Status == BookingStatus.DepositedHaft
            //            || b.Status == BookingStatus.Paid
            //            || b.Status == BookingStatus.Completed)
            //    .Sum(b => b.Status == BookingStatus.DepositedHaft ? b.TotalPrice * 0.5 : b.TotalPrice * 1.0);

            double availableBalance = totalBalance /*- deductedAmount*/;

            // Kiểm tra xem HDV có đủ tiền cọc không
            return availableBalance >= totalRequiredDeposit;

        }

        public async Task<string> CreatePayOSPaymentHaftLink(CreatePayOSPaymentRequest request)
        {
            // 1. Lấy thông tin booking từ database dựa trên BookingId
            var booking = await _unitOfWork.GetRepository<Booking>().Entities
                .FirstOrDefaultAsync(x => x.Id == request.BookingId
                                        && x.Status == BookingStatus.DepositHaft
                                        && !x.DeletedTime.HasValue) ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NotFound, "Không tìm thấy booking");

            var buyer = await _unitOfWork.GetRepository<ApplicationUser>().Entities.FirstOrDefaultAsync(x => x.Id.ToString() == booking.CreatedBy && !x.DeletedTime.HasValue) ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NotFound, "Không tìm thấy người mua!");

            //Điều kiện số tiền HDV Đủ cọc không
            if (!CheckGuideDeposit(booking.Schedule.Tour.UserId, booking))
            {
                //Đơn bị hủy do ko đủ slot
                booking.Status = BookingStatus.Cancelled;
                await _unitOfWork.SaveAsync();
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Tour hiện tại không còn đủ chỗ để đặt!");
            }

            //Điều kiện số người hợp lệ
            // Lấy danh sách booking hợp lệ (cùng Schedule, cùng ngày, trạng thái hợp lệ)
            var existingBookings = await _unitOfWork.GetRepository<Booking>().Entities
                .Include(p => p.Payments)
                .Where(x => x.ScheduleId == booking.ScheduleId
                            && x.Status != BookingStatus.Cancelled
                            && x.Status != BookingStatus.Refunded
                            && x.Status != BookingStatus.Completed
                            && x.Status != BookingStatus.DepositAll
                            && x.Status != BookingStatus.DepositHaft
                            && x.RentalDate.Date == booking.RentalDate.Date
                            && !x.DeletedTime.HasValue) // Chỉ lấy booking có ngày đặt trùng với model
                .ToListAsync();

            // Tính tổng số người đã đặt trước đó trong ngày
            int totalBooked = existingBookings.Sum(b => b.TotalTravelers);

            // Tính số chỗ còn trống
            int availableSlots = booking.Schedule.MaxTraveler - totalBooked;

            if (booking.TotalTravelers > availableSlots)
            {
                //Đơn bị hủy do ko đủ slot
                booking.Status = BookingStatus.Cancelled;
                await _unitOfWork.SaveAsync();
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest,
                    $"Số người đăng ký ({booking.TotalTravelers}) vượt quá số slot trống ({availableSlots}) trong ngày {booking.RentalDate:dd/MM/yyyy}!");
            }

            // 2. Tạo PayOSPaymentRequest từ thông tin booking
            var payOSRequest = new PayOSPaymentRequest
            {
                orderCode = await GenerateUniqueOrderCodeAsync(),
                amount = (long)(booking.TotalPrice * 0.5), // Chuyển đổi TotalPrice sang long
                description = $"Cọc 50% đầu!!!",
                buyerName = buyer.FullName, // Lấy tên người dùng từ booking.User
                buyerEmail = buyer.Email,   // Lấy email người dùng từ booking.User
                buyerPhone = buyer.PhoneNumber, // Lấy số điện thoại từ booking.User
                buyerAddress = buyer.Address,  // Lấy địa chỉ từ booking.User
                /*items = GetBookingItems(booking.Id), */// Hàm này sẽ lấy danh sách sản phẩm từ booking (xem bên dưới)
                cancelUrl = "https://wanvi-landing-page.vercel.app/", // Thay thế bằng URL của bạn
                returnUrl = "https://wanvi-landing-page.vercel.app/",  // Thay thế bằng URL của bạn
                expiredAt = DateTimeOffset.Now.ToUnixTimeSeconds() + 1800,

                // ... các trường khác 
            };

            // 3. Tạo chữ ký
            payOSRequest.signature = CalculateSignature(payOSRequest);

            // 7. Tạo bản ghi Payment mới
            var payment = new Payment
            {
                Id = Guid.NewGuid().ToString("N"),
                Method = PaymentMethod.Banking, // Hoặc PaymentMethod phù hợp với PayOS
                Status = PaymentStatus.Unpaid,
                Amount = booking.TotalPrice * 0.5,
                OrderCode = payOSRequest.orderCode,
                BuyerAddress = payOSRequest.buyerAddress,
                Description = payOSRequest.description,
                Signature = payOSRequest.signature,
                BuyerEmail = payOSRequest.buyerEmail,
                BuyerPhone = payOSRequest.buyerPhone,
                BuyerName = payOSRequest.buyerName,
                CreatedBy = buyer.Id.ToString(),
                LastUpdatedBy = buyer.Id.ToString(),
                CreatedTime = DateTime.Now,
                LastUpdatedTime = DateTime.Now,
                BookingId = booking.Id,
                //... các thông tin khác (nếu cần)...
            };

            await _unitOfWork.GetRepository<Payment>().InsertAsync(payment);

            // 4. Gọi API PayOS
            string checkoutUrl = await CallPayOSApi(payOSRequest);
            await _unitOfWork.SaveAsync();

            // 5. Trả về checkout URL
            return checkoutUrl;
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
                                    && (x.Status == BookingStatus.DepositedHaft /*|| x.Status == BookingStatus.DepositHaftEnd*/));
            //Tìm người dùng đặt và kt số tiền có đủ để thanh toán không
            var user = await _unitOfWork.GetRepository<ApplicationUser>().Entities.FirstOrDefaultAsync(x => x.Id == cb && !x.DeletedTime.HasValue) ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Không tìm thấy người dùng!");

            // 2. Tạo PayOSPaymentRequest từ thông tin booking
            var payOSRequest = new PayOSPaymentRequest
            {
                orderCode = await GenerateUniqueOrderCodeAsync(),
                amount = (long)(existingBookings.TotalPrice * 0.5), // Chuyển đổi TotalPrice sang long
                description = $"50% tiền cọc còn lại!",
                buyerName = user.FullName, // Lấy tên người dùng từ booking.User
                buyerEmail = user.Email,   // Lấy email người dùng từ booking.User
                buyerPhone = user.PhoneNumber, // Lấy số điện thoại từ booking.User
                buyerAddress = user.Address,  // Lấy địa chỉ từ booking.User
                /*items = GetBookingItems(booking.Id), */// Hàm này sẽ lấy danh sách sản phẩm từ booking (xem bên dưới)
                cancelUrl = "https://wanvi-landing-page.vercel.app/", // Thay thế bằng URL của bạn
                returnUrl = "https://wanvi-landing-page.vercel.app/",  // Thay thế bằng URL của bạn
                expiredAt = DateTimeOffset.Now.ToUnixTimeSeconds() + 1800,

                // ... các trường khác 
            };

            // 3. Tạo chữ ký
            payOSRequest.signature = CalculateSignature(payOSRequest);

            // 7. Tạo bản ghi Payment mới
            var payment = new Payment
            {
                Id = Guid.NewGuid().ToString("N"),
                Method = PaymentMethod.Banking, // Hoặc PaymentMethod phù hợp với PayOS
                Status = PaymentStatus.Unpaid,
                Amount = existingBookings.TotalPrice * 0.5,
                OrderCode = payOSRequest.orderCode,
                BuyerAddress = payOSRequest.buyerAddress,
                Description = payOSRequest.description,
                Signature = payOSRequest.signature,
                BuyerEmail = payOSRequest.buyerEmail,
                BuyerPhone = payOSRequest.buyerPhone,
                BuyerName = payOSRequest.buyerName,
                CreatedBy = user.Id.ToString(),
                LastUpdatedBy = user.Id.ToString(),
                CreatedTime = DateTime.Now,
                LastUpdatedTime = DateTime.Now,
                BookingId = existingBookings.Id,
                //... các thông tin khác (nếu cần)...
            };

            await _unitOfWork.GetRepository<Payment>().InsertAsync(payment);

            // 4. Gọi API PayOS
            string checkoutUrl = await CallPayOSApi(payOSRequest);
            await _unitOfWork.SaveAsync();

            // 5. Trả về checkout URL
            return checkoutUrl;
        }

        private async Task<long> GenerateUniqueOrderCodeAsync()
        {
            Random random = new Random();
            long orderCode;
            bool exists;

            do
            {
                orderCode = random.NextInt64(10000000, 9999999999); // Sinh số ngẫu nhiên 8 chữ số
                exists = await _unitOfWork.GetRepository<Payment>().Entities
                    .AnyAsync(x => x.OrderCode == orderCode && !x.DeletedTime.HasValue);
            }
            while (exists);

            return orderCode;
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
            // 1. Đảm bảo amount là số nguyên
            int amount = (int)request.amount;

            // 2. Chỉ lấy các thông tin có trong dữ liệu PayOS gửi về (không có `cancelUrl`, `returnUrl`)
            string data = $"amount={amount}&cancelUrl={request.cancelUrl}&description={request.description}&orderCode={request.orderCode}&returnUrl={request.returnUrl}";

            Console.WriteLine($"Data to sign: {data}");

            // 3. Tạo HMAC-SHA256 signature
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

        public async Task PayOSCallback(PayOSWebhookRequest request)
        {
            if (request?.data == null)
            {
                Console.WriteLine("Webhook request không có data, bỏ qua xử lý.");
                return;
            }

            // Tìm Payment theo orderCode
            var payment = await _unitOfWork.GetRepository<Payment>().Entities
                .FirstOrDefaultAsync(x => x.OrderCode == request.data.orderCode && !x.DeletedTime.HasValue);

            if (payment == null)
            {
                Console.WriteLine($"Không tìm thấy thanh toán với orderCode: {request.data.orderCode}. Bỏ qua xử lý.");
                return;
            }

            // Kiểm tra Payment có phải là giao dịch nạp tiền hay không
            bool isRecharge = payment.Status == PaymentStatus.UnpaidRecharge;

            switch (request.data.code)
            {
                case "00": // Thành công
                    if (isRecharge)
                    {
                        // Xử lý nạp tiền vào tài khoản người dùng
                        var user = await _unitOfWork.GetRepository<ApplicationUser>().Entities
                            .FirstOrDefaultAsync(x => x.Id.ToString() == payment.CreatedBy && !x.DeletedTime.HasValue);

                        if (user != null)
                        {
                            user.Balance += (int)payment.Amount;
                            payment.Status = PaymentStatus.Recharged;

                            await _unitOfWork.GetRepository<ApplicationUser>().UpdateAsync(user);
                            await _unitOfWork.GetRepository<Payment>().UpdateAsync(payment);

                            await _unitOfWork.SaveAsync();

                            // Gửi email xác nhận nạp tiền thành công
                            await SendDepositSuccessEmail(user, payment.Amount);
                        }
                        else
                        {
                            Console.WriteLine("Không tìm thấy người dùng liên quan đến giao dịch nạp tiền.");
                        }
                    }
                    else
                    {
                        // Xử lý thanh toán booking như trước
                        var booking = await _unitOfWork.GetRepository<Booking>().Entities
                            .FirstOrDefaultAsync(x => x.Id == payment.BookingId && !x.DeletedTime.HasValue);

                        if (booking != null)
                        {
                            double totalPaid = booking.Payments
                                .Where(x => x.Status == PaymentStatus.Paid && !x.DeletedTime.HasValue)
                                .Sum(x => x.Amount);

                            if (totalPaid >= booking.TotalPrice)
                            {
                                booking.Status = BookingStatus.Paid;
                            }
                            else if (totalPaid >= booking.TotalPrice * 0.5)
                            {
                                booking.Status = BookingStatus.DepositedHaft;
                            }

                            var schedule = await _unitOfWork.GetRepository<Schedule>().Entities
                                .FirstOrDefaultAsync(x => x.Id == booking.ScheduleId && !x.DeletedTime.HasValue);

                            var tourGuide = await _unitOfWork.GetRepository<ApplicationUser>().Entities
                                .FirstOrDefaultAsync(x => x.Id == schedule.Tour.UserId && !x.DeletedTime.HasValue);

                            if (tourGuide != null)
                            {
                                tourGuide.Deposit += (int)payment.Amount;
                                await _unitOfWork.GetRepository<ApplicationUser>().UpdateAsync(tourGuide);
                            }

                            if (booking.Status == BookingStatus.DepositedHaft)
                            {
                                await SendMailHaft(booking.User, booking, payment);
                            }
                            else
                            {
                                await SendMailAll(booking.User, booking, payment);
                            }
                            payment.Status = PaymentStatus.Paid;

                            await _unitOfWork.GetRepository<Payment>().UpdateAsync(payment);
                            await _unitOfWork.GetRepository<Booking>().UpdateAsync(booking);
                        }
                        else
                        {
                            Console.WriteLine("Không tìm thấy đơn hàng liên quan.");
                        }
                    }
                    break;

                case "01": // Giao dịch thất bại
                    payment.Status = PaymentStatus.Unpaid;
                    break;

                case "02": // Hủy giao dịch
                    payment.Status = PaymentStatus.Canceled;
                    break;

                default:
                    Console.WriteLine($"Trạng thái không xác định: {request.data.code}, bỏ qua xử lý.");
                    return;
            }

            await _unitOfWork.SaveAsync();
        }

        public async Task<string> DepositMoney(DepositMoneyRequest request)
        {
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);
            Guid.TryParse(userId, out Guid cb);

            var user = await _unitOfWork.GetRepository<ApplicationUser>().Entities
                .FirstOrDefaultAsync(x => x.Id == cb && !x.DeletedTime.HasValue)
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NotFound, "Không tìm thấy người dùng!");

            // Tạo mã giao dịch
            long orderCode = await GenerateUniqueOrderCodeAsync();

            // Tạo yêu cầu thanh toán PayOS
            var payOSRequest = new PayOSPaymentRequest
            {
                orderCode = orderCode,
                amount = (long)request.Amount, // Chuyển số tiền sang long
                description = "Nạp tiền vào tài khoản",
                buyerName = user.FullName,
                buyerEmail = user.Email,
                buyerPhone = user.PhoneNumber,
                buyerAddress = user.Address,
                cancelUrl = "https://wanvi-landing-page.vercel.app/",
                returnUrl = "https://wanvi-landing-page.vercel.app/",
                expiredAt = DateTimeOffset.Now.ToUnixTimeSeconds() + 1800
            };

            // Tạo chữ ký bảo mật
            payOSRequest.signature = CalculateSignature(payOSRequest);

            // Lưu giao dịch vào DB
            var payment = new Payment
            {
                Id = Guid.NewGuid().ToString("N"),
                Method = PaymentMethod.Banking,
                Status = PaymentStatus.UnpaidRecharge,
                Amount = request.Amount,
                OrderCode = orderCode,
                BuyerAddress = user.Address,
                Description = "Nạp tiền vào tài khoản",
                Signature = payOSRequest.signature,
                BuyerEmail = user.Email,
                BuyerPhone = user.PhoneNumber,
                BuyerName = user.FullName,
                CreatedBy = user.Id.ToString(),
                LastUpdatedBy = user.Id.ToString(),
                CreatedTime = DateTime.Now,
                LastUpdatedTime = DateTime.Now,

            };

            await _unitOfWork.GetRepository<Payment>().InsertAsync(payment);
            await _unitOfWork.SaveAsync();

            // Gọi API PayOS để lấy link thanh toán
            string checkoutUrl = await CallPayOSApi(payOSRequest);

            return checkoutUrl; // Trả về URL để người dùng thanh toán
        }

        private async Task SendDepositSuccessEmail(ApplicationUser user, double amount)
        {
            await _emailService.SendEmailAsync(
                user.Email,
                "Nạp Tiền Thành Công",
                $@"
            <html>
            <body>
                <h2>THÔNG BÁO NẠP TIỀN</h2>
                <p>Xin chào {user.FullName},</p>
                <p>Bạn đã nạp thành công số tiền <strong>{amount:N0} đ</strong> vào tài khoản.</p>
                <p>Số dư hiện tại của bạn là <strong>{user.Balance:N0} đ</strong>.</p>
                <p>Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!</p>
            </body>
            </html>"
            );
        }

        private async Task SendMailHaft(ApplicationUser user, Booking booking, Payment payment)
        {
            int countHour = (booking.Schedule.EndTime.Hours - booking.Schedule.StartTime.Hours);

            await _emailService.SendEmailAsync(
                user.Email,
                "Hóa Đơn Thanh Toán Thành Công",
                $@"
        <html>
        <head>
            <style>
                body {{
                    font-family: Arial, sans-serif;
                    background-color: #f4f4f4;
                    margin: 0;
                    padding: 0;
                }}
                .container {{
                    width: 100%;
                    max-width: 600px;
                    margin: 20px auto;
                    background: #ffffff;
                    padding: 20px;
                    border-radius: 8px;
                    box-shadow: 0px 0px 10px rgba(0, 0, 0, 0.1);
                }}
                h2, h3 {{
                    color: #333;
                }}
                p {{
                    font-size: 16px;
                    line-height: 1.6;
                    color: #555;
                }}
                .section {{
                    margin-bottom: 20px;
                    padding-bottom: 10px;
                    border-bottom: 1px solid #ddd;
                }}
                .footer {{
                    margin-top: 20px;
                    font-size: 14px;
                    color: #777;
                    text-align: center;
                }}
                table {{
                    width: 100%;
                    border-collapse: collapse;
                }}
                table, th, td {{
                    border: 1px solid #ddd;
                }}
                th, td {{
                    padding: 10px;
                    text-align: left;
                }}
                th {{
                    background-color: #f8f8f8;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <h2>HÓA ĐƠN THANH TOÁN THÀNH CÔNG</h2>
                <p><strong>Công ty Cổ phần WANVI</strong></p>
                <p>Số ĐKKD: XXXXXXXX</p>
                <p>Địa chỉ: [Địa chỉ công ty]</p>
                <p>Hotline: [Số hotline]</p>
                <p>Email: wanvi.wandervietnam@gmail.com</p>

                <div class='section'>
                    <h3>THÔNG TIN HÓA ĐƠN</h3>
                    <p><strong>Mã đơn hàng:</strong> {booking.OrderCode}</p>
                    <p><strong>Ngày giao dịch:</strong> {DateTime.Now.Date:dd/MM/yyyy}</p>
                    <p><strong>Hình thức thanh toán:</strong> Banking</p>
                </div>

                <div class='section'>
                    <h3>THÔNG TIN KHÁCH HÀNG</h3>
                    <p><strong>Họ và tên:</strong> {user.FullName}</p>
                    <p><strong>Email:</strong> {user.Email}</p>
                    <p><strong>Số điện thoại:</strong> {user.PhoneNumber}</p>
                </div>

                <div class='section'>
                    <h3>CHI TIẾT ĐƠN HÀNG</h3>
                    <table>
                        <thead>
                            <tr>
                                <th>Tên tour</th>
                                <th>Ngày khởi hành</th>
                                <th>Giờ bắt đầu</th>
                                <th>Số lượng</th>
                                <th>Đơn giá</th>
                                <th>Tổng tiền</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr>
                                <td>{booking.Schedule.Tour.Name}</td>
                                <td>{booking.RentalDate:dd/MM/yyyy}</td>
                                <td>{booking.Schedule.StartTime:HH:mm} - {booking.Schedule.EndTime:HH:mm}</td>
                                <td>{booking.TotalTravelers}</td>
                                <td>{booking.Schedule.Tour.HourlyRate * countHour:N0} đ</td>
                                <td>{booking.TotalPrice:N0} đ</td>
                            </tr>
                        </tbody>
                    </table>
                    <p><strong>Tổng tiền tour:</strong> {booking.TotalPrice:C}</p>
                    <p><strong>Đã thanh toán (đầu tiên):</strong> 50% ({payment.Amount:N0} đ)</p>
                    <p><strong>Còn lại thanh toán sau khi hoàn tất tour:</strong> {booking.TotalPrice - payment.Amount:N0} đ</p>
                </div>
                <div class='footer'>
                    <p>Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!</p>
                </div>
            </div>
        </body>
        </html>"
            );

        }
        private async Task SendMailAll(ApplicationUser user, Booking booking, Payment payment)
        {
            int countHour = (booking.Schedule.EndTime.Hours - booking.Schedule.StartTime.Hours);

            await _emailService.SendEmailAsync(
      user.Email,
      "Hóa Đơn Thanh Toán Thành Công",
      $@"
        <html>
        <head>
            <style>
                body {{
                    font-family: Arial, sans-serif;
                    background-color: #f4f4f4;
                    margin: 0;
                    padding: 0;
                }}
                .container {{
                    width: 100%;
                    max-width: 600px;
                    margin: 20px auto;
                    background: #ffffff;
                    padding: 20px;
                    border-radius: 8px;
                    box-shadow: 0px 0px 10px rgba(0, 0, 0, 0.1);
                }}
                h2, h3 {{
                    color: #333;
                }}
                p {{
                    font-size: 16px;
                    line-height: 1.6;
                    color: #555;
                }}
                .section {{
                    margin-bottom: 20px;
                    padding-bottom: 10px;
                    border-bottom: 1px solid #ddd;
                }}
                .footer {{
                    margin-top: 20px;
                    font-size: 14px;
                    color: #777;
                    text-align: center;
                }}
                table {{
                    width: 100%;
                    border-collapse: collapse;
                }}
                table, th, td {{
                    border: 1px solid #ddd;
                }}
                th, td {{
                    padding: 10px;
                    text-align: left;
                }}
                th {{
                    background-color: #f8f8f8;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <h2>HÓA ĐƠN THANH TOÁN THÀNH CÔNG</h2>
                <p><strong>Công ty Cổ phần WANVI</strong></p>
                <p>Số ĐKKD: XXXXXXXX</p>
                <p>Địa chỉ: [Địa chỉ công ty]</p>
                <p>Hotline: [Số hotline]</p>
                <p>Email: wanvi.wandervietnam@gmail.com</p>

                <div class='section'>
                    <h3>THÔNG TIN HÓA ĐƠN</h3>
                    <p><strong>Mã đơn hàng:</strong> {booking.OrderCode}</p>
                    <p><strong>Ngày giao dịch:</strong> {DateTime.Now.Date:dd/MM/yyyy}</p>
                    <p><strong>Hình thức thanh toán:</strong> Banking</p>
                </div>

                <div class='section'>
                    <h3>THÔNG TIN KHÁCH HÀNG</h3>
                    <p><strong>Họ và tên:</strong> {user.FullName}</p>
                    <p><strong>Email:</strong> {user.Email}</p>
                    <p><strong>Số điện thoại:</strong> {user.PhoneNumber}</p>
                </div>

                <div class='section'>
                    <h3>CHI TIẾT ĐƠN HÀNG</h3>
                    <table>
                        <thead>
                            <tr>
                                <th>Tên tour</th>
                                <th>Ngày khởi hành</th>
                                <th>Giờ bắt đầu</th>
                                <th>Số lượng</th>
                                <th>Đơn giá</th>
                                <th>Tổng tiền</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr>
                                <td>{booking.Schedule.Tour.Name}</td>
                                <td>{booking.RentalDate:dd/MM/yyyy}</td>
                                <td>{booking.Schedule.StartTime:HH:mm} - {booking.Schedule.EndTime:HH:mm}</td>
                                <td>{booking.TotalTravelers}</td>
                                <td>{booking.Schedule.Tour.HourlyRate * countHour:N0} đ</td>
                                <td>{booking.TotalPrice:N0} đ</td>
                            </tr>
                        </tbody>
                    </table>
                    <p><strong>Tổng tiền tour:</strong> {booking.TotalPrice:đ}</p>
                    <p><strong>Đã thanh toán (đầu tiên):</strong> 50% ({booking.TotalPrice:N0} đ)</p>
                    <p><strong>Còn lại thanh toán sau khi hoàn tất tour:</strong> {0:N0} đ</p>
                </div>
                <div class='footer'>
                    <p>Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!</p>
                </div>
            </div>
        </body>
        </html>"
  );
        }

    }
}

