using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Dental_Clinic_System.Data;
using Dental_Clinic_System.Models;
using System.Windows.Media.Imaging;

namespace Dental_Clinic_System.Dashboard
{
    public partial class AddStaffWindow : Window
    {
        private AppDbContext _dbContext;
        private string _selectedImagePath = null;

        public AddStaffWindow()
        {
            InitializeComponent();
            _dbContext = new AppDbContext();
            GenerateScheduleTimes();
            LoadServices();
        }

        private void ChooseImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*";
            if (dlg.ShowDialog() == true)
            {
                _selectedImagePath = dlg.FileName;
                PreviewImage.Source = new BitmapImage(new Uri(_selectedImagePath));
            }
        }
        private void GenerateScheduleTimes()
        {
            StartTimeBox.Items.Clear();
            EndTimeBox.Items.Clear();
            // Generate 1-hour intervals from 6:00 AM to 9:00 PM
            for (int hour = 6; hour <= 21; hour++)
            {
                string time = DateTime.Today.AddHours(hour).ToString("hh:mm tt");
                StartTimeBox.Items.Add(time);
                EndTimeBox.Items.Add(time);
            }
            // Set smart defaults (e.g., 9:00 AM to 5:00 PM)
            StartTimeBox.SelectedItem = "09:00 AM";
            EndTimeBox.SelectedItem = "05:00 PM";
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.DialogResult = false;

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text)) { MessageBox.Show("Enter staff name.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            try
            {
                byte[] imageData = null;
                if (_selectedImagePath != null)
                {
                    imageData = StaffPage.ResizeImageToBytes(_selectedImagePath, 300, 300);
                }

                _dbContext.Staff.Add(new StaffItem
                {
                    Name = NameBox.Text.Trim(),
                    Role = ((ComboBoxItem)RoleBox.SelectedItem).Content.ToString(),
                    Specialization = SpecBox.Text.Trim(),
                    Status = "On Duty", // Default status
                    Schedule = $"{((ComboBoxItem)DaysBox.SelectedItem).Content} {StartTimeBox.SelectedItem} - {EndTimeBox.SelectedItem}",
                    ServicesOffered = string.Join(", ", ServicesListBox.SelectedItems.Cast<string>()),
                    ImageData = imageData
                });
                _dbContext.SaveChanges();
                this.DialogResult = true;
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }
        private void LoadServices()
        {
            ServicesListBox.Items.Clear();
            using (var db = new AppDbContext())
            {
                foreach (var service in db.Services.ToList())
                {
                    ServicesListBox.Items.Add(service.Name);
                }
            }
        }
    }
}