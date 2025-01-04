using Wanvi.Core.Bases;

namespace Wanvi.Contract.Repositories.Entities
{
    public class Post : BaseEntity
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public int LikeCount { get; set; }
        public bool IsLikedByUser { get; set; }

        public Guid UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Media> Medias { get; set; }
        public virtual ICollection<PostHashtag> PostHashtags { get; set; }
    }
}
