using System;
using System.IO;

namespace ClipSnap.Models
{
    public class AppSettings
    {
        // Save folder
        public string SaveFolderPath { get; set; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
            "ClipSnap Screenshots");

        // Clipboard
        public bool CopyToClipboard { get; set; } = true;

        // Hotkey: Win+Shift+S
        public bool EnableWinShiftS { get; set; } = true;
        public uint WinShiftSModifiers { get; set; } = 0x0008 | 0x0004; // MOD_WIN | MOD_SHIFT
        public uint WinShiftSKey { get; set; } = 0x53; // 'S' key

        // Hotkey: Print Screen
        public bool EnablePrintScreen { get; set; } = true;
        public uint PrintScreenModifiers { get; set; } = 0x0000; // No modifiers
        public uint PrintScreenKey { get; set; } = 0x2C; // VK_SNAPSHOT (Print Screen)

        // Startup
        public bool StartWithWindows { get; set; } = false;

        // Screenshot settings
        public string ImageFormat { get; set; } = "png";
        public string FileNamePattern { get; set; } = "Screenshot_{timestamp}";
    }
}