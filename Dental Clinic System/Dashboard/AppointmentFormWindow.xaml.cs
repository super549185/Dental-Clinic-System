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

        public AppointmentFormWindow(AppointmentItem existingAppointment = null)
        {
            InitializeComponent();
            _dbContext = new AppDbContext();

            // Load dentists FIRST (before generating times, since times depend on selected dentist)
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
                // For new appointments: generate times based on the first dentist in the list
                if (DentistBox.Items.Count > 0)
                {
                    DentistBox.SelectedIndex = 0;
                    // DentistBox_SelectionChanged will fire and populate times + services
                }
            }
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
                    // Show name with status tag so the user knows who is off-duty
                    // We store just the name but display status as a suffix
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

            int startHour = 8;  // default fallback
            int endHour = 17; // default fallback

            if (!string.IsNullOrEmpty(realDentistName))
            {
                using (var db = new AppDbContext())
                {
                    var staff = db.Staff.FirstOrDefault(s => s.Name == realDentistName);
                    if (staff != null && !string.IsNullOrEmpty(staff.Schedule))
                    {
                        // Schedule format: "Mon-Fri 09:00 AM - 05:00 PM"
                        // parts[0]=days, parts[1]=startTime, parts[2]=AM/PM,
                        // parts[3]="-",  parts[4]=endTime,   parts[5]=AM/PM
                        string[] parts = staff.Schedule.Split(
                            new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        if (parts.Length >= 6)
                        {
                            string startStr = parts[1] + " " + parts[2]; // "09:00 AM"
                            string endStr = parts[4] + " " + parts[5]; // "05:00 PM"

                            if (DateTime.TryParse(startStr, out DateTime parsedStart))
                                startHour = parsedStart.Hour;

                            if (DateTime.TryParse(endStr, out DateTime parsedEnd))
                                endHour = parsedEnd.Hour;
                        }
                    }
                }
            }

            // Build 1-hour slots from startHour up to (but not including) endHour
            // e.g. 9 AM to 5 PM → 09:00 AM, 10:00 AM … 04:00 PM  (last slot starts at endHour-1)
            for (int hour = startHour; hour < endHour; hour++)
            {
                string time = DateTime.Today.AddHours(hour).ToString("hh:mm tt");
                TimeBox.Items.Add(time);
            }

            // Re-select a specific time if requested (edit mode)
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
        // Refreshes both the TIME slots and SERVICES when dentist changes
        // ─────────────────────────────────────────────────────────────
        private void DentistBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoadingData) return;
            if (DentistBox.SelectedItem == null) return;

            string realName = GetRealDentistName(DentistBox.SelectedItem.ToString());

            // Refresh time slots to match this dentist's working hours
            GenerateTimeIntervalsForDentist(realName);

            // Refresh services to match this dentist's offered services
            PopulateServicesForDentist(realName);

            // Show a subtle hint if the dentist is off-duty
            UpdateOffDutyWarning(DentistBox.SelectedItem.ToString());
        }

        // ─────────────────────────────────────────────────────────────
        // OFF-DUTY WARNING — reuses the subtitle TextBlock in the header
        // ─────────────────────────────────────────────────────────────
        private void UpdateOffDutyWarning(string displayName)
        {
            // The XAML header has a subtitle TextBlock with Text="Fill in the details below"
            // We update it to warn when an off-duty dentist is selected
            if (displayName.Contains("(Off Duty)"))
            {
                string realName = GetRealDentistName(displayName);
                // Find the subtitle TextBlock in the header and update it
                // We'll use the Tag property trick via the named element if available,
                // otherwise walk the visual tree from FormTitle's parent
                UpdateSubtitle($"⚠  {realName} is currently Off Duty — appointment will still be saved.");
            }
            else
            {
                UpdateSubtitle("Fill in the details below");
            }
        }

        private void UpdateSubtitle(string text)
        {
            // FormTitle is in a StackPanel; its sibling is the subtitle TextBlock
            if (FormTitle.Parent is StackPanel sp && sp.Children.Count > 1)
            {
                if (sp.Children[1] is TextBlock subtitle)
                {
                    subtitle.Text = text;
                    subtitle.Foreground = text.StartsWith("⚠")
                        ? new SolidColorBrush(Color.FromRgb(0xFF, 0xD7, 0x00))  // yellow warning
                        : new SolidColorBrush(Color.FromRgb(0xA7, 0xF3, 0xD0)); // original green
                }
            }
        }

        // ─────────────────────────────────────────────────────────────
        // LOAD DATA for edit mode
        // ─────────────────────────────────────────────────────────────
        private void LoadData(AppointmentItem apt)
        {
            _isLoadingData = true;

            // 1. Patient name
            PatientNameBox.Text = apt.PatientName;

            // 2. Select dentist — match by real name (strip Off Duty suffix if present)
            var dentistMatch = DentistBox.Items.OfType<string>()
                .FirstOrDefault(x => GetRealDentistName(x) == apt.Dentist);

            if (dentistMatch != null)
                DentistBox.SelectedItem = dentistMatch;
            else if (DentistBox.Items.Count > 0)
                DentistBox.SelectedIndex = 0;

            string realDentistName = GetRealDentistName(DentistBox.SelectedItem?.ToString() ?? "");

            // 3. Generate time slots for this dentist AND pre-select existing time
            GenerateTimeIntervalsForDentist(realDentistName, apt.Time);

            // 4. Populate services AND pre-select existing service
            PopulateServicesForDentist(realDentistName, apt.Service);

            // 5. Date
            if (!string.IsNullOrEmpty(apt.Date))
                DateBox.SelectedDate = DateTime.Parse(apt.Date);

            // 6. Status
            var statusItem = StatusBox.Items.Cast<ComboBoxItem>()
                .FirstOrDefault(x => x.Content.ToString() == apt.Status);
            StatusBox.SelectedItem = statusItem ?? (StatusBox.Items.Count > 0 ? StatusBox.Items[0] : null);

            // 7. Show off-duty warning if applicable
            UpdateOffDutyWarning(DentistBox.SelectedItem?.ToString() ?? "");

            _isLoadingData = false;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.DialogResult = false;

        // ─────────────────────────────────────────────────────────────
        // SAVE — step-by-step validation + conflict checks
        // ─────────────────────────────────────────────────────────────
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Step 1: Patient name
            if (string.IsNullOrWhiteSpace(PatientNameBox.Text) || PatientNameBox.Text == "Enter patient name")
            {
                ShowValidationError(
                    "Step 1 of 5 — Patient Name Missing",
                    "Please enter the patient's full name before saving.\n\n" +
                    "→ Click the 'PATIENT NAME' field and type the patient's name.");
                PatientNameBox.Focus();
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
                string patientName = PatientNameBox.Text.Trim();
                // Always use the real name (strip Off Duty tag) when saving
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