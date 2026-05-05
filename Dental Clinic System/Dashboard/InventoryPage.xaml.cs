using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Dental_Clinic_System.Data;
using Dental_Clinic_System.Models;

namespace Dental_Clinic_System.Dashboard
{
    public partial class InventoryPage : Page
    {
        private AppDbContext _dbContext;

        public InventoryPage()
        {
            InitializeComponent();
            _dbContext = new AppDbContext();
            _dbContext.Database.EnsureCreated();
            SeedData();
            LoadInventory();
        }

        private void SeedData()
        {
            if (!_dbContext.Inventory.Any())
            {
                _dbContext.Inventory.AddRange(new List<InventoryItem>
                {
                    new InventoryItem { Name = "Dental Gloves (Box)", Category = "Supplies", Quantity = 45, ReorderLevel = 20, ExpiryDate = "2026-05-10" },
                    new InventoryItem { Name = "Face Masks (Box)", Category = "Supplies", Quantity = 12, ReorderLevel = 15, ExpiryDate = "2025-12-01" },
                    new InventoryItem { Name = "Dental Floss", Category = "Supplies", Quantity = 8, ReorderLevel = 10, ExpiryDate = "2026-08-15" },
                    new InventoryItem { Name = "Anesthetic Cartridges", Category = "Medication", Quantity = 150, ReorderLevel = 50, ExpiryDate = "2025-11-20" },
                    new InventoryItem { Name = "Dental Burs (Set)", Category = "Equipment", Quantity = 5, ReorderLevel = 10, ExpiryDate = "2027-01-01" },
                    new InventoryItem { Name = "Impression Material", Category = "Supplies", Quantity = 20, ReorderLevel = 10, ExpiryDate = "2025-09-30" },
                    new InventoryItem { Name = "Bibs (Pack)", Category = "Supplies", Quantity = 100, ReorderLevel = 50, ExpiryDate = "2028-02-15" },
                    new InventoryItem { Name = "Gloves (Nitrile)", Category = "Supplies", Quantity = 30, ReorderLevel = 40, ExpiryDate = "2026-06-10" }
                });
                _dbContext.SaveChanges();
            }
        }

        private void LoadInventory()
        {
            _dbContext.Dispose();
            _dbContext = new AppDbContext();

            var items = _dbContext.Inventory.ToList();
            var lowStockItems = items.Where(i => i.Quantity <= i.ReorderLevel).ToList();

            // Update Stats
            StatTotal.Text = items.Count.ToString();
            StatInStock.Text = (items.Count - lowStockItems.Count).ToString();
            StatLow.Text = lowStockItems.Count.ToString();

            // Update Alerts
            AlertList.Children.Clear();
            if (lowStockItems.Any())
            {
                AlertPanel.Visibility = Visibility.Visible;
                foreach (var item in lowStockItems)
                {
                    Border alertRow = new Border { Background = Brushes.White, CornerRadius = new CornerRadius(6), Padding = new Thickness(15, 10, 15, 10), Margin = new Thickness(0, 0, 0, 8) };
                    Grid g = new Grid();
                    g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    StackPanel info = new StackPanel();
                    info.Children.Add(new TextBlock { Text = item.Name, FontSize = 13, FontWeight = FontWeights.Medium, Foreground = "#991B1B".ToColor() });
                    info.Children.Add(new TextBlock { Text = $"Current: {item.Quantity}  |  Reorder at: {item.ReorderLevel}", FontSize = 11, Foreground = "#6B7280".ToColor(), Margin = new Thickness(0, 2, 0, 0) });
                    g.Children.Add(info);

                    Button reorderBtn = new Button { Content = "Reorder", Background = "#EF4444".ToColor(), Foreground = Brushes.White, BorderThickness = new Thickness(0), FontSize = 12, FontWeight = FontWeights.SemiBold, Cursor = Cursors.Hand, Padding = new Thickness(15, 6, 15, 6), HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center };
                    reorderBtn.Click += (s, e) => ReorderItem(item);
                    Grid.SetColumn(reorderBtn, 1); g.Children.Add(reorderBtn);

                    alertRow.Child = g;
                    AlertList.Children.Add(alertRow);
                }
            }
            else { AlertPanel.Visibility = Visibility.Collapsed; }

            // Update Table
            StockList.Children.Clear();
            foreach (var item in items) StockList.Children.Add(CreateRow(item));
        }

        private Border CreateRow(InventoryItem item)
        {
            bool isLow = item.Quantity <= item.ReorderLevel;

            // Determine colors based on stock level
            string nameColor = isLow ? "#991B1B" : "#111827"; // Red if low, Black if ok
            string qtyColor = isLow ? "#991B1B" : "#4B5563";   // Red if low, Gray if ok

            Border row = new Border
            {
                Height = 50,
                Background = isLow ? new SolidColorBrush(Color.FromRgb(0xFF, 0xFB, 0xFB)) : Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xF3, 0xF4, 0xF6)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(25, 0, 25, 0)
            };

            Grid grid = new Grid();
            // Must match XAML header exactly!
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90, GridUnitType.Pixel) });

            grid.Children.Add(CreateCell(item.Name, nameColor, FontWeights.Medium, 0));
            grid.Children.Add(CreateCell(item.Category, "#4B5563", FontWeights.Normal, 1));
            grid.Children.Add(CreateCell(item.Quantity.ToString(), qtyColor, FontWeights.Bold, 2)); // Bold QTY
            grid.Children.Add(CreateCell(item.ReorderLevel.ToString(), "#4B5563", FontWeights.Normal, 3));
            grid.Children.Add(CreateCell(FormatDate(item.ExpiryDate), "#4B5563", FontWeights.Normal, 4));

            // ACTIONS COLUMN (+ and - buttons)
            StackPanel actions = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            Button minusBtn = new Button
            {
                Content = "-",
                Background = "#FEE2E2".ToColor(),
                Foreground = "#991B1B".ToColor(),
                BorderThickness = new Thickness(0),
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Cursor = Cursors.Hand,
                Width = 28,
                Height = 28,
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 5, 0)
            };
            minusBtn.Click += (s, e) => AdjustStock(item, -1);
            minusBtn.MouseEnter += (s, e) => minusBtn.Background = "#FECACA".ToColor();
            minusBtn.MouseLeave += (s, e) => minusBtn.Background = "#FEE2E2".ToColor();

            Button plusBtn = new Button
            {
                Content = "+",
                Background = "#D1FAE5".ToColor(),
                Foreground = "#065F46".ToColor(),
                BorderThickness = new Thickness(0),
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Cursor = Cursors.Hand,
                Width = 28,
                Height = 28,
                Padding = new Thickness(0, 0, 0, 0)
            };
            plusBtn.Click += (s, e) => AdjustStock(item, 1);
            plusBtn.MouseEnter += (s, e) => plusBtn.Background = "#A7F3D0".ToColor();
            plusBtn.MouseLeave += (s, e) => plusBtn.Background = "#D1FAE5".ToColor();

            actions.Children.Add(minusBtn);
            actions.Children.Add(plusBtn);

            Grid.SetColumn(actions, 5);
            grid.Children.Add(actions);

            row.Child = grid;
            return row;
        }

        private TextBlock CreateCell(string text, string hex, FontWeight weight, int col)
        {
            var tb = new TextBlock { Text = text, FontSize = 13, FontWeight = weight, Foreground = hex.ToColor(), VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(tb, col);
            return tb;
        }

        private string FormatDate(string d) { if (DateTime.TryParse(d, out DateTime dt)) return dt.ToString("MMM dd, yyyy"); return d; }

        private void AdjustStock(InventoryItem item, int amount)
        {
            // Prevent negative stock
            item.Quantity = Math.Max(0, item.Quantity + amount);
            _dbContext.Inventory.Update(item);
            _dbContext.SaveChanges();

            // Refresh UI to update stats, alerts, and table instantly
            LoadInventory();
        }

        private void ReorderItem(InventoryItem item)
        {
            item.Quantity += item.ReorderLevel;
            _dbContext.Inventory.Update(item);
            _dbContext.SaveChanges();
            LoadInventory();
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            if (new AddInventoryWindow().ShowDialog() == true) LoadInventory();
        }
    }

    public static class StringExtensions
    {
        public static SolidColorBrush ToColor(this string hex)
        {
            try { byte r = byte.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber); byte g = byte.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber); byte b = byte.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber); return new SolidColorBrush(Color.FromRgb(r, g, b)); }
            catch { return Brushes.Black; }
        }
    }
}