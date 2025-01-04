namespace Wanvi.Contract.Repositories.Entities
{
    public class MessageMedia
    {
        public string MessageId { get; set; }
        public virtual Message Message { get; set; }
        public string MediaId { get; set; }
        public virtual Media Media { get; set; }
    }
}
