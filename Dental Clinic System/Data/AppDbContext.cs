using System;
using System.IO;
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
            // Save database to AppData so Visual Studio never overwrites it
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Dental_Clinic_System");

            Directory.CreateDirectory(folder);

            string dbPath = Path.Combine(folder, "DentalClinic.db");

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StaffItem>().Property(s => s.ImageData)
                .HasColumnType("BLOB")
                .IsRequired(false);
        }
    }
}