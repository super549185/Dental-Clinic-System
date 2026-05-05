using Microsoft.EntityFrameworkCore;
using Dental_Clinic_System.Models;

namespace Dental_Clinic_System.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<AppointmentItem> Appointments { get; set; }
        public DbSet<PatientItem> Patients { get; set; }
        public DbSet<ServiceItem> Services { get; set; }
        public DbSet<InventoryItem> Inventory { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=DentalClinic.db");
        }
    }
}