namespace Wanvi.ModelViews.UserModelViews
{
    public class UpdateTravelerProfileModel
    {
        public string? FullName { get; set; }
        public bool? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? BankAccount { get; set; }
        public string? BankAccountName { get; set; }
        public string? Bank { get; set; }
        public string? Address { get; set; }
        public bool? IsPremium { get; set; }
        public bool? IsVerified { get; set; }
        public string? IdentificationNumber { get; set; }
        public bool? Violate { get; set; }
    }
}
