using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Dental_Clinic_System.Data;
using Dental_Clinic_System.Models;

namespace Dental_Clinic_System.Dashboard
{
    public partial class BillingPage : Page
    {
        private AppDbContext _dbContext;
        private List<BillingItem> _billings;

        public BillingPage()
        {
            InitializeComponent();
            _dbContext = new AppDbContext();
            _dbContext.Database.EnsureCreated(); // EnsureDeleted is already removed
            LoadBillingData();
        }

        private void LoadBillingData()
        {
            try
            {
                _billings = _dbContext.Billings.ToList();

                // Calculate statistics
                decimal totalRevenue = _billings.Sum(b => b.Amount);
                decimal paidAmount = _billings.Where(b => b.Status == "Paid").Sum(b => b.Amount);
                decimal pendingAmount = _billings.Where(b => b.Status == "Pending" || b.Status == "Partial").Sum(b => b.Amount);

                // Update statistics TextBlocks
                TotalRevenueText.Text = $"₱{totalRevenue:F2}";
                PaidInvoicesText.Text = $"₱{paidAmount:F2}";
                PendingInvoicesText.Text = $"₱{pendingAmount:F2}";
                TotalBillingsText.Text = _billings.Count.ToString();

                // Bind data grid
                BillingDataGrid.ItemsSource = _billings.OrderByDescending(b => b.Date).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading billing data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewReceipt_Click(object sender, RoutedEventArgs e)
        {
            // Get the exact row where the "View" button was clicked
            if (sender is Button btn && btn.DataContext is BillingItem billing)
            {
                // Pass the billing data to the ReceiptPage
                NavigationService?.Navigate(new ReceiptPage(billing));
            }
        }

        private void EditBilling_Click(object sender, RoutedEventArgs e)
        {
            if (BillingDataGrid.SelectedItem is BillingItem billing)
            {
                MessageBox.Show($"Edit functionality for Invoice: {billing.InvoiceNo}", "Edit Billing", MessageBoxButton.OK, MessageBoxImage.Information);
                // TODO: Open edit window/dialog
            }
        }

        private void DeleteBilling_Click(object sender, RoutedEventArgs e)
        {
            if (BillingDataGrid.SelectedItem is BillingItem billing)
            {
                var result = MessageBox.Show($"Are you sure you want to delete Invoice {billing.InvoiceNo}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _dbContext.Billings.Remove(billing);
                        _dbContext.SaveChanges();
                        LoadBillingData();
                        MessageBox.Show("Invoice deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting invoice: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void NewInvoice_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddInvoicePage());
        }

        private void ToggleStatus_Click(object sender, RoutedEventArgs e)
        {
            // Get the specific billing item that was clicked
            if (sender is Button btn && btn.DataContext is BillingItem billing)
            {
                // Toggle the status
                if (billing.Status == "Paid")
                {
                    billing.Status = "Pending";
                }
                else
                {
                    billing.Status = "Paid"; // Changes both "Pending" and "Partial" to "Paid"
                }

                try
                {
                    // Save changes to database
                    _dbContext.Billings.Update(billing);
                    _dbContext.SaveChanges();

                    // Refresh the table and the summary cards at the top
                    LoadBillingData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating status: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
