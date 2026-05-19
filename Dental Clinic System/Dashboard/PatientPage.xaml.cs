using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Dental_Clinic_System.Data;
using Dental_Clinic_System.Models;
using System.Windows.Media.Imaging;
namespace Dental_Clinic_System.Dashboard
{
    public partial class PatientPage : Page
    {
        private AppDbContext _dbContext;
        private string currentSearch = "";

        public PatientPage()
        {
            InitializeComponent();
            _dbContext = new AppDbContext();
            _dbContext.Database.EnsureCreated();
            SeedDatabase();
            LoadPatients();
        }

        private void SeedDatabase()
        {
            if (!_dbContext.Patients.Any())
            {
                _dbContext.Patients.AddRange(new List<PatientItem>
                {
                    new PatientItem { PatientId = "P001", Name = "Gilbert Torres", Contact = "(555) 123-4567", Email = "g.torres.549720@umindanao.edu.ph", LastVisit = "2026-01-15", Status = "Active", DateOfBirth = "1985-06-15", Address = "123 Main St, City", Allergies = "Penicillin", MedicalConditions = "Diabetes", BloodType = "O+" },
                    new PatientItem { PatientId = "P002", Name = "Richfield Bernaldez", Contact = "(555) 987-6543", Email = "r.bernaldez.549185@umindanao.edu.ph", LastVisit = "2026-01-10", Status = "Active", DateOfBirth = "1990-02-20", Address = "456 Oak Ave, Town", Allergies = "None", MedicalConditions = "None", BloodType = "A+" },
                    new PatientItem { PatientId = "P003", Name = "Jerfel Maamo", Contact = "(555) 555-1234", Email = "j.maamo.545112@umindanao.edu.ph", LastVisit = "2025-12-20", Status = "Inactive", DateOfBirth = "1978-11-05", Address = "789 Pine Rd, Village", Allergies = "Sulfa drugs", MedicalConditions = "Hypertension", BloodType = "B-" }
                });
                _dbContext.SaveChanges();
            }
        }

        private void LoadPatients()
        {
            _dbContext.Dispose();
            _dbContext = new AppDbContext();
            _dbContext.Database.EnsureCreated();

            PatientList.Children.Clear();
            // ✅ FILTER: Only show non-archived patients
            var patients = _dbContext.Patients.Where(p => !p.IsArchived).ToList();

            if (!string.IsNullOrEmpty(currentSearch) && currentSearch != "Search patient...")
            {
                patients = patients.Where(p => p.Name.ToLower().Contains(currentSearch.ToLower())).ToList();
            }

            if (patients.Count == 0) { EmptyState.Visibility = Visibility.Visible; return; }
            EmptyState.Visibility = Visibility.Collapsed;

            int index = 0;
            foreach (var p in patients)
            {
                PatientList.Children.Add(CreateRow(p, index));
                index++;
            }
        }

        private Border CreateRow(PatientItem p, int index)
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
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80, GridUnitType.Pixel) });

            grid.Children.Add(CreateCell(p.PatientId, "#6B7280", FontWeights.Medium, 0));
            grid.Children.Add(CreateCell(p.Name, "#111827", FontWeights.SemiBold, 1));
            grid.Children.Add(CreateCell(p.Contact, "#4B5563", FontWeights.Normal, 2));
            grid.Children.Add(CreateCell(p.Email, "#4B5563", FontWeights.Normal, 3));
            grid.Children.Add(CreateCell(FormatDate(p.LastVisit), "#4B5563", FontWeights.Normal, 4));
            grid.Children.Add(CreateStatusBadge(p.Status, 5));

            // Actions (Image Buttons)
            StackPanel actions = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            actions.Children.Add(CreateImageButton("Images/view.png", p, false));
            actions.Children.Add(CreateImageButton("Images/delete.png", p, true));

            Grid.SetColumn(actions, 6);
            grid.Children.Add(actions);

            row.Child = grid;
            return row;
        }

        // --- Helpers ---
        private TextBlock CreateCell(string text, string hex, FontWeight weight, int col)
        {
            TextBlock tb = new TextBlock { Text = text, FontSize = 13, FontWeight = weight, Foreground = GetColor(hex), VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(tb, col); return tb;
        }

        private Border CreateStatusBadge(string status, int col)
        {
            SolidColorBrush bg = Brushes.White, fg = Brushes.Black;
            if (status == "Active") { bg = new SolidColorBrush(Color.FromRgb(0xD1, 0xFA, 0xE5)); fg = new SolidColorBrush(Color.FromRgb(0x06, 0x5F, 0x46)); }
            else if (status == "Inactive") { bg = new SolidColorBrush(Color.FromRgb(0xFE, 0xE2, 0xE2)); fg = new SolidColorBrush(Color.FromRgb(0x99, 0x1B, 0x1B)); }

            Border badge = new Border { Background = bg, CornerRadius = new CornerRadius(12), Padding = new Thickness(10, 4, 10, 4), HorizontalAlignment = HorizontalAlignment.Left };
            badge.Child = new TextBlock { Text = status, FontSize = 12, FontWeight = FontWeights.Medium, Foreground = fg };
            Grid.SetColumn(badge, col); return badge;
        }

        private SolidColorBrush GetColor(string hex) { try { byte r = byte.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber); byte g = byte.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber); byte b = byte.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber); return new SolidColorBrush(Color.FromRgb(r, g, b)); } catch { return Brushes.Black; } }
        private string FormatDate(string d) { if (DateTime.TryParse(d, out DateTime dt)) return dt.ToString("MMM dd, yyyy"); return d; }

        // --- Search ---
        private void SearchBox_GotFocus(object sender, RoutedEventArgs e) { if (SearchBox.Text == "Search patient...") { SearchBox.Text = ""; SearchBox.Foreground = Brushes.Black; } }
        private void SearchBox_LostFocus(object sender, RoutedEventArgs e) { if (string.IsNullOrWhiteSpace(SearchBox.Text)) { SearchBox.Text = "Search patient..."; SearchBox.Foreground = Brushes.Gray; currentSearch = ""; LoadPatients(); } }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) { if (SearchBox.Text != "Search patient...") { currentSearch = SearchBox.Text; LoadPatients(); } }

        // --- Actions ---
        private void AddPatient_Click(object sender, RoutedEventArgs e) { if (new PatientFormWindow().ShowDialog() == true) LoadPatients(); }
        private void ViewProfile_Click(PatientItem p) { new PatientProfileWindow(p).ShowDialog(); LoadPatients(); }

        // ✅ UPDATED: RemovePatient_Click now archives instead of deleting
        private void RemovePatient_Click(PatientItem p)
        {
            var result = MessageBox.Show(
                $"Archive patient: {p.Name}?\n\n" +
                "This will move them to the archived section.\n" +
                "You can restore them later from Settings.",
                "Archive Patient",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                p.IsArchived = true;
                p.ArchiveDate = DateTime.Now.ToString("yyyy-MM-dd");
                p.ArchiveReason = "Deleted from patient list";

                _dbContext.Patients.Update(p);
                _dbContext.SaveChanges();

                MessageBox.Show("Patient archived successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                LoadPatients();
            }
        }

        private Button CreateImageButton(string imagePath, PatientItem p, bool isDelete)
        {
            Image img = new Image
            {
                Width = 18,
                Height = 18,
                Stretch = Stretch.UniformToFill
            };

            try
            {
                img.Source = new BitmapImage(new Uri($"pack://application:,,,/{imagePath}", UriKind.Absolute));
            }
            catch
            {
                // Fallback to text icons if the image file is missing
                var fallbackText = new TextBlock
                {
                    Text = isDelete ? "✕" : "👁",
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = isDelete ? Brushes.Red : Brushes.Gray
                };

                Button fallbackBtn = new Button
                {
                    Content = fallbackText,
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand,
                    Width = 32,
                    Height = 32,
                    Padding = new Thickness(0),
                    Margin = new Thickness(2, 0, 2, 0)
                };

                if (!isDelete) fallbackBtn.Click += (s, e) => ViewProfile_Click(p);
                else fallbackBtn.Click += (s, e) => RemovePatient_Click(p);
                return fallbackBtn;
            }

            Button btn = new Button
            {
                Content = img,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                Width = 34,
                Height = 34,
                Padding = new Thickness(0),
                Margin = new Thickness(2, 0, 2, 0)
            };

            if (!isDelete)
                btn.Click += (s, e) => ViewProfile_Click(p);
            else
                btn.Click += (s, e) => RemovePatient_Click(p);

            // Hover effect
            btn.MouseEnter += (s, e) => btn.Background = new SolidColorBrush(Color.FromRgb(0xF3, 0xF4, 0xF6));
            btn.MouseLeave += (s, e) => btn.Background = Brushes.Transparent;

            return btn;
        }
    }
}