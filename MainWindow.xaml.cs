using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;
using Microsoft.Data.Sqlite;

namespace license1
{
    public sealed partial class MainWindow : Window
    {
        public ObservableCollection<License> Licenses { get; set; } = new ObservableCollection<License>();

        private const string DbName = "licenses.db";

        public MainWindow()
        {
            this.InitializeComponent();
            LicenseTable.ItemsSource = Licenses;
            InitializeDatabase();
            LoadData();
        }

        private void InitializeDatabase()
        {
            using (var connection = new SqliteConnection($"Data Source={DbName}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS licenses (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        software TEXT NOT NULL,
                        license_key TEXT NOT NULL,
                        expiry_date TEXT NOT NULL,
                        category TEXT
                    )";
                command.ExecuteNonQuery();
            }
        }

        private void LoadData()
        {
            Licenses.Clear();
            using (var connection = new SqliteConnection($"Data Source={DbName}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT software, license_key, expiry_date, category FROM licenses";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Licenses.Add(new License
                        {
                            Software = reader.GetString(0),
                            LicenseKey = reader.GetString(1),
                            ExpiryDate = reader.GetString(2),
                            Category = reader.GetString(3)
                        });
                    }
                }
            }
        }

        private void AddLicense_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new LicenseDialog();
            dialog.Activate(); // Show the dialog
            dialog.Closed += (s, args) =>
            {
                if (dialog.IsDialogConfirmed) // Check if the dialog was confirmed
                {
                    Licenses.Add(dialog.License); // Add the new license to the collection
                    using (var connection = new SqliteConnection($"Data Source={DbName}"))
                    {
                        connection.Open();
                        var command = connection.CreateCommand();
                        command.CommandText = @"
                            INSERT INTO licenses (software, license_key, expiry_date, category)
                            VALUES ($software, $licenseKey, $expiryDate, $category)";
                        command.Parameters.AddWithValue("$software", dialog.License.Software);
                        command.Parameters.AddWithValue("$licenseKey", dialog.License.LicenseKey);
                        command.Parameters.AddWithValue("$expiryDate", dialog.License.ExpiryDate);
                        command.Parameters.AddWithValue("$category", dialog.License.Category);
                        command.ExecuteNonQuery();
                    }
                }
            };
        }

        private void EditLicense_Click(object sender, RoutedEventArgs e)
        {
            if (LicenseTable.SelectedItem is License selectedLicense)
            {
                var dialog = new LicenseDialog(selectedLicense);
                dialog.Activate();
                dialog.Closed += (s, args) =>
                {
                    if (dialog.IsDialogConfirmed)
                    {
                        var updatedLicense = dialog.License;
                        using (var connection = new SqliteConnection($"Data Source={DbName}"))
                        {
                            connection.Open();
                            var command = connection.CreateCommand();
                            command.CommandText = @"
                                UPDATE licenses
                                SET software = $software, license_key = $licenseKey, expiry_date = $expiryDate, category = $category
                                WHERE software = $oldSoftware AND license_key = $oldLicenseKey";
                            command.Parameters.AddWithValue("$software", updatedLicense.Software);
                            command.Parameters.AddWithValue("$licenseKey", updatedLicense.LicenseKey);
                            command.Parameters.AddWithValue("$expiryDate", updatedLicense.ExpiryDate);
                            command.Parameters.AddWithValue("$category", updatedLicense.Category);
                            command.Parameters.AddWithValue("$oldSoftware", selectedLicense.Software);
                            command.Parameters.AddWithValue("$oldLicenseKey", selectedLicense.LicenseKey);
                            command.ExecuteNonQuery();
                        }
                    }
                };
            }
        }

        private void DeleteLicense_Click(object sender, RoutedEventArgs e)
        {
            if (LicenseTable.SelectedItem is License selectedLicense)
            {
                using (var connection = new SqliteConnection($"Data Source={DbName}"))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "DELETE FROM licenses WHERE software = $software AND license_key = $licenseKey";
                    command.Parameters.AddWithValue("$software", selectedLicense.Software);
                    command.Parameters.AddWithValue("$licenseKey", selectedLicense.LicenseKey);
                    command.ExecuteNonQuery();
                }
                LoadData();
            }
        }

        private void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            // Implement CSV export logic
        }

        private void ImportCsv_Click(object sender, RoutedEventArgs e)
        {
            // Implement CSV import logic
        }

        private void BackupDatabase_Click(object sender, RoutedEventArgs e)
        {
            // Implement database backup logic
        }

        private void RestoreDatabase_Click(object sender, RoutedEventArgs e)
        {
            // Implement database restore logic
        }
    }

    public class License
    {
        public string Software { get; set; }
        public string LicenseKey { get; set; }
        public string ExpiryDate { get; set; }
        public string Category { get; set; }
    }
}
