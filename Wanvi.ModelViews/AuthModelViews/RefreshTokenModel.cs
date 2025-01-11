using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wanvi.ModelViews.AuthModelViews
{
    public class RefreshTokenModel
    {
        [Required(ErrorMessage = "RefreshToken không được để trống")]
        public string refreshToken { get; set; }
    }
}
