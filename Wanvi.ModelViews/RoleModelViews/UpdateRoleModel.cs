using System.ComponentModel.DataAnnotations;

namespace Wanvi.ModelViews.RoleModelViews
{
    public class UpdateRoleModel
    {
        [Required(ErrorMessage = "Tên vai trò không được để trống.")]
        public string Name { get; set; }
    }
}
