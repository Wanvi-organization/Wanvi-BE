using Wanvi.Core.Bases;
using static Wanvi.Core.Constants.Enum;

namespace Wanvi.Contract.Repositories.Entities
{
    public class Media : BaseEntity
    {
        public string Url { get; set; }
        public MediaType Type { get; set; }
        public string? AltText { get; set; }

        public string? TourId { get; set; }
        public virtual Tour Tour { get; set; }
        public string? PostId { get; set; }
        public virtual Post Post { get; set; }
        public virtual ICollection<MessageMedia> MessageMedias { get; set; }
    }
}
