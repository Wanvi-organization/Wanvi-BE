namespace Wanvi.ModelViews.UserModelViews
{
    public class AdminResponseUserModel
    {
        public Guid Id { get; set; }
        public Guid RoleId { get; set; }
        public string FullName { get; set; }
        public bool Gender { get; set; } = true;
        public DateTime? DateOfBirth { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? BankAccount { get; set; }
        public string? BankAccountName { get; set; }
        public string? Bank { get; set; }
        public int Balance { get; set; } = 0;
        public int Deposit { get; set; } = 0;
        public int Point { get; set; } = 0;
        public string? Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double AvgRating { get; set; }
        public double MinHourlyRate { get; set; }
        public bool IsPremium { get; set; } = false;
        public bool IsVerified { get; set; } = false;
        public string? IdentificationNumber { get; set; }
        public string? Bio { get; set; }
        public string? Language { get; set; }
        public string? PersonalVehicle { get; set; }
        public int? EmailCode { get; set; }
        public DateTime? CodeGeneratedTime { get; set; }
        public string? CreatedBy { get; set; }
        public bool? Violate { get; set; } = false;
        public string? LastUpdatedBy { get; set; }
        public string? DeletedBy { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public DateTimeOffset LastUpdatedTime { get; set; }
        public DateTimeOffset? DeletedTime { get; set; }
    }
}
