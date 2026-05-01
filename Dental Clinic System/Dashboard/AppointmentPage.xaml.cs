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
        private string currentFilter = "All"; // Tracks the active tab

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
            if (!_dbContext.Appointments.Any())
            {
                // Adding some "Today" data for testing the filter
                string todayStr = DateTime.Today.ToString("yyyy-MM-dd");

                _dbContext.Appointments.AddRange(new List<AppointmentItem>
                {

                });
                _dbContext.SaveChanges();
            }
        }

        // ================= FILTERING LOGIC =================
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
            // Reset all tabs to inactive style
            ResetTabStyle(TabAll);
            ResetTabStyle(TabConfirmed);
            ResetTabStyle(TabCancelled);
            ResetTabStyle(TabToday);

            // Set active tab style
            Border activeTab = null;
            if (currentFilter == "All") activeTab = TabAll;
            else if (currentFilter == "Confirmed") activeTab = TabConfirmed;
            else if (currentFilter == "Cancelled") activeTab = TabCancelled;
            else if (currentFilter == "Today") activeTab = TabToday;

            if (activeTab != null)
            {
                activeTab.BorderBrush = new SolidColorBrush(Color.FromRgb(0x00, 0x6B, 0x6B));
                activeTab.BorderThickness = new Thickness(0, 0, 0, 2);
                TextBlock txt = (TextBlock)activeTab.Child;
                txt.FontWeight = FontWeights.SemiBold;
                txt.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x6B, 0x6B));
            }
        }

        private void ResetTabStyle(Border tab)
        {
            tab.BorderBrush = Brushes.Transparent;
            tab.BorderThickness = new Thickness(0);
            TextBlock txt = (TextBlock)tab.Child;
            txt.FontWeight = FontWeights.Medium;
            txt.Foreground = new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80));
        }

        // ================= LOAD DATA =================
        private void LoadAppointments()
        {
            AppointmentList.Children.Clear();
            var appointments = _dbContext.Appointments.ToList();

            // Apply Filter based on selected tab
            if (currentFilter == "Confirmed")
            {
                appointments = appointments.Where(a => a.Status == "Confirmed").ToList();
            }
            else if (currentFilter == "Cancelled")
            {
                appointments = appointments.Where(a => a.Status == "Cancelled").ToList();
            }
            else if (currentFilter == "Today")
            {
                string todayStr = DateTime.Today.ToString("yyyy-MM-dd");
                appointments = appointments.Where(a => a.Date == todayStr).ToList();
            }

            if (appointments.Count == 0)
            {
                EmptyState.Visibility = Visibility.Visible;
                return;
            }
            EmptyState.Visibility = Visibility.Collapsed;

            int index = 0;
            foreach (var apt in appointments)
            {
                AppointmentList.Children.Add(CreateRow(apt, index));
                index++;
            }
        }

        // ================= UI GENERATION =================
        private Border CreateRow(AppointmentItem apt, int index)
        {
            Border row = new Border
            {
                Height = 55,
                Background = index % 2 == 0 ? Brushes.White : new SolidColorBrush(Color.FromRgb(0xF9, 0xFA, 0xFB)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xF3, 0xF4, 0xF6)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(24, 0, 24, 0)
            };

            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(190, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80, GridUnitType.Pixel) });

            grid.Children.Add(CreateCell(apt.AppointmentId, "#6B7280", FontWeights.Medium, 0));
            grid.Children.Add(CreateCell(apt.PatientName, "#111827", FontWeights.SemiBold, 1));
            grid.Children.Add(CreateCell(apt.Dentist, "#4B5563", FontWeights.Normal, 2));
            grid.Children.Add(CreateCell(apt.Service, "#4B5563", FontWeights.Normal, 3));

            string dateTimeStr = FormatDate(apt.Date) + "  " + apt.Time;
            grid.Children.Add(CreateCell(dateTimeStr, "#4B5563", FontWeights.Normal, 4));
            grid.Children.Add(CreateStatusBadge(apt.Status, 5));

            StackPanel actions = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
            actions.Children.Add(CreateIconButton("&#xE70F;", "#6B7280", apt)); // Edit
            actions.Children.Add(CreateIconButton("&#xE74D;", "#EF4444", apt)); // Delete
            Grid.SetColumn(actions, 6);
            grid.Children.Add(actions);

            row.Child = grid;
            return row;
        }

        private Button CreateIconButton(string iconGlyph, string hexColor, AppointmentItem apt)
        {
            TextBlock icon = new TextBlock { Text = iconGlyph, FontFamily = new FontFamily("Segoe MDL2 Assets"), FontSize = 14, Foreground = GetColorFromHex(hexColor), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            Button btn = new Button { Content = icon, Background = Brushes.Transparent, BorderThickness = new Thickness(0), Cursor = Cursors.Hand, Width = 32, Height = 32, Padding = new Thickness(0), Margin = new Thickness(2, 0, 2, 0) };

            if (hexColor == "#6B7280") btn.Click += (s, e) => EditAppointment_Click(apt);
            else btn.Click += (s, e) => RemoveAppointment_Click(apt);

            btn.MouseEnter += (s, e) => btn.Background = new SolidColorBrush(Color.FromRgb(0xF3, 0xF4, 0xF6));
            btn.MouseLeave += (s, e) => btn.Background = Brushes.Transparent;
            return btn;
        }

        private TextBlock CreateCell(string text, string hexColor, FontWeight weight, int col)
        {
            TextBlock tb = new TextBlock { Text = text, FontSize = 13, FontWeight = weight, Foreground = GetColorFromHex(hexColor), VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(tb, col);
            return tb;
        }

        private Border CreateStatusBadge(string status, int col)
        {
            SolidColorBrush bg = Brushes.White, fg = Brushes.Black;
            if (status == "Confirmed") { bg = new SolidColorBrush(Color.FromRgb(0xD1, 0xFA, 0xE5)); fg = new SolidColorBrush(Color.FromRgb(0x06, 0x5F, 0x46)); }
            else if (status == "Cancelled") { bg = new SolidColorBrush(Color.FromRgb(0xFE, 0xE2, 0xE2)); fg = new SolidColorBrush(Color.FromRgb(0x99, 0x1B, 0x1B)); }

            Border badge = new Border { Background = bg, CornerRadius = new CornerRadius(12), Padding = new Thickness(10, 4, 10, 4), HorizontalAlignment = HorizontalAlignment.Left };
            badge.Child = new TextBlock { Text = status, FontSize = 12, FontWeight = FontWeights.Medium, Foreground = fg };
            Grid.SetColumn(badge, col);
            return badge;
        }

        private SolidColorBrush GetColorFromHex(string hex)
        {
            try { byte r = byte.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber); byte g = byte.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber); byte b = byte.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber); return new SolidColorBrush(Color.FromRgb(r, g, b)); }
            catch { return Brushes.Black; }
        }

        private string FormatDate(string dateStr) { if (DateTime.TryParse(dateStr, out DateTime date)) return date.ToString("MMM dd, yyyy"); return dateStr; }

        // ================= ACTIONS =================
        private void AddAppointment_Click(object sender, RoutedEventArgs e) { if (new AppointmentFormWindow().ShowDialog() == true) LoadAppointments(); }
        private void EditAppointment_Click(AppointmentItem apt) { if (new AppointmentFormWindow(apt).ShowDialog() == true) LoadAppointments(); }
        private void RemoveAppointment_Click(AppointmentItem apt)
        {
            if (MessageBox.Show($"Delete appointment {apt.AppointmentId}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            { _dbContext.Appointments.Remove(apt); _dbContext.SaveChanges(); LoadAppointments(); }
        }
    }
}