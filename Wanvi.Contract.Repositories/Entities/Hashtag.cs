using Wanvi.Core.Bases;

namespace Wanvi.Contract.Repositories.Entities
{
    public class Hashtag : BaseEntity
    {
        public string Name { get; set; }
        public virtual ICollection<PostHashtag> PostHashtags { get; set; }
    }
}
