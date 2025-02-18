using AutoMapper;
using Microsoft.AspNetCore.Http;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
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

        public async Task<IEnumerable<ResponseCityModel>> GetAllAsync()
        {
            var cities = await _unitOfWork.GetRepository<City>().FindAllAsync(c => !c.DeletedTime.HasValue);

            if (!cities.Any())
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NotFound, "Tỉnh/thành phố không tồn tại.");
            }

            return _mapper.Map<IEnumerable<ResponseCityModel>>(cities.OrderBy(x => x.Name));
        }
    }
}
