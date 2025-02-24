using Wanvi.Core.Bases;

namespace Wanvi.Contract.Repositories.Entities
{
    public class News : BaseEntity
    {
        public string Title { get; set; }
        public string Summary { get; set; }
        public int LikeCount { get; set; } = 0;
        public bool IsLikedByUser { get; set; } = false;

        public string CategoryId { get; set; }
        public virtual Category Category { get; set; }
        public Guid UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<NewsDetail> NewsDetails { get; set; }
    }
}
