using System.ComponentModel.DataAnnotations;

namespace Dental_Clinic_System.Models
{
    public class PatientItem
    {
        [Key]
        public string PatientId { get; set; }
        public string Name { get; set; }
        public string Contact { get; set; }
        public string Email { get; set; }
        public string LastVisit { get; set; }
        public string Status { get; set; }

        public string DateOfBirth { get; set; }
        public string Address { get; set; }
        public string Allergies { get; set; }
        public string MedicalConditions { get; set; }
        public string BloodType { get; set; }
    }
}