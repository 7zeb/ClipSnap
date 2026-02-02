using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;

namespace ClipSnap.Services
{
    public class ScreenshotService
    {
        private readonly SettingsService _settingsService;

        public ScreenshotService(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public Bitmap CaptureRegion(int x, int y, int width, int height)
        {
            var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(x, y, 0, 0, new System.Drawing.Size(width, height), CopyPixelOperation.SourceCopy);
            }
            return bitmap;
        }

        public Bitmap CaptureAllScreens()
        {
            var virtualScreen = System.Windows.Forms.SystemInformation.VirtualScreen;
            return CaptureRegion(virtualScreen.X, virtualScreen.Y, virtualScreen.Width, virtualScreen.Height);
        }

        public string SaveScreenshot(Bitmap bitmap)
        {
            _settingsService.EnsureSaveFolderExists();

            var settings = _settingsService.CurrentSettings;
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var fileName = settings.FileNamePattern.Replace("{timestamp}", timestamp);
            
            var extension = settings.ImageFormat.ToLower();
            var filePath = Path.Combine(settings.SaveFolderPath, $"{fileName}.{extension}");

            // Ensure unique filename
            int counter = 1;
            while (File.Exists(filePath))
            {
                filePath = Path.Combine(settings.SaveFolderPath, $"{fileName}_{counter}.{extension}");
                counter++;
            }

            var format = GetImageFormat(extension);
            bitmap.Save(filePath, format);

            return filePath;
        }

        private ImageFormat GetImageFormat(string extension)
        {
            return extension.ToLower() switch
            {
                "jpg" or "jpeg" => ImageFormat.Jpeg,
                "bmp" => ImageFormat.Bmp,
                "gif" => ImageFormat.Gif,
                _ => ImageFormat.Png
            };
        }
    }
}