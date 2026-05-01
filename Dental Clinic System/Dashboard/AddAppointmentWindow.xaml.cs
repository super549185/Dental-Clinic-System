using System;
using System.Windows;
using Dental_Clinic_System.Data;
using Dental_Clinic_System.Models;

namespace Dental_Clinic_System.Dashboard
{
    public partial class AddAppointmentWindow : Window
    {
        private AppDbContext _dbContext;

        public AddAppointmentWindow()
        {
            InitializeComponent();
            _dbContext = new AppDbContext();
        }

        // ================= PLACEHOLDER LOGIC =================
        private void PatientNameBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (PatientNameBox.Text == "Enter patient name")
            {
                PatientNameBox.Text = "";
                PatientNameBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void PatientNameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PatientNameBox.Text))
            {
                PatientNameBox.Text = "Enter patient name";
                PatientNameBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        // ================= BUTTONS =================
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. Get values from inputs
            string patientName = PatientNameBox.Text;

            // Check if placeholder text is still there
            if (patientName == "Enter patient name" || string.IsNullOrWhiteSpace(patientName))
            {
                MessageBox.Show("Please enter a patient name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DateBox.SelectedDate == null)
            {
                MessageBox.Show("Please select a date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string date = DateBox.SelectedDate.Value.ToString("yyyy-MM-dd");
            string time = ((System.Windows.Controls.ContentControl)TimeBox.SelectedItem).Content.ToString();
            string service = ((System.Windows.Controls.ContentControl)ServiceBox.SelectedItem).Content.ToString();
            string dentist = ((System.Windows.Controls.ContentControl)DentistBox.SelectedItem).Content.ToString();
            string status = ((System.Windows.Controls.ContentControl)StatusBox.SelectedItem).Content.ToString();

            // 2. Create new appointment object
            var newAppointment = new AppointmentItem
            {
                PatientName = patientName.Trim(),
                Date = date,
                Time = time,
                Service = service,
                Dentist = dentist,
                Status = status
            };

            // 3. Save to database
            try
            {
                _dbContext.Appointments.Add(newAppointment);
                _dbContext.SaveChanges();

                // 4. Close window and tell caller it was successful
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving appointment:\n{ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}