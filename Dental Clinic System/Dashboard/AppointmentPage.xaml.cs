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
            if (!_dbContext.Appointments.Any())
            {
                using (var db = new AppDbContext())
                {
                    string todayStr = DateTime.Today.ToString("yyyy-MM-dd");
                    var services = db.Services.Select(s => s.Name).ToList();
                    var dentists = db.Staff.Where(s => s.Role == "Dentist").Select(s => s.Name).ToList();
                    if (!services.Any() || !dentists.Any()) return;

                    List<string> validTimes = new List<string>();
                    for (int hour = 8; hour <= 17; hour++)
                        validTimes.Add(DateTime.Today.AddHours(hour).ToString("hh:mm tt"));

                    Random rnd = new Random();
                    _dbContext.Appointments.AddRange(new List<AppointmentItem>
                    {
                        new AppointmentItem { AppointmentId = "APT001", PatientName = "John Doe",     Dentist = dentists[rnd.Next(dentists.Count)], Service = services[rnd.Next(services.Count)], Date = todayStr,      Time = validTimes[0], Status = "Confirmed" },
                        new AppointmentItem { AppointmentId = "APT002", PatientName = "Jane Wilson",  Dentist = dentists[rnd.Next(dentists.Count)], Service = services[rnd.Next(services.Count)], Date = todayStr,      Time = validTimes[1], Status = "Confirmed" },
                        new AppointmentItem { AppointmentId = "APT003", PatientName = "Mark Sanchez", Dentist = dentists[rnd.Next(dentists.Count)], Service = services[rnd.Next(services.Count)], Date = todayStr,      Time = validTimes[2], Status = "Confirmed" },
                        new AppointmentItem { AppointmentId = "APT004", PatientName = "Emily Davis",  Dentist = dentists[rnd.Next(dentists.Count)], Service = services[rnd.Next(services.Count)], Date = "2025-02-05",  Time = validTimes[3], Status = "Cancelled" },
                        new AppointmentItem { AppointmentId = "APT005", PatientName = "Chris Brown",  Dentist = dentists[rnd.Next(dentists.Count)], Service = services[rnd.Next(services.Count)], Date = "2025-02-06",  Time = validTimes[4], Status = "Cancelled" }
                    });
                }
                _dbContext.SaveChanges();
            }
        }

        private void LoadAppointments()
        {
            _dbContext.Dispose();
            _dbContext = new AppDbContext();

            AppointmentList.Children.Clear();
            var appointments = _dbContext.Appointments.ToList();

            if (currentFilter == "Confirmed")
                appointments = appointments.Where(a => a.Status == "Confirmed").ToList();
            else if (currentFilter == "Cancelled")
                appointments = appointments.Where(a => a.Status == "Cancelled").ToList();
            else if (currentFilter == "Today")
            {
                string todayStr = DateTime.Today.ToString("yyyy-MM-dd");
                appointments = appointments.Where(a => a.Date == todayStr).ToList();
            }

            if (!string.IsNullOrEmpty(currentSearch) && currentSearch != "Search patient...")
                appointments = appointments.Where(a => a.PatientName.ToLower().Contains(currentSearch.ToLower())).ToList();

            if (appointments.Count == 0)
            {
                EmptyState.Visibility = Visibility.Visible;
                ResultCount.Text = "No appointments found.";
                return;
            }

            EmptyState.Visibility = Visibility.Collapsed;
            ResultCount.Text = $"Showing {appointments.Count} appointment{(appointments.Count != 1 ? "s" : "")}";

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
                MinHeight = 55,
                Background = index % 2 == 0 ? Brushes.White : new SolidColorBrush(Color.FromRgb(0xF9, 0xFA, 0xFB)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xF3, 0xF4, 0xF6)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(15, 0, 15, 0)
            };

            // Hover highlight
            row.MouseEnter += (s, e) => row.Background = new SolidColorBrush(Color.FromRgb(0xF0, 0xFD, 0xFA));
            row.MouseLeave += (s, e) => row.Background = index % 2 == 0
                ? Brushes.White
                : new SolidColorBrush(Color.FromRgb(0xF9, 0xFA, 0xFB));

            Grid grid = new Grid { VerticalAlignment = VerticalAlignment.Center };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(145, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(95, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90, GridUnitType.Pixel) });

            grid.Children.Add(CreateCell(apt.AppointmentId, "#6B7280", FontWeights.Normal, 0));
            grid.Children.Add(CreateCell(apt.PatientName, "#111827", FontWeights.SemiBold, 1));
            grid.Children.Add(CreateCell(apt.Dentist, "#4B5563", FontWeights.Normal, 2));
            grid.Children.Add(CreateCell(apt.Service, "#4B5563", FontWeights.Normal, 3));
            grid.Children.Add(CreateCell(FormatDate(apt.Date) + "  " + apt.Time, "#4B5563", FontWeights.Normal, 4));
            grid.Children.Add(CreateStatusBadge(apt.Status, 5));
            grid.Children.Add(CreateActionButtons(apt, 6));

            row.Child = grid;
            return row;
        }

        // ─────────────────────────────────────────────────────────────
        // IMPROVED ACTION BUTTONS — pill-shaped labeled buttons
        // ─────────────────────────────────────────────────────────────
        private StackPanel CreateActionButtons(AppointmentItem apt, int col)
        {
            StackPanel panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // ── EDIT BUTTON ──
            Border editBtn = CreatePillButton(
                icon: "✏",
                label: "Edit",
                normalBg: Color.FromRgb(0xDB, 0xEA, 0xFE),  // light blue
                hoverBg: Color.FromRgb(0xBF, 0xDB, 0xFE),
                textColor: Color.FromRgb(0x1D, 0x4E, 0xD8)   // dark blue
            );
            editBtn.Margin = new Thickness(0, 0, 6, 0);
            editBtn.MouseLeftButtonUp += (s, e) =>
            {
                e.Handled = true;
                EditAppointment_Click(apt);
            };

            // ── DELETE BUTTON ──
            Border delBtn = CreatePillButton(
                icon: "🗑",
                label: "Delete",
                normalBg: Color.FromRgb(0xFE, 0xE2, 0xE2),  // light red
                hoverBg: Color.FromRgb(0xFE, 0xCA, 0xCA),
                textColor: Color.FromRgb(0x99, 0x1B, 0x1B)   // dark red
            );
            delBtn.MouseLeftButtonUp += (s, e) =>
            {
                e.Handled = true;
                RemoveAppointment_Click(apt);
            };

            panel.Children.Add(editBtn);
            panel.Children.Add(delBtn);

            Grid.SetColumn(panel, col);
            return panel;
        }

        /// <summary>
        /// Creates a small pill-shaped button with an icon and text label.
        /// </summary>
        private Border CreatePillButton(string icon, string label,
            Color normalBg, Color hoverBg, Color textColor)
        {
            var bg = new SolidColorBrush(normalBg);
            var hover = new SolidColorBrush(hoverBg);
            var fg = new SolidColorBrush(textColor);

            StackPanel content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };

            content.Children.Add(new TextBlock
            {
                Text = icon,
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 4, 0)
            });
            content.Children.Add(new TextBlock
            {
                Text = label,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = fg,
                VerticalAlignment = VerticalAlignment.Center
            });

            Border pill = new Border
            {
                Background = bg,
                CornerRadius = new CornerRadius(20),
                Padding = new Thickness(10, 4, 10, 4),
                Cursor = Cursors.Hand,
                Child = content
            };

            pill.MouseEnter += (s, e) => pill.Background = hover;
            pill.MouseLeave += (s, e) => pill.Background = bg;

            return pill;
        }

        // ─────────────────────────────────────────────────────────────
        // UI HELPERS
        // ─────────────────────────────────────────────────────────────
        private TextBlock CreateCell(string text, string hexColor, FontWeight weight, int col)
        {
            var tb = new TextBlock
            {
                Text = text,
                FontSize = 13,
                FontWeight = weight,
                Foreground = GetColor(hexColor),
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            Grid.SetColumn(tb, col);
            return tb;
        }

        private Border CreateStatusBadge(string status, int col)
        {
            SolidColorBrush bg = Brushes.White, fg = Brushes.Black;
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
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };
            badge.Child = new TextBlock
            {
                Text = status,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = fg
            };
            Grid.SetColumn(badge, col);
            return badge;
        }

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

        private string FormatDate(string d)
        {
            if (DateTime.TryParse(d, out DateTime dt)) return dt.ToString("MMM dd, yyyy");
            return d;
        }

        // ─────────────────────────────────────────────────────────────
        // EVENTS
        // ─────────────────────────────────────────────────────────────
        private void AddAppointment_Click(object sender, RoutedEventArgs e)
        {
            if (new AppointmentFormWindow().ShowDialog() == true) LoadAppointments();
        }

        private void EditAppointment_Click(AppointmentItem apt)
        {
            if (new AppointmentFormWindow(apt).ShowDialog() == true) LoadAppointments();
        }

        private void RemoveAppointment_Click(AppointmentItem apt)
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete appointment {apt.AppointmentId}?\n\n" +
                $"Patient : {apt.PatientName}\n" +
                $"Date    : {FormatDate(apt.Date)}  {apt.Time}\n" +
                $"Dentist : {apt.Dentist}\n\n" +
                $"This action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _dbContext.Appointments.Remove(apt);
                _dbContext.SaveChanges();
                LoadAppointments();
            }
        }

        private void Filter_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border clickedTab)
            {
                currentFilter = clickedTab.Tag.ToString();
                UpdateFilterUI();
                LoadAppointments();
            }
        }

        private void UpdateFilterUI()
        {
            ResetTabStyle(TabAll);
            ResetTabStyle(TabConfirmed);
            ResetTabStyle(TabCancelled);
            ResetTabStyle(TabToday);

            Border activeTab = currentFilter switch
            {
                "All" => TabAll,
                "Confirmed" => TabConfirmed,
                "Cancelled" => TabCancelled,
                "Today" => TabToday,
                _ => TabAll
            };

            activeTab.BorderBrush = new SolidColorBrush(Color.FromRgb(0x00, 0x6B, 0x6B));
            activeTab.BorderThickness = new Thickness(0, 0, 0, 2);
            if (activeTab.Child is TextBlock activeTxt)
            {
                activeTxt.FontWeight = FontWeights.SemiBold;
                activeTxt.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x6B, 0x6B));
            }
        }

        private void ResetTabStyle(Border tab)
        {
            tab.BorderBrush = Brushes.Transparent;
            tab.BorderThickness = new Thickness(0);
            if (tab.Child is TextBlock txt)
            {
                txt.FontWeight = FontWeights.Medium;
                txt.Foreground = new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80));
            }
        }

        // Search
        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Search patient...")
            { SearchBox.Text = ""; SearchBox.Foreground = Brushes.Black; }
        }
        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            { SearchBox.Text = "Search patient..."; SearchBox.Foreground = Brushes.Gray; currentSearch = ""; LoadAppointments(); }
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchBox.Text != "Search patient...") { currentSearch = SearchBox.Text; LoadAppointments(); }
        }
    }
}