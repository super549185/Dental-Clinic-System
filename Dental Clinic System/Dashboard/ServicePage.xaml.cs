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
    public partial class ServicePage : Page
    {
        private AppDbContext _dbContext;
        private ServiceItem _editingService;
        private bool isEditMode = false;

        // Form Elements
        private TextBox NameBox, PriceBox, DurationBox, DescBox;
        private ComboBox CategoryBox;
        private Button SaveBtn, CancelBtn;

        public ServicePage()
        {
            InitializeComponent();
            _dbContext = new AppDbContext();
            _dbContext.Database.EnsureCreated();
            BuildFormUI();
            SeedDatabase();
            LoadServices();
        }

        private void BuildFormUI()
        {
            Grid formGrid = new Grid();

            for (int i = 0; i < 7; i++)
                formGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            formGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            formGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(15, GridUnitType.Pixel) });
            formGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            formGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(15, GridUnitType.Pixel) });
            formGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Row 0 labels, Row 1 inputs
            formGrid.Children.Add(CreateFormLabel("Service Name", 0, 0));
            NameBox = CreateFormTextBox("e.g., Dental Cleaning", 0, 1);
            formGrid.Children.Add(NameBox);

            formGrid.Children.Add(CreateFormLabel("Category", 2, 0));
            CategoryBox = new ComboBox
            {
                Height = 38,
                Padding = new Thickness(10, 0, 10, 0),
                Background = new SolidColorBrush(Color.FromRgb(0xF9, 0xFA, 0xFB)),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xE5, 0xE7, 0xEB)),
                FontSize = 13,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            // ── Updated category list ──
            foreach (var cat in new[] { "Preventive", "Restorative", "Cosmetic", "Orthodontic", "Surgical", "Diagnostic" })
                CategoryBox.Items.Add(cat);
            CategoryBox.SelectedIndex = 0;
            Grid.SetColumn(CategoryBox, 2);
            Grid.SetRow(CategoryBox, 1);
            formGrid.Children.Add(CategoryBox);

            // Row 2/3: Price & Duration
            formGrid.Children.Add(CreateFormLabel("Price", 0, 2));
            PriceBox = CreateFormTextBox("₱0.00", 0, 3);
            formGrid.Children.Add(PriceBox);

            formGrid.Children.Add(CreateFormLabel("Duration", 2, 2));
            DurationBox = CreateFormTextBox("e.g., 30 min", 2, 3);
            formGrid.Children.Add(DurationBox);

            // Row 4/5: Description (full width)
            formGrid.Children.Add(CreateFormLabel("Description", 0, 4));
            DescBox = new TextBox
            {
                Height = 60,
                Padding = new Thickness(10, 5, 10, 5),
                Background = new SolidColorBrush(Color.FromRgb(0xF9, 0xFA, 0xFB)),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xE5, 0xE7, 0xEB)),
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            Grid.SetColumnSpan(DescBox, 5);
            Grid.SetRow(DescBox, 5);
            formGrid.Children.Add(DescBox);

            // Buttons
            StackPanel btnPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 15, 0, 0)
            };
            CancelBtn = new Button
            {
                Content = "Cancel",
                Height = 38,
                Background = new SolidColorBrush(Color.FromRgb(0xF3, 0xF4, 0xF6)),
                Foreground = new SolidColorBrush(Color.FromRgb(0x37, 0x41, 0x51)),
                BorderThickness = new Thickness(0),
                FontSize = 13,
                FontWeight = FontWeights.Medium,
                Cursor = Cursors.Hand,
                Width = 100,
                Margin = new Thickness(0, 0, 10, 0)
            };
            CancelBtn.Click += (s, e) => HideForm();

            SaveBtn = new Button
            {
                Content = "Save Service",
                Height = 38,
                Background = new SolidColorBrush(Color.FromRgb(0x00, 0x6B, 0x6B)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand,
                Width = 120
            };
            SaveBtn.Click += SaveService_Click;

            btnPanel.Children.Add(CancelBtn);
            btnPanel.Children.Add(SaveBtn);
            Grid.SetColumnSpan(btnPanel, 5);
            Grid.SetRow(btnPanel, 6);
            formGrid.Children.Add(btnPanel);

            FormPanel.Child = formGrid;
        }

        private TextBlock CreateFormLabel(string text, int col, int row)
        {
            var tb = new TextBlock
            {
                Text = text,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80)),
                Margin = new Thickness(0, 0, 5, 0)
            };
            Grid.SetColumn(tb, col);
            Grid.SetRow(tb, row);
            return tb;
        }

        private TextBox CreateFormTextBox(string placeholder, int col, int row)
        {
            var tb = new TextBox
            {
                Height = 38,
                Padding = new Thickness(10, 0, 10, 0),
                Background = new SolidColorBrush(Color.FromRgb(0xF9, 0xFA, 0xFB)),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xE5, 0xE7, 0xEB)),
                FontSize = 13,
                VerticalContentAlignment = VerticalAlignment.Center,
                Text = placeholder,
                Foreground = Brushes.Gray
            };
            tb.GotFocus += (s, e) => { if (tb.Text == placeholder) { tb.Text = ""; tb.Foreground = Brushes.Black; } };
            tb.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(tb.Text)) { tb.Text = placeholder; tb.Foreground = Brushes.Gray; } };
            Grid.SetColumn(tb, col);
            Grid.SetRow(tb, row);
            return tb;
        }

        // ── Expanded seed data covering all 6 categories ──────────────
        private void SeedDatabase()
        {
            if (!_dbContext.Services.Any())
            {
                _dbContext.Services.AddRange(new List<ServiceItem>
                {
                    // Preventive
                    new ServiceItem { Name = "Dental Cleaning",        Category = "Preventive",  Price = "₱80",   Duration = "30 min",  Description = "Professional scaling and polishing to remove plaque and tartar." },
                    new ServiceItem { Name = "Dental Checkup",         Category = "Preventive",  Price = "₱50",   Duration = "20 min",  Description = "Routine oral examination and health assessment." },
                    new ServiceItem { Name = "Fluoride Treatment",     Category = "Preventive",  Price = "₱40",   Duration = "15 min",  Description = "Application of fluoride to strengthen enamel and prevent cavities." },
                    new ServiceItem { Name = "Dental Sealants",        Category = "Preventive",  Price = "₱60",   Duration = "20 min",  Description = "Protective coating applied to the chewing surfaces of back teeth." },

                    // Restorative
                    new ServiceItem { Name = "Cavity Filling",         Category = "Restorative", Price = "₱150",  Duration = "45 min",  Description = "Removal of decay and filling with composite or amalgam material." },
                    new ServiceItem { Name = "Dental Crown",           Category = "Restorative", Price = "₱900",  Duration = "60 min",  Description = "Custom cap placed over a damaged or weakened tooth." },
                    new ServiceItem { Name = "Dental Bridge",          Category = "Restorative", Price = "₱1,200",Duration = "90 min",  Description = "Fixed replacement for one or more missing teeth anchored to adjacent teeth." },
                    new ServiceItem { Name = "Root Canal",             Category = "Restorative", Price = "₱500",  Duration = "90 min",  Description = "Endodontic therapy to remove infected pulp and seal the tooth." },
                    new ServiceItem { Name = "Dental Implant",         Category = "Restorative", Price = "₱2,500",Duration = "120 min", Description = "Titanium post surgically placed in the jawbone as a permanent tooth root." },

                    // Cosmetic
                    new ServiceItem { Name = "Teeth Whitening",        Category = "Cosmetic",    Price = "₱300",  Duration = "60 min",  Description = "In-office bleaching procedure for a noticeably brighter smile." },
                    new ServiceItem { Name = "Dental Veneers",         Category = "Cosmetic",    Price = "₱800",  Duration = "60 min",  Description = "Thin porcelain shells bonded to the front surface of teeth." },
                    new ServiceItem { Name = "Composite Bonding",      Category = "Cosmetic",    Price = "₱200",  Duration = "45 min",  Description = "Tooth-colored resin applied to repair chips, cracks, or gaps." },
                    new ServiceItem { Name = "Smile Makeover",         Category = "Cosmetic",    Price = "₱3,000",Duration = "180 min", Description = "Comprehensive treatment combining multiple cosmetic procedures." },

                    // Orthodontic
                    new ServiceItem { Name = "Braces Adjustment",      Category = "Orthodontic", Price = "₱120",  Duration = "30 min",  Description = "Routine tightening and wire replacement for traditional braces." },
                    new ServiceItem { Name = "Retainer",               Category = "Orthodontic", Price = "₱250",  Duration = "30 min",  Description = "Custom appliance to maintain teeth position after braces." },
                    new ServiceItem { Name = "Clear Aligners",         Category = "Orthodontic", Price = "₱3,500",Duration = "30 min",  Description = "Series of removable transparent trays to gradually straighten teeth." },

                    // Surgical
                    new ServiceItem { Name = "Tooth Extraction",       Category = "Surgical",    Price = "₱200",  Duration = "30 min",  Description = "Safe removal of a severely damaged, decayed, or impacted tooth." },
                    new ServiceItem { Name = "Wisdom Tooth Removal",   Category = "Surgical",    Price = "₱350",  Duration = "60 min",  Description = "Surgical extraction of impacted or problematic third molars." },
                    new ServiceItem { Name = "Apicoectomy",            Category = "Surgical",    Price = "₱700",  Duration = "60 min",  Description = "Minor surgery to remove the tip of a tooth root when root canal fails." },
                    new ServiceItem { Name = "Bone Grafting",          Category = "Surgical",    Price = "₱1,500",Duration = "90 min",  Description = "Procedure to rebuild jawbone density before implant placement." },

                    // Diagnostic
                    new ServiceItem { Name = "Dental X-Ray",           Category = "Diagnostic",  Price = "₱80",   Duration = "15 min",  Description = "Radiographic images to detect hidden decay, bone loss, and other issues." },
                    new ServiceItem { Name = "Panoramic X-Ray",        Category = "Diagnostic",  Price = "₱120",  Duration = "15 min",  Description = "Full-mouth X-ray showing all teeth, jaws, and surrounding structures." },
                    new ServiceItem { Name = "Oral Cancer Screening",  Category = "Diagnostic",  Price = "₱60",   Duration = "15 min",  Description = "Visual and tactile examination to detect early signs of oral cancer." },
                    new ServiceItem { Name = "CT Scan (CBCT)",         Category = "Diagnostic",  Price = "₱350",  Duration = "20 min",  Description = "3D cone-beam computed tomography for detailed jaw and tooth structure imaging." },
                });
                _dbContext.SaveChanges();
            }
        }

        private void LoadServices()
        {
            ServiceList.Children.Clear();
            foreach (var s in _dbContext.Services.ToList())
                ServiceList.Children.Add(CreateRow(s));
        }

        private Border CreateRow(ServiceItem s)
        {
            Border row = new Border
            {
                Height = 50,
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xF3, 0xF4, 0xF6)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(25, 0, 25, 0)
            };

            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80, GridUnitType.Pixel) });

            grid.Children.Add(CreateCell(s.Name, "#111827", FontWeights.Medium, 0));
            grid.Children.Add(CreateCell(s.Category, "#4B5563", FontWeights.Normal, 1));
            grid.Children.Add(CreateCell(s.Price, "#4B5563", FontWeights.Normal, 2));
            grid.Children.Add(CreateCell(s.Duration, "#4B5563", FontWeights.Normal, 3));

            // Action buttons
            StackPanel actions = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            Button editBtn = new Button
            {
                Content = new System.Windows.Controls.Image
                {
                    Source = new System.Windows.Media.Imaging.BitmapImage(
                        new Uri("pack://application:,,,/Images/edit.png", UriKind.Absolute)),
                    Width = 18,
                    Height = 18,
                    Stretch = Stretch.UniformToFill
                },
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                Width = 34,
                Height = 34,
                Padding = new Thickness(0),
                Margin = new Thickness(2, 0, 2, 0)
            };
            editBtn.MouseEnter += (s2, e) => editBtn.Background = new SolidColorBrush(Color.FromRgb(0xDB, 0xEA, 0xFE));
            editBtn.MouseLeave += (s2, e) => editBtn.Background = Brushes.Transparent;
            editBtn.Click += (sender, e) => ShowEditForm(s);

            Button delBtn = new Button
            {
                Content = new System.Windows.Controls.Image
                {
                    Source = new System.Windows.Media.Imaging.BitmapImage(
                        new Uri("pack://application:,,,/Images/delete.png", UriKind.Absolute)),
                    Width = 18,
                    Height = 18,
                    Stretch = Stretch.UniformToFill
                },
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                Width = 34,
                Height = 34,
                Padding = new Thickness(0),
                Margin = new Thickness(2, 0, 2, 0)
            };
            delBtn.MouseEnter += (s2, e) => delBtn.Background = new SolidColorBrush(Color.FromRgb(0xFE, 0xE2, 0xE2));
            delBtn.MouseLeave += (s2, e) => delBtn.Background = Brushes.Transparent;
            delBtn.Click += (sender, e) =>
            {
                if (MessageBox.Show("Delete this service?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    _dbContext.Services.Remove(s);
                    _dbContext.SaveChanges();
                    LoadServices();
                }
            };

            actions.Children.Add(editBtn);
            actions.Children.Add(delBtn);
            Grid.SetColumn(actions, 4);
            grid.Children.Add(actions);

            row.Child = grid;
            return row;
        }

        private TextBlock CreateCell(string text, string hex, FontWeight weight, int col)
        {
            var tb = new TextBlock
            {
                Text = text,
                FontSize = 13,
                FontWeight = weight,
                Foreground = GetColor(hex),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(tb, col);
            return tb;
        }

        private SolidColorBrush GetColor(string hex)
        {
            try
            {
                byte r = byte.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
                byte g = byte.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
                byte b = byte.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
                return new SolidColorBrush(Color.FromRgb(r, g, b));
            }
            catch { return Brushes.Black; }
        }

        private void ToggleForm_Click(object sender, RoutedEventArgs e) => ShowAddForm();

        private void ShowAddForm()
        {
            isEditMode = false;
            _editingService = null;
            FormTitle.Text = "Add New Service";
            ClearForm();
            FormPanel.Visibility = Visibility.Visible;
        }

        private void ShowEditForm(ServiceItem s)
        {
            isEditMode = true;
            _editingService = s;
            FormTitle.Text = "Edit Service";
            NameBox.Text = s.Name; NameBox.Foreground = Brushes.Black;
            CategoryBox.SelectedItem = s.Category;
            PriceBox.Text = s.Price; PriceBox.Foreground = Brushes.Black;
            DurationBox.Text = s.Duration; DurationBox.Foreground = Brushes.Black;
            DescBox.Text = s.Description; DescBox.Foreground = Brushes.Black;
            FormPanel.Visibility = Visibility.Visible;
        }

        private void HideForm()
        {
            FormPanel.Visibility = Visibility.Collapsed;
            ClearForm();
        }

        private void ClearForm()
        {
            NameBox.Text = "e.g., Dental Cleaning"; NameBox.Foreground = Brushes.Gray;
            CategoryBox.SelectedIndex = 0;
            PriceBox.Text = "₱0.00"; PriceBox.Foreground = Brushes.Gray;
            DurationBox.Text = "e.g., 30 min"; DurationBox.Foreground = Brushes.Gray;
            DescBox.Text = "";
        }

        private void SaveService_Click(object sender, RoutedEventArgs e)
        {
            if (NameBox.Text == "e.g., Dental Cleaning" || string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Enter service name.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (isEditMode && _editingService != null)
                {
                    _editingService.Name = NameBox.Text.Trim();
                    _editingService.Category = CategoryBox.SelectedItem.ToString();
                    _editingService.Price = PriceBox.Text.Trim();
                    _editingService.Duration = DurationBox.Text.Trim();
                    _editingService.Description = DescBox.Text.Trim();
                    _dbContext.Services.Update(_editingService);
                }
                else
                {
                    _dbContext.Services.Add(new ServiceItem
                    {
                        Name = NameBox.Text.Trim(),
                        Category = CategoryBox.SelectedItem.ToString(),
                        Price = PriceBox.Text.Trim(),
                        Duration = DurationBox.Text.Trim(),
                        Description = DescBox.Text.Trim()
                    });
                }
                _dbContext.SaveChanges();
                HideForm();
                LoadServices();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}
