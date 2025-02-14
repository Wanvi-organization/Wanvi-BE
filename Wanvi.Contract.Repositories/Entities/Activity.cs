using Wanvi.Contract.Repositories.Base;

namespace Wanvi.Contract.Repositories.Entities
{
    public class Activity : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public virtual ICollection<TourActivity> TourActivities { get; set; }
    }
}
