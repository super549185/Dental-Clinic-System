// ReceiptPage.xaml.cs

using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Dental_Clinic_System.Dashboard
{
    public partial class ReceiptPage : Page
    {
        public ReceiptPage()
        {
            InitializeComponent();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}