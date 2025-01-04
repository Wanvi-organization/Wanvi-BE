namespace Wanvi.Contract.Repositories.Entities
{
    public class PostHashtag
    {
        public string PostId { get; set; }
        public virtual Post Post { get; set; }

        public string HashtagId { get; set; }
        public virtual Hashtag Hashtag { get; set; }
    }
}
