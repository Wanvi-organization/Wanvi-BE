using Wanvi.Contract.Repositories.Base;

namespace Wanvi.Contract.Repositories.Entities
{
    public class Subscription : BaseEntity
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int RemainingTrialUses { get; set; }

        public string PremiumPackageId { get; set; }
        public virtual PremiumPackage PremiumPackage { get; set; }
        public Guid UserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}
