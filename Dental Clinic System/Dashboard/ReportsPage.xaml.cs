using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Dental_Clinic_System.Dashboard
{
    public partial class ReportsPage : Page
    {
        // ObservableCollection automatically updates the DataGrid when items are added/removed
        public ObservableCollection<DentistPerformance> Dentists { get; set; }

        public ReportsPage()
        {
            InitializeComponent();
            LoadDentistData();
        }

        private void LoadDentistData()
        {
            // Generating random mock data for the Dentist Performance table
            Dentists = new ObservableCollection<DentistPerformance>
            {
                new DentistPerformance
                {
                    Name = "Dr. Smith",
                    Appointments = 156,
                    AvgRate = "$292/apt",
                    Revenue = "$45,600"
                },
                new DentistPerformance
                {
                    Name = "Dr. Johnson",
                    Appointments = 142,
                    AvgRate = "$310/apt",
                    Revenue = "$44,020"
                },
                new DentistPerformance
                {
                    Name = "Dr. Williams",
                    Appointments = 118,
                    AvgRate = "$275/apt",
                    Revenue = "$32,450"
                },
                new DentistPerformance
                {
                    Name = "Dr. Brown",
                    Appointments = 98,
                    AvgRate = "$320/apt",
                    Revenue = "$31,360"
                }
            };

            // Bind the data to the DataGrid
            DentistDataGrid.ItemsSource = Dentists;
        }
    }

    // Data model for the Dentist Performance Table
    public class DentistPerformance
    {
        public string Name { get; set; } = string.Empty;
        public int Appointments { get; set; }
        public string AvgRate { get; set; } = string.Empty;
        public string Revenue { get; set; } = string.Empty;
    }
}