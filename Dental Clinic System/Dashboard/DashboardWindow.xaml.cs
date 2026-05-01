using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace Dental_Clinic_System.Dashboard
{
    public partial class DashboardWindow : Window
    {
        public DashboardWindow()
        {
            InitializeComponent();
            LoadPage("Dashboard");
            SidebarMenu.SelectionChanged += SidebarMenu_SelectionChanged;
        }

        private void SidebarMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SidebarMenu.SelectedItem is ListBoxItem item)
            {
                LoadPage(item.Content.ToString());
            }
        }

        private void LoadPage(string pageName)
        {
            Grid mainGrid = (Grid)this.Content;
            Border contentBorder = (Border)mainGrid.Children[1];
            Grid contentGrid = (Grid)contentBorder.Child;
            contentGrid.Children.Clear();

            switch (pageName)
            {
                case "Dashboard":
                    Frame dashFrame = new Frame { NavigationUIVisibility = NavigationUIVisibility.Hidden };
                    dashFrame.Navigate(new DashboardPage()); // LOAD NEW DASHBOARD
                    contentGrid.Children.Add(dashFrame);
                    break;

                case "Appointments":
                    Frame aptFrame = new Frame { NavigationUIVisibility = NavigationUIVisibility.Hidden };
                    aptFrame.Navigate(new AppointmentPage());
                    contentGrid.Children.Add(aptFrame);
                    break;

                default:
                    TextBlock comingSoon = new TextBlock
                    {
                        Text = $"{pageName} - Coming Soon",
                        FontSize = 24,
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33)),
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    contentGrid.Children.Add(comingSoon);
                    break;
            }
        }

        private void logoutButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                MainWindow mainWindow = new MainWindow();
                Application.Current.MainWindow = mainWindow;
                mainWindow.Show();
                this.Close();
            }
        }
    }
}