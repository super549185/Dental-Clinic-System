using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Dental_Clinic_System.Data;
using Dental_Clinic_System.Models;
using System.Windows.Media.Imaging;

namespace Dental_Clinic_System.Dashboard
{
    public partial class EditStaffWindow : Window
    {
        private AppDbContext _dbContext;
        private StaffItem _existingStaff;
        private string _selectedImagePath = null;
        private bool imageChanged = false;

        public EditStaffWindow(StaffItem staff)
        {
            InitializeComponent();
            _dbContext = new AppDbContext();
            _existingStaff = staff;
            GenerateScheduleTimes();
            LoadServices();
            LoadData();
        }

        private void LoadData()
        {
            NameBox.Text = _existingStaff.Name;

            // Safely load comboboxes with fallbacks
            RoleBox.SelectedItem = RoleBox.Items.Cast<ComboBoxItem>().FirstOrDefault(x => x.Content.ToString() == _existingStaff.Role) ?? RoleBox.Items[0];
            SpecBox.Text = _existingStaff.Specialization;

            if (!string.IsNullOrEmpty(_existingStaff.Schedule))
            {
                string[] parts = _existingStaff.Schedule.Split(' ');
                if (parts.Length >= 6)
                {
                    DaysBox.SelectedItem = DaysBox.Items.Cast<ComboBoxItem>().FirstOrDefault(x => x.Content.ToString() == parts[0]) ?? DaysBox.Items[0];
                    StartTimeBox.SelectedItem = StartTimeBox.Items.OfType<string>().FirstOrDefault(x => x == (parts[1] + " " + parts[2])) ?? StartTimeBox.Items[0];
                    EndTimeBox.SelectedItem = EndTimeBox.Items.OfType<string>().FirstOrDefault(x => x == (parts[4] + " " + parts[5])) ?? EndTimeBox.Items[0];
                }
            }

            // Load Image if exists
            if (_existingStaff.ImageData != null)
            {
                _selectedImagePath = "existing";
                PreviewImage.Source = StaffPage.GetImageFromBytes(_existingStaff.ImageData);
            }
        }

        private void GenerateScheduleTimes()
        {
            StartTimeBox.Items.Clear();
            EndTimeBox.Items.Clear();
            for (int hour = 6; hour <= 21; hour++)
            {
                string time = DateTime.Today.AddHours(hour).ToString("hh:mm tt");
                StartTimeBox.Items.Add(time);
                EndTimeBox.Items.Add(time);
            }
        }

        private void ChooseImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*";
            if (dlg.ShowDialog() == true)
            {
                _selectedImagePath = dlg.FileName;
                PreviewImage.Source = new BitmapImage(new Uri(_selectedImagePath));
                imageChanged = true;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.DialogResult = false;

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text)) { MessageBox.Show("Enter staff name.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            try
            {
                byte[] imageData = _existingStaff.ImageData;

                if (imageChanged)
                {
                    if (_selectedImagePath == "existing")
                        imageData = _existingStaff.ImageData;
                    else
                        imageData = StaffPage.ResizeImageToBytes(_selectedImagePath, 300, 300);
                }

                // SAFE LOADING: Use null checks so it never crashes
                string role = RoleBox.SelectedItem != null ? ((ComboBoxItem)RoleBox.SelectedItem).Content.ToString() : "Dentist";
                string days = DaysBox.SelectedItem != null ? ((ComboBoxItem)DaysBox.SelectedItem).Content.ToString() : "Mon-Fri";
                string startTime = StartTimeBox.SelectedItem != null ? StartTimeBox.SelectedItem.ToString() : "09:00 AM";
                string endTime = EndTimeBox.SelectedItem != null ? EndTimeBox.SelectedItem.ToString() : "05:00 PM";
                string services = string.Join(", ", ServicesListBox.SelectedItems.OfType<string>());

                _existingStaff.Name = NameBox.Text.Trim();
                _existingStaff.Role = role;
                _existingStaff.Specialization = SpecBox.Text.Trim();
                _existingStaff.Schedule = $"{days} {startTime} - {endTime}";
                _existingStaff.ServicesOffered = services;
                _existingStaff.ImageData = imageData;

                _dbContext.Staff.Update(_existingStaff);
                _dbContext.SaveChanges();
                this.DialogResult = true;
            }
            catch (Exception ex) { MessageBox.Show("Error saving: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
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

            if (!string.IsNullOrEmpty(_existingStaff.ServicesOffered))
            {
                var currentServices = _existingStaff.ServicesOffered.Split(',').Select(s => s.Trim()).ToList();
                foreach (var item in currentServices)
                {
                    var lbItem = ServicesListBox.Items.Cast<string>().FirstOrDefault(x => x == item);
                    if (lbItem != null) ServicesListBox.SelectedItems.Add(lbItem);
                }
            }
        }
    }
}