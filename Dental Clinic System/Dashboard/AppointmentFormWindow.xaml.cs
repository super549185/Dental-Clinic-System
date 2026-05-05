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
        private bool isEditMode;

        public AppointmentFormWindow(AppointmentItem existingAppointment = null)
        {
            InitializeComponent();
            _dbContext = new AppDbContext();
            LoadServicesFromDatabase();

            if (existingAppointment != null)
            {
                isEditMode = true;
                _existingAppointment = existingAppointment;
                FormTitle.Text = "Edit Appointment";
                LoadData(existingAppointment);
            }
        }

        private void LoadData(AppointmentItem apt)
        {
            PatientNameBox.Text = apt.PatientName;

            // Dentist (Still uses ComboBoxItems from XAML)
            var dentistItem = DentistBox.Items.Cast<ComboBoxItem>().FirstOrDefault(x => x.Content.ToString() == apt.Dentist);
            DentistBox.SelectedItem = dentistItem ?? DentistBox.Items[0];

            // Service (NOW uses Strings from Database - this is what was crashing!)
            var serviceMatch = ServiceBox.Items.Cast<string>().FirstOrDefault(x => x == apt.Service);
            if (serviceMatch != null)
                ServiceBox.SelectedItem = serviceMatch;
            else if (ServiceBox.Items.Count > 0)
                ServiceBox.SelectedItem = ServiceBox.Items[0];

            // Time (Still uses ComboBoxItems from XAML)
            var timeItem = TimeBox.Items.Cast<ComboBoxItem>().FirstOrDefault(x => x.Content.ToString() == apt.Time);
            TimeBox.SelectedItem = timeItem ?? TimeBox.Items[0];

            // Status (Still uses ComboBoxItems from XAML)
            var statusItem = StatusBox.Items.Cast<ComboBoxItem>().FirstOrDefault(x => x.Content.ToString() == apt.Status);
            StatusBox.SelectedItem = statusItem ?? StatusBox.Items[0];
        }
        private void LoadServicesFromDatabase()
        {
            ServiceBox.Items.Clear();
            using (var db = new AppDbContext())
            {
                foreach (var service in db.Services.ToList())
                {
                    ServiceBox.Items.Add(service.Name);
                }
            }
            if (ServiceBox.Items.Count > 0) ServiceBox.SelectedIndex = 0;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(PatientNameBox.Text))
            {
                MessageBox.Show("Patient Name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DateBox.SelectedDate == null)
            {
                MessageBox.Show("Please select a date.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Get values
            string patientName = PatientNameBox.Text.Trim();
            string dentist = ((ComboBoxItem)DentistBox.SelectedItem).Content.ToString(); 
            string service = ServiceBox.SelectedItem.ToString();                       
            string date = DateBox.SelectedDate.Value.ToString("yyyy-MM-dd");
            string time = ((ComboBoxItem)TimeBox.SelectedItem).Content.ToString();     
            string status = ((ComboBoxItem)StatusBox.SelectedItem).Content.ToString();

            try
            {
                if (isEditMode)
                {
                    // Update existing record
                    _existingAppointment.PatientName = patientName;
                    _existingAppointment.Dentist = dentist;
                    _existingAppointment.Service = service;
                    _existingAppointment.Date = date;
                    _existingAppointment.Time = time;
                    _existingAppointment.Status = status;

                    // CRITICAL: Tell the new database context to track and save this changed object
                    _dbContext.Appointments.Update(_existingAppointment);
                    _dbContext.SaveChanges();
                }
                else
                {
                    // Generate new ID safely (ToList prevents LINQ/SQLite error)
                    int nextId = _dbContext.Appointments.ToList()
                        .Select(a => int.Parse(a.AppointmentId.Substring(3)))
                        .DefaultIfEmpty(0)
                        .Max() + 1;

                    var newApt = new AppointmentItem
                    {
                        AppointmentId = "APT" + nextId.ToString("D3"),
                        PatientName = patientName,
                        Dentist = dentist,
                        Service = service,
                        Date = date,
                        Time = time,
                        Status = status // Will only be "Confirmed" or "Cancelled"
                    };
                    _dbContext.Appointments.Add(newApt);
                    _dbContext.SaveChanges();
                }

                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}