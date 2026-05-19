// BillingPage.xaml.cs

using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Dental_Clinic_System.Dashboard

{
    public partial class BillingPage : Page
    {
        public BillingPage()
        {
            InitializeComponent();
        }

        private void ViewReceipt_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null)
            {
                NavigationService.Navigate(new ReceiptPage());
            }
        }
    }
}