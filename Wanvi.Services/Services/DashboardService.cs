using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.ModelViews.DashboardModelViews;

namespace Wanvi.Services.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DashboardService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseDashboardModel> GetDashboardDataAsync()
        {
            var dashboard = new Dashboard
            {
                TotalTraveler = await _unitOfWork.GetRepository<ApplicationUser>().Entities.CountAsync(u => u.UserRoles.Any(ur => ur.Role.Name == "Traveler")),
                TotalLocalGuide = await _unitOfWork.GetRepository<ApplicationUser>().Entities.CountAsync(u => u.UserRoles.Any(ur => ur.Role.Name == "LocalGuide")),
                TotalTour = await _unitOfWork.GetRepository<Tour>().Entities.CountAsync(),
                TotalBooking = await _unitOfWork.GetRepository<Booking>().Entities.CountAsync(),
                TotalRevenue = await _unitOfWork.GetRepository<Booking>().Entities.SumAsync(b => b.TotalPrice),
                TotalSubscription = await _unitOfWork.GetRepository<Subscription>().Entities.CountAsync()
            };

            return _mapper.Map<ResponseDashboardModel>(dashboard);
        }
    }
}
