using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using Wanvi.Core.Utils;

namespace Wanvi.Contract.Repositories.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FullName { get; set; } = string.Empty;
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
        public double AvgRating { get; set; } = 0.0;
        public double MinHourlyRate { get; set; } = 0.0;
        public bool IsPremium { get; set; } = false;
        public bool IsVerified { get; set; } = false;
        public string? IdentificationNumber { get; set; }
        public string? Bio {  get; set; }
        public string? Language {  get; set; }
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
        public virtual ICollection<ApplicationUserLogin> Logins { get; set; } = new List<ApplicationUserLogin>();
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
        public virtual ICollection<News> News { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<Request> Requests { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        [InverseProperty("Traveler")]
        public virtual ICollection<Review> TravelerReviews { get; set; }

        [InverseProperty("LocalGuide")]
        public virtual ICollection<Review> LocalGuideReviews { get; set; }
        public virtual ICollection<Subscription> Subscriptions { get; set; }
        public virtual ICollection<Tour> Tours { get; set; }
        public ApplicationUser()
        {
            CreatedTime = CoreHelper.SystemTimeNow;
            LastUpdatedTime = CreatedTime;
        }
    }
}
