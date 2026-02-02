using System.Windows;
using Microsoft.Win32;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace ClipSnap.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            var settings = App.Settings.CurrentSettings;
            
            FolderPathTextBox.Text = settings.SaveFolderPath;
            CopyToClipboardCheckBox.IsChecked = settings.CopyToClipboard;
            EnableWinShiftSCheckBox.IsChecked = settings.EnableWinShiftS;
            EnablePrintScreenCheckBox.IsChecked = settings.EnablePrintScreen;
            StartWithWindowsCheckBox.IsChecked = settings.StartWithWindows;
        }

        private void BrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Select Screenshots Folder",
                UseDescriptionForTitle = true,
                SelectedPath = FolderPathTextBox.Text
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FolderPathTextBox.Text = dialog.SelectedPath;
            }
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            var settings = App.Settings.CurrentSettings;
            
            settings.SaveFolderPath = FolderPathTextBox.Text;
            settings.CopyToClipboard = CopyToClipboardCheckBox.IsChecked ?? true;
            settings.EnableWinShiftS = EnableWinShiftSCheckBox.IsChecked ?? true;
            settings.EnablePrintScreen = EnablePrintScreenCheckBox.IsChecked ?? true;
            settings.StartWithWindows = StartWithWindowsCheckBox.IsChecked ?? false;

            App.Settings.Save();
            
            // Re-register hotkeys with new settings
            ((App)System.Windows.Application.Current).ReregisterHotkeys();
            
            MessageBox.Show("Settings saved successfully!", "ClipSnap", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void TakeScreenshot_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            
            // Small delay to ensure window is hidden
            System.Threading.Tasks.Task.Delay(200).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() =>
                {
                    ((App)System.Windows.Application.Current).TakeScreenshot();
                });
            });
        }
    }
}