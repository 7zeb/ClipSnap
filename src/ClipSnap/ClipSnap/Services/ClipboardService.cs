using System.Drawing;
using System.Windows;

namespace ClipSnap.Services
{
    public static class ClipboardService
    {
        public static void CopyImageToClipboard(Bitmap bitmap)
        {
            try
            {
                // Convert to BitmapSource for WPF clipboard
                var hBitmap = bitmap.GetHbitmap();
                try
                {
                    var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        hBitmap,
                        System.IntPtr.Zero,
                        Int32Rect.Empty,
                        System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                    Clipboard.SetImage(bitmapSource);
                }
                finally
                {
                    ClipSnap.Helpers.NativeMethods.DeleteObject(hBitmap);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Failed to copy to clipboard: {ex.Message}",
                    "ClipSnap", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}