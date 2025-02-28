using Wanvi.Core.Bases;

namespace Wanvi.Contract.Repositories.Entities
{
    public enum MediaType
    {
        Image,
        Video
    }

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
