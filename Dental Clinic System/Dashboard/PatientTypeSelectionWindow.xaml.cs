using System.Windows;

namespace Dental_Clinic_System.Dashboard
{
    public partial class PatientTypeSelectionWindow : Window
    {
        public bool IsOldPatient { get; private set; }

        public PatientTypeSelectionWindow()
        {
            InitializeComponent();
        }

        private void OldPatient_Click(object sender, RoutedEventArgs e)
        {
            IsOldPatient = true;
            this.DialogResult = true;
        }

        private void NewPatient_Click(object sender, RoutedEventArgs e)
        {
            IsOldPatient = false;
            this.DialogResult = true;
        }
    }
}