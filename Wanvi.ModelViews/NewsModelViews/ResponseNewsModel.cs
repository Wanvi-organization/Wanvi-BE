using Wanvi.Contract.Repositories.Entities;

namespace Wanvi.ModelViews.NewsModelViews
{
    public class ResponseNewsModel
    {
        public string Title { get; set; }
        public string Summary { get; set; }
        public int LikeCount { get; set; }
        public bool IsLikedByUser { get; set; }
        public string CategoryId { get; set; }
        public Guid UserId { get; set; }
        public IEnumerable<Comment> Comments { get; set; }
        public IEnumerable<NewsDetail> NewsDetails { get; set; }
    }
}
