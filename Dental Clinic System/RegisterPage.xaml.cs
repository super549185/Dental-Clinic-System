using System;
using System.Collections.Generic;
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
    /// Interaction logic for RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
        // ================= PASSWORD TOGGLE STATES =================
        private bool isPasswordVisible = false;
        private bool isConfirmPasswordVisible = false;

        public RegisterPage()
        {
            InitializeComponent();
        }   

        // ================= REGISTER BUTTON =================
        

        // ================= PLACEHOLDERS =================
        private void FullNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (FullNameTextBox.Text == "Full Name")
            {
                FullNameTextBox.Text = "";
                FullNameTextBox.Foreground = Brushes.Black;
            }
        }
        private void FullNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FullNameTextBox.Text))
            {
                FullNameTextBox.Text = "Full Name";
                FullNameTextBox.Foreground = Brushes.Gray;
            }
        }

        private void ClinicNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ClinicNameTextBox.Text == "Clinic Name")
            {
                ClinicNameTextBox.Text = "";
                ClinicNameTextBox.Foreground = Brushes.Black;
            }
        }
        private void ClinicNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ClinicNameTextBox.Text))
            {
                ClinicNameTextBox.Text = "Clinic Name";
                ClinicNameTextBox.Foreground = Brushes.Gray;
            }
        }

        private void UserNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (UsernameTextBox.Text == "Username")
            {
                UsernameTextBox.Text = "";
                UsernameTextBox.Foreground = Brushes.Black;
            }
        }
        private void UserNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                UsernameTextBox.Text = "Username";
                UsernameTextBox.Foreground = Brushes.Gray;                  
            }
        }

        private void EmailTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (EmailTextBox.Text == "Email")
            {
                EmailTextBox.Text = "";
                EmailTextBox.Foreground = Brushes.Black;
            }
        }
        private void EmailTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                EmailTextBox.Text = "Email";
                EmailTextBox.Foreground = Brushes.Gray;
            }
        }

        private void PasswordTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility = Visibility.Hidden;
        }

        private void PasswordTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PasswordTextBox.Text))
                PasswordPlaceholder.Visibility = Visibility.Visible;
        }

        private void ConfirmPasswordTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ConfirmPasswordPlaceholder.Visibility = Visibility.Hidden;
        }

        private void ConfirmPasswordTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ConfirmPasswordTextBox.Text))
                ConfirmPasswordPlaceholder.Visibility = Visibility.Visible;
        }

        private void PasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PasswordPlaceholder.Visibility = string.IsNullOrEmpty(PasswordTextBox.Text) ? Visibility.Visible : Visibility.Hidden;
        }

        private void ConfirmPasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConfirmPasswordPlaceholder.Visibility = string.IsNullOrEmpty(ConfirmPasswordTextBox.Text) ? Visibility.Visible : Visibility.Hidden;
        }

        // ================= PASSWORD PLACEHOLDERS =================
        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Password.Length == 0) PasswordPlaceholder.Visibility = Visibility.Hidden;
        }
        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Password.Length == 0) PasswordPlaceholder.Visibility = Visibility.Visible;
        }
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility = PasswordBox.Password.Length == 0 ? Visibility.Visible : Visibility.Hidden;
        }

        private void ConfirmPasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ConfirmPasswordBox.Password.Length == 0) ConfirmPasswordPlaceholder.Visibility = Visibility.Hidden;
        }
        private void ConfirmPasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (ConfirmPasswordBox.Password.Length == 0) ConfirmPasswordPlaceholder.Visibility = Visibility.Visible;
        }
        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ConfirmPasswordPlaceholder.Visibility = ConfirmPasswordBox.Password.Length == 0 ? Visibility.Visible : Visibility.Hidden;
        }

        // ================= LOGIN NAVIGATION =================
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow)?.NavigateToLogin();
        }

        // ================= REGISTER BUTTON =================
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var fullName = FullNameTextBox.Text?.Trim();
            var clinicName = ClinicNameTextBox.Text?.Trim();
            var username = UsernameTextBox.Text?.Trim();
            var email = EmailTextBox.Text?.Trim();
            var password = isPasswordVisible ? PasswordTextBox.Text : PasswordBox.Password;
            var confirm = isConfirmPasswordVisible ? ConfirmPasswordTextBox.Text : ConfirmPasswordBox.Password;

            // Treat placeholder values as empty
            if (string.IsNullOrEmpty(fullName) || fullName == "Full Name" ||
                string.IsNullOrEmpty(clinicName) || clinicName == "Clinic Name" ||
                string.IsNullOrEmpty(username) || username == "Username" ||
                string.IsNullOrEmpty(email) || email == "Email" ||
                string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please fill in all fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;             
            }

            if (password != confirm)
            {
                MessageBox.Show("Passwords do not match.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // TODO: Save registration details
            MessageBox.Show("Registration successful.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            (Application.Current.MainWindow as MainWindow)?.NavigateToLogin();
        }

        // ================= PASSWORD TOGGLE =================
        private void TogglePassword_Click(object sender, RoutedEventArgs e)
        {
            if (!isPasswordVisible)
            {
                PasswordTextBox.Text = PasswordBox.Password;
                PasswordBox.Visibility = Visibility.Collapsed;
                PasswordTextBox.Visibility = Visibility.Visible;
                PasswordEyeIcon.Text = "🙈";
                isPasswordVisible = true;

                PasswordPlaceholder.Visibility = string.IsNullOrEmpty(PasswordTextBox.Text) ? Visibility.Visible : Visibility.Hidden;
            }
            else
            {
                PasswordBox.Password = PasswordTextBox.Text;
                PasswordTextBox.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;
                PasswordEyeIcon.Text = "👁";
                isPasswordVisible = false;
           
                PasswordPlaceholder.Visibility = PasswordBox.Password.Length == 0 ? Visibility.Visible : Visibility.Hidden;
            }
        }

        private void ToggleConfirmPassword_Click(object sender, RoutedEventArgs e)
        {
            if (!isConfirmPasswordVisible)
            {
                ConfirmPasswordTextBox.Text = ConfirmPasswordBox.Password;
                ConfirmPasswordBox.Visibility = Visibility.Collapsed;
                ConfirmPasswordTextBox.Visibility = Visibility.Visible;
                ConfirmEyeIcon.Text = "🙈";
                isConfirmPasswordVisible = true;
                ConfirmPasswordPlaceholder.Visibility = string.IsNullOrEmpty(ConfirmPasswordTextBox.Text) ? Visibility.Visible : Visibility.Hidden;
            }
            else
            {
                ConfirmPasswordBox.Password = ConfirmPasswordTextBox.Text;
                ConfirmPasswordTextBox.Visibility = Visibility.Collapsed;
                ConfirmPasswordBox.Visibility = Visibility.Visible;
                ConfirmEyeIcon.Text = "👁";
                isConfirmPasswordVisible = false;
                ConfirmPasswordPlaceholder.Visibility = ConfirmPasswordBox.Password.Length == 0 ? Visibility.Visible : Visibility.Hidden;
            }
        }
    }
}
