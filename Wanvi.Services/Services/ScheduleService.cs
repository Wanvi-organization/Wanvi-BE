using AutoMapper;
using Microsoft.AspNetCore.Http;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.ScheduleModelViews;
using Wanvi.Services.Services.Infrastructure;

namespace Wanvi.Services.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _contextAccessor;

        public ScheduleService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _contextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<ResponseScheduleModel>> GetLocalGuideSchedulesAsync()
        {
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);

            var schedules = await _unitOfWork.GetRepository<Schedule>()
                .FindAllAsync(s => s.Tour.CreatedBy == userId && !s.DeletedTime.HasValue);

            if (!schedules.Any())
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Không có lịch trình nào.");
            }

            return _mapper.Map<IEnumerable<ResponseScheduleModel>>(schedules
            .OrderBy(s => (int)s.Day)
            .ThenBy(s => s.StartTime));
        }
    }
}
