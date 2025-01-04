using Wanvi.Core.Bases;

namespace Wanvi.Contract.Repositories.Entities
{
    public class TourAddress
    {
        public string TourId { get; set; }
        public Tour Tour { get; set; }
        public string AddressId { get; set; }
        public Address Address { get; set; }
    }
}
