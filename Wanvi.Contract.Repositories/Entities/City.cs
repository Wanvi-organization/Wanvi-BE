using Wanvi.Core.Bases;

namespace Wanvi.Contract.Repositories.Entities
{
    public class City : BaseEntity
    {
        public string Name { get; set; }

        public ICollection<District> Districts { get; set; }
    }
}
