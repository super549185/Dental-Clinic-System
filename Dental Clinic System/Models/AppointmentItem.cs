using System.ComponentModel.DataAnnotations;

namespace Dental_Clinic_System.Models
{
    public class AppointmentItem
    {
        [Key]
        public string AppointmentId { get; set; }

        public string PatientName { get; set; }
        public string Dentist { get; set; }
        public string Service { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Status { get; set; }
    }
}