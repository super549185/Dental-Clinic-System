using System;
using System.Collections.Generic;
using System.IO; // ★ ADDED FOR SEED FLAG
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
            string appDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Dental_Clinic_System");

            string flagFile = Path.Combine(appDataFolder, "seeded.flag");

            // 1. If flag exists, skip completely (prevents re-seeding after deletions)
            if (File.Exists(flagFile))
                return;

            var seedIds = new[] { "APT001", "APT002", "APT003", "APT004", "APT005" };

            // 2. If these IDs exist (e.g., imported backup), create flag and exit
            if (_dbContext.Appointments.Any(a => seedIds.Contains(a.AppointmentId)))
            {
                Directory.CreateDirectory(appDataFolder);
                File.WriteAllText(flagFile, "Seeded on " + DateTime.Now.ToString());
                return;
            }

            // 3. Fresh DB: Try to seed only if Services and Dentists exist
            string todayStr = DateTime.Today.ToString("yyyy-MM-dd");
            var services = _dbContext.Services.Select(s => s.Name).ToList();
            var dentists = _dbContext.Staff.Where(s => s.Role == "Dentist").Select(s => s.Name).ToList();

            bool canSeed = services.Any() && dentists.Any();

            if (canSeed)
            {
                List<string> validTimes = new List<string>();
                for (int hour = 8; hour <= 17; hour++)
                    validTimes.Add(DateTime.Today.AddHours(hour).ToString("hh:mm tt"));

                Random rnd = new Random();
                _dbContext.Appointments.AddRange(new List<AppointmentItem>
        {
            new AppointmentItem { AppointmentId = "APT001", PatientName = "Gilbert Torres",     Dentist = dentists[rnd.Next(dentists.Count)], Service = services[rnd.Next(services.Count)], Date = todayStr,      Time = validTimes[0], Status = "Confirmed" },
            new AppointmentItem { AppointmentId = "APT002", PatientName = "Richfield Bernaldez",  Dentist = dentists[rnd.Next(dentists.Count)], Service = services[rnd.Next(services.Count)], Date = todayStr,      Time = validTimes[1], Status = "Confirmed" },
            new AppointmentItem { AppointmentId = "APT003", PatientName = "Jerfel Maamo",        Dentist = dentists[rnd.Next(dentists.Count)], Service = services[rnd.Next(services.Count)], Date = todayStr,      Time = validTimes[2], Status = "Confirmed" },
            new AppointmentItem { AppointmentId = "APT004", PatientName = "Angelo Macalibo",     Dentist = dentists[rnd.Next(dentists.Count)], Service = services[rnd.Next(services.Count)], Date = "2026-02-05",  Time = validTimes[3], Status = "Cancelled" },
            new AppointmentItem { AppointmentId = "APT005", PatientName = "Jay-Al Gallenero",    Dentist = dentists[rnd.Next(dentists.Count)], Service = services[rnd.Next(services.Count)], Date = "2026-02-06",  Time = validTimes[4], Status = "Cancelled" }
        });

                _dbContext.SaveChanges();

                
                Directory.CreateDirectory(appDataFolder);
                File.WriteAllText(flagFile, "Seeded on " + DateTime.Now.ToString());
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
                MinHeight = 50,
                Background = index % 2 == 0 ? Brushes.White : new SolidColorBrush(Color.FromRgb(0xF9, 0xFA, 0xFB)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xF3, 0xF4, 0xF6)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(12, 0, 12, 0)
            };

            row.MouseEnter += (s, e) => row.Background = new SolidColorBrush(Color.FromRgb(0xF0, 0xFD, 0xFA));
            row.MouseLeave += (s, e) => row.Background = index % 2 == 0
                ? Brushes.White
                : new SolidColorBrush(Color.FromRgb(0xF9, 0xFA, 0xFB));

            Grid grid = new Grid { VerticalAlignment = VerticalAlignment.Center };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(115, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(95, GridUnitType.Pixel) });

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

        private StackPanel CreateActionButtons(AppointmentItem apt, int col)
        {
            StackPanel panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            if (apt.Status == "Confirmed" && !apt.IsCompleted)
            {
                Button completeBtn = new Button
                {
                    Content = "✓",
                    Background = new SolidColorBrush(Color.FromRgb(0xD1, 0xFA, 0xE5)),
                    Foreground = new SolidColorBrush(Color.FromRgb(0x06, 0x5F, 0x46)),
                    BorderThickness = new Thickness(0),
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Cursor = Cursors.Hand,
                    Width = 28,
                    Height = 28,
                    Padding = new Thickness(0),
                    Margin = new Thickness(1, 0, 1, 0)
                };
                completeBtn.Click += (s, e) => CompleteAppointment_Click(apt);
                completeBtn.MouseEnter += (s, e) => completeBtn.Background = new SolidColorBrush(Color.FromRgb(0xA7, 0xF3, 0xD0));
                completeBtn.MouseLeave += (s, e) => completeBtn.Background = new SolidColorBrush(Color.FromRgb(0xD1, 0xFA, 0xE5));
                panel.Children.Add(completeBtn);
            }

            panel.Children.Add(CreateImageButton("Images/edit.png", apt, isDelete: false));
            panel.Children.Add(CreateImageButton("Images/delete.png", apt, isDelete: true));

            Grid.SetColumn(panel, col);
            return panel;
        }

        private Button CreateImageButton(string imagePath, AppointmentItem apt, bool isDelete)
        {
            System.Windows.Controls.Image img = new System.Windows.Controls.Image
            {
                Width = 14,
                Height = 14,
                Stretch = Stretch.UniformToFill
            };

            object btnContent;
            try
            {
                img.Source = new BitmapImage(new Uri($"pack://application:,,,/{imagePath}", UriKind.Absolute));
                btnContent = img;
            }
            catch
            {
                btnContent = new TextBlock
                {
                    Text = isDelete ? "✕" : "✏",
                    FontSize = 13,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = isDelete ? Brushes.Red : Brushes.SteelBlue
                };
            }

            Button btn = new Button
            {
                Content = btnContent,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                Width = 28,
                Height = 28,
                Padding = new Thickness(0),
                Margin = new Thickness(1, 0, 1, 0)
            };

            Color hoverColor = isDelete
                ? Color.FromRgb(0xFE, 0xE2, 0xE2)
                : Color.FromRgb(0xDB, 0xEA, 0xFE);

            btn.MouseEnter += (s, e) => btn.Background = new SolidColorBrush(hoverColor);
            btn.MouseLeave += (s, e) => btn.Background = Brushes.Transparent;

            if (!isDelete)
                btn.Click += (s, e) => EditAppointment_Click(apt);
            else
                btn.Click += (s, e) => RemoveAppointment_Click(apt);

            return btn;
        }

        private TextBlock CreateCell(string text, string hexColor, FontWeight weight, int col)
        {
            var tb = new TextBlock
            {
                Text = text,
                FontSize = 12,
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
            else if (status == "Completed")
            {
                bg = new SolidColorBrush(Color.FromRgb(0xE5, 0xE7, 0xEB));
                fg = new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80));
            }

            Border badge = new Border
            {
                Background = bg,
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(8, 3, 8, 3),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };
            badge.Child = new TextBlock
            {
                Text = status,
                FontSize = 10,
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

        private void EditAppointment_Click(AppointmentItem apt)
        {
            var detachedCopy = new AppointmentItem
            {
                AppointmentId = apt.AppointmentId,
                PatientName = apt.PatientName,
                Dentist = apt.Dentist,
                Service = apt.Service,
                Date = apt.Date,
                Time = apt.Time,
                Status = apt.Status,
                IsCompleted = apt.IsCompleted,
                CompletionDate = apt.CompletionDate,
                CompletionNotes = apt.CompletionNotes
            };

            if (new AppointmentFormWindow(detachedCopy).ShowDialog() == true)
                LoadAppointments();
        }

        private void AddAppointment_Click(object sender, RoutedEventArgs e)
        {
            if (new AppointmentFormWindow().ShowDialog() == true) LoadAppointments();
        }

        private void CompleteAppointment_Click(AppointmentItem apt)
        {
            Window completionWindow = new Window
            {
                Title = "Complete Appointment",
                Width = 500,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Background = Brushes.White,
                WindowStyle = WindowStyle.SingleBorderWindow,
                ResizeMode = ResizeMode.NoResize
            };

            StackPanel sp = new StackPanel { Margin = new Thickness(20) };

            sp.Children.Add(new TextBlock
            {
                Text = "Complete Appointment",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 15),
                Foreground = new SolidColorBrush(Color.FromRgb(0x11, 0x18, 0x27))
            });

            sp.Children.Add(new TextBlock { Text = "Appointment Details:", FontSize = 12, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 8) });
            sp.Children.Add(new TextBlock { Text = $"Patient: {apt.PatientName}", FontSize = 11, Margin = new Thickness(0, 0, 0, 4), Foreground = new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80)) });
            sp.Children.Add(new TextBlock { Text = $"Dentist: {apt.Dentist}", FontSize = 11, Margin = new Thickness(0, 0, 0, 4), Foreground = new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80)) });
            sp.Children.Add(new TextBlock { Text = $"Service: {apt.Service}", FontSize = 11, Margin = new Thickness(0, 0, 0, 15), Foreground = new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80)) });

            sp.Children.Add(new TextBlock { Text = "Diagnosis:", FontSize = 12, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 8) });
            TextBox diagnosisBox = new TextBox
            {
                Height = 50,
                Padding = new Thickness(10),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xE5, 0xE7, 0xEB)),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 15),
                FontSize = 12
            };
            sp.Children.Add(diagnosisBox);

            sp.Children.Add(new TextBlock { Text = "Treatment Notes:", FontSize = 12, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 8) });
            TextBox notesBox = new TextBox
            {
                Height = 60,
                Padding = new Thickness(10),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xE5, 0xE7, 0xEB)),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 15),
                FontSize = 12,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            sp.Children.Add(notesBox);

            StackPanel buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 15, 0, 0) };

            Button cancelBtn = new Button
            {
                Content = "Cancel",
                Width = 80,
                Height = 35,
                Margin = new Thickness(0, 0, 10, 0),
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xE5, 0xE7, 0xEB)),
                BorderThickness = new Thickness(1),
                FontSize = 12
            };
            cancelBtn.Click += (s, e) => completionWindow.Close();
            buttonPanel.Children.Add(cancelBtn);

            Button completeBtn = new Button
            {
                Content = "✓ Complete",
                Width = 120,
                Height = 35,
                Background = new SolidColorBrush(Color.FromRgb(0x06, 0x5F, 0x46)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand
            };
            completeBtn.Click += (s, e) =>
            {
                CompleteAppointmentAndAddToHistory(apt, diagnosisBox.Text, notesBox.Text);
                completionWindow.Close();
            };
            buttonPanel.Children.Add(completeBtn);

            sp.Children.Add(buttonPanel);

            ScrollViewer scroll = new ScrollViewer { Content = sp, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            completionWindow.Content = scroll;
            completionWindow.ShowDialog();
        }

        private void CompleteAppointmentAndAddToHistory(AppointmentItem apt, string diagnosis, string notes)
        {
            using (var db = new AppDbContext())
            {
                // ★ FIXED: Fetch fresh entity instead of using tracked one
                var trackedEntity = db.Appointments.FirstOrDefault(a => a.AppointmentId == apt.AppointmentId);
                if (trackedEntity != null)
                {
                    trackedEntity.IsCompleted = true;
                    trackedEntity.CompletionDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                    trackedEntity.CompletionNotes = notes;
                    trackedEntity.Status = "Completed";
                }

                var patient = db.Patients.FirstOrDefault(p => p.Name == apt.PatientName);

                var historyEntry = new DentalHistoryItem
                {
                    PatientId = patient?.PatientId ?? "UNKNOWN",
                    PatientName = apt.PatientName,
                    AppointmentDate = apt.Date,
                    AppointmentTime = apt.Time,
                    Service = apt.Service,
                    Dentist = apt.Dentist,
                    TreatmentNotes = notes,
                    Diagnosis = diagnosis,
                    TreatmentOutcome = "Completed",
                    DateCreated = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
                };

                db.DentalHistory.Add(historyEntry);
                db.SaveChanges();
            }

            MessageBox.Show($"✓ Appointment completed!\n\nAdded to {apt.PatientName}'s dental history.",
                "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadAppointments();
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