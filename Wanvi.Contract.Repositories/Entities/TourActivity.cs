namespace Wanvi.Contract.Repositories.Entities
{
    public class TourActivity
    {
        public string TourId { get; set; }
        public virtual Tour Tour { get; set; }
        public string ActivityId { get; set; }
        public virtual Activity Activity { get; set; }
    }
}
