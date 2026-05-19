using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Dental_Clinic_System.Data;
using Dental_Clinic_System.Models;

namespace Dental_Clinic_System.Dashboard
{
    public partial class DashboardPage : Page
    {
        private AppDbContext _dbContext;

        public DashboardPage()
        {
            InitializeComponent();
            _dbContext = new AppDbContext();
            _dbContext.Database.EnsureCreated();
            LoadTodayData();
            LoadDentistsOnDuty();
            LoadBillingStats();
        }

        private void LoadTodayData()
        {
            string todayStr = DateTime.Today.ToString("yyyy-MM-dd");

            var todayApts = _dbContext.Appointments
                .Where(a => a.Date == todayStr)
                .ToList();

            StatAppointments.Text = todayApts.Count.ToString();

            TodayAppointmentsList.Children.Clear();

            if (todayApts.Count == 0)
            {
                EmptyState.Visibility = Visibility.Visible;
                return;
            }

            EmptyState.Visibility = Visibility.Collapsed;

            int index = 0;
            foreach (var apt in todayApts)
            {
                TodayAppointmentsList.Children.Add(CreateTableRow(apt, index));
                index++;
            }
        }

        private void LoadDentistsOnDuty()
        {
            OnDutyList.Children.Clear();
            // Get all dentists, then filter dynamically based on time
            var allDentists = _dbContext.Staff.Where(s => s.Role == "Dentist").ToList();
            var activeDentists = allDentists.Where(s => StaffHelper.GetDynamicStatus(s) == "On Duty").ToList();

            int totalDentists = _dbContext.Staff.Count(s => s.Role == "Dentist");
            DutyCount.Text = $"{activeDentists.Count} / {totalDentists} Active";

            if (!activeDentists.Any())
            {
                OnDutyList.Children.Add(new TextBlock
                {
                    Text = "No dentists on duty",
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x9C, 0xA3, 0xAF))
                });
                return;
            }

            // Display each active dentist beautifully
            foreach (var doc in activeDentists)
            {
                Border row = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(0xF9, 0xFA, 0xFB)),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(8, 5, 8, 5),
                    Margin = new Thickness(0, 0, 0, 4)
                };

                StackPanel sp = new StackPanel { Orientation = Orientation.Horizontal };

                // Green dot indicator
                Border dot = new Border
                {
                    Width = 8,
                    Height = 8,
                    CornerRadius = new CornerRadius(4),
                    Background = new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81)),
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 8, 0)
                };

                sp.Children.Add(dot);
                sp.Children.Add(new TextBlock
                {
                    Text = doc.Name,
                    FontSize = 12,
                    FontWeight = FontWeights.Medium,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x11, 0x18, 0x27)),
                    VerticalAlignment = VerticalAlignment.Center
                });

                row.Child = sp;
                OnDutyList.Children.Add(row);
            }
        }

        private void LoadBillingStats()
        {
            try
            {
                var billings = _dbContext.Billings.ToList();
                decimal totalBilled = billings.Sum(b => b.Amount);
                decimal totalPaid = billings.Where(b => b.PaymentStatus == "Paid").Sum(b => b.Amount);
                int pendingCount = billings.Count(b => b.PaymentStatus == "Pending" || b.PaymentStatus == "Partial");

                // Add billing stats to dashboard (if you have UI elements for these)
                // This could update TextBlocks or other UI elements
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Error loading billing stats: {ex.Message}");
            }
        }

        private Border CreateTableRow(AppointmentItem apt, int index)
        {
            Border row = new Border
            {
                Height = 55,
                Background = index % 2 == 0 ? Brushes.White : new SolidColorBrush(Color.FromRgb(0xF9, 0xFA, 0xFB)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xF3, 0xF4, 0xF6)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(25, 0, 25, 0)
            };

            Grid grid = new Grid();

            // WIDTHS MUST EXACTLY MATCH THE XAML COLUMN DEFINITIONS
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Patient Name
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120, GridUnitType.Pixel) }); // Time
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120, GridUnitType.Pixel) }); // Dentist
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140, GridUnitType.Pixel) }); // Service
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110, GridUnitType.Pixel) }); // Status

            grid.Children.Add(CreateCell(apt.PatientName, "#111827", FontWeights.Medium, 0));
            grid.Children.Add(CreateCell(apt.Time, "#4B5563", FontWeights.Normal, 1));
            grid.Children.Add(CreateCell(apt.Dentist, "#4B5563", FontWeights.Normal, 2));
            grid.Children.Add(CreateCell(apt.Service, "#4B5563", FontWeights.Normal, 3));

            grid.Children.Add(CreateStatusBadge(apt.Status, 4));

            row.Child = grid;
            return row;
        }

        private TextBlock CreateCell(string text, string hexColor, FontWeight weight, int col)
        {
            TextBlock tb = new TextBlock
            {
                Text = text,
                FontSize = 13,
                FontWeight = weight,
                Foreground = GetColorFromHex(hexColor),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(tb, col);
            return tb;
        }

        private Border CreateStatusBadge(string status, int col)
        {
            SolidColorBrush bg = Brushes.White;
            SolidColorBrush fg = Brushes.Black;

            if (status == "Confirmed")
            {
                bg = new SolidColorBrush(Color.FromRgb(0xD1, 0xFA, 0xE5));
                fg = new SolidColorBrush(Color.FromRgb(0x06, 0x5F, 0x46));
            }
            else if (status == "Cancelled")
            {
                bg = new SolidColorBrush(Color.FromRgb(0xFE, 0xE2, 0xE2));
                fg = new SolidColorBrush(Color.FromRgb(0x99, 0x1B, 0x1B));
            }

            Border badge = new Border
            {
                Background = bg,
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(10, 4, 10, 4),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            badge.Child = new TextBlock
            {
                Text = status,
                FontSize = 12,
                FontWeight = FontWeights.Medium,
                Foreground = fg
            };
            Grid.SetColumn(badge, col);
            return badge;
        }

        private SolidColorBrush GetColorFromHex(string hex)
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
    }
}
