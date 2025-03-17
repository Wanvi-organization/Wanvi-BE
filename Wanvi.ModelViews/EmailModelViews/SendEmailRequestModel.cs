namespace Wanvi.ModelViews.EmailModelViews
{
    public class SendEmailRequestModel
    {
        public List<Guid>? UserIds { get; set; }
        public Guid? RoleId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }
}
