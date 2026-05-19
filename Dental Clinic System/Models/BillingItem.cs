using System.ComponentModel.DataAnnotations;

namespace Dental_Clinic_System.Models
{
    public class BillingItem
    {
        [Key]
        public string InvoiceNo { get; set; }
        public string Patient { get; set; }
        public decimal Amount { get; set; }
        public string PaymentStatus { get; set; }
        public string Status { get; set; }
        public string Date { get; set; }
        public string PaymentMethod { get; set; }
        public decimal PaidAmount { get; set; }
        public string DueDate { get; set; }
    }
}
