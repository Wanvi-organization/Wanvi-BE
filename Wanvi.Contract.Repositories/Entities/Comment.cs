using System.Text.Json.Serialization;
using Wanvi.Core.Bases;

namespace Wanvi.Contract.Repositories.Entities
{
    public class Comment : BaseEntity
    {
        public string Content { get; set; }
        public bool IsLikedByUser { get; set; } = false;
        public int LikeCount { get; set; } = 0;
        public Guid UserId { get; set; }
        public string? PostId { get; set; }
        public string? NewsId { get; set; }
        public string? ParentCommentId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual Comment ParentComment { get; set; }
        public virtual Post Post { get; set; }
        public virtual News News { get; set; }
        public virtual ICollection<Comment> Replies { get; set; }
    }
}
