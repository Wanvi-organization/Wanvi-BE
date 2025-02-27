using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.RequestModelViews;

namespace Wanvi.Services.Services
{
    public class RequestService : IRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _contextAccessor;

        public RequestService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _contextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<ResponseRequestModel>> GetAllAsync(RequestStatus? status = null)
        {
            var query = _unitOfWork.GetRepository<Request>().Entities.Where(r => !r.DeletedTime.HasValue);

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
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
    }
}
