using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Dental_Clinic_System.Data;
using Dental_Clinic_System.Models;

namespace Dental_Clinic_System.Dashboard
{
    public partial class AppointmentFormWindow : Window
    {
        private AppDbContext _dbContext;
        private AppointmentItem _existingAppointment;
        private bool isEditMode = false;
        private bool _isLoadingData = false;

        public AppointmentFormWindow(AppointmentItem existingAppointment = null)
        {
            InitializeComponent();
            _dbContext = new AppDbContext();
            GenerateTimeIntervals();
            LoadDentistsFromDatabase();

            if (existingAppointment != null)
            {
                isEditMode = true;
                _existingAppointment = existingAppointment;
                FormTitle.Text = "Edit Appointment";
                LoadData(existingAppointment);
            }
            else
            {
                if (DentistBox.Items.Count > 0) DentistBox.SelectedIndex = 0;
            }
        }

        private void LoadDentistsFromDatabase()
        {
            DentistBox.Items.Clear();
            using (var db = new AppDbContext())
            {
                foreach (var staff in db.Staff.Where(s => s.Role == "Dentist").ToList())
                {
                    if (StaffHelper.GetDynamicStatus(staff) == "On Duty")
                    {
                        DentistBox.Items.Add(staff.Name);
                    }
                }
            }
        }

        private void GenerateTimeIntervals()
        {
            TimeBox.Items.Clear();
            for (int hour = 8; hour <= 17; hour++)
            {
                string time = DateTime.Today.AddHours(hour).ToString("hh:mm tt");
                TimeBox.Items.Add(time);
            }
        }

        private void DentistBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoadingData) return;

            ServiceBox.Items.Clear();
            if (DentistBox.SelectedItem == null) return;

            string selectedDentist = DentistBox.SelectedItem.ToString();

            using (var db = new AppDbContext())
            {
                var staff = db.Staff.FirstOrDefault(s => s.Name == selectedDentist);
                if (staff == null || string.IsNullOrEmpty(staff.ServicesOffered)) return;

                var offeredServices = staff.ServicesOffered
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim().ToLower())
                    .ToList();

                foreach (var service in db.Services.ToList())
                {
                    if (offeredServices.Contains(service.Name.ToLower()))
                    {
                        ServiceBox.Items.Add(service.Name);
                    }
                }
            }

            if (ServiceBox.Items.Count > 0) ServiceBox.SelectedIndex = 0;
        }

        private void LoadData(AppointmentItem apt)
        {
            _isLoadingData = true;

            PatientNameBox.Text = apt.PatientName;

            var dentistMatch = DentistBox.Items.OfType<string>().FirstOrDefault(x => x == apt.Dentist);
            if (dentistMatch != null) DentistBox.SelectedItem = dentistMatch;
            else if (DentistBox.Items.Count > 0) DentistBox.SelectedItem = DentistBox.Items[0];

            if (!string.IsNullOrEmpty(apt.Date)) DateBox.SelectedDate = DateTime.Parse(apt.Date);

            var timeMatch = TimeBox.Items.OfType<string>().FirstOrDefault(x => x == apt.Time);
            TimeBox.SelectedItem = timeMatch ?? (TimeBox.Items.Count > 0 ? TimeBox.Items[0] : null);

            var serviceMatch = ServiceBox.Items.OfType<string>().FirstOrDefault(x => x == apt.Service);
            if (serviceMatch != null)
            {
                ServiceBox.SelectedItem = serviceMatch;
            }
            else if (ServiceBox.Items.Count > 0)
            {
                ServiceBox.SelectedIndex = 0;
            }

            var statusItem = StatusBox.Items.Cast<ComboBoxItem>().FirstOrDefault(x => x.Content.ToString() == apt.Status);
            StatusBox.SelectedItem = statusItem ?? (StatusBox.Items.Count > 0 ? StatusBox.Items[0] : null);

            _isLoadingData = false;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.DialogResult = false;

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PatientNameBox.Text) || PatientNameBox.Text == "Enter patient name")
            {
                MessageBox.Show("Please enter a patient name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (DateBox.SelectedDate == null)
            {
                MessageBox.Show("Please select a date.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string patientName = PatientNameBox.Text.Trim();
                string dentist = DentistBox.SelectedItem != null ? DentistBox.SelectedItem.ToString() : "Unassigned";
                string date = DateBox.SelectedDate.Value.ToString("yyyy-MM-dd");
                string time = TimeBox.SelectedItem != null ? TimeBox.SelectedItem.ToString() : "09:00 AM";
                string service = ServiceBox.SelectedItem != null ? ServiceBox.SelectedItem.ToString() : "General Checkup";
                string status = StatusBox.SelectedItem != null ? ((ComboBoxItem)StatusBox.SelectedItem).Content.ToString() : "Confirmed";

                // PREVENT DOUBLE BOOKING FOR NEW APPOINTMENTS ONLY
                if (!isEditMode)
                {
                    bool isDoubleBooked = _dbContext.Appointments.Any(a =>
                        a.PatientName == patientName &&
                        a.Dentist == dentist &&
                        a.Date == date &&
                        a.Status != "Cancelled");

                    if (isDoubleBooked)
                    {
                        MessageBox.Show("This patient already has an active appointment with this doctor on this date.\n\nPlease choose a different time, date, or doctor.", "Double Booking Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                if (isEditMode && _existingAppointment != null)
                {
                    _existingAppointment.PatientName = patientName;
                    _existingAppointment.Dentist = dentist;
                    _existingAppointment.Service = service;
                    _existingAppointment.Date = date;
                    _existingAppointment.Time = time;
                    _existingAppointment.Status = status;
                    _dbContext.Appointments.Update(_existingAppointment);
                }
                else
                {
                    int nextId = _dbContext.Appointments.ToList().Select(a => int.Parse(a.AppointmentId.Substring(3))).DefaultIfEmpty(0).Max() + 1;
                    _dbContext.Appointments.Add(new AppointmentItem
                    {
                        AppointmentId = "APT" + nextId.ToString("D3"),
                        PatientName = patientName,
                        Dentist = dentist,
                        Service = service,
                        Date = date,
                        Time = time,
                        Status = status
                    });
                }
                _dbContext.SaveChanges();
                this.DialogResult = true;
            }
            catch (Exception ex) { MessageBox.Show("Error saving: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
    }
}