using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.RequestModelViews;
using Wanvi.Services.Services.Infrastructure;

namespace Wanvi.Services.Services
{
    public class RequestService : IRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IEmailService _emailService;

        public RequestService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _contextAccessor = httpContextAccessor;
            _emailService = emailService;
        }

        public async Task<IEnumerable<ResponseRequestModel>> GetAllAsync(Guid roleId, RequestStatus? status = null, RequestType? type = null)
        {
            var query = _unitOfWork.GetRepository<Request>().Entities
                .Include(r => r.User)
                .ThenInclude(u => u.UserRoles)
                .Where(r => !r.DeletedTime.HasValue && r.User.UserRoles.Any(ur => ur.RoleId == roleId));

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            if (type.HasValue)
            {
                query = query.Where(r => r.Type == type.Value);
            }

            var requests = await query.ToListAsync();

            if (!requests.Any())
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NotFound, "Không có yêu cầu nào.");
            }

            return _mapper.Map<IEnumerable<ResponseRequestModel>>(requests);
        }

        public async Task<ResponseRequestModel> GetByIdAsync(string id)
        {
            var request = await _unitOfWork.GetRepository<Request>().GetByIdAsync(id.Trim())
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NotFound, "Yêu cầu không tồn tại.");

            if (request.DeletedTime.HasValue)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Yêu cầu đã bị xóa.");
            }

            return _mapper.Map<ResponseRequestModel>(request);
        }

        public async Task<string> CreateRequest(CreateRequestModel model)
        {
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);
            Guid.TryParse(userId, out Guid cb);
            if (model.Type == RequestType.BalanceWithdrawal)
            {
                if (model.Balance < 0)
                {
                    throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Vui lòng nhập số tiền lớn hơn 0");
                }
                var user = await _unitOfWork.GetRepository<ApplicationUser>().Entities.FirstOrDefaultAsync(x => x.Id == cb && !x.DeletedTime.HasValue) ?? throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "không tìm thấy tài khoản");
                if (model.Balance > user.Balance)
                {
                    throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, $"Tài khoản của quý khách({user.Balance} đ) không đủ để yêu cầu rút {model.Balance} đ");
                }
                var request = new Request()
                {
                    Note = model.Note,
                    Type = RequestType.BalanceWithdrawal,
                    Balance = model.Balance,
                    CreatedTime = DateTime.Now,
                    LastUpdatedTime = DateTime.Now,
                    CreatedBy = userId,
                    UserId = cb,
                    LastUpdatedBy = userId,
                    Status = RequestStatus.Pending,
                };
                await _unitOfWork.GetRepository<Request>().InsertAsync(request);
                await _unitOfWork.SaveAsync();
                return "Gửi yêu cầu rút tiền từ ví thành công";
            }
            if (model.Type == RequestType.Complaint)
            {
                if (string.IsNullOrWhiteSpace(model.Note))
                {
                    throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Vui lòng nhập điều muốn khiếu nại");
                }
                var request = new Request()
                {
                    Note = model.Note,
                    Type = RequestType.Complaint,
                    Balance = model.Balance,
                    CreatedTime = DateTime.Now,
                    LastUpdatedTime = DateTime.Now,
                    CreatedBy = userId,
                    UserId = cb,
                    LastUpdatedBy = userId,
                    Status = RequestStatus.Pending,
                };
                await _unitOfWork.GetRepository<Request>().InsertAsync(request);
                await _unitOfWork.SaveAsync();
                return "Gửi yêu cầu khiếu nại thành công";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(model.Note))
                {
                    throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Vui lòng nhập câu hỏi");
                }
                var request = new Request()
                {
                    Note = model.Note,
                    Type = RequestType.Question,
                    Balance = model.Balance,
                    CreatedTime = DateTime.Now,
                    LastUpdatedTime = DateTime.Now,
                    CreatedBy = userId,
                    UserId = cb,
                    LastUpdatedBy = userId,
                    Status = RequestStatus.Pending,
                };
                await _unitOfWork.GetRepository<Request>().InsertAsync(request);
                await _unitOfWork.SaveAsync();
                return "Gửi câu hỏi thành công";
            }
        }

        public async Task<string> AccecptFromAdmin(AccecptRequestFromAdminModel model)
        {
            var request = await _unitOfWork.GetRepository<Request>().Entities
                .FirstOrDefaultAsync(x => x.Id == model.Id && !x.DeletedTime.HasValue)
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NotFound, "Yêu cầu không tồn tại.");

            var resultMessage = "Chấp nhận yêu cầu thành công";

            switch (request.Type)
            {
                case RequestType.BookingWithdrawal:
                case RequestType.BalanceWithdrawal:
                    request.Status = RequestStatus.Confirmed;

                    if (request.Type == RequestType.BookingWithdrawal)
                    {
                        var booking = await _unitOfWork.GetRepository<Booking>().Entities
                            .FirstOrDefaultAsync(x => x.OrderCode == model.OrderCode && !x.DeletedTime.HasValue)
                            ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NotFound, "Hóa đơn không tồn tại.");

                        booking.Status = BookingStatus.Refunded;
                        await _unitOfWork.GetRepository<Booking>().UpdateAsync(booking);
                    }

                    await _unitOfWork.GetRepository<Request>().UpdateAsync(request);
                    await _unitOfWork.SaveAsync();

                    await SendWithdrawalSuccessEmail(request.User, request);
                    break;

                case RequestType.Complaint:
                case RequestType.Question:
                    request.Status = RequestStatus.Confirmed;
                    await _unitOfWork.GetRepository<Request>().UpdateAsync(request);
                    await _unitOfWork.SaveAsync();
                    break;

                default:
                    throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Loại yêu cầu không hợp lệ.");
            }

            return resultMessage;
        }

        private async Task SendWithdrawalSuccessEmail(ApplicationUser user, Request request)
        {
            await _emailService.SendEmailAsync(
                user.Email,
                "Xác nhận rút tiền thành công",
                $@"
                <html>
                <body>
                    <h2>THÔNG BÁO RÚT TIỀN THÀNH CÔNG</h2>
                    <p>Xin chào {user.FullName},</p>
                    <p>Chúng tôi xin thông báo rằng yêu cầu rút tiền của bạn đã được xử lý thành công.</p>
                    <p><strong>Số tiền rút:</strong> {request.Balance:N0} đ</p>
                    <p><strong>Ngân hàng:</strong> {user.Bank}</p>
                    <p><strong>Số tài khoản:</strong> {user.BankAccount}</p>
                    <p>Số tiền đã được chuyển vào tài khoản ngân hàng của bạn.</p>
                    <p>Nếu có bất kỳ vấn đề nào, vui lòng liên hệ ngay với chúng tôi.</p>
                    <p>Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!</p>
                </body>
                </html>"
            );
        }

        public async Task<string> CancelFromAdmin(CancelRequestFromAdminModel model)
        {
            var request = await _unitOfWork.GetRepository<Request>().Entities
                .FirstOrDefaultAsync(x => x.Id == model.Id && !x.DeletedTime.HasValue)
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NotFound, "Yêu cầu không tồn tại.");

            var resultMessage = "Hủy yêu cầu thành công";

            switch (request.Type)
            {
                case RequestType.BookingWithdrawal:
                    var booking = await _unitOfWork.GetRepository<Booking>().Entities
                        .FirstOrDefaultAsync(x => x.OrderCode == model.OrderCode && !x.DeletedTime.HasValue)
                        ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NotFound, "Hóa đơn không tồn tại.");

                    request.Status = RequestStatus.Cancelled;
                    booking.Request = false;

                    await _unitOfWork.GetRepository<Booking>().UpdateAsync(booking);
                    await _unitOfWork.GetRepository<Request>().UpdateAsync(request);

                    await _unitOfWork.SaveAsync();
                    await SendRequestCancellationEmail(booking.User, request, "Yêu cầu rút tiền từ booking");
                    break;

                case RequestType.BalanceWithdrawal:
                    request.Status = RequestStatus.Cancelled;
                    await _unitOfWork.GetRepository<Request>().UpdateAsync(request);
                    await _unitOfWork.SaveAsync();
                    await SendRequestCancellationEmail(request.User, request, "Yêu cầu rút tiền từ ví");
                    break;

                case RequestType.Complaint:
                    request.Status = RequestStatus.Cancelled;
                    await _unitOfWork.GetRepository<Request>().UpdateAsync(request);
                    await _unitOfWork.SaveAsync();
                    await SendRequestCancellationEmail(request.User, request, "Yêu cầu khiếu nại");
                    break;

                case RequestType.Question:
                    request.Status = RequestStatus.Cancelled;
                    await _unitOfWork.GetRepository<Request>().UpdateAsync(request);
                    await _unitOfWork.SaveAsync();
                    await SendRequestCancellationEmail(request.User, request, "Yêu cầu thắc mắc");
                    break;

                default:
                    throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Loại yêu cầu không hợp lệ.");
            }

            return resultMessage;
        }

        private async Task SendRequestCancellationEmail(ApplicationUser user, Request request, string requestType)
        {
            await _emailService.SendEmailAsync(
                user.Email,
                "Thông Báo Yêu Cầu Bị Hủy",
                $@"
<html>
<body>
    <h2>THÔNG BÁO HỦY {requestType.ToUpper()}</h2>
    <p>Xin chào {user.FullName},</p>
    <p>Chúng tôi xin thông báo rằng {requestType.ToLower()} của bạn đã bị hủy bởi quản trị viên.</p>
    <p><strong>Mã yêu cầu:</strong> {request.Id}</p>
    <p><strong>Lý do hủy:</strong> {request.Reason ?? "Không có lý do cụ thể."}</p>
    <p>Nếu có bất kỳ thắc mắc nào, vui lòng liên hệ với chúng tôi để được hỗ trợ.</p>
    <p>Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!</p>
</body>
</html>"
            );
        }
    }
}
