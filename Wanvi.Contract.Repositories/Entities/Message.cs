using Wanvi.Core.Bases;

namespace Wanvi.Contract.Repositories.Entities
{
    public class Message : BaseEntity
    {
        public string Content { get; set; }
        public bool IsRead { get; set; }
        public string ConversationId { get; set; }
        public virtual Conversation Conversation { get; set; }
        public Guid SenderId { get; set; }
        public virtual ApplicationUser Sender { get; set; }
        public Guid ReceiverId { get; set; }
        public virtual ApplicationUser Receiver { get; set; }
        public virtual ICollection<MessageMedia> MessageMedias { get; set; }
    }
}
