using System;
using System.Windows;
using System.Linq;
using ClipSnap.Services;
using ClipSnap.Views;
using Hardcodet.Wpf.TaskbarNotification;

namespace ClipSnap
{
    public partial class App : Application
    {
        private TaskbarIcon? _trayIcon;
        private HotkeyService? _hotkeyService;
        private SettingsService? _settingsService;
        private ScreenshotService? _screenshotService;

        public static SettingsService Settings { get; private set; } = null!;
        public static ScreenshotService Screenshot { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize services
            _settingsService = new SettingsService();
            _settingsService.Load();
            Settings = _settingsService;

            _screenshotService = new ScreenshotService(_settingsService);
            Screenshot = _screenshotService;

            // Create system tray icon
            _trayIcon = new TaskbarIcon
            {
                Icon = new System.Drawing.Icon(GetResourceStream(new Uri("pack://application:,,,/Resources/tray-icon.ico")).Stream),
                ToolTipText = "ClipSnap - Screenshot Tool",
                ContextMenu = CreateContextMenu()
            };
            _trayIcon.TrayMouseDoubleClick += (s, args) => OpenSettings();

            // Initialize hotkey service
            _hotkeyService = new HotkeyService();
            _hotkeyService.HotkeyPressed += OnHotkeyPressed;
            RegisterHotkeys();

            // Ensure screenshots folder exists
            _settingsService.EnsureSaveFolderExists();
        }

        private void RegisterHotkeys()
        {
            if (_hotkeyService == null || _settingsService == null) return;

            _hotkeyService.UnregisterAll();

            var settings = _settingsService.CurrentSettings;

            if (settings.EnableWinShiftS)
            {
                _hotkeyService.RegisterHotkey(1, settings.WinShiftSModifiers, settings.WinShiftSKey);
            }

            if (settings.EnablePrintScreen)
            {
                _hotkeyService.RegisterHotkey(2, settings.PrintScreenModifiers, settings.PrintScreenKey);
            }
        }

        public void ReregisterHotkeys()
        {
            RegisterHotkeys();
        }

        private void OnHotkeyPressed(object? sender, int hotkeyId)
        {
            TakeScreenshot();
        }

        public void TakeScreenshot()
        {
            var overlay = new SelectionOverlay();
            overlay.Show();
        }

        private System.Windows.Controls.ContextMenu CreateContextMenu()
        {
            var menu = new System.Windows.Controls.ContextMenu();

            var takeScreenshotItem = new System.Windows.Controls.MenuItem { Header = "Take Screenshot" };
            takeScreenshotItem.Click += (s, e) => TakeScreenshot();
            menu.Items.Add(takeScreenshotItem);

            menu.Items.Add(new System.Windows.Controls.Separator());

            var openFolderItem = new System.Windows.Controls.MenuItem { Header = "Open Screenshots Folder" };
            openFolderItem.Click += (s, e) => OpenScreenshotsFolder();
            menu.Items.Add(openFolderItem);

            var settingsItem = new System.Windows.Controls.MenuItem { Header = "Settings" };
            settingsItem.Click += (s, e) => OpenSettings();
            menu.Items.Add(settingsItem);

            menu.Items.Add(new System.Windows.Controls.Separator());

            var exitItem = new System.Windows.Controls.MenuItem { Header = "Exit" };
            exitItem.Click += (s, e) => ExitApplication();
            menu.Items.Add(exitItem);

            return menu;
        }

        private void OpenSettings()
        {
            var existingWindow = Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (existingWindow != null)
            {
                existingWindow.Activate();
                return;
            }

            var mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void OpenScreenshotsFolder()
        {
            var folder = _settingsService?.CurrentSettings.SaveFolderPath;
            if (!string.IsNullOrEmpty(folder) && System.IO.Directory.Exists(folder))
            {
                System.Diagnostics.Process.Start("explorer.exe", folder);
            }
            else
            {
                MessageBox.Show("Screenshots folder does not exist. Please configure it in Settings.",
                    "ClipSnap", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ExitApplication()
        {
            _hotkeyService?.UnregisterAll();
            _hotkeyService?.Dispose();
            _trayIcon?.Dispose();
            Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _hotkeyService?.UnregisterAll();
            _hotkeyService?.Dispose();
            _trayIcon?.Dispose();
            base.OnExit(e);
        }
    }
}
