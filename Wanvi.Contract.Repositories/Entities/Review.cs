using Wanvi.Core.Bases;

namespace Wanvi.Contract.Repositories.Entities
{
    public class Review : BaseEntity
    {
        public double Rating { get; set; }
        public string Content { get; set; }

        public string TourId { get; set; }
        public virtual Tour Tour { get; set; }
        public Guid UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
