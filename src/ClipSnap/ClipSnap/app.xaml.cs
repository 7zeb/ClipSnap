using System;
using System.Linq;
using System.Windows;
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

            System.Diagnostics.Debug.WriteLine("[ClipSnap] Application starting...");

            // Initialize services
            _settingsService = new SettingsService();
            _settingsService.Load();
            Settings = _settingsService;

            _screenshotService = new ScreenshotService(_settingsService);
            Screenshot = _screenshotService;

            // Create system tray icon
            _trayIcon = new TaskbarIcon
            {
                Icon = GetTrayIcon(),
                ToolTipText = "ClipSnap - Screenshot Tool",
                ContextMenu = CreateContextMenu()
            };
            _trayIcon.TrayMouseDoubleClick += (s, args) => OpenSettings();

            // Initialize hotkey service AFTER UI thread is fully initialized
            Dispatcher.BeginInvoke(new Action(() =>
            {
                System.Diagnostics.Debug.WriteLine("[ClipSnap] Initializing hotkey service...");
                _hotkeyService = new HotkeyService();
                _hotkeyService.HotkeyPressed += OnHotkeyPressed;
                RegisterHotkeys();
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);

            // Ensure screenshots folder exists
            _settingsService.EnsureSaveFolderExists();

            System.Diagnostics.Debug.WriteLine("[ClipSnap] Application started successfully");
        }

        private void RegisterHotkeys()
        {
            if (_hotkeyService == null || _settingsService == null)
            {
                System.Diagnostics.Debug.WriteLine("[ClipSnap] Cannot register hotkeys - services not initialized");
                return;
            }

            _hotkeyService.UnregisterAll();

            var settings = _settingsService.CurrentSettings;

            System.Diagnostics.Debug.WriteLine($"[ClipSnap] Registering hotkeys - WinShiftS: {settings.EnableWinShiftS}, PrintScreen: {settings.EnablePrintScreen}");

            if (settings.EnableWinShiftS)
            {
                bool success = _hotkeyService.RegisterHotkey(1, settings.WinShiftSModifiers, settings.WinShiftSKey);
                System.Diagnostics.Debug.WriteLine($"[ClipSnap] Win+Shift+S registration: {success}");
            }

            if (settings.EnablePrintScreen)
            {
                bool success = _hotkeyService.RegisterHotkey(2, settings.PrintScreenModifiers, settings.PrintScreenKey);
                System.Diagnostics.Debug.WriteLine($"[ClipSnap] Print Screen registration: {success}");
            }
        }

        public void ReregisterHotkeys()
        {
            System.Diagnostics.Debug.WriteLine("[ClipSnap] Re-registering hotkeys...");
            RegisterHotkeys();
        }

        private void OnHotkeyPressed(object? sender, int hotkeyId)
        {
            System.Diagnostics.Debug.WriteLine($"[ClipSnap] Hotkey {hotkeyId} triggered - taking screenshot");
            TakeScreenshot();
        }

        public void TakeScreenshot()
        {
            System.Diagnostics.Debug.WriteLine("[ClipSnap] TakeScreenshot called");
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

        private System.Drawing.Icon GetTrayIcon()
        {
            try
            {
                var iconUri = new Uri("pack://application:,,,/Resources/tray-icon.ico");
                var streamInfo = GetResourceStream(iconUri);
                if (streamInfo != null)
                {
                    return new System.Drawing.Icon(streamInfo.Stream);
                }
            }
            catch
            {
                // Icon not found, use default
            }

            // Fallback to system default icon
            return System.Drawing.SystemIcons.Application;
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
            System.Diagnostics.Debug.WriteLine("[ClipSnap] Exiting application");
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
