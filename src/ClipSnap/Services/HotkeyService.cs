using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using ClipSnap.Helpers;

namespace ClipSnap.Services
{
    public class HotkeyService : IDisposable
    {
        private readonly Dictionary<int, (uint Modifiers, uint Key)> _registeredHotkeys = new();
        private HwndSource? _hwndSource;
        private IntPtr _hwnd;

        public event EventHandler<int>? HotkeyPressed;

        public HotkeyService()
        {
            // Create a hidden window to receive hotkey messages
            var parameters = new HwndSourceParameters("ClipSnapHotkeyWindow")
            {
                Width = 0,
                Height = 0,
                PositionX = 0,
                PositionY = 0,
                WindowStyle = 0x00000000 // Hidden
            };

            _hwndSource = new HwndSource(parameters);
            _hwndSource.AddHook(WndProc);
            _hwnd = _hwndSource.Handle;
        }

        public bool RegisterHotkey(int id, uint modifiers, uint key)
        {
            if (_registeredHotkeys.ContainsKey(id))
            {
                UnregisterHotkey(id);
            }

            bool success = NativeMethods.RegisterHotKey(_hwnd, id, modifiers, key);
            if (success)
            {
                _registeredHotkeys[id] = (modifiers, key);
            }
            return success;
        }

        public void UnregisterHotkey(int id)
        {
            if (_registeredHotkeys.ContainsKey(id))
            {
                NativeMethods.UnregisterHotKey(_hwnd, id);
                _registeredHotkeys.Remove(id);
            }
        }

        public void UnregisterAll()
        {
            foreach (var id in _registeredHotkeys.Keys)
            {
                NativeMethods.UnregisterHotKey(_hwnd, id);
            }
            _registeredHotkeys.Clear();
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;

            if (msg == WM_HOTKEY)
            {
                int hotkeyId = wParam.ToInt32();
                HotkeyPressed?.Invoke(this, hotkeyId);
                handled = true;
            }

            return IntPtr.Zero;
        }

        public void Dispose()
        {
            UnregisterAll();
            _hwndSource?.RemoveHook(WndProc);
            _hwndSource?.Dispose();
        }
    }
}