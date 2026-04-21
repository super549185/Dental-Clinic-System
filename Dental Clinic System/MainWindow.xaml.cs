using Dental_Clinic_System.Dashboard;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Dental_Clinic_System
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isLoginPasswordVisible = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        /* ===================== TEXTBOX PLACEHOLDER ===================== */

        private void UsernameBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (UsernameBox.Text == "Username")
            {
                UsernameBox.Text = "";
                UsernameBox.Foreground = Brushes.Black;
            }
        }

        private void UsernameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameBox.Text))
            {
                UsernameBox.Text = "Username";
                UsernameBox.Foreground = Brushes.Gray;
            }
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (LoginPasswordBox.Password.Length == 0)
                PasswordPlaceholder.Visibility = Visibility.Hidden;
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (LoginPasswordBox.Password.Length == 0)
                PasswordPlaceholder.Visibility = Visibility.Visible;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility =
                LoginPasswordBox.Password.Length == 0
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        /* ===================== SHOW / HIDE PASSWORD ===================== */

        private void ToggleLoginPassword_Click(object sender, RoutedEventArgs e)
        {
            if (!isLoginPasswordVisible)
            {
                LoginPasswordTextBox.Text = LoginPasswordBox.Password;
                LoginPasswordBox.Visibility = Visibility.Collapsed;
                LoginPasswordTextBox.Visibility = Visibility.Visible;
                LoginEyeIcon.Text = "🙈";
                isLoginPasswordVisible = true;
            }
            else
            {
                LoginPasswordBox.Password = LoginPasswordTextBox.Text;
                LoginPasswordTextBox.Visibility = Visibility.Collapsed;
                LoginPasswordBox.Visibility = Visibility.Visible;
                LoginEyeIcon.Text = "👁";
                isLoginPasswordVisible = false;
            }
        }
        /* ===================== NAVIGATE TO REGISTER ===================== */

        private void gotoregButton_Click(object sender, RoutedEventArgs e)
        {
            LoginGrid.Visibility = Visibility.Collapsed;
            MainFrame.Visibility = Visibility.Visible;
            MainFrame.Navigate(new RegisterPage());
        }

        // Wrapper for the register button declared in XAML (regButton_Click)
        private void regButton_Click(object sender, RoutedEventArgs e)
        {
            gotoregButton_Click(sender, e);
        }

        // Handle Enter key on input boxes
        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                loginButton_Click(sender, e);
            }
        }

        // Basic login click handler to match XAML wiring
        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameBox.Text?.Trim();
            var password = isLoginPasswordVisible ? LoginPasswordTextBox.Text : LoginPasswordBox.Password;

            // Consider the placeholder text "Username" as empty
            if (string.IsNullOrEmpty(username) || username == "Username" || string.IsNullOrEmpty(password))
            {
                ErrorMessage.Visibility = Visibility.Visible;
                return;
            }

            // For now accept any non-empty credentials as successful login
            ErrorMessage.Visibility = Visibility.Collapsed;
            MessageBox.Show("Login successful.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            DashboardWindow dashboard = new DashboardWindow();
            dashboard.Show();
            this.Close();
        }

        /* ===================== NAVIGATE BACK TO LOGIN ===================== */

        public void NavigateToLogin()
        {
            MainFrame.Visibility = Visibility.Collapsed;
            LoginGrid.Visibility = Visibility.Visible;
        }

       
    }
    }