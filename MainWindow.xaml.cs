using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Microsoft.Data.Sqlite;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Windows.Storage;

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
            ShowLicenseDialog();
        }

        private void EditLicense_Click(object sender, RoutedEventArgs e)
        {
            if (LicenseTable.SelectedItem is License selectedLicense)
            {
                SoftwareInput.Text = selectedLicense.Software;
                LicenseKeyInput.Text = selectedLicense.LicenseKey;
                ExpiryDateInput.Date = DateTime.Parse(selectedLicense.ExpiryDate);
                CategoryInput.SelectedItem = selectedLicense.Category;
                ShowLicenseDialog();
            }
        }

        private void DeleteLicense_Click(object sender, RoutedEventArgs e)
        {
            if (LicenseTable.SelectedItem is License selectedLicense)
            {
                Licenses.Remove(selectedLicense);
            }
        }

        private void SaveLicense_Click(object sender, RoutedEventArgs e)
        {
            var newLicense = new License
            {
                Software = SoftwareInput.Text,
                LicenseKey = LicenseKeyInput.Text,
                ExpiryDate = ExpiryDateInput.Date.ToString("yyyy-MM-dd"),
                Category = (CategoryInput.SelectedItem as ComboBoxItem)?.Content.ToString()
            };

            if (LicenseTable.SelectedItem is License selectedLicense)
            {
                // Update existing license
                selectedLicense.Software = newLicense.Software;
                selectedLicense.LicenseKey = newLicense.LicenseKey;
                selectedLicense.ExpiryDate = newLicense.ExpiryDate;
                selectedLicense.Category = newLicense.Category;
            }
            else
            {
                // Add new license
                Licenses.Add(newLicense);
            }

            HideLicenseDialog();
        }

        private void CancelLicense_Click(object sender, RoutedEventArgs e)
        {
            HideLicenseDialog();
        }

        private void ShowLicenseDialog()
        {
            LicenseDialogPanel.Visibility = Visibility.Visible;
        }

        private void HideLicenseDialog()
        {
            LicenseDialogPanel.Visibility = Visibility.Collapsed;
        }

        private async void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileSavePicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeChoices.Add("CSV File", new List<string> { ".csv" });
            picker.SuggestedFileName = "licenses";

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                var csvBuilder = new StringBuilder();
                csvBuilder.AppendLine("Software,License Key,Expiry Date,Category");

                foreach (var license in Licenses)
                {
                    csvBuilder.AppendLine($"{license.Software},{license.LicenseKey},{license.ExpiryDate},{license.Category}");
                }

                await FileIO.WriteTextAsync(file, csvBuilder.ToString());
            }
        }

        private async void BackupDatabase_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileSavePicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeChoices.Add("Database File", new List<string> { ".db" });
            picker.SuggestedFileName = "licenses_backup";

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                File.Copy(DbName, file.Path, overwrite: true);
            }
        }

        private async void RestoreDatabase_Click(object sender, RoutedEventArgs e)
{
    var picker = new Windows.Storage.Pickers.FileOpenPicker();
    picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
    picker.FileTypeFilter.Add(".db");
    picker.FileTypeFilter.Add("*");

    // Ensure the picker works in a WinUI 3 desktop app
    var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
    WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

    var file = await picker.PickSingleFileAsync();
    if (file != null)
    {
        try
        {
            string backupFilePath = file.Path;
            string destinationPath = DbName;

            // Copy the backup file to replace the current database
            System.IO.File.Copy(backupFilePath, destinationPath, overwrite: true);

            // Reload the data from the restored database
            LoadData();

            var dialog = new ContentDialog
            {
                Title = "Restore Successful",
                Content = "The database has been successfully restored.",
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            var dialog = new ContentDialog
            {
                Title = "Restore Failed",
                Content = $"An error occurred while restoring the database: {ex.Message}",
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }
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
}
