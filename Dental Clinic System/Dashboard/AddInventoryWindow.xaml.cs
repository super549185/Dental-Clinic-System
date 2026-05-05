using System;
using System.Windows;
using System.Windows.Controls;
using Dental_Clinic_System.Data;
using Dental_Clinic_System.Models;

namespace Dental_Clinic_System.Dashboard
{
    public partial class AddInventoryWindow : Window
    {
        private AppDbContext _dbContext;
        public AddInventoryWindow()
        {
            InitializeComponent();
            _dbContext = new AppDbContext();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.DialogResult = false;

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text)) { MessageBox.Show("Enter item name.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (!int.TryParse(QtyBox.Text, out int qty) || !int.TryParse(ReorderBox.Text, out int reorder) || ExpiryBox.SelectedDate == null)
            { MessageBox.Show("Enter valid numbers for Qty/Reorder and select a date.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            try
            {
                _dbContext.Inventory.Add(new InventoryItem
                {
                    Name = NameBox.Text.Trim(),
                    Category = ((ComboBoxItem)CategoryBox.SelectedItem).Content.ToString(),
                    Quantity = qty,
                    ReorderLevel = reorder,
                    ExpiryDate = ExpiryBox.SelectedDate.Value.ToString("yyyy-MM-dd")
                });
                _dbContext.SaveChanges();
                this.DialogResult = true;
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }
    }
}