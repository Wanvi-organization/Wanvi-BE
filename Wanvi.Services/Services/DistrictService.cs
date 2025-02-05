using AutoMapper;
using Microsoft.AspNetCore.Http;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.DistrictModelViews;

namespace Wanvi.Services.Services
{
    public class DistrictService : IDistrictService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DistrictService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public IEnumerable<ResponseDistrictModel> GetByCityId(string id)
        {
            var districts = _unitOfWork.GetRepository<District>().Entities.Where(d => d.CityId == id).OrderBy(c => c.Name);

            if (!districts.Any())
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NotFound, "Quận/huyện không tồn tại.");
            }

            return _mapper.Map<IEnumerable<ResponseDistrictModel>>(districts.ToList());
        }
    }
}
