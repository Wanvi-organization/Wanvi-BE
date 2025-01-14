using Wanvi.Core.Bases;

namespace Wanvi.Contract.Repositories.Entities
{
    public class City : BaseEntity
    {
        public string Name { get; set; }

        public virtual ICollection<District> Districts { get; set; }
    }
}
