using Microsoft.AspNetCore.Mvc;
using Wanvi.Contract.Repositories.Base;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.CategoryViewModels;

namespace Wanvi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        /// <summary>
        /// Lấy toàn bộ danh mục.
        /// </summary>
        [HttpGet("Get_All_Categories")]
        public async Task<IActionResult> GetAllCategories()
        {
            return Ok(new BaseResponseModel<IEnumerable<ResponseCategoryModel>>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: await _categoryService.GetAllAsync()));
        }
        /// <summary>
        /// Lấy danh mục bằng id.
        /// </summary>
        /// <param name="id">ID của danh mục cần lấy</param>
        [HttpGet("Get_Category_By_Id/{id}")]
        public async Task<IActionResult> GetCategoryById(string id)
        {
            return Ok(new BaseResponseModel<ResponseCategoryModel>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: await _categoryService.GetByIdAsync(id)));
        }
        /// <summary>
        /// Tạo một danh mục mới.
        /// </summary>
        /// <param name="model">Thông tin danh mục cần tạo</param>
        [HttpPost("Create_Category")]
        public async Task<IActionResult> CreateCategory(CreateCategoryModel model)
        {
            await _categoryService.CreateAsync(model);
            return Ok(new BaseResponseModel<string?>(
              statusCode: StatusCodes.Status200OK,
              code: ResponseCodeConstants.SUCCESS,
              message: "Tạo mới danh mục thành công."));
        }
        /// <summary>
        /// Cập nhật một danh mục.
        /// </summary>
        /// <param name="id">ID của danh mục cần cập nhật</param>
        /// <param name="model">Thông tin cập nhật cho danh mục</param>
        [HttpPatch("Update_Category/{id}")]
        public async Task<IActionResult> UpdateCategory(string id, UpdateCategoryModel model)
        {
            await _categoryService.UpdateAsync(id, model);
            return Ok(new BaseResponseModel<string?>(
               statusCode: StatusCodes.Status200OK,
               code: ResponseCodeConstants.SUCCESS,
               message: "Cập nhật danh mục thành công."));
        }
        /// <summary>
        /// Xóa một danh mục.
        /// </summary>
        /// <param name="id">id của danh mục cần xóa.</param>
        ///
        [HttpDelete("Delete_Category/{id}")]
        public async Task<IActionResult> DeleteCategory(string id)
        {
            await _categoryService.DeleteAsync(id);
            return Ok(new BaseResponseModel<string?>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                message: "Xóa danh mục thành công."));
        }

    }
}
