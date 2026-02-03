using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using ClipSnap.Helpers;

namespace ClipSnap.Services
{
    public class HotkeyService : IDisposable
    {
        private readonly Dictionary<int, (uint Modifiers, uint Key)> _registeredHotkeys = new();
        private HwndSource? _hwndSource;
        private IntPtr _hwnd;
        private bool _isDisposed;

        public event EventHandler<int>? HotkeyPressed;

        public HotkeyService()
        {
            // Must be created on the UI thread
            if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(() => Initialize());
            }
            else
            {
                Initialize();
            }
        }

        private void Initialize()
        {
            try
            {
                // Create a message-only window to receive hotkey messages
                var parameters = new HwndSourceParameters("ClipSnapHotkeyWindow")
                {
                    Width = 0,
                    Height = 0,
                    PositionX = 0,
                    PositionY = 0,
                    WindowStyle = 0, // WS_POPUP - hidden window
                    ParentWindow = new IntPtr(-3) // HWND_MESSAGE - message-only window
                };

                _hwndSource = new HwndSource(parameters);
                _hwndSource.AddHook(WndProc);
                _hwnd = _hwndSource.Handle;

                System.Diagnostics.Debug.WriteLine($"[HotkeyService] Initialized with HWND: {_hwnd}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HotkeyService] Failed to initialize: {ex.Message}");
                MessageBox.Show($"Failed to initialize hotkey service: {ex.Message}", 
                    "ClipSnap", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public bool RegisterHotkey(int id, uint modifiers, uint key)
        {
            if (_hwnd == IntPtr.Zero)
            {
                System.Diagnostics.Debug.WriteLine($"[HotkeyService] Cannot register hotkey - invalid window handle");
                return false;
            }

            if (_registeredHotkeys.ContainsKey(id))
            {
                UnregisterHotkey(id);
            }

            // Add MOD_NOREPEAT to prevent key repeat
            uint modsWithNoRepeat = modifiers | NativeMethods.MOD_NOREPEAT;

            bool success = NativeMethods.RegisterHotKey(_hwnd, id, modsWithNoRepeat, key);
            
            if (success)
            {
                _registeredHotkeys[id] = (modifiers, key);
                System.Diagnostics.Debug.WriteLine($"[HotkeyService] Registered hotkey {id}: Modifiers={modifiers:X}, Key={key:X}");
            }
            else
            {
                int error = Marshal.GetLastWin32Error();
                System.Diagnostics.Debug.WriteLine($"[HotkeyService] Failed to register hotkey {id}: Error={error}");
                
                // Error 1409 means hotkey already registered by another app
                if (error == 1409)
                {
                    MessageBox.Show($"The hotkey is already in use by another application.\nPlease choose a different hotkey in Settings.",
                        "ClipSnap - Hotkey Registration Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            
            return success;
        }

        public void UnregisterHotkey(int id)
        {
            if (_registeredHotkeys.ContainsKey(id) && _hwnd != IntPtr.Zero)
            {
                bool success = NativeMethods.UnregisterHotKey(_hwnd, id);
                _registeredHotkeys.Remove(id);
                System.Diagnostics.Debug.WriteLine($"[HotkeyService] Unregistered hotkey {id}: {success}");
            }
        }

        public void UnregisterAll()
        {
            if (_hwnd == IntPtr.Zero) return;

            foreach (var id in new List<int>(_registeredHotkeys.Keys))
            {
                NativeMethods.UnregisterHotKey(_hwnd, id);
            }
            _registeredHotkeys.Clear();
            System.Diagnostics.Debug.WriteLine("[HotkeyService] Unregistered all hotkeys");
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;

            if (msg == WM_HOTKEY)
            {
                int hotkeyId = wParam.ToInt32();
                System.Diagnostics.Debug.WriteLine($"[HotkeyService] Hotkey {hotkeyId} pressed!");
                
                // Invoke on UI thread to be safe
                Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
                {
                    HotkeyPressed?.Invoke(this, hotkeyId);
                }), DispatcherPriority.Normal);
                
                handled = true;
            }

            return IntPtr.Zero;
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            UnregisterAll();
            
            if (_hwndSource != null)
            {
                _hwndSource.RemoveHook(WndProc);
                _hwndSource.Dispose();
                _hwndSource = null;
            }

            System.Diagnostics.Debug.WriteLine("[HotkeyService] Disposed");
        }
    }
}
