using System.ComponentModel.DataAnnotations;

namespace Dental_Clinic_System.Models
{
    public class InventoryItem
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public int Quantity { get; set; }
        public int ReorderLevel { get; set; }
        public string ExpiryDate { get; set; }
    }
}