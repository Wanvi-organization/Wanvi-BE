using Microsoft.AspNetCore.Mvc;
using Wanvi.Contract.Repositories.Base;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.RoleModelViews;

namespace Wanvi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpPost("create_role")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleModel model)
        {
            await _roleService.CreateAsync(model);

            return Ok(new BaseResponseModel<string>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: "Vai trò đã được tạo thành công!"
            ));
        }

        [HttpGet("get_all_roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _roleService.GetAllAsync();

            return Ok(new BaseResponseModel<List<ResponseRoleModel>>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: roles
            ));
        }

        [HttpPut("update_role/{roleId}")]
        public async Task<IActionResult> UpdateRole(Guid roleId, [FromBody] UpdateRoleModel model)
        {
            await _roleService.UpdateAsync(roleId, model);

            return Ok(new BaseResponseModel<string>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: "Vai trò đã được cập nhật thành công!"
            ));
        }

        [HttpDelete("delete_role/{roleId}")]
        public async Task<IActionResult> DeleteRole(Guid roleId)
        {
            await _roleService.DeleteAsync(roleId);

            return Ok(new BaseResponseModel<string>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: "Vai trò đã được xóa thành công!"
            ));
        }
    }
}
