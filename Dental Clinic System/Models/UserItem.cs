using System.ComponentModel.DataAnnotations;

namespace Dental_Clinic_System.Models
{
    public class UserItem
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string FullName { get; set; }

        public string ClinicName { get; set; }

        public string CreatedDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
