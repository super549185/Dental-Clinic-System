using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dental_Clinic_System.Data;
using Dental_Clinic_System.Models;
using Microsoft.Win32;

namespace Dental_Clinic_System.Dashboard
{
    public partial class ReceiptPage : Page
    {
        private AppDbContext _dbContext;
        private BillingItem _billing;

        public ReceiptPage(BillingItem billing)
        {
            InitializeComponent();
            _dbContext = new AppDbContext();
            _billing = billing;
            LoadReceiptData();
        }

        private void LoadReceiptData()
        {
            // 1. Fill Header Details
            TxtInvoiceNo.Text = _billing.InvoiceNo;
            TxtDate.Text = _billing.Date;
            TxtDueDate.Text = _billing.DueDate; // Make sure DueDate exists in your model
            TxtPatient.Text = _billing.Patient;
            TxtMethod.Text = _billing.PaymentMethod;
            TxtStatus.Text = _billing.Status.ToUpper();

            if (_billing.Status == "Pending" || _billing.Status == "Partial")
            {
                TxtStatus.Foreground = new SolidColorBrush(Color.FromRgb(0x99, 0x1B, 0x1B)); // Red text
                (TxtStatus.Parent as Border).Background = new SolidColorBrush(Color.FromRgb(0xFE, 0xE2, 0xE2)); // Red bg
            }

            // 2. Handle Services (Splitting "Dental Cleaning, Cavity Filling" into rows)
            string[] serviceNames = _billing.Service.Split(new[] { ", " }, StringSplitOptions.None);
            decimal calculatedTotal = 0;

            foreach (var serviceName in serviceNames)
            {
                // Look up the price from the Services master list
                var serviceData = _dbContext.Services.FirstOrDefault(s => s.Name == serviceName);
                decimal price = 0;

                if (serviceData != null)
                {
                    string cleanPrice = serviceData.Price.Replace("₱", "").Replace(",", "");
                    decimal.TryParse(cleanPrice, out price);
                }

                calculatedTotal += price;

                // Create Row UI
                Grid rowGrid = new Grid { Margin = new Thickness(0, 0, 0, 8) };
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80, GridUnitType.Pixel) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100, GridUnitType.Pixel) });

                rowGrid.Children.Add(new TextBlock { Text = serviceName, FontSize = 12, Foreground = new SolidColorBrush(Color.FromRgb(0x37, 0x41, 0x51)) });

                TextBlock qty = new TextBlock { Text = "1", FontSize = 12, Foreground = new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80)), HorizontalAlignment = HorizontalAlignment.Center };
                Grid.SetColumn(qty, 1);
                rowGrid.Children.Add(qty);

                TextBlock amt = new TextBlock { Text = $"₱{price:F2}", FontSize = 12, Foreground = new SolidColorBrush(Color.FromRgb(0x37, 0x41, 0x51)), FontWeight = FontWeights.Medium, HorizontalAlignment = HorizontalAlignment.Right };
                Grid.SetColumn(amt, 2);
                rowGrid.Children.Add(amt);

                ServicesList.Children.Add(rowGrid);
            }

            // 3. Fill Totals (Use calculated total if we found prices, else use the billing amount)
            decimal finalTotal = calculatedTotal > 0 ? calculatedTotal : _billing.Amount;
            TxtSubtotal.Text = $"₱{finalTotal:F2}";
            TxtTotal.Text = $"₱{finalTotal:F2}";
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Force the UI to render perfectly before taking screenshot
                ReceiptCard.UpdateLayout();
                ReceiptCard.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                ReceiptCard.Arrange(new Rect(new Point(0, 0), ReceiptCard.DesiredSize));

                // Create a screenshot of the ReceiptCard
                RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                    (int)ReceiptCard.ActualWidth,
                    (int)ReceiptCard.ActualHeight,
                    96d, 96d, PixelFormats.Pbgra32);

                renderBitmap.Render(ReceiptCard);

                // Setup Save File Dialog
                SaveFileDialog saveDlg = new SaveFileDialog
                {
                    Filter = "PNG Image|*.png",
                    FileName = $"{_billing.InvoiceNo}_Receipt.png"
                };

                if (saveDlg.ShowDialog() == true)
                {
                    // Convert to PNG and save
                    using (FileStream outStream = new FileStream(saveDlg.FileName, FileMode.Create))
                    {
                        PngBitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                        encoder.Save(outStream);
                    }

                    MessageBox.Show("Receipt downloaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error downloading receipt: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}