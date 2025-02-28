using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.Core.Utils;
using Wanvi.ModelViews.CategoryViewModels;
using Wanvi.Services.Services.Infrastructure;

namespace Wanvi.Services.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _contextAccessor;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _contextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<ResponseCategoryModel>> GetAllAsync()
        {
            var categories = await _unitOfWork.GetRepository<Category>().FindAllAsync(a => !a.DeletedTime.HasValue);

            if (!categories.Any())
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NotFound, "Danh mục không tồn tại.");
            }

            return _mapper.Map<IEnumerable<ResponseCategoryModel>>(categories);
        }

        public async Task<ResponseCategoryModel> GetByIdAsync(string id)
        {
            var category = await _unitOfWork.GetRepository<Category>().GetByIdAsync(id.Trim())
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Danh mục không tồn tại.");

            if (category.DeletedTime.HasValue)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Danh mục đã bị xóa.");
            }

            return _mapper.Map<ResponseCategoryModel>(category);
        }

        public async Task CreateAsync(CreateCategoryModel model)
        {
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);
            model.TrimAllStrings();

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Vui lòng điền tên danh mục.");
            }

            var existCategoryName = await _unitOfWork.GetRepository<Category>().FindAsync(c => c.Name == model.Name && !c.DeletedTime.HasValue);

            if (existCategoryName != null)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST, "Tên danh mục đã tồn tại.");
            }

            var newCategory = _mapper.Map<Category>(model);

            newCategory.CreatedBy = userId;
            newCategory.LastUpdatedBy = userId;

            await _unitOfWork.GetRepository<Category>().InsertAsync(newCategory);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(string id, UpdateCategoryModel model)
        {
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);
            model.TrimAllStrings();

            var category = await _unitOfWork.GetRepository<Category>().GetByIdAsync(id.Trim())
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Danh mục không tồn tại.");

            if (category.DeletedTime.HasValue)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Danh mục đã bị xóa.");
            }

            if (model.Name != null && string.IsNullOrWhiteSpace(model.Name))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST, "Tên danh mục không hợp lệ.");
            }

            var existCategoryName = await _unitOfWork.GetRepository<Category>().FindAsync(c => c.Name == model.Name && c.Id != category.Id && !c.DeletedTime.HasValue);

            if (existCategoryName != null)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST, "Tên danh mục đã tồn tại.");
            }

            _mapper.Map(model, category);
            category.LastUpdatedTime = CoreHelper.SystemTimeNow;
            category.LastUpdatedBy = userId;

            await _unitOfWork.GetRepository<Category>().UpdateAsync(category);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteAsync(string id)
        {
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);

            var category = await _unitOfWork.GetRepository<Category>().GetByIdAsync(id.Trim())
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Danh mục không tồn tại.");

            if (category.DeletedTime.HasValue)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Danh mục đã bị xóa.");
            }

            var isExistAnyNews = await _unitOfWork.GetRepository<News>().Entities.AnyAsync(n => n.CategoryId == id && !n.DeletedTime.HasValue);

            if (isExistAnyNews)
            {
                throw new ErrorException(StatusCodes.Status409Conflict, ResponseCodeConstants.FAILED, "Không thể xóa vì vẫn còn tin tức thuộc danh mục này.");
            }

            category.LastUpdatedTime = CoreHelper.SystemTimeNow;
            category.LastUpdatedBy = userId;
            category.DeletedTime = CoreHelper.SystemTimeNow;
            category.DeletedBy = userId;

            await _unitOfWork.GetRepository<Category>().UpdateAsync(category);
            await _unitOfWork.SaveAsync();
        }
    }
}
