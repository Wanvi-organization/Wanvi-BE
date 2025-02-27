using Wanvi.Contract.Repositories.Base;

namespace Wanvi.Contract.Repositories.Entities
{
    public enum RequestStatus
    {
        Pending,
        Confirmed,
        Cancelled,
    }

    public class Request : BaseEntity
    {
        public string? BankAccount { get; set; }
        public string? BankAccountName { get; set; }
        public string? Bank { get; set; }
        public int Balance { get; set; } = 0;
        public string? Note { get; set; }
        public long OrderCode { get; set; } = 0;
        public string? Reason { get; set; }
        public RequestStatus Status { get; set; }
        public Guid UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
