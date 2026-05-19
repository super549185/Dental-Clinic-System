using Dental_Clinic_System.Models;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Dental_Clinic_System.Dashboard
{
    public partial class SettingsPage : Page
    {
        // Tracks the currently active nav border
        private Border _activeNav;

        public SettingsPage()
        {
            InitializeComponent();
            // Default to Clinic Information on load
            ActivateNav(NavClinic);
            ShowClinicPanel();
        }

        // ─────────────────────────────────────────────────────────────
        // NAV ROUTING
        // ─────────────────────────────────────────────────────────────
        private void Nav_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border nav)
            {
                ActivateNav(nav);
                switch (nav.Tag?.ToString())
                {
                    case "Clinic": ShowClinicPanel(); break;
                    case "Account": ShowAccountPanel(); break;
                    case "Appointments": ShowAppointmentsPanel(); break;
                    case "Archive": ShowArchivePatientsPanel(); break;
                    case "Notifications": ShowNotificationsPanel(); break;
                    case "Database": ShowDatabasePanel(); break;
                    case "Guide": ShowGuidePanel(); break;
                    case "About": ShowAboutPanel(); break;
                }
            }
        }

        private void ActivateNav(Border target)
        {
            // Reset previous
            if (_activeNav != null)
            {
                _activeNav.Background = Brushes.Transparent;
                if (_activeNav.Child is StackPanel sp)
                    foreach (var child in sp.Children)
                        if (child is TextBlock tb && tb.FontSize == 13)
                            tb.Foreground = new SolidColorBrush(Color.FromRgb(0x37, 0x41, 0x51));
            }
            // Highlight new
            target.Background = new SolidColorBrush(Color.FromRgb(0xF0, 0xFD, 0xFA));
            if (target.Child is StackPanel sp2)
                foreach (var child in sp2.Children)
                    if (child is TextBlock tb2 && tb2.FontSize == 13)
                        tb2.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x6B, 0x6B));

            _activeNav = target;
        }

        // ─────────────────────────────────────────────────────────────
        // PANEL BUILDERS
        // ─────────────────────────────────────────────────────────────

        // ── 1. CLINIC INFORMATION ──────────────────────────────────
        private void ShowClinicPanel()
        {
            SettingsContent.Children.Clear();

            SettingsContent.Children.Add(MakeSectionHeader("🏥", "Clinic Information",
                "Update your clinic's basic details shown across the system."));

            var card = MakeCard();
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(15) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(14) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(14) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Row 0: Clinic Name + Phone
            AddLabeledField(grid, "Clinic Name", "e.g. Smile Dental Clinic", 0, 0);
            AddLabeledField(grid, "Phone Number", "e.g. (555) 123-4567", 0, 2);
            // Row 2: Email + Address
            AddLabeledField(grid, "Email Address", "e.g. info@clinic.com", 2, 0);
            AddLabeledField(grid, "Address", "Street, City, ZIP", 2, 2);
            // Row 4: Operating Hours (full width)
            AddLabeledField(grid, "Operating Hours", "e.g. Mon–Fri  8:00 AM – 6:00 PM", 4, 0, colSpan: 3);

            card.Child = grid;
            SettingsContent.Children.Add(card);
            SettingsContent.Children.Add(MakeSaveButton("Save Clinic Info", null));
        }

        // ── 2. ACCOUNT & SECURITY ──────────────────────────────────
        private void ShowAccountPanel()
        {
            SettingsContent.Children.Clear();
            SettingsContent.Children.Add(MakeSectionHeader("👤", "Account & Security",
                "Update your login credentials and keep your account secure."));

            // Change password — step by step guide
            SettingsContent.Children.Add(MakeSubHeader("Change Password"));
            SettingsContent.Children.Add(MakeStepGuide(new[]
            {
                ("Step 1", "Enter your current password in the 'Current Password' field."),
                ("Step 2", "Type your new password — minimum 8 characters, mix of letters and numbers."),
                ("Step 3", "Re-enter the new password in 'Confirm New Password'."),
                ("Step 4", "Click 'Save Password'. You will be logged out and asked to log in again.")
            }));

            var card = MakeCard();
            var sp = new StackPanel();
            sp.Children.Add(MakeFieldLabel("Current Password"));
            sp.Children.Add(MakePasswordBox());
            sp.Children.Add(new Border { Height = 12 });
            sp.Children.Add(MakeFieldLabel("New Password"));
            sp.Children.Add(MakePasswordBox());
            sp.Children.Add(new Border { Height = 12 });
            sp.Children.Add(MakeFieldLabel("Confirm New Password"));
            sp.Children.Add(MakePasswordBox());
            card.Child = sp;
            SettingsContent.Children.Add(card);
            SettingsContent.Children.Add(MakeSaveButton("Save Password", null));

            // Change username
            SettingsContent.Children.Add(new Border { Height = 20 });
            SettingsContent.Children.Add(MakeSubHeader("Change Username"));
            var card2 = MakeCard();
            var sp2 = new StackPanel();
            sp2.Children.Add(MakeFieldLabel("New Username"));
            sp2.Children.Add(MakeTextBox("Enter new username"));
            card2.Child = sp2;
            SettingsContent.Children.Add(card2);
            SettingsContent.Children.Add(MakeSaveButton("Save Username", null));
        }

        // ── 3. APPOINTMENTS ───────────────────────────────────────
        private void ShowAppointmentsPanel()
        {
            SettingsContent.Children.Clear();
            SettingsContent.Children.Add(MakeSectionHeader("📅", "Appointment Settings",
                "Configure how appointments are scheduled and managed."));

            SettingsContent.Children.Add(MakeSubHeader("How to Add a New Appointment"));
            SettingsContent.Children.Add(MakeStepGuide(new[]
            {
                ("Step 1", "Go to the Appointments page from the sidebar."),
                ("Step 2", "Click the green 'Add Appointment' button in the top-right corner."),
                ("Step 3", "Fill in the Patient Name field with the patient's full name."),
                ("Step 4", "Select a Dentist from the dropdown. Only on-duty dentists appear by default; off-duty dentists are labeled '(Off Duty)' and can still be selected."),
                ("Step 5", "Pick a Date using the calendar picker."),
                ("Step 6", "Choose a Time — the available slots are automatically limited to the selected dentist's working hours."),
                ("Step 7", "Select a Service — only services offered by the chosen dentist will appear."),
                ("Step 8", "Set the Status to 'Confirmed' or 'Cancelled'."),
                ("Step 9", "Click 'Save Appointment'. The system will check for scheduling conflicts before saving.")
            }));

            SettingsContent.Children.Add(new Border { Height = 16 });
            SettingsContent.Children.Add(MakeSubHeader("How to Edit an Appointment"));
            SettingsContent.Children.Add(MakeStepGuide(new[]
            {
                ("Step 1", "Go to the Appointments page."),
                ("Step 2", "Find the appointment you want to change."),
                ("Step 3", "Click the blue '✏ Edit' button on that row."),
                ("Step 4", "Update any fields — dentist, date, time, service, or status."),
                ("Step 5", "Click 'Save Appointment'. Conflict checks run automatically.")
            }));

            SettingsContent.Children.Add(new Border { Height = 16 });
            SettingsContent.Children.Add(MakeSubHeader("How to Delete an Appointment"));
            SettingsContent.Children.Add(MakeStepGuide(new[]
            {
                ("Step 1", "Find the appointment in the list."),
                ("Step 2", "Click the red '🗑 Delete' button on that row."),
                ("Step 3", "A confirmation dialog will appear showing the appointment details."),
                ("Step 4", "Click 'Yes' to permanently delete, or 'No' to cancel.")
            }));

            SettingsContent.Children.Add(new Border { Height = 16 });
            SettingsContent.Children.Add(MakeSubHeader("Understanding Conflict Checks"));
            SettingsContent.Children.Add(MakeInfoBox(
                "The system automatically prevents:\n\n" +
                "• A dentist being double-booked at the same date and time.\n" +
                "• The same patient being booked with the same dentist on the same day.\n\n" +
                "If a conflict is found, a detailed error message will show exactly which " +
                "appointment is conflicting and how to resolve it."));
        }

        // ── 4. NOTIFICATIONS ──────────────────────────────────────
        private void ShowNotificationsPanel()
        {
            SettingsContent.Children.Clear();
            SettingsContent.Children.Add(MakeSectionHeader("🔔", "Notification Settings",
                "Control which alerts and reminders the system shows."));

            var card = MakeCard();
            var sp = new StackPanel();

            sp.Children.Add(MakeToggleRow("Show low-stock inventory alerts on Dashboard", true));
            sp.Children.Add(MakeDivider());
            sp.Children.Add(MakeToggleRow("Show appointment reminders for today", true));
            sp.Children.Add(MakeDivider());
            sp.Children.Add(MakeToggleRow("Warn when booking an off-duty dentist", true));
            sp.Children.Add(MakeDivider());
            sp.Children.Add(MakeToggleRow("Show scheduling conflict warnings", true));
            sp.Children.Add(MakeDivider());
            sp.Children.Add(MakeToggleRow("Play a sound on successful save", false));

            card.Child = sp;
            SettingsContent.Children.Add(card);
            SettingsContent.Children.Add(MakeSaveButton("Save Notification Preferences", null));
        }

        // ── 5. DATABASE & BACKUP ──────────────────────────────────
        private void ShowDatabasePanel()
        {
            SettingsContent.Children.Clear();
            SettingsContent.Children.Add(MakeSectionHeader("🗄", "Database & Backup",
                "Manage your local database and keep your data safe."));

            SettingsContent.Children.Add(MakeSubHeader("How to Back Up Your Data"));
            SettingsContent.Children.Add(MakeStepGuide(new[]
            {
                ("Step 1", "Click 'Export Backup' below. A save dialog will open."),
                ("Step 2", "Choose a folder — ideally an external drive or cloud-synced folder."),
                ("Step 3", "Click Save. The system exports the full database as a .db file."),
                ("Step 4", "Store backups regularly — recommended once a week.")
            }));

            var card = MakeCard();
            var sp = new StackPanel();

            sp.Children.Add(new TextBlock
            {
                Text = $"Current database location:\n{GetDbPath()}",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80)),
                Margin = new Thickness(0, 0, 0, 15),
                TextWrapping = TextWrapping.Wrap
            });

            var btnRow = new StackPanel { Orientation = Orientation.Horizontal };
            btnRow.Children.Add(MakeActionButton("📤  Export Backup", "#006B6B", "#005555", ExportBackup_Click));
            btnRow.Children.Add(new Border { Width = 12 });
            btnRow.Children.Add(MakeActionButton("📥  Restore Backup", "#4B5563", "#374151", RestoreBackup_Click));

            sp.Children.Add(btnRow);
            card.Child = sp;
            SettingsContent.Children.Add(card);

            SettingsContent.Children.Add(new Border { Height = 20 });
            SettingsContent.Children.Add(MakeSubHeader("Reset Data"));
            SettingsContent.Children.Add(MakeInfoBox(
                "⚠  Use with extreme caution.\n\n" +
                "Resetting will permanently erase ALL records (appointments, patients, staff, " +
                "inventory, and services). This cannot be undone. Always export a backup first."));
            SettingsContent.Children.Add(new Border { Height = 10 });
            SettingsContent.Children.Add(MakeActionButton("🗑  Reset All Data", "#EF4444", "#DC2626", ResetData_Click));
        }

        // ── 6. HOW-TO GUIDE ───────────────────────────────────────
        private void ShowGuidePanel()
        {
            SettingsContent.Children.Clear();
            SettingsContent.Children.Add(MakeSectionHeader("📖", "How-To Guide",
                "Step-by-step instructions for all major features."));

            // ── Patients
            SettingsContent.Children.Add(MakeSubHeader("Managing Patients"));
            SettingsContent.Children.Add(MakeStepGuide(new[]
            {
                ("Add",    "Click 'New Patient', fill in the form fields, then click 'Save Patient'."),
                ("View",   "Click the 👁 icon on any patient row to open their full profile."),
                ("Edit",   "Open the patient profile, then click 'Edit Profile' in the top-right."),
                ("Delete", "Click the ✕ icon on the patient row and confirm the deletion.")
            }));

            SettingsContent.Children.Add(new Border { Height = 16 });

            // ── Staff
            SettingsContent.Children.Add(MakeSubHeader("Managing Dentists & Staff"));
            SettingsContent.Children.Add(MakeStepGuide(new[]
            {
                ("Add",      "Click 'Add Staff Member', fill in name, role, schedule, and services offered."),
                ("Edit",     "Click the ✏️ icon on a staff card to edit their details."),
                ("Profile",  "Click anywhere on a staff card (not the buttons) to open the staff profile."),
                ("Delete",   "Click the ✕ icon on a staff card and confirm. Past appointments are kept.")
            }));

            SettingsContent.Children.Add(new Border { Height = 16 });

            // ── Services
            SettingsContent.Children.Add(MakeSubHeader("Managing Services"));
            SettingsContent.Children.Add(MakeStepGuide(new[]
            {
                ("Add",    "Click 'Add Service', fill in the name, category, price, duration, and description."),
                ("Edit",   "Click the ✏️ icon on a service row. The form will re-open pre-filled."),
                ("Delete", "Click the 🗑️ icon on a service row and confirm.")
            }));

            SettingsContent.Children.Add(new Border { Height = 16 });

            // ── Inventory
            SettingsContent.Children.Add(MakeSubHeader("Managing Inventory"));
            SettingsContent.Children.Add(MakeStepGuide(new[]
            {
                ("Add item",    "Click 'Add Item' and fill in name, category, quantity, reorder level, and expiry date."),
                ("Adjust qty",  "Use the '+' and '−' buttons on each row to increase or decrease stock."),
                ("Reorder",     "Items at or below the reorder level appear in the red alert panel. Click 'Reorder' to replenish."),
                ("Delete",      "Click the 🗑️ icon on a row and confirm.")
            }));
        }

        // ── 7. ABOUT ──────────────────────────────────────────────
        private void ShowAboutPanel()
        {
            SettingsContent.Children.Clear();
            SettingsContent.Children.Add(MakeSectionHeader("ℹ", "About System",
                "Version and developer information."));

            var card = MakeCard();
            var sp = new StackPanel();

            void AddRow(string label, string value)
            {
                var row = new Grid { Margin = new Thickness(0, 0, 0, 12) };
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(160) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                row.Children.Add(new TextBlock { Text = label, FontSize = 13, Foreground = new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80)), FontWeight = FontWeights.Medium });
                var val = new TextBlock { Text = value, FontSize = 13, Foreground = new SolidColorBrush(Color.FromRgb(0x11, 0x18, 0x27)), FontWeight = FontWeights.SemiBold };
                Grid.SetColumn(val, 1);
                row.Children.Add(val);
                sp.Children.Add(row);
            }

            AddRow("System Name", "Dental Clinic Management System");
            AddRow("Version", "1.0.0");
            AddRow("Framework", ".NET 10 (WPF)");
            AddRow("Database", "SQLite via Entity Framework Core");
            AddRow("Build Date", DateTime.Today.ToString("MMMM dd, yyyy"));

            sp.Children.Add(new Border
            {
                Height = 1,
                Background = new SolidColorBrush(Color.FromRgb(0xE5, 0xE7, 0xEB)),
                Margin = new Thickness(0, 8, 0, 16)
            });
            sp.Children.Add(new TextBlock
            {
                Text = "© 2026 Dental Clinic System. All rights reserved.",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(0x9C, 0xA3, 0xAF))
            });

            card.Child = sp;
            SettingsContent.Children.Add(card);
        }

        // ─────────────────────────────────────────────────────────────
        // ★ FIXED: DATABASE PATH HELPER & BUTTON HANDLERS
        // ─────────────────────────────────────────────────────────────

        // Matches the exact path used in AppDbContext.cs
        private string GetDbPath()
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Dental_Clinic_System");
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, "DentalClinic.db");
        }

        private void ExportBackup_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Export Database Backup",
                Filter = "Database files (*.db)|*.db|All files (*.*)|*.*",
                FileName = $"DentalClinic_Backup_{DateTime.Today:yyyy-MM-dd}.db"
            };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    // Force close any open connections before copying
                    Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

                    System.IO.File.Copy(GetDbPath(), dlg.FileName, overwrite: true);
                    MessageBox.Show("Backup exported successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Export failed:\n" + ex.Message, "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RestoreBackup_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Restore Database Backup",
                Filter = "Database files (*.db)|*.db|All files (*.*)|*.*"
            };
            if (dlg.ShowDialog() == true)
            {
                var confirm = MessageBox.Show(
                    "Restoring will REPLACE the current database.\nAll unsaved changes will be lost.\n\nContinue?",
                    "Confirm Restore", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (confirm == MessageBoxResult.Yes)
                {
                    try
                    {
                        string targetDb = GetDbPath();

                        // ★ CRITICAL: Force close all SQLite connections so the file isn't locked
                        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

                        // Delete the old database completely to ensure a clean swap
                        if (System.IO.File.Exists(targetDb))
                        {
                            System.IO.File.Delete(targetDb);
                        }

                        // Copy the backup in
                        System.IO.File.Copy(dlg.FileName, targetDb);

                        // Delete the seed flag so imported data loads without crashing
                        string flagFile = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            "Dental_Clinic_System", "seeded.flag");
                        if (System.IO.File.Exists(flagFile))
                            System.IO.File.Delete(flagFile);

                        MessageBox.Show("Backup restored successfully! Please close and restart the application.",
                            "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Restore failed:\n" + ex.Message, "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ResetData_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show(
                "⚠  WARNING: This will permanently delete ALL data.\n\n" +
                "This includes all patients, appointments, staff, inventory, and services.\n\n" +
                "This action CANNOT be undone. Are you absolutely sure?",
                "Confirm Reset", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new Data.AppDbContext())
                    {
                        db.Appointments.RemoveRange(db.Appointments);
                        db.Patients.RemoveRange(db.Patients);
                        db.Staff.RemoveRange(db.Staff);
                        db.Inventory.RemoveRange(db.Inventory);
                        db.Services.RemoveRange(db.Services);
                        db.DentalHistory.RemoveRange(db.DentalHistory);
                        db.SaveChanges();
                    }

                    // ★ FIXED: Clear the seed flag so appointments re-appear on a true reset
                    string flagFile = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Dental_Clinic_System", "seeded.flag");
                    if (File.Exists(flagFile)) File.Delete(flagFile);

                    MessageBox.Show("All data has been reset. The system will now restart.",
                        "Reset Complete", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Restart
                    System.Windows.Application.Current.Shutdown();
                    System.Diagnostics.Process.Start(
                        System.Windows.Application.ResourceAssembly.Location);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Reset failed:\n" + ex.Message, "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ─────────────────────────────────────────────────────────────
        // UI COMPONENT HELPERS
        // ─────────────────────────────────────────────────────────────

        private Border MakeSectionHeader(string icon, string title, string subtitle)
        {
            var sp = new StackPanel { Margin = new Thickness(0, 0, 0, 20) };
            sp.Children.Add(new TextBlock
            {
                Text = $"{icon}  {title}",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0x11, 0x18, 0x27))
            });
            sp.Children.Add(new TextBlock
            {
                Text = subtitle,
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80)),
                Margin = new Thickness(0, 4, 0, 0)
            });
            return new Border { Child = sp };
        }

        private Border MakeSubHeader(string text)
        {
            return new Border
            {
                Margin = new Thickness(0, 0, 0, 10),
                Child = new TextBlock
                {
                    Text = text,
                    FontSize = 15,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x11, 0x18, 0x27))
                }
            };
        }

        private Border MakeStepGuide((string label, string desc)[] steps)
        {
            var sp = new StackPanel { Margin = new Thickness(0, 0, 0, 4) };

            for (int i = 0; i < steps.Length; i++)
            {
                var row = new Grid { Margin = new Thickness(0, 0, 0, 10) };
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(32) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                Border circle = new Border
                {
                    Width = 26,
                    Height = 26,
                    CornerRadius = new CornerRadius(13),
                    Background = new SolidColorBrush(Color.FromRgb(0x00, 0x6B, 0x6B)),
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, 1, 0, 0)
                };
                circle.Child = new TextBlock
                {
                    Text = (i + 1).ToString(),
                    FontSize = 11,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                row.Children.Add(circle);

                var textSp = new StackPanel { Margin = new Thickness(8, 0, 0, 0) };
                textSp.Children.Add(new TextBlock
                {
                    Text = steps[i].label,
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x6B, 0x6B))
                });
                textSp.Children.Add(new TextBlock
                {
                    Text = steps[i].desc,
                    FontSize = 13,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x37, 0x41, 0x51)),
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 2, 0, 0)
                });
                Grid.SetColumn(textSp, 1);
                row.Children.Add(textSp);

                sp.Children.Add(row);
            }

            return new Border
            {
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xE5, 0xE7, 0xEB)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(20),
                Margin = new Thickness(0, 0, 0, 16),
                Child = sp
            };
        }

        private Border MakeInfoBox(string text)
        {
            return new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0xF0, 0xFD, 0xFA)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x6E, 0xE7, 0xB7)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 0, 16),
                Child = new TextBlock
                {
                    Text = text,
                    FontSize = 13,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x06, 0x4E, 0x3B)),
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 20
                }
            };
        }

        private Border MakeCard()
        {
            return new Border
            {
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xE5, 0xE7, 0xEB)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(20),
                Margin = new Thickness(0, 0, 0, 16)
            };
        }

        private Border MakeDivider() => new Border
        {
            Height = 1,
            Background = new SolidColorBrush(Color.FromRgb(0xF3, 0xF4, 0xF6)),
            Margin = new Thickness(0, 4, 0, 4)
        };

        private void AddLabeledField(Grid grid, string label, string placeholder,
            int row, int col, int colSpan = 1)
        {
            var sp = new StackPanel();
            sp.Children.Add(new TextBlock
            {
                Text = label.ToUpper(),
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80)),
                Margin = new Thickness(0, 0, 0, 5)
            });
            sp.Children.Add(MakeTextBox(placeholder));
            Grid.SetRow(sp, row);
            Grid.SetColumn(sp, col);
            if (colSpan > 1) Grid.SetColumnSpan(sp, colSpan);
            grid.Children.Add(sp);
        }

        private TextBlock MakeFieldLabel(string text) => new TextBlock
        {
            Text = text.ToUpper(),
            FontSize = 11,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80)),
            Margin = new Thickness(0, 0, 0, 5)
        };

        private TextBox MakeTextBox(string placeholder) => new TextBox
        {
            Height = 38,
            Padding = new Thickness(10, 0, 10, 0),
            Background = new SolidColorBrush(Color.FromRgb(0xF9, 0xFA, 0xFB)),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0xE5, 0xE7, 0xEB)),
            FontSize = 13,
            VerticalContentAlignment = VerticalAlignment.Center,
            Text = placeholder,
            Foreground = new SolidColorBrush(Color.FromRgb(0x9C, 0xA3, 0xAF))
        };

        private PasswordBox MakePasswordBox() => new PasswordBox
        {
            Height = 38,
            Padding = new Thickness(10, 0, 10, 0),
            Background = new SolidColorBrush(Color.FromRgb(0xF9, 0xFA, 0xFB)),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0xE5, 0xE7, 0xEB)),
            FontSize = 13
        };

        private Border MakeToggleRow(string label, bool isOn)
        {
            var grid = new Grid { Margin = new Thickness(0, 8, 0, 8) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            grid.Children.Add(new TextBlock
            {
                Text = label,
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(0x37, 0x41, 0x51)),
                VerticalAlignment = VerticalAlignment.Center
            });

            Border toggle = new Border
            {
                Width = 44,
                Height = 24,
                CornerRadius = new CornerRadius(12),
                Background = isOn
                    ? new SolidColorBrush(Color.FromRgb(0x00, 0x6B, 0x6B))
                    : new SolidColorBrush(Color.FromRgb(0xD1, 0xD5, 0xDB)),
                Cursor = Cursors.Hand,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
            bool state = isOn;
            toggle.MouseLeftButtonUp += (s, e) =>
            {
                state = !state;
                toggle.Background = state
                    ? new SolidColorBrush(Color.FromRgb(0x00, 0x6B, 0x6B))
                    : new SolidColorBrush(Color.FromRgb(0xD1, 0xD5, 0xDB));
            };
            Grid.SetColumn(toggle, 1);
            grid.Children.Add(toggle);

            return new Border { Child = grid };
        }

        private Button MakeSaveButton(string label, RoutedEventHandler handler)
        {
            var btn = new Button
            {
                Content = label,
                Height = 40,
                Padding = new Thickness(24, 0, 24, 0),
                Background = new SolidColorBrush(Color.FromRgb(0x00, 0x6B, 0x6B)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 24)
            };
            if (handler != null) btn.Click += handler;
            else btn.Click += (s, e) =>
                MessageBox.Show("Settings saved!", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            return btn;
        }

        private Button MakeActionButton(string label, string bgHex, string hoverHex, RoutedEventHandler handler)
        {
            var bg = HexBrush(bgHex);
            var hover = HexBrush(hoverHex);

            var btn = new Button
            {
                Content = label,
                Height = 38,
                Padding = new Thickness(18, 0, 18, 0),
                Background = bg,
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand
            };
            btn.MouseEnter += (s, e) => btn.Background = hover;
            btn.MouseLeave += (s, e) => btn.Background = bg;
            if (handler != null) btn.Click += handler;
            return btn;
        }

        private SolidColorBrush HexBrush(string hex)
        {
            try
            {
                byte r = byte.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
                byte g = byte.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
                byte b = byte.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
                return new SolidColorBrush(Color.FromRgb(r, g, b));
            }
            catch { return Brushes.Gray; }
        }

        private void ShowArchivePatientsPanel()
        {
            SettingsContent.Children.Clear();
            SettingsContent.Children.Add(MakeSectionHeader("📦", "Archived Patients",
                "Manage deleted/archived patient records."));

            using (var db = new Data.AppDbContext())
            {
                var archivedPatients = db.Patients.Where(p => p.IsArchived).ToList();

                if (archivedPatients.Count == 0)
                {
                    SettingsContent.Children.Add(MakeInfoBox("No archived patients yet."));
                    return;
                }

                var card = MakeCard();
                var sp = new StackPanel();

                foreach (var patient in archivedPatients)
                {
                    Border patientRow = new Border
                    {
                        Height = 60,
                        Background = Brushes.White,
                        BorderBrush = new SolidColorBrush(Color.FromRgb(0xF3, 0xF4, 0xF6)),
                        BorderThickness = new Thickness(0, 0, 0, 1),
                        Padding = new Thickness(15, 10, 15, 10),
                        Margin = new Thickness(0, 0, 0, 5)
                    };

                    Grid grid = new Grid();
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    StackPanel info = new StackPanel();
                    info.Children.Add(new TextBlock
                    {
                        Text = patient.Name,
                        FontSize = 14,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = new SolidColorBrush(Color.FromRgb(0x11, 0x18, 0x27))
                    });
                    info.Children.Add(new TextBlock
                    {
                        Text = $"Archived: {patient.ArchiveDate} | Reason: {patient.ArchiveReason}",
                        FontSize = 11,
                        Foreground = new SolidColorBrush(Color.FromRgb(0x9C, 0xA3, 0xAF)),
                        Margin = new Thickness(0, 4, 0, 0)
                    });
                    grid.Children.Add(info);

                    Button restoreBtn = new Button
                    {
                        Content = "↩️  Restore",
                        Background = new SolidColorBrush(Color.FromRgb(0x06, 0x5F, 0x46)),
                        Foreground = Brushes.White,
                        BorderThickness = new Thickness(0),
                        FontSize = 12,
                        Padding = new Thickness(10, 6, 10, 6),
                        Cursor = Cursors.Hand,
                        Margin = new Thickness(10, 0, 0, 0)
                    };
                    restoreBtn.Click += (s, e) => RestorePatient(patient);
                    Grid.SetColumn(restoreBtn, 1);
                    grid.Children.Add(restoreBtn);

                    Button deleteBtn = new Button
                    {
                        Content = "🗑️  Delete",
                        Background = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)),
                        Foreground = Brushes.White,
                        BorderThickness = new Thickness(0),
                        FontSize = 12,
                        Padding = new Thickness(10, 6, 10, 6),
                        Cursor = Cursors.Hand,
                        Margin = new Thickness(5, 0, 0, 0)
                    };
                    deleteBtn.Click += (s, e) => PermanentlyDeletePatient(patient);
                    Grid.SetColumn(deleteBtn, 2);
                    grid.Children.Add(deleteBtn);

                    patientRow.Child = grid;
                    sp.Children.Add(patientRow);
                }

                card.Child = sp;
                SettingsContent.Children.Add(card);
            }
        }

        private void RestorePatient(PatientItem patient)
        {
            var result = MessageBox.Show(
                $"Restore patient: {patient.Name}?", "Restore Patient",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                patient.IsArchived = false;
                patient.ArchiveDate = null;
                patient.ArchiveReason = null;

                using (var db = new Data.AppDbContext())
                {
                    db.Patients.Update(patient);
                    db.SaveChanges();
                }

                MessageBox.Show("Patient restored successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                ShowArchivePatientsPanel();
            }
        }

        private void PermanentlyDeletePatient(PatientItem patient)
        {
            var result = MessageBox.Show(
                $"⚠️  Permanently delete patient: {patient.Name}?\n\n" +
                "This cannot be undone and will also delete:\n" +
                "- All their appointments\n" +
                "- All their dental history\n\n" +
                "Continue?", "Permanent Delete",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                using (var db = new Data.AppDbContext())
                {
                    var history = db.DentalHistory.Where(h => h.PatientId == patient.PatientId).ToList();
                    db.DentalHistory.RemoveRange(history);

                    var appointments = db.Appointments.Where(a => a.PatientName == patient.Name).ToList();
                    db.Appointments.RemoveRange(appointments);

                    db.Patients.Remove(patient);
                    db.SaveChanges();
                }

                MessageBox.Show("Patient permanently deleted!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                ShowArchivePatientsPanel();
            }
        }
    }
}