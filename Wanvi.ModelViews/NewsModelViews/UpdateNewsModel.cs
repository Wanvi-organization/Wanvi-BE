using Wanvi.ModelViews.NewsDetailModelViews;

namespace Wanvi.ModelViews.NewsModelViews
{
    public class UpdateNewsModel
    {
        public string? Title { get; set; }
        public string? Summary { get; set; }
        public string? CategoryId { get; set; }
        public string? MediaId { get; set; }
        public List<UpdateNewsDetailModel>? NewsDetails { get; set; }
    }
}
