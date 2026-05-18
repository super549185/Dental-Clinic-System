using System.ComponentModel.DataAnnotations;

namespace Dental_Clinic_System.Models
{
    public class StaffItem
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string Specialization { get; set; }
        public string Status { get; set; }
        public string Schedule { get; set; }
        public int PatientLoad { get; set; }
        public int TotalAppointments { get; set; }
        public string ServicesOffered { get; set; }
        public byte[] ImageData { get; set; }
    }
}