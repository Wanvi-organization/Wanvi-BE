using AutoMapper;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.ModelViews.ActivityModelViews;

namespace Wanvi.Services.Services
{
    public class ActivityService : IActivityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ActivityService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public IEnumerable<ResponseActivityModel> GetAll()
        {
            var activities = _unitOfWork.GetRepository<Activity>().Entities.Where(c => !c.DeletedTime.HasValue).OrderBy(c => c.Name);
            return _mapper.Map<IEnumerable<ResponseActivityModel>>(activities.ToList());
        }
    }
}
