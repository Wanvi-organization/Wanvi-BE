namespace Wanvi.ModelViews.CommentModelViews
{
    public class ResponseCommentModel
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Content { get; set; }
        public bool IsLikedByUser { get; set; }
        public int LikeCount { get; set; }
        public List<ResponseCommentModel> Replies { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public DateTimeOffset LastUpdatedTime { get; set; }
    }
}
