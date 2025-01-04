namespace Wanvi.Contract.Repositories.Entities
{
    public class TourCategory
    {
        public string TourId { get; set; }
        public Tour Tour { get; set; }
        public string CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
