using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace license1
{
    public sealed partial class LicenseDialog : Window
    {
        public License License { get; private set; }
        public bool IsDialogConfirmed { get; private set; } = false; // Custom property to track dialog result

        public LicenseDialog(License license = null)
        {
            this.InitializeComponent();
            if (license != null)
            {
                SoftwareInput.Text = license.Software;
                LicenseKeyInput.Text = license.LicenseKey;
                ExpiryDateInput.Date = DateTime.Parse(license.ExpiryDate);
                CategoryInput.SelectedItem = license.Category;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            License = new License
            {
                Software = SoftwareInput.Text,
                LicenseKey = LicenseKeyInput.Text,
                ExpiryDate = ExpiryDateInput.Date.ToString("yyyy-MM-dd"),
                Category = (CategoryInput.SelectedItem as ComboBoxItem)?.Content.ToString()
            };
            IsDialogConfirmed = true; // Set to true when the dialog is confirmed
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            IsDialogConfirmed = false; // Set to false when the dialog is canceled
            this.Close();
        }
    }
}
