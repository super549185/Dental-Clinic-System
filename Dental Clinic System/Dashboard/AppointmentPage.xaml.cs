using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Dental_Clinic_System.Data;
using Dental_Clinic_System.Models;

namespace Dental_Clinic_System.Dashboard
{
    public partial class AppointmentPage : Page
    {
        private AppDbContext _dbContext;
        private string currentFilter = "All";
        private string currentSearch = "";

        public AppointmentPage()
        {
            InitializeComponent();
            _dbContext = new AppDbContext();
            _dbContext.Database.EnsureCreated();
            SeedDatabase();
            LoadAppointments();
        }

        private void SeedDatabase()
        {
            // Only seed if table is completely empty
            if (!_dbContext.Appointments.Any())
            {
                using (var db = new AppDbContext())
                {
                    string todayStr = DateTime.Today.ToString("yyyy-MM-dd");

                    // DYNAMICALLY get available services and dentists from DB
                    var services = db.Services.Select(s => s.Name).ToList();
                    var dentists = db.Staff.Where(s => s.Role == "Dentist").Select(s => s.Name).ToList();

                    if (!services.Any() || !dentists.Any()) return; // Wait until user adds data

                    // DYNAMICALLY generate 1-hour time intervals
                    List<string> validTimes = new List<string>();
                    for (int hour = 8; hour <= 17; hour++)
                    {
                        validTimes.Add(DateTime.Today.AddHours(hour).ToString("hh:mm tt"));
                    }

                    Random rnd = new Random();

                    // Generate sample data using real DB data
                    _dbContext.Appointments.AddRange(new List<AppointmentItem>
                    {
                        new AppointmentItem { AppointmentId = "APT001", PatientName = "John Doe", Dentist = dentists[rnd.Next(dentists.Count)], Service = services[rnd.Next(services.Count)], Date = todayStr, Time = validTimes[0], Status = "Confirmed" },
                        new AppointmentItem { AppointmentId = "APT002", PatientName = "Jane Wilson", Dentist = dentists[rnd.Next(dentists.Count)], Service = services[rnd.Next(services.Count)], Date = todayStr, Time = validTimes[1], Status = "Confirmed" },
                        new AppointmentItem { AppointmentId = "APT003", PatientName = "Mark Sanchez", Dentist = dentists[rnd.Next(dentists.Count)], Service = services[rnd.Next(services.Count)], Date = todayStr, Time = validTimes[2], Status = "Confirmed" },
                        new AppointmentItem { AppointmentId = "APT004", PatientName = "Emily Davis", Dentist = dentists[rnd.Next(dentists.Count)], Service = services[rnd.Next(services.Count)], Date = "2025-02-05", Time = validTimes[3], Status = "Cancelled" },
                        new AppointmentItem { AppointmentId = "APT005", PatientName = "Chris Brown", Dentist = dentists[rnd.Next(dentists.Count)], Service = services[rnd.Next(services.Count)], Date = "2025-02-06", Time = validTimes[4], Status = "Cancelled" }
                    });
                }
                _dbContext.SaveChanges();
            }
        }

        private void LoadAppointments()
        {
            // ALWAYS force fresh database read
            _dbContext.Dispose();
            _dbContext = new AppDbContext();

            AppointmentList.Children.Clear();
            var appointments = _dbContext.Appointments.ToList();

            // Apply Filters
            if (currentFilter == "Confirmed")
                appointments = appointments.Where(a => a.Status == "Confirmed").ToList();
            else if (currentFilter == "Cancelled")
                appointments = appointments.Where(a => a.Status == "Cancelled").ToList();
            else if (currentFilter == "Today")
            {
                string todayStr = DateTime.Today.ToString("yyyy-MM-dd");
                appointments = appointments.Where(a => a.Date == todayStr).ToList();
            }

            // Apply Search
            if (!string.IsNullOrEmpty(currentSearch) && currentSearch != "Search patient...")
                appointments = appointments.Where(a => a.PatientName.ToLower().Contains(currentSearch.ToLower())).ToList();

            // Render UI
            if (appointments.Count == 0)
            {
                EmptyState.Visibility = Visibility.Visible;
                ResultCount.Text = "No appointments found for this filter.";
                return;
            }
            EmptyState.Visibility = Visibility.Collapsed;
            ResultCount.Text = $"Showing 1-{appointments.Count} of {appointments.Count} appointments";

            int index = 0;
            foreach (var apt in appointments)
            {
                AppointmentList.Children.Add(CreateRow(apt, index));
                index++;
            }
        }

        private Border CreateRow(AppointmentItem apt, int index)
        {
            Border row = new Border
            {
                Height = 55,
                Background = index % 2 == 0 ? Brushes.White : new SolidColorBrush(Color.FromRgb(0xF9, 0xFA, 0xFB)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xF3, 0xF4, 0xF6)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(15, 0, 15, 0)
            };

            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(95, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60, GridUnitType.Pixel) });

            grid.Children.Add(CreateCell(apt.AppointmentId, "#6B7280", FontWeights.Medium, 0));
            grid.Children.Add(CreateCell(apt.PatientName, "#111827", FontWeights.SemiBold, 1));
            grid.Children.Add(CreateCell(apt.Dentist, "#4B5563", FontWeights.Normal, 2));
            grid.Children.Add(CreateCell(apt.Service, "#4B5563", FontWeights.Normal, 3));
            grid.Children.Add(CreateCell(FormatDate(apt.Date) + "  " + apt.Time, "#4B5563", FontWeights.Normal, 4));
            grid.Children.Add(CreateStatusBadge(apt.Status, 5));

            // Action Icons
            StackPanel actions = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
            actions.Children.Add(CreateIconButton("&#xE70F;", "#6B7280", apt, false));
            actions.Children.Add(CreateIconButton("&#xE74D;", "#EF4444", apt, true));
            Grid.SetColumn(actions, 6);
            grid.Children.Add(actions);

            row.Child = grid;
            return row;
        }

        // --- UI Helpers ---
        private TextBlock CreateCell(string text, string hexColor, FontWeight weight, int col)
        {
            TextBlock tb = new TextBlock { Text = text, FontSize = 13, FontWeight = weight, Foreground = GetColor(hexColor), VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(tb, col); return tb;
        }

        private Border CreateStatusBadge(string status, int col)
        {
            SolidColorBrush bg = Brushes.White, fg = Brushes.Black;
            if (status == "Confirmed") { bg = new SolidColorBrush(Color.FromRgb(0xD1, 0xFA, 0xE5)); fg = new SolidColorBrush(Color.FromRgb(0x06, 0x5F, 0x46)); }
            else if (status == "Cancelled") { bg = new SolidColorBrush(Color.FromRgb(0xFE, 0xE2, 0xE2)); fg = new SolidColorBrush(Color.FromRgb(0x99, 0x1B, 0x1B)); }
            Border badge = new Border { Background = bg, CornerRadius = new CornerRadius(12), Padding = new Thickness(10, 4, 10, 4), HorizontalAlignment = HorizontalAlignment.Left };
            badge.Child = new TextBlock { Text = status, FontSize = 12, FontWeight = FontWeights.Medium, Foreground = fg };
            Grid.SetColumn(badge, col); return badge;
        }

        private Button CreateIconButton(string iconGlyph, string hexColor, AppointmentItem apt, bool isDelete)
        {
            TextBlock icon = new TextBlock { Text = iconGlyph, FontFamily = new FontFamily("Segoe MDL2 Assets"), FontSize = 14, Foreground = GetColor(hexColor), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            Button btn = new Button { Content = icon, Background = Brushes.Transparent, BorderThickness = new Thickness(0), Cursor = Cursors.Hand, Width = 32, Height = 32, Padding = new Thickness(0), Margin = new Thickness(2, 0, 2, 0) };
            if (!isDelete) btn.Click += (s, e) => EditAppointment_Click(apt);
            else btn.Click += (s, e) => RemoveAppointment_Click(apt);
            btn.MouseEnter += (s, e) => btn.Background = new SolidColorBrush(Color.FromRgb(0xF3, 0xF4, 0xF6));
            btn.MouseLeave += (s, e) => btn.Background = Brushes.Transparent;
            return btn;
        }

        private SolidColorBrush GetColor(string hex)
        {
            try { byte r = byte.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber); byte g = byte.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber); byte b = byte.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber); return new SolidColorBrush(Color.FromRgb(r, g, b)); } catch { return Brushes.Black; }
        }
        private string FormatDate(string d) { if (DateTime.TryParse(d, out DateTime dt)) return dt.ToString("MMM dd, yyyy"); return d; }

        // --- Events ---
        private void AddAppointment_Click(object sender, RoutedEventArgs e) { if (new AppointmentFormWindow().ShowDialog() == true) LoadAppointments(); }
        private void EditAppointment_Click(AppointmentItem apt) { if (new AppointmentFormWindow(apt).ShowDialog() == true) LoadAppointments(); }
        private void RemoveAppointment_Click(AppointmentItem apt)
        {
            if (MessageBox.Show($"Delete appointment {apt.AppointmentId}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            { _dbContext.Appointments.Remove(apt); _dbContext.SaveChanges(); LoadAppointments(); }
        }

        private void Filter_Click(object sender, MouseButtonEventArgs e)
        {
            Border clickedTab = sender as Border;
            if (clickedTab != null)
            {
                currentFilter = clickedTab.Tag.ToString();
                UpdateFilterUI();
                LoadAppointments();
            }
        }

        private void UpdateFilterUI()
        {
            ResetTabStyle(TabAll); ResetTabStyle(TabConfirmed); ResetTabStyle(TabCancelled); ResetTabStyle(TabToday);
            Border activeTab = null;
            if (currentFilter == "All") activeTab = TabAll;
            else if (currentFilter == "Confirmed") activeTab = TabConfirmed;
            else if (currentFilter == "Cancelled") activeTab = TabCancelled;
            else if (currentFilter == "Today") activeTab = TabToday;
            if (activeTab != null) { activeTab.BorderBrush = new SolidColorBrush(Color.FromRgb(0x00, 0x6B, 0x6B)); activeTab.BorderThickness = new Thickness(0, 0, 0, 2); TextBlock txt = (TextBlock)activeTab.Child; txt.FontWeight = FontWeights.SemiBold; txt.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x6B, 0x6B)); }
        }

        private void ResetTabStyle(Border tab) { tab.BorderBrush = Brushes.Transparent; tab.BorderThickness = new Thickness(0); TextBlock txt = (TextBlock)tab.Child; txt.FontWeight = FontWeights.Medium; txt.Foreground = new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80)); }

        // --- Search ---
        private void SearchBox_GotFocus(object sender, RoutedEventArgs e) { if (SearchBox.Text == "Search patient...") { SearchBox.Text = ""; SearchBox.Foreground = Brushes.Black; } }
        private void SearchBox_LostFocus(object sender, RoutedEventArgs e) { if (string.IsNullOrWhiteSpace(SearchBox.Text)) { SearchBox.Text = "Search patient..."; SearchBox.Foreground = Brushes.Gray; currentSearch = ""; LoadAppointments(); } }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) { if (SearchBox.Text != "Search patient...") { currentSearch = SearchBox.Text; LoadAppointments(); } }
    }
}