using Dental_Clinic_System.Data;
using Dental_Clinic_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Dental_Clinic_System.Dashboard
{
    public partial class AddInvoicePage : Page
    {
        private AppDbContext _dbContext;
        private List<ComboBox> _serviceComboBoxes = new List<ComboBox>(); // Tracks all added service dropdowns

        public AddInvoicePage()
        {
            InitializeComponent();
            _dbContext = new AppDbContext();
            LoadTodayPatients();

            // Automatically add 1 empty service row when page opens
            AddServiceRow();
        }

        private void LoadTodayPatients()
        {
            string todayStr = DateTime.Today.ToString("yyyy-MM-dd");
            var todayPatients = _dbContext.Appointments
                .Where(a => a.Date == todayStr)
                .Select(a => a.PatientName)
                .Distinct()
                .ToList();

            CmbPatient.ItemsSource = todayPatients;
            if (todayPatients.Count == 0)
                CmbPatient.ItemsSource = new List<string> { "No appointments today" };
            else
                CmbPatient.SelectedIndex = 0;
        }

        // --- THIS IS THE PLUS (+) BUTTON LOGIC ---
        private void AddServiceRow_Click(object sender, RoutedEventArgs e)
        {
            AddServiceRow();
        }

        private void AddServiceRow()
        {
            // Create a horizontal row for 1 service
            Grid rowGrid = new Grid { Margin = new Thickness(0, 0, 0, 8) };
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // ComboBox
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90, GridUnitType.Pixel) }); // Price Display
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40, GridUnitType.Pixel) }); // Minus Button

            // 1. Service ComboBox
            ComboBox cmb = new ComboBox
            {
                Height = 35,
                Padding = new Thickness(8),
                FontSize = 13,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xD1, 0xD5, 0xDB)),
                BorderThickness = new Thickness(1)
            };
            cmb.ItemsSource = _dbContext.Services.ToList();
            cmb.DisplayMemberPath = "Name";

            // 2. Price TextBlock (Shows price of selected service)
            TextBlock priceTxt = new TextBlock
            {
                Text = "₱0.00",
                FontSize = 13,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0)
            };
            cmb.Tag = priceTxt; // Attach price text to combobox so we can update it easily

            // Event: When user selects a service, update its price and recalculate total
            cmb.SelectionChanged += (s, e) =>
            {
                if (cmb.SelectedItem is ServiceItem svc)
                {
                    string cleanPrice = svc.Price.Replace("₱", "").Replace(",", "");
                    if (decimal.TryParse(cleanPrice, out decimal p))
                        priceTxt.Text = $"₱{p:F2}";
                    else
                        priceTxt.Text = "₱0.00";
                }
                else
                {
                    priceTxt.Text = "₱0.00";
                }
                CalculateTotal(); // Recalculate entire total
            };

            // 3. Minus (-) Button to remove this specific row
            Button removeBtn = new Button
            {
                Content = "−",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Background = new SolidColorBrush(Color.FromRgb(0xFE, 0xE2, 0xE2)), // Light red
                Foreground = new SolidColorBrush(Color.FromRgb(0x99, 0x1B, 0x1B)),
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                Width = 30,
                Height = 30,
                Margin = new Thickness(5, 0, 0, 0)
            };
            removeBtn.Click += (s, e) =>
            {
                ServicesPanel.Children.Remove(rowGrid); // Remove UI
                _serviceComboBoxes.Remove(cmb);         // Remove from tracking list
                CalculateTotal();                       // Recalculate total
            };

            // Put them together in the grid row
            Grid.SetColumn(cmb, 0);
            Grid.SetColumn(priceTxt, 1);
            Grid.SetColumn(removeBtn, 2);

            rowGrid.Children.Add(cmb);
            rowGrid.Children.Add(priceTxt);
            rowGrid.Children.Add(removeBtn);

            // Add to UI and tracking list
            ServicesPanel.Children.Add(rowGrid);
            _serviceComboBoxes.Add(cmb);
        }

        // --- CALCULATES THE SUM OF ALL SELECTED SERVICES ---
        private void CalculateTotal()
        {
            decimal total = 0;
            foreach (var cmb in _serviceComboBoxes)
            {
                if (cmb.SelectedItem is ServiceItem svc)
                {
                    string cleanPrice = svc.Price.Replace("₱", "").Replace(",", "");
                    if (decimal.TryParse(cleanPrice, out decimal p))
                        total += p;
                }
            }
            TxtAmount.Text = total.ToString("F2"); // Updates the Total Amount TextBox
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

        private void SaveInvoice_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validate Patient
            if (CmbPatient.SelectedItem == null || CmbPatient.SelectedItem.ToString() == "No appointments today")
            {
                MessageBox.Show("Please select a valid patient.", "Missing Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Check if at least one service is selected
            var selectedServices = _serviceComboBoxes
                .Where(c => c.SelectedItem != null)
                .Select(c => (c.SelectedItem as ServiceItem).Name)
                .ToList();

            if (selectedServices.Count == 0)
            {
                MessageBox.Show("Please select at least one service.", "Missing Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Join multiple services with a comma (e.g., "Dental Cleaning, Tooth Extraction")
                string serviceString = string.Join(", ", selectedServices);

                // 3. Create Invoice
                BillingItem newInvoice = new BillingItem
                {
                    InvoiceNo = "INV-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                    Patient = CmbPatient.SelectedItem.ToString(),
                    Date = DateTime.Today.ToString("yyyy-MM-dd"),
                    DueDate = DateTime.Today.ToString("yyyy-MM-dd"), // <-- ADDED THIS LINE
                    Service = serviceString,
                    Amount = decimal.Parse(TxtAmount.Text),
                    PaymentMethod = (CmbPaymentMethod.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    Status = (CmbStatus.SelectedItem as ComboBoxItem)?.Content.ToString()
                };

                _dbContext.Billings.Add(newInvoice);
                _dbContext.SaveChanges();

                MessageBox.Show("Invoice created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                if (NavigationService != null && NavigationService.CanGoBack)
                    NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving invoice: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}