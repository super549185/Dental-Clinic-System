using System.Windows;

namespace Dental_Clinic_System
{
    public partial class LogoutDialog : Window
    {
        public bool Confirmed { get; private set; } = false;

        public LogoutDialog()
        {
            InitializeComponent();
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            Confirmed = true;
            this.Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            Confirmed = false;
            this.Close();
        }
    }
}