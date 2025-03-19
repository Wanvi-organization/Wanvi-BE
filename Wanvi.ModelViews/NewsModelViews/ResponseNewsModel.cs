using Wanvi.Contract.Repositories.Entities;
using Wanvi.ModelViews.CommentModelViews;
using Wanvi.ModelViews.NewsDetailModelViews;

namespace Wanvi.ModelViews.NewsModelViews
{
    public class ResponseNewsModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public int LikeCount { get; set; }
        public bool IsLikedByUser { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public Guid UserId { get; set; }
        public string Author {  get; set; }
        public IEnumerable<ResponseCommentModel> Comments { get; set; }
        public IEnumerable<ResponseNewsDetailModel> NewsDetails { get; set; }
    }
}
