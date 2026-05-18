using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dental_Clinic_System.Data;
using Dental_Clinic_System.Models;

namespace Dental_Clinic_System.Dashboard
{
    public partial class StaffProfileWindow : Window
    {
        private AppDbContext _dbContext;
        private StaffItem _staff;

        public StaffProfileWindow(StaffItem staff)
        {
            InitializeComponent();
            _dbContext = new AppDbContext();
            _staff = staff;
            LoadData();
        }

        private void LoadData()
        {
            TxtName.Text = _staff.Name;
            TxtRole.Text = _staff.Specialization;
            TxtSchedule.Text = _staff.Schedule;
            // Calculate stats dynamically from actual appointments
            var staffApps = _dbContext.Appointments.Where(a => a.Dentist == _staff.Name).ToList();
            TxtLoad.Text = staffApps.Select(a => a.PatientName).Distinct().Count().ToString(); // Total unique patients
            TxtApps.Text = staffApps.Count.ToString(); // Total appointments
            ServicesPanel.Children.Clear();
            if (!string.IsNullOrEmpty(_staff.ServicesOffered))
            {
                var services = _staff.ServicesOffered.Split(',').Select(s => s.Trim()).ToList();
                foreach (var svc in services)
                {
                    if (string.IsNullOrEmpty(svc)) continue;

                    Border badge = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(0xE0, 0xF2, 0xFE)),
                        CornerRadius = new CornerRadius(12),
                        Padding = new Thickness(10, 5, 10, 5),
                        Margin = new Thickness(0, 0, 5, 5)
                    };
                    badge.Child = new TextBlock { Text = svc, FontSize = 12, FontWeight = FontWeights.Medium, Foreground = GetColor("#1D4ED8") };
                    ServicesPanel.Children.Add(badge);
                }
            }
            else
            {
                ServicesPanel.Children.Add(new TextBlock { Text = "No services listed.", FontSize = 12, Foreground = GetColor("#9CA3AF") });
            }

            string dynStatus = StaffHelper.GetDynamicStatus(_staff);
            StatusBadge.Child = new TextBlock { Text = dynStatus, FontSize = 11, FontWeight = FontWeights.Medium, Foreground = dynStatus == "On Duty" ? GetColor("#065F46") : GetColor("#6B7280") };
            StatusBadge.Background = dynStatus == "On Duty" ? GetColor("#D1FAE5") : GetColor("#F3F4F6");

            ProfileImage.Source = _staff.ImageData != null ? StaffPage.GetImageFromBytes(_staff.ImageData) : new BitmapImage(new Uri("pack://application:,,,/Images/default-avatar.png", UriKind.Absolute));

            var apps = _dbContext.Appointments.Where(a => a.Dentist == _staff.Name).OrderByDescending(a => a.Date).Take(10).ToList();
            AppointmentsList.Children.Clear();
            if (!apps.Any()) { AppointmentsList.Children.Add(new TextBlock { Text = "No appointments found.", FontSize = 13, Foreground = GetColor("#9CA3AF") }); return; }

            int i = 0;
            foreach (var a in apps)
            {
                Border row = new Border
                {
                    Height = 35,
                    Background = i % 2 == 0 ? Brushes.White : GetColor("#F9FAFB"),
                    CornerRadius = new CornerRadius(4),
                    Margin = new Thickness(0, 0, 0, 2) // FIXED THICKNESS
                };

                Grid g = new Grid { Margin = new Thickness(10, 0, 0, 0) }; // FIXED THICKNESS
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90, GridUnitType.Pixel) });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100, GridUnitType.Pixel) });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70, GridUnitType.Pixel) });

                g.Children.Add(new TextBlock { Text = FormatDate(a.Date), FontSize = 12, Foreground = GetColor("#4B5563"), VerticalAlignment = VerticalAlignment.Center });

                TextBlock p = new TextBlock { Text = a.PatientName, FontSize = 12, Foreground = GetColor("#111827"), FontWeight = FontWeights.Medium, VerticalAlignment = VerticalAlignment.Center };
                Grid.SetColumn(p, 1); g.Children.Add(p);

                TextBlock t = new TextBlock { Text = a.Service, FontSize = 12, Foreground = GetColor("#4B5563"), VerticalAlignment = VerticalAlignment.Center };
                Grid.SetColumn(t, 2); g.Children.Add(t);

                TextBlock d = new TextBlock { Text = a.Time, FontSize = 12, Foreground = GetColor("#6B7280"), VerticalAlignment = VerticalAlignment.Center };
                Grid.SetColumn(d, 3); g.Children.Add(d);

                row.Child = g;
                AppointmentsList.Children.Add(row);
                i++;
            }
        }

        private string FormatDate(string d) { if (DateTime.TryParse(d, out DateTime dt)) return dt.ToString("MMM dd, yyyy"); return d; }

        // --- Safe Color Helper ---
        private SolidColorBrush GetColor(string hex)
        {
            try
            {
                byte r = byte.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
                byte g = byte.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
                byte b = byte.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
                return new SolidColorBrush(Color.FromRgb(r, g, b));
            }
            catch { return Brushes.Black; }
        }

        private void Close_Click(object sender, RoutedEventArgs e) { this.Close(); }
    }
}