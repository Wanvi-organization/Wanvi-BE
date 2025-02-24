using Wanvi.Core.Bases;

namespace Wanvi.Contract.Repositories.Entities
{
    public class NewsDetail : BaseEntity
    {
        public string NewsId { get; set; }
        public string? Url { get; set; }
        public string Content { get; set; }
        public int SortOrder { get; set; }

        public virtual News News { get; set; }
    }
}
