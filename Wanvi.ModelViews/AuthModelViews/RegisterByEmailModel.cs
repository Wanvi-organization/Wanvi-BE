using System.ComponentModel.DataAnnotations;

namespace Wanvi.ModelViews.AuthModelViews
{
    public class RegisterByEmailModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string PlaceOfBirth { get; set; }
        public bool Gender { get; set; }
        public string ConfirmPassword  { get; set; }
        public bool RoleName { get; set; }
    }
}
