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
    public partial class AppointmentFormWindow : Window
    {
        private AppDbContext _dbContext;
        private AppointmentItem _existingAppointment;
        private bool isEditMode = false;
        private bool _isLoadingData = false;

        // 1. UPDATED CONSTRUCTOR
        public AppointmentFormWindow(AppointmentItem existingAppointment = null, bool isOldPatientMode = false, string preselectedNewPatient = null)
        {
            InitializeComponent();
            _dbContext = new AppDbContext();

            LoadDentistsFromDatabase();

            if (isOldPatientMode)
            {
                // OLD PATIENT MODE: Hide TextBox, show ComboBox, load patients
                PatientNameBox.Visibility = Visibility.Collapsed;
                PatientComboBox.Visibility = Visibility.Visible;
                LoadPatientsFromDatabase();

                if (DentistBox.Items.Count > 0) DentistBox.SelectedIndex = 0;
            }
            else if (!string.IsNullOrEmpty(preselectedNewPatient))
            {
                // NEW PATIENT MODE: Show TextBox, make it readonly, fill with new name
                PatientNameBox.Visibility = Visibility.Visible;
                PatientComboBox.Visibility = Visibility.Collapsed;
                PatientNameBox.Text = preselectedNewPatient;
                PatientNameBox.IsReadOnly = true;
                PatientNameBox.Background = new SolidColorBrush(Color.FromRgb(0xF3, 0xF4, 0xF6));

                if (DentistBox.Items.Count > 0) DentistBox.SelectedIndex = 0;
            }
            else if (existingAppointment != null)
            {
                // EDIT MODE: Normal behavior
                isEditMode = true;
                _existingAppointment = existingAppointment;
                FormTitle.Text = "Edit Appointment";
                LoadData(existingAppointment);
            }
        }

        // 2. ADD THIS NEW METHOD (Load Patients)
        private void LoadPatientsFromDatabase()
        {
            PatientComboBox.Items.Clear();
            using (var db = new AppDbContext())
            {
                foreach (var patient in db.Patients.OrderBy(p => p.Name).ToList())
                {
                    PatientComboBox.Items.Add(patient.Name);
                }
            }
            if (PatientComboBox.Items.Count > 0)
                PatientComboBox.SelectedIndex = 0;
        }

        // ─────────────────────────────────────────────────────────────
        // DENTIST LOADING — shows ALL dentists with off-duty label
        // ─────────────────────────────────────────────────────────────
        private void LoadDentistsFromDatabase()
        {
            DentistBox.Items.Clear();
            using (var db = new AppDbContext())
            {
                foreach (var staff in db.Staff.Where(s => s.Role == "Dentist").ToList())
                {
                    string dynamicStatus = StaffHelper.GetDynamicStatus(staff);
                    string displayName = dynamicStatus == "On Duty"
                        ? staff.Name
                        : $"{staff.Name} (Off Duty)";

                    DentistBox.Items.Add(displayName);
                }
            }
        }

        // ─────────────────────────────────────────────────────────────
        // HELPER: strip the " (Off Duty)" suffix to get the real name
        // ─────────────────────────────────────────────────────────────
        private string GetRealDentistName(string displayName)
        {
            if (string.IsNullOrEmpty(displayName)) return displayName;
            return displayName.Replace(" (Off Duty)", "").Trim();
        }

        // ─────────────────────────────────────────────────────────────
        // TIME GENERATION — respects the dentist's schedule hours
        // ─────────────────────────────────────────────────────────────
        private void GenerateTimeIntervalsForDentist(string realDentistName, string preSelectTime = null)
        {
            TimeBox.Items.Clear();

            int startHour = 8;
            int endHour = 17;

            if (!string.IsNullOrEmpty(realDentistName))
            {
                using (var db = new AppDbContext())
                {
                    var staff = db.Staff.FirstOrDefault(s => s.Name == realDentistName);
                    if (staff != null && !string.IsNullOrEmpty(staff.Schedule))
                    {
                        string[] parts = staff.Schedule.Split(
                            new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        if (parts.Length >= 6)
                        {
                            string startStr = parts[1] + " " + parts[2];
                            string endStr = parts[4] + " " + parts[5];

                            if (DateTime.TryParse(startStr, out DateTime parsedStart))
                                startHour = parsedStart.Hour;

                            if (DateTime.TryParse(endStr, out DateTime parsedEnd))
                                endHour = parsedEnd.Hour;
                        }
                    }
                }
            }

            for (int hour = startHour; hour < endHour; hour++)
            {
                string time = DateTime.Today.AddHours(hour).ToString("hh:mm tt");
                TimeBox.Items.Add(time);
            }

            if (!string.IsNullOrEmpty(preSelectTime))
            {
                var match = TimeBox.Items.OfType<string>()
                    .FirstOrDefault(x => x == preSelectTime);
                TimeBox.SelectedItem = match ?? (TimeBox.Items.Count > 0 ? TimeBox.Items[0] : null);
            }
            else if (TimeBox.Items.Count > 0)
            {
                TimeBox.SelectedIndex = 0;
            }
        }

        // ─────────────────────────────────────────────────────────────
        // SERVICE LOADING — loads services the dentist offers
        // ─────────────────────────────────────────────────────────────
        private void PopulateServicesForDentist(string realDentistName, string preSelectService = null)
        {
            ServiceBox.Items.Clear();
            if (string.IsNullOrEmpty(realDentistName)) return;

            using (var db = new AppDbContext())
            {
                var staff = db.Staff.FirstOrDefault(s => s.Name == realDentistName);

                if (staff != null && !string.IsNullOrEmpty(staff.ServicesOffered))
                {
                    var offeredServices = staff.ServicesOffered
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim().ToLower())
                        .ToList();

                    foreach (var service in db.Services.ToList())
                    {
                        if (offeredServices.Contains(service.Name.ToLower()))
                            ServiceBox.Items.Add(service.Name);
                    }
                }
            }

            if (!string.IsNullOrEmpty(preSelectService))
            {
                var match = ServiceBox.Items.OfType<string>()
                    .FirstOrDefault(x => x.Equals(preSelectService, StringComparison.OrdinalIgnoreCase));
                ServiceBox.SelectedItem = match ?? (ServiceBox.Items.Count > 0 ? (object)ServiceBox.Items[0] : null);
            }
            else if (ServiceBox.Items.Count > 0)
            {
                ServiceBox.SelectedIndex = 0;
            }
        }

        // ─────────────────────────────────────────────────────────────
        // DENTIST SELECTION CHANGED
        // ─────────────────────────────────────────────────────────────
        private void DentistBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoadingData) return;
            if (DentistBox.SelectedItem == null) return;

            string realName = GetRealDentistName(DentistBox.SelectedItem.ToString());

            GenerateTimeIntervalsForDentist(realName);
            PopulateServicesForDentist(realName);
            UpdateOffDutyWarning(DentistBox.SelectedItem.ToString());
        }

        // ─────────────────────────────────────────────────────────────
        // OFF-DUTY WARNING
        // ─────────────────────────────────────────────────────────────
        private void UpdateOffDutyWarning(string displayName)
        {
            if (displayName.Contains("(Off Duty)"))
            {
                string realName = GetRealDentistName(displayName);
                UpdateSubtitle($"⚠  {realName} is currently Off Duty — appointment will still be saved.");
            }
            else
            {
                UpdateSubtitle("Fill in the details below");
            }
        }

        private void UpdateSubtitle(string text)
        {
            if (FormTitle.Parent is StackPanel sp && sp.Children.Count > 1)
            {
                if (sp.Children[1] is TextBlock subtitle)
                {
                    subtitle.Text = text;
                    subtitle.Foreground = text.StartsWith("⚠")
                        ? new SolidColorBrush(Color.FromRgb(0xFF, 0xD7, 0x00))
                        : new SolidColorBrush(Color.FromRgb(0xA7, 0xF3, 0xD0));
                }
            }
        }

        // ─────────────────────────────────────────────────────────────
        // LOAD DATA for edit mode
        // ─────────────────────────────────────────────────────────────
        private void LoadData(AppointmentItem apt)
        {
            _isLoadingData = true;

            PatientNameBox.Text = apt.PatientName;

            var dentistMatch = DentistBox.Items.OfType<string>()
                .FirstOrDefault(x => GetRealDentistName(x) == apt.Dentist);

            if (dentistMatch != null)
                DentistBox.SelectedItem = dentistMatch;
            else if (DentistBox.Items.Count > 0)
                DentistBox.SelectedIndex = 0;

            string realDentistName = GetRealDentistName(DentistBox.SelectedItem?.ToString() ?? "");

            GenerateTimeIntervalsForDentist(realDentistName, apt.Time);
            PopulateServicesForDentist(realDentistName, apt.Service);

            if (!string.IsNullOrEmpty(apt.Date))
                DateBox.SelectedDate = DateTime.Parse(apt.Date);

            var statusItem = StatusBox.Items.Cast<ComboBoxItem>()
                .FirstOrDefault(x => x.Content.ToString() == apt.Status);
            StatusBox.SelectedItem = statusItem ?? (StatusBox.Items.Count > 0 ? StatusBox.Items[0] : null);

            UpdateOffDutyWarning(DentistBox.SelectedItem?.ToString() ?? "");

            _isLoadingData = false;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.DialogResult = false;

        // ─────────────────────────────────────────────────────────────
        // SAVE — validation + conflict checks + FIXED edit logic
        // ─────────────────────────────────────────────────────────────
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Step 1: Patient name (Get from ComboBox or TextBox)
            string patientName = "";
            if (PatientComboBox.Visibility == Visibility.Visible)
                patientName = PatientComboBox.SelectedItem?.ToString()?.Trim() ?? "";
            else
                patientName = PatientNameBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(patientName))
            {
                ShowValidationError(
                    "Step 1 of 5 — Patient Name Missing",
                    "Please select or enter the patient's full name.");

                if (PatientComboBox.Visibility == Visibility.Visible) PatientComboBox.Focus();
                else PatientNameBox.Focus();
                return;
            }

            // Step 2: Dentist
            if (DentistBox.SelectedItem == null)
            {
                ShowValidationError(
                    "Step 2 of 5 — Dentist Not Selected",
                    "Please select a dentist for this appointment.\n\n" +
                    "→ Click the 'DENTIST' dropdown and choose a dentist.\n\n" +
                    "Note: Dentists marked '(Off Duty)' can still be assigned.");
                DentistBox.Focus();
                return;
            }

            // Step 3: Date
            if (DateBox.SelectedDate == null)
            {
                ShowValidationError(
                    "Step 3 of 5 — Date Not Selected",
                    "Please select an appointment date.\n\n" +
                    "→ Click the 'DATE' field and pick a date from the calendar.");
                DateBox.Focus();
                return;
            }

            // Step 4: Time
            if (TimeBox.SelectedItem == null)
            {
                ShowValidationError(
                    "Step 4 of 5 — Time Not Selected",
                    "Please select an appointment time.\n\n" +
                    "→ Click the 'TIME' dropdown and choose a time slot.\n\n" +
                    "Note: Available times are based on the selected dentist's schedule.");
                TimeBox.Focus();
                return;
            }

            // Step 5: Service
            if (ServiceBox.SelectedItem == null)
            {
                ShowValidationError(
                    "Step 5 of 5 — Service Not Selected",
                    "Please select a service for this appointment.\n\n" +
                    "→ Click the 'SERVICE' dropdown and choose a dental service.\n\n" +
                    "Note: Services shown are only those offered by the selected dentist.");
                ServiceBox.Focus();
                return;
            }

            try
            {
                
                string dentist = GetRealDentistName(DentistBox.SelectedItem.ToString());
                string date = DateBox.SelectedDate.Value.ToString("yyyy-MM-dd");
                string time = TimeBox.SelectedItem.ToString();
                string service = ServiceBox.SelectedItem.ToString();
                string status = StatusBox.SelectedItem != null
                                        ? ((ComboBoxItem)StatusBox.SelectedItem).Content.ToString()
                                        : "Confirmed";

                // ── Conflict Check 1: Doctor already booked at same date + time ──
                var doctorTimeConflict = _dbContext.Appointments
                    .Where(a =>
                        a.Dentist == dentist &&
                        a.Date == date &&
                        a.Time == time &&
                        a.Status != "Cancelled")
                    .ToList();

                if (isEditMode && _existingAppointment != null)
                    doctorTimeConflict = doctorTimeConflict
                        .Where(a => a.AppointmentId != _existingAppointment.AppointmentId)
                        .ToList();

                if (doctorTimeConflict.Any())
                {
                    var conflict = doctorTimeConflict.First();
                    ShowValidationError(
                        "Scheduling Conflict — Doctor Already Booked",
                        $"❌  {dentist} already has an appointment at this date and time.\n\n" +
                        $"   Conflicting appointment:\n" +
                        $"   • Patient  : {conflict.PatientName}\n" +
                        $"   • Date     : {FormatDate(conflict.Date)}\n" +
                        $"   • Time     : {conflict.Time}\n" +
                        $"   • Service  : {conflict.Service}\n\n" +
                        $"How to fix:\n" +
                        $"   1. Choose a different time slot, OR\n" +
                        $"   2. Choose a different date, OR\n" +
                        $"   3. Assign a different dentist.");
                    return;
                }

                // ── Conflict Check 2: Patient double-booked with same doctor same day ──
                if (!isEditMode)
                {
                    bool patientDoubleBooked = _dbContext.Appointments.Any(a =>
                        a.PatientName == patientName &&
                        a.Dentist == dentist &&
                        a.Date == date &&
                        a.Status != "Cancelled");

                    if (patientDoubleBooked)
                    {
                        ShowValidationError(
                            "Double Booking — Patient Already Scheduled",
                            $"❌  {patientName} already has an active appointment\n" +
                            $"    with {dentist} on {FormatDate(date)}.\n\n" +
                            $"How to fix:\n" +
                            $"   1. Choose a different date, OR\n" +
                            $"   2. Choose a different time slot, OR\n" +
                            $"   3. Assign a different dentist, OR\n" +
                            $"   4. Cancel the existing appointment first.");
                        return;
                    }
                }

                // ── All checks passed — Save ──
                if (isEditMode && _existingAppointment != null)
                {
                    // ★★★ THE FIX: Fetch a fresh tracked entity from THIS context,
                    // then update its properties. Never call Update() on the
                    // detached copy that came from AppointmentPage's context. ★★★
                    var trackedEntity = _dbContext.Appointments
                        .FirstOrDefault(a => a.AppointmentId == _existingAppointment.AppointmentId);

                    if (trackedEntity != null)
                    {
                        trackedEntity.PatientName = patientName;
                        trackedEntity.Dentist = dentist;
                        trackedEntity.Service = service;
                        trackedEntity.Date = date;
                        trackedEntity.Time = time;
                        trackedEntity.Status = status;

                        // No .Update() call needed — EF Core auto-detects
                        // changes on tracked entities during SaveChanges()
                    }
                    else
                    {
                        ShowValidationError(
                            "Not Found",
                            $"Appointment {_existingAppointment.AppointmentId} was not found in the database.\n" +
                            $"It may have been deleted by another user.");
                        return;
                    }
                }
                else
                {
                    int nextId = _dbContext.Appointments
                        .ToList()
                        .Select(a => int.Parse(a.AppointmentId.Substring(3)))
                        .DefaultIfEmpty(0)
                        .Max() + 1;

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
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An unexpected error occurred while saving:\n\n" + ex.Message,
                    "Save Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ShowValidationError(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private string FormatDate(string d)
        {
            if (DateTime.TryParse(d, out DateTime dt))
                return dt.ToString("MMM dd, yyyy");
            return d;
        }
    }
}