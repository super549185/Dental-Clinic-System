using System.ComponentModel.DataAnnotations;

namespace Dental_Clinic_System.Models
{
    public class DentalHistoryItem
    {
        [Key]
        public int HistoryId { get; set; }

        public string PatientId { get; set; }
        public string PatientName { get; set; }

        public string AppointmentDate { get; set; }
        public string AppointmentTime { get; set; }
        public string Service { get; set; }
        public string Dentist { get; set; }

        public string TreatmentNotes { get; set; }
        public string Diagnosis { get; set; }
        public string TreatmentOutcome { get; set; }

        public string DateCreated { get; set; }
    }
}