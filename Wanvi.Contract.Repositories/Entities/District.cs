using Wanvi.Core.Bases;

namespace Wanvi.Contract.Repositories.Entities
{
    public class District : BaseEntity
    {
        public string Name { get; set; }

        public string CityId { get; set; }
        public City City { get; set; }
        public virtual ICollection<Address> Addresses { get; set; }
    }
}
