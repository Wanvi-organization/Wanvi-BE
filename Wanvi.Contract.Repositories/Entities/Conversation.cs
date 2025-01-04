using Wanvi.Core.Bases;

namespace Wanvi.Contract.Repositories.Entities
{
    public class Conversation : BaseEntity
    {
        public bool IsCompleted { get; set; }
        public Guid User1Id { get; set; }
        public virtual ApplicationUser User1 { get; set; }
        public Guid User2Id { get; set; }
        public virtual ApplicationUser User2 { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
    }
}
