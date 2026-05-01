using System;
using System.Collections.Generic;
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
            LoadTodayData();
        }

        private void LoadTodayData()
        {
            _dbContext.Database.EnsureCreated();

            // Get today's date in the format saved in DB (yyyy-MM-dd)
            string todayStr = DateTime.Today.ToString("yyyy-MM-dd");

            // Query database for today's appointments
            var todayApts = _dbContext.Appointments
                .Where(a => a.Date == todayStr)
                .ToList();

            // Update Stat Card
            StatAppointments.Text = todayApts.Count.ToString();

            // Load Table
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
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110, GridUnitType.Pixel) });

            // Cells
            grid.Children.Add(CreateCell(apt.PatientName, "#111827", FontWeights.Medium, 0));
            grid.Children.Add(CreateCell(apt.Time, "#4B5563", FontWeights.Normal, 1));
            grid.Children.Add(CreateCell(apt.Dentist, "#4B5563", FontWeights.Normal, 2));
            grid.Children.Add(CreateCell(apt.Service, "#4B5563", FontWeights.Normal, 3));

            // Status Badge (Added "In Progress" style just in case)
            grid.Children.Add(CreateDashboardStatusBadge(apt.Status, 4));

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

        private Border CreateDashboardStatusBadge(string status, int col)
        {
            SolidColorBrush bg = Brushes.White;
            SolidColorBrush fg = Brushes.Black;

            if (status == "Confirmed")
            {
                bg = new SolidColorBrush(Color.FromRgb(0xD1, 0xFA, 0xE5)); // Light Green
                fg = new SolidColorBrush(Color.FromRgb(0x06, 0x5F, 0x46)); // Dark Green
            }
            else if (status == "Cancelled")
            {
                bg = new SolidColorBrush(Color.FromRgb(0xFE, 0xE2, 0xE2)); // Light Red
                fg = new SolidColorBrush(Color.FromRgb(0x99, 0x1B, 0x1B)); // Dark Red
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