using Wanvi.ModelViews.UserModelViews;

namespace Wanvi.ModelViews.AuthModelViews
{
    public class TokenResponse
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public ResponseUserModel User { get; set; }
    }
}
