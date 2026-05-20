using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Dental_Clinic_System.Data;
using Dental_Clinic_System.Models;

namespace Dental_Clinic_System.Dashboard
{
    public partial class PatientFormWindow : Window
    {
        private AppDbContext _dbContext;
        private PatientItem _existingPatient;
        private bool isEditMode;
        public string SavedPatientName { get; private set; }

        public PatientFormWindow(PatientItem existingPatient = null)
        {
            InitializeComponent();
            _dbContext = new AppDbContext();
            if (existingPatient != null) { isEditMode = true; _existingPatient = existingPatient; FormTitle.Text = "Edit Patient"; LoadData(existingPatient); }
        }

        private void LoadData(PatientItem p)
        {
            NameBox.Text = p.Name; ContactBox.Text = p.Contact; EmailBox.Text = p.Email;
            LastVisitBox.SelectedDate = DateTime.Parse(p.LastVisit);
            StatusBox.SelectedItem = StatusBox.Items.Cast<ComboBoxItem>().FirstOrDefault(x => x.Content.ToString() == p.Status) ?? StatusBox.Items[0];

            DobBox.SelectedDate = string.IsNullOrEmpty(p.DateOfBirth) ? null : DateTime.Parse(p.DateOfBirth);
            AddressBox.Text = p.Address;
            AllergiesBox.Text = p.Allergies;
            ConditionsBox.Text = p.MedicalConditions;
            BloodBox.SelectedItem = BloodBox.Items.Cast<ComboBoxItem>().FirstOrDefault(x => x.Content.ToString() == p.BloodType) ?? BloodBox.Items[0];
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.DialogResult = false;

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text)) { MessageBox.Show("Patient Name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (LastVisitBox.SelectedDate == null) { MessageBox.Show("Please select a last visit date.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            try
            {
                string name = NameBox.Text.Trim(), contact = ContactBox.Text.Trim(), email = EmailBox.Text.Trim();
                string lastVisit = LastVisitBox.SelectedDate.Value.ToString("yyyy-MM-dd");
                string status = ((ComboBoxItem)StatusBox.SelectedItem).Content.ToString();

                // New Fields
                string dob = DobBox.SelectedDate?.ToString("yyyy-MM-dd") ?? "";
                string address = AddressBox.Text.Trim();
                string allergies = AllergiesBox.Text.Trim();
                string conditions = ConditionsBox.Text.Trim();
                string blood = ((ComboBoxItem)BloodBox.SelectedItem).Content.ToString();

                if (isEditMode)
                {
                    _existingPatient.Name = name; _existingPatient.Contact = contact; _existingPatient.Email = email;
                    _existingPatient.LastVisit = lastVisit; _existingPatient.Status = status;
                    _existingPatient.DateOfBirth = dob; _existingPatient.Address = address;
                    _existingPatient.Allergies = allergies; _existingPatient.MedicalConditions = conditions;
                    _existingPatient.BloodType = blood;
                    _dbContext.Patients.Update(_existingPatient);
                    _dbContext.SaveChanges();
                    // ... your existing save code ...
                  

                    // ADD THIS LINE
                    SavedPatientName = name;

                    this.DialogResult = true;
                }

                else
                {
                    int nextId = _dbContext.Patients.ToList().Select(p => int.Parse(p.PatientId.Substring(1))).DefaultIfEmpty(0).Max() + 1;
                    _dbContext.Patients.Add(new PatientItem { PatientId = "P" + nextId.ToString("D3"), Name = name, Contact = contact, Email = email, LastVisit = lastVisit, Status = status, DateOfBirth = dob, Address = address, Allergies = allergies, MedicalConditions = conditions, BloodType = blood });
                    _dbContext.SaveChanges();
                }
                this.DialogResult = true;
            }
            catch (Exception ex) { MessageBox.Show("Error saving: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
    }
}