namespace Wanvi.ModelViews.RequestModelViews
{
    public class ResponseRequestModel
    {
        public string Id { get; set; }
        public string BankAccount { get; set; }
        public string BankAccountName { get; set; }
        public string Bank { get; set; }
        public int Balance { get; set; }
        public string Note { get; set; }
        public long OrderCode { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; }
    }
}
