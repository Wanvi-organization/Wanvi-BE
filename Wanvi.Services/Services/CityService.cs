using AutoMapper;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.ModelViews.CityModelViews;

namespace Wanvi.Services.Services
{
    public class CityService : ICityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CityService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public IEnumerable<ResponseCityModel> GetAll()
        {
            var cities = _unitOfWork.GetRepository<City>().Entities.Where(c => !c.DeletedTime.HasValue).OrderBy(c => c.Name);
            return _mapper.Map<IEnumerable<ResponseCityModel>>(cities.ToList());
        }
    }
}
