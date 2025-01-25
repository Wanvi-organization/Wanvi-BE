using Wanvi.Contract.Repositories.Base;

namespace Wanvi.Contract.Repositories.Entities
{
    public class PremiumPackage : BaseEntity
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public int DurationInDays { get; set; }
    }
}
