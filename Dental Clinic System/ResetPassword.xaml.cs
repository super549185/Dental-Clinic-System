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
using System.Windows.Shapes;

namespace Dental_Clinic_System
{
    public partial class ResetPassword : Window
    {


        private int _currentStep = 1;

        // Step accent colors
        private readonly SolidColorBrush _stepBlue = new SolidColorBrush(Color.FromRgb(0x3B, 0x82, 0xF6));
        private readonly SolidColorBrush _stepPurple = new SolidColorBrush(Color.FromRgb(0x7C, 0x3A, 0xED));
        private readonly SolidColorBrush _stepGreen = new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81));
        private readonly SolidColorBrush _dotInactive = new SolidColorBrush(Color.FromRgb(0xD1, 0xD5, 0xDB));

        public ResetPassword()
        {
            InitializeComponent();
        }

        // ─── Drag Window ───────────────────────────────
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        // ─── Close ─────────────────────────────────────
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // ─── Back ──────────────────────────────────────
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStep > 1 && _currentStep < 4)
            {
                _currentStep--;
                NavigateToStep(_currentStep);
            }
        }

        // ─── Step 1 → 2 ───────────────────────────────
        private void BtnSendCode_Click(object sender, RoutedEventArgs e)
        {
            _currentStep = 2;
            NavigateToStep(2);
        }

        // ─── Step 2 → 3 ───────────────────────────────
        private void BtnVerifyCode_Click(object sender, RoutedEventArgs e)
        {
            _currentStep = 3;
            NavigateToStep(3);
        }

        // ─── Resend Code ──────────────────────────────
        private void ResendLink_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Verification code has been resent!", "Info",
                            MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ─── Step 3 → 4 ───────────────────────────────
        private void BtnResetPassword_Click(object sender, RoutedEventArgs e)
        {
            _currentStep = 4;
            NavigateToStep(4);
        }

        // ─── Step 4 Done ──────────────────────────────
        private void BtnDone_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // ─── Password Visibility Toggles ──────────────
        private void BtnToggleNewPwd_Click(object sender, RoutedEventArgs e)
        {
            TogglePasswordVisibility(TxtNewPassword, BtnToggleNewPwd);
        }

        private void BtnToggleConfirmPwd_Click(object sender, RoutedEventArgs e)
        {
            TogglePasswordVisibility(TxtConfirmPassword, BtnToggleConfirmPwd);
        }

        private void TogglePasswordVisibility(PasswordBox pwdBox, Button toggleBtn)
        {
            // Note: For a true toggle you'd swap to a TextBox.
            // This is a simplified version that clears/restores a placeholder approach.
            // In production, use a bound ViewModel with SecureString → string conversion.
            if (toggleBtn.Tag?.ToString() == "hidden")
            {
                toggleBtn.Tag = "visible";
                // In production: swap PasswordBox with TextBox showing the text
            }
            else
            {
                toggleBtn.Tag = "hidden";
            }
        }

        // ─── Navigation Logic ──────────────────────────
        private void NavigateToStep(int step)
        {
            // Hide all step panels
            Step1Content.Visibility = Visibility.Collapsed;
            Step2Content.Visibility = Visibility.Collapsed;
            Step3Content.Visibility = Visibility.Collapsed;
            Step4Content.Visibility = Visibility.Collapsed;

            // Reset all dots to inactive
            Dot1.Fill = _dotInactive;
            Dot2.Fill = _dotInactive;
            Dot3.Fill = _dotInactive;
            Dot4.Fill = _dotInactive;

            // Show the target step
            switch (step)
            {
                case 1:
                    Step1Content.Visibility = Visibility.Visible;
                    TxtTitle.Text = "Reset Password";
                    Dot1.Fill = _stepBlue;
                    BtnBack.Visibility = Visibility.Collapsed;
                    break;

                case 2:
                    Step2Content.Visibility = Visibility.Visible;
                    TxtTitle.Text = "Verify Code";
                    RunEmail2.Text = TxtEmail.Text;
                    Dot1.Fill = _stepBlue;
                    Dot2.Fill = _stepPurple;
                    BtnBack.Visibility = Visibility.Visible;
                    break;

                case 3:
                    Step3Content.Visibility = Visibility.Visible;
                    TxtTitle.Text = "New Password";
                    Dot1.Fill = _stepBlue;
                    Dot2.Fill = _stepPurple;
                    Dot3.Fill = _stepGreen;
                    BtnBack.Visibility = Visibility.Visible;
                    break;

                case 4:
                    Step4Content.Visibility = Visibility.Visible;
                    TxtTitle.Text = "Success!";
                    Dot1.Fill = _stepBlue;
                    Dot2.Fill = _stepPurple;
                    Dot3.Fill = _stepGreen;
                    Dot4.Fill = _stepGreen;
                    BtnBack.Visibility = Visibility.Collapsed;
                    break;
            }
        }
    }
}
