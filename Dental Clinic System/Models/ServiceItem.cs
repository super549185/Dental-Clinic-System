using System.ComponentModel.DataAnnotations;

namespace Dental_Clinic_System.Models
{
    public class ServiceItem
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Price { get; set; }
        public string Duration { get; set; }
        public string Description { get; set; }
    }
}