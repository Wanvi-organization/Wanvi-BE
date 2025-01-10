using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wanvi.ModelViews.AuthModelViews
{
    public class LoginResponse
    {
        public TokenResponse TokenResponse { get; set; }
        public string Role { get; set; }
        public string token { get; set; }
    }
}
