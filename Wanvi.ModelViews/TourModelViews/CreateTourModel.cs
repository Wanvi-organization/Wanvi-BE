namespace Wanvi.ModelViews.TourModelViews
{
    public class CreateTourModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double HourlyRate { get; set; }
        public string PickupAddressId { get; set; }
        public string DropoffAddressId { get; set; }
    }
}
