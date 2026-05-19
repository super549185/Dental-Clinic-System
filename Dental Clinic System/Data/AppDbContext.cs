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
        public DbSet<StaffItem> Staff { get; set; }
        public DbSet<DentalHistoryItem> DentalHistory { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=DentalClinic.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StaffItem>().Property(s => s.ImageData)
                .HasColumnType("BLOB")
                .IsRequired(false);
        }
    }
}