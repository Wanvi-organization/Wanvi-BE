using Wanvi.Core.Bases;

namespace Wanvi.Contract.Repositories.Entities
{
    public enum ReviewType
    {
        TourReview = 0,
        LocalGuideReview = 1
    }

    public class Review : BaseEntity
    {
        public int Rating { get; set; }
        public string Content { get; set; }
        public ReviewType ReviewType { get; set; }
        public string? TourId { get; set; }
        public virtual Tour Tour { get; set; }
        public string? BookingId { get; set; }
        public virtual Booking Booking { get; set; }
        public Guid? TravelerId { get; set; }
        public virtual ApplicationUser Traveler { get; set; }
        public Guid? LocalGuideId { get; set; }
        public virtual ApplicationUser LocalGuide { get; set; }
        public virtual ICollection<Media> Medias { get; set; } = new List<Media>();
    }
}
