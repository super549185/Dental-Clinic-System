using System;
using System.Collections.Generic;
using System.IO;
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
    public partial class StaffPage : Page
    {
        private AppDbContext _dbContext;
        private string currentSearch = "";

        public StaffPage()
        {
            InitializeComponent();
            _dbContext = new AppDbContext();
            _dbContext.Database.EnsureCreated();
            SeedData();
            LoadStaff();
        }

        private void SeedData()
        {
            if (!_dbContext.Staff.Any())
            {
                _dbContext.Staff.AddRange(new List<StaffItem>
                {
                    new StaffItem { Name = "Dr. Sarah Smith", Role = "Dentist", Specialization = "General Dentistry", Status = "On Duty", Schedule = "Mon-Fri 9:00 AM - 5:00 PM", PatientLoad = 24, TotalAppointments = 156, ServicesOffered = "Cleaning, Checkup, Extraction", ImageData = null },
                    new StaffItem { Name = "Dr. Michael Johnson", Role = "Dentist", Specialization = "Orthodontics", Status = "On Duty", Schedule = "Mon-Thu 10:00 AM - 6:00 PM", PatientLoad = 18, TotalAppointments = 142, ServicesOffered = "Braces Adjustment, Retainer", ImageData = null },
                    new StaffItem { Name = "Dr. Emily Lee", Role = "Dentist", Specialization = "Endodontics", Status = "On Duty", Schedule = "Tue-Sat 9:00 AM - 5:00 PM", PatientLoad = 15, TotalAppointments = 98, ServicesOffered = "Root Canal, Apicoectomy", ImageData = null },
                    new StaffItem { Name = "Dr. David Martinez", Role = "Dentist", Specialization = "Oral Surgery", Status = "Off Duty", Schedule = "Mon-Fri 8:00 AM - 4:00 PM", PatientLoad = 12, TotalAppointments = 87, ServicesOffered = "Tooth Extraction, Implants", ImageData = null }
                });
                _dbContext.SaveChanges();
            }
        }

        private void LoadStaff()
        {
            _dbContext.Dispose(); // Clear cache
            _dbContext = new AppDbContext(); // Fresh connection

            StaffCardsPanel.Children.Clear();
            var staff = _dbContext.Staff.ToList();

            if (!string.IsNullOrEmpty(currentSearch) && currentSearch != "Search staff...")
                staff = staff.Where(s => s.Name.ToLower().Contains(currentSearch.ToLower())).ToList();

            foreach (var s in staff) StaffCardsPanel.Children.Add(CreateCard(s));
        }

        private Border CreateCard(StaffItem s)
        {
            // 1. Create the Card Border
            Border card = new Border
            {
                Width = 280,
                Height = 360,
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xE5, 0xE7, 0xEB)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(0, 0, 15, 15),
                Cursor = Cursors.Hand
            };

            // 2. Create the Content StackPanel FIRST
            StackPanel sp = new StackPanel();

            // Add Image to StackPanel
            Border imgBorder = new Border { Height = 180, CornerRadius = new CornerRadius(10, 10, 0, 0), Background = new SolidColorBrush(Color.FromRgb(0xF3, 0xF4, 0xF6)) };
            System.Windows.Controls.Image img = new System.Windows.Controls.Image { Stretch = Stretch.UniformToFill, ClipToBounds = true };
            img.Source = s.ImageData != null ? GetImageFromBytes(s.ImageData) : new BitmapImage(new Uri("pack://application:,,,/Images/default-avatar.png", UriKind.Absolute));
            imgBorder.Child = img;
            sp.Children.Add(imgBorder);

            // Add Info to StackPanel
            StackPanel info = new StackPanel { Margin = new Thickness(15, 15, 15, 0) };
            info.Children.Add(new TextBlock { Text = s.Name, FontSize = 15, FontWeight = FontWeights.Bold, Foreground = GetColor("#111827") });
            info.Children.Add(new TextBlock { Text = s.Specialization, FontSize = 12, Foreground = GetColor("#6B7280"), Margin = new Thickness(0, 3, 0, 10) });

            string currentStatus = StaffHelper.GetDynamicStatus(s); // Dynamic check!
            Border badge = new Border { HorizontalAlignment = HorizontalAlignment.Left, CornerRadius = new CornerRadius(12), Padding = new Thickness(10, 3, 10, 3), Background = currentStatus == "On Duty" ? GetColor("#D1FAE5") : GetColor("#F3F4F6") };
            badge.Child = new TextBlock { Text = currentStatus, FontSize = 11, FontWeight = FontWeights.Medium, Foreground = currentStatus == "On Duty" ? GetColor("#065F46") : GetColor("#6B7280") };
            info.Children.Add(badge);

            Grid stats = new Grid { Margin = new Thickness(0, 15, 0, 0) };
            stats.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            stats.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            stats.Children.Add(new TextBlock { Text = $"Load: {s.PatientLoad}", FontSize = 12, Foreground = GetColor("#4B5563") });
            TextBlock apps = new TextBlock { Text = $"Apps: {s.TotalAppointments}", FontSize = 12, Foreground = GetColor("#4B5563"), HorizontalAlignment = HorizontalAlignment.Right };
            Grid.SetColumn(apps, 1); stats.Children.Add(apps);
            info.Children.Add(stats);

            sp.Children.Add(info);

            // 3. Create Delete Button
            Button delBtn = new Button
            {
                Content = "✕",
                Background = new SolidColorBrush(Color.FromRgb(0xFE, 0xE2, 0xE2)),
                Foreground = new SolidColorBrush(Color.FromRgb(0x99, 0x1B, 0x1B)),
                BorderThickness = new Thickness(0),
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Cursor = Cursors.Hand,
                Width = 24,
                Height = 24,
                Padding = new Thickness(0, 0, 0, 0)
            };
            delBtn.Click += (sender, e) => DeleteStaff(s);
            delBtn.MouseEnter += (sender, e) => delBtn.Background = new SolidColorBrush(Color.FromRgb(0xF8, 0x71, 0x71));
            delBtn.MouseLeave += (sender, e) => delBtn.Background = new SolidColorBrush(Color.FromRgb(0xFE, 0xE2, 0xE2));
            delBtn.HorizontalAlignment = HorizontalAlignment.Right;
            delBtn.VerticalAlignment = VerticalAlignment.Top;
            delBtn.Margin = new Thickness(0, 5, 5, 0);

            // Edit Button
            Button editBtn = new Button
            {
                Content = "✏️",
                Background = new SolidColorBrush(Color.FromRgb(0xDB, 0xEA, 0xFE)),
                Foreground = new SolidColorBrush(Color.FromRgb(0x1D, 0x4E, 0xD8)),
                BorderThickness = new Thickness(0),
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Cursor = Cursors.Hand,
                Width = 24,
                Height = 24,
                Padding = new Thickness(0, 0, 0, 0)
            };
            editBtn.Click += (sender, e) => EditStaff(s);
            editBtn.MouseEnter += (sender, e) => editBtn.Background = new SolidColorBrush(Color.FromRgb(0xBF, 0xDB, 0xFE));
            editBtn.MouseLeave += (sender, e) => editBtn.Background = new SolidColorBrush(Color.FromRgb(0xDB, 0xEA, 0xFE));
            editBtn.HorizontalAlignment = HorizontalAlignment.Right;
            editBtn.VerticalAlignment = VerticalAlignment.Top;
            editBtn.Margin = new Thickness(0, 5, 30, 0); // Offset to not overlap delete button

            // Wrap everything in a Grid overlay
            Grid cardGrid = new Grid();
            cardGrid.Children.Add(sp);
            cardGrid.Children.Add(delBtn);
            cardGrid.Children.Add(editBtn);

            // 5. Assign Grid to Card and Events
            card.Child = cardGrid; // ONLY set this once at the very end

            card.MouseEnter += (sender, e) => card.BorderBrush = new SolidColorBrush(Color.FromRgb(0x00, 0x6B, 0x6B));
            card.MouseLeave += (sender, e) => card.BorderBrush = new SolidColorBrush(Color.FromRgb(0xE5, 0xE7, 0xEB));
            card.MouseLeftButtonUp += (sender, e) => new StaffProfileWindow(s).ShowDialog();

            return card;
        }

        // --- Image Helpers ---
        public static BitmapImage GetImageFromBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return null;
            using (var ms = new MemoryStream(bytes))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                // Image is already resized to 300x300 during upload, so we just load it directly!
                image.StreamSource = ms;
                image.EndInit();
                image.Freeze();
                return image;
            }
        }

        public static byte[] ResizeImageToBytes(string filePath, int maxW = 300, int maxH = 300)
        {
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource = new Uri(filePath);
            bmp.EndInit();

            double scale = Math.Min((double)maxW / bmp.PixelWidth, (double)maxH / bmp.PixelHeight);
            TransformedBitmap tb = new TransformedBitmap(bmp, new ScaleTransform(scale, scale));
            PngBitmapEncoder enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(tb));

            using (var ms = new MemoryStream())
            {
                enc.Save(ms);
                return ms.ToArray();
            }
        }

        // --- Safe Color Helper (Replaces .ToColor() extension to avoid namespace errors) ---
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

        // --- Search ---
        private void SearchBox_GotFocus(object sender, RoutedEventArgs e) { if (SearchBox.Text == "Search staff...") { SearchBox.Text = ""; SearchBox.Foreground = Brushes.Black; } }
        private void SearchBox_LostFocus(object sender, RoutedEventArgs e) { if (string.IsNullOrWhiteSpace(SearchBox.Text)) { SearchBox.Text = "Search staff..."; SearchBox.Foreground = Brushes.Gray; currentSearch = ""; LoadStaff(); } }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) { if (SearchBox.Text != "Search staff...") { currentSearch = SearchBox.Text; LoadStaff(); } }
        private void DeleteStaff(StaffItem staff)
        {
            MessageBoxResult result = MessageBox.Show($"Are you sure you want to remove {staff.Name}?\n\n(This will NOT delete their past appointments)", "Confirm Remove", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _dbContext.Staff.Remove(staff);
                _dbContext.SaveChanges();
                LoadStaff(); // Refresh UI
            }
        }
        private void AddStaff_Click(object sender, RoutedEventArgs e) { if (new AddStaffWindow().ShowDialog() == true) LoadStaff(); }
        private void EditStaff(StaffItem s)
        {
            if (new EditStaffWindow(s).ShowDialog() == true)
            {
                LoadStaff();
            }
        }
    }

}