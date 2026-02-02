using System;
using System.IO;
using System.Text.Json;
using ClipSnap.Models;
using Microsoft.Win32;

namespace ClipSnap.Services
{
    public class SettingsService
    {
        private readonly string _settingsFilePath;
        private const string StartupRegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "ClipSnap";

        public AppSettings CurrentSettings { get; private set; } = new();

        public SettingsService()
        {
            var appDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ClipSnap");
            
            Directory.CreateDirectory(appDataFolder);
            _settingsFilePath = Path.Combine(appDataFolder, "settings.json");
        }

        public void Load()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    CurrentSettings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch (Exception)
            {
                CurrentSettings = new AppSettings();
            }
        }

        public void Save()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(CurrentSettings, options);
                File.WriteAllText(_settingsFilePath, json);
                UpdateStartupRegistry();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to save settings: {ex.Message}",
                    "ClipSnap", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public void EnsureSaveFolderExists()
        {
            try
            {
                if (!Directory.Exists(CurrentSettings.SaveFolderPath))
                {
                    Directory.CreateDirectory(CurrentSettings.SaveFolderPath);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to create screenshots folder: {ex.Message}",
                    "ClipSnap", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void UpdateStartupRegistry()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(StartupRegistryKey, true);
                if (key == null) return;

                if (CurrentSettings.StartWithWindows)
                {
                    var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                    if (!string.IsNullOrEmpty(exePath))
                    {
                        key.SetValue(AppName, $"\"{exePath}\"");
                    }
                }
                else
                {
                    key.DeleteValue(AppName, false);
                }
            }
            catch (Exception)
            {
                // Silently fail - user might not have permissions
            }
        }
    }
}