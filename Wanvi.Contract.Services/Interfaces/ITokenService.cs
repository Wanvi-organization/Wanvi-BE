using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.ModelViews.AuthModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface ITokenService
    {
        TokenResponse GenerateTokens(ApplicationUser user, string role);
    }
}
