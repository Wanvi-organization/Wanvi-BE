namespace Wanvi.Contract.Repositories.Entities
{
    public class TourCategory
    {
        public string TourId { get; set; }
        public virtual Tour Tour { get; set; }
        public string CategoryId { get; set; }
        public virtual Category Category { get; set; }
    }
}
