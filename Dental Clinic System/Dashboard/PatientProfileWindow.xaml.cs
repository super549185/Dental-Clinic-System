using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Dental_Clinic_System.Data;
using Dental_Clinic_System.Models;

namespace Dental_Clinic_System.Dashboard
{
    public partial class PatientProfileWindow : Window
    {
        private AppDbContext _dbContext;
        private PatientItem _patient;

        public PatientProfileWindow(PatientItem patient)
        {
            InitializeComponent();
            _dbContext = new AppDbContext();
            _patient = patient;
            LoadData();
        }

        private void LoadData()
        {
            ProfileTitle.Text = _patient.Name;

            // Basic Info
            TxtName.Text = _patient.Name;
            TxtDOB.Text = FormatDate(_patient.DateOfBirth);
            TxtPhone.Text = _patient.Contact;
            TxtEmail.Text = _patient.Email;
            TxtAddress.Text = _patient.Address;

            // Medical Info
            TxtAllergies.Text = string.IsNullOrEmpty(_patient.Allergies) ? "None" : _patient.Allergies;
            TxtConditions.Text = string.IsNullOrEmpty(_patient.MedicalConditions) ? "None" : _patient.MedicalConditions;
            TxtBlood.Text = string.IsNullOrEmpty(_patient.BloodType) ? "-" : _patient.BloodType;

            // Fetch Appointments for this patient
            var appointments = _dbContext.Appointments.Where(a => a.PatientName == _patient.Name).ToList();
            string todayStr = DateTime.Today.ToString("yyyy-MM-dd");

            // Upcoming Appointments
            var upcoming = appointments.Where(a => a.Date.CompareTo(todayStr) >= 0).OrderBy(a => a.Date).ToList();
            UpcomingPanel.Children.Clear();
            if (upcoming.Any())
            {
                foreach (var apt in upcoming)
                {
                    // FIXED: Removed quotes from FontSize and Foreground
                    UpcomingPanel.Children.Add(new TextBlock
                    {
                        Text = $"{FormatDate(apt.Date)} at {apt.Time}",
                        FontSize = 13,
                        Foreground = new SolidColorBrush(Color.FromRgb(0x06, 0x5F, 0x46)),
                        Margin = new Thickness(0, 0, 0, 5)
                    });
                    UpcomingPanel.Children.Add(new TextBlock
                    {
                        Text = $"{apt.Service} with {apt.Dentist}",
                        FontSize = 12,
                        Foreground = new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80)),
                        Margin = new Thickness(0, 0, 0, 15)
                    });
                }
            }
            else
            {
                UpcomingPanel.Children.Add(new TextBlock
                {
                    Text = "No upcoming appointments.",
                    FontSize = 13,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x9C, 0xA3, 0xAF))
                });
            }

            // Dental History (Past Appointments)
            // Dental History (Past Appointments + Custom History)
            var history = _dbContext.Appointments.Where(a => a.PatientName == _patient.Name && a.Date.CompareTo(todayStr) < 0).OrderByDescending(a => a.Date).ToList();
            var customHistory = _dbContext.DentalHistory.Where(h => h.PatientId == _patient.PatientId).OrderByDescending(h => h.DateCreated).ToList();

            HistoryPanel.Children.Clear();

            if (!history.Any() && !customHistory.Any())
            {
                HistoryPanel.Children.Add(new TextBlock
                {
                    Text = "No dental history available.",
                    FontSize = 13,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x9C, 0xA3, 0xAF))
                });
            }
            else
            {
                // Show custom dental history entries first
                int i = 0;
                foreach (var entry in customHistory)
                {
                    Border row = new Border
                    {
                        Height = 80,
                        Background = i % 2 == 0 ? Brushes.White : new SolidColorBrush(Color.FromRgb(0xF9, 0xFA, 0xFB)),
                        BorderBrush = new SolidColorBrush(Color.FromRgb(0xE5, 0xE7, 0xEB)),
                        BorderThickness = new Thickness(0, 0, 0, 1),
                        Padding = new Thickness(12, 8, 12, 8),
                        Margin = new Thickness(0, 0, 0, 5)
                    };

                    StackPanel info = new StackPanel();
                    info.Children.Add(new TextBlock
                    {
                        Text = $"{entry.Service} | {entry.Dentist}",
                        FontSize = 12,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = new SolidColorBrush(Color.FromRgb(0x11, 0x18, 0x27))
                    });
                    info.Children.Add(new TextBlock
                    {
                        Text = $"Date: {FormatDate(entry.AppointmentDate)} at {entry.AppointmentTime}",
                        FontSize = 11,
                        Foreground = new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80)),
                        Margin = new Thickness(0, 4, 0, 0)
                    });
                    info.Children.Add(new TextBlock
                    {
                        Text = $"Diagnosis: {entry.Diagnosis}",
                        FontSize = 10,
                        Foreground = new SolidColorBrush(Color.FromRgb(0x4B, 0x55, 0x63)),
                        Margin = new Thickness(0, 2, 0, 0)
                    });
                    info.Children.Add(new TextBlock
                    {
                        Text = $"Notes: {entry.TreatmentNotes}",
                        FontSize = 10,
                        Foreground = new SolidColorBrush(Color.FromRgb(0x4B, 0x55, 0x63)),
                        Margin = new Thickness(0, 2, 0, 0)
                    });

                    row.Child = info;
                    HistoryPanel.Children.Add(row);
                    i++;
                }
            }
        }

        private string FormatDate(string d)
        {
            if (DateTime.TryParse(d, out DateTime dt)) return dt.ToString("MMM dd, yyyy");
            return d;
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            if (new PatientFormWindow(_patient).ShowDialog() == true)
            {
                _dbContext = new AppDbContext();
                _patient = _dbContext.Patients.FirstOrDefault(p => p.PatientId == _patient.PatientId);
                if (_patient != null) LoadData();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) { this.Close(); }
    }
}