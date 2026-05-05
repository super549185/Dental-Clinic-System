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
            var history = appointments.Where(a => a.Date.CompareTo(todayStr) < 0).OrderByDescending(a => a.Date).ToList();
            HistoryPanel.Children.Clear();
            if (!history.Any())
            {
                HistoryPanel.Children.Add(new TextBlock
                {
                    Text = "No past history available.",
                    FontSize = 13,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x9C, 0xA3, 0xAF))
                });
            }

            int i = 0;
            foreach (var apt in history)
            {
                Border row = new Border
                {
                    Height = 35,
                    Background = i % 2 == 0 ? Brushes.White : new SolidColorBrush(Color.FromRgb(0xF9, 0xFA, 0xFB)),
                    CornerRadius = new CornerRadius(4),
                    Margin = new Thickness(0, 0, 0, 2)
                };

                Grid g = new Grid { Margin = new Thickness(10, 0, 0, 0) };
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90, GridUnitType.Pixel) });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80, GridUnitType.Pixel) });

                // FIXED: Removed quotes from FontSize and FontWeight
                g.Children.Add(new TextBlock
                {
                    Text = FormatDate(apt.Date),
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x4B, 0x55, 0x63)),
                    VerticalAlignment = VerticalAlignment.Center
                });

                TextBlock serv = new TextBlock
                {
                    Text = apt.Service,
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x11, 0x18, 0x27)),
                    FontWeight = FontWeights.Medium,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(serv, 1);
                g.Children.Add(serv);

                TextBlock doc = new TextBlock
                {
                    Text = apt.Dentist,
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80)),
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(doc, 2);
                g.Children.Add(doc);

                row.Child = g;
                HistoryPanel.Children.Add(row);
                i++;
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