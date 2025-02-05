namespace Wanvi.ModelViews.AuthModelViews
{
    public class LoginResponse
    {
        public Task<TokenResponse> TokenResponse { get; set; }
        public string Role { get; set; }
    }
}
