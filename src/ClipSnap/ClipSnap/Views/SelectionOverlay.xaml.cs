using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ClipSnap.Services;

namespace ClipSnap.Views
{
    public partial class SelectionOverlay : Window
    {
        private Point _startPoint;
        private Rectangle? _selectionRectangle;
        private bool _isSelecting;
        private System.Drawing.Bitmap? _screenCapture;
        
        // DPI scaling factors
        private double _dpiScaleX = 1.0;
        private double _dpiScaleY = 1.0;
        
        // Virtual screen offset (for multi-monitor setups)
        private int _virtualScreenX;
        private int _virtualScreenY;

        public SelectionOverlay()
        {
            InitializeComponent();
            
            // Get virtual screen bounds (covers all monitors)
            var virtualScreen = System.Windows.Forms.SystemInformation.VirtualScreen;
            _virtualScreenX = virtualScreen.X;
            _virtualScreenY = virtualScreen.Y;
            
            // Position window to cover all screens
            Left = virtualScreen.X;
            Top = virtualScreen.Y;
            Width = virtualScreen.Width;
            Height = virtualScreen.Height;

            // Capture the screen before showing overlay
            _screenCapture = App.Screenshot.CaptureAllScreens();

            // Get DPI scaling after window is loaded
            Loaded += OnLoaded;
            
            MouseLeftButtonDown += OnMouseLeftButtonDown;
            MouseLeftButtonUp += OnMouseLeftButtonUp;
            MouseMove += OnMouseMove;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Get DPI scaling from the visual tree
            var source = PresentationSource.FromVisual(this);
            if (source?.CompositionTarget != null)
            {
                _dpiScaleX = source.CompositionTarget.TransformToDevice.M11;
                _dpiScaleY = source.CompositionTarget.TransformToDevice.M22;
                System.Diagnostics.Debug.WriteLine($"[SelectionOverlay] DPI Scale: {_dpiScaleX}x{_dpiScaleY}");
            }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(this);
            _isSelecting = true;

            // Create selection rectangle
            _selectionRectangle = new Rectangle
            {
                Stroke = new SolidColorBrush(Color.FromRgb(0, 120, 212)),
                StrokeThickness = 2,
                Fill = new SolidColorBrush(Color.FromArgb(50, 0, 120, 212))
            };

            Canvas.SetLeft(_selectionRectangle, _startPoint.X);
            Canvas.SetTop(_selectionRectangle, _startPoint.Y);
            SelectionCanvas.Children.Add(_selectionRectangle);

            DimensionBorder.Visibility = Visibility.Visible;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isSelecting || _selectionRectangle == null) return;

            var currentPoint = e.GetPosition(this);

            var x = Math.Min(_startPoint.X, currentPoint.X);
            var y = Math.Min(_startPoint.Y, currentPoint.Y);
            var width = Math.Abs(currentPoint.X - _startPoint.X);
            var height = Math.Abs(currentPoint.Y - _startPoint.Y);

            Canvas.SetLeft(_selectionRectangle, x);
            Canvas.SetTop(_selectionRectangle, y);
            _selectionRectangle.Width = width;
            _selectionRectangle.Height = height;

            // Update dimension display (show actual pixel dimensions)
            var actualWidth = (int)(width * _dpiScaleX);
            var actualHeight = (int)(height * _dpiScaleY);
            DimensionText.Text = $"{actualWidth} × {actualHeight}";
            Canvas.SetLeft(DimensionBorder, x);
            Canvas.SetTop(DimensionBorder, y + height + 10);
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isSelecting || _selectionRectangle == null) return;

            _isSelecting = false;

            var currentPoint = e.GetPosition(this);

            // Calculate selection bounds in WPF coordinates
            var wpfX = Math.Min(_startPoint.X, currentPoint.X);
            var wpfY = Math.Min(_startPoint.Y, currentPoint.Y);
            var wpfWidth = Math.Abs(currentPoint.X - _startPoint.X);
            var wpfHeight = Math.Abs(currentPoint.Y - _startPoint.Y);

            // Convert to physical pixels (accounting for DPI scaling)
            var x = (int)(wpfX * _dpiScaleX);
            var y = (int)(wpfY * _dpiScaleY);
            var width = (int)(wpfWidth * _dpiScaleX);
            var height = (int)(wpfHeight * _dpiScaleY);

            System.Diagnostics.Debug.WriteLine($"[SelectionOverlay] WPF coords: ({wpfX}, {wpfY}) {wpfWidth}x{wpfHeight}");
            System.Diagnostics.Debug.WriteLine($"[SelectionOverlay] Physical coords: ({x}, {y}) {width}x{height}");

            // Minimum size check
            if (width < 10 || height < 10)
            {
                Close();
                return;
            }

            // Hide overlay before capturing
            this.Hide();

            try
            {
                // Crop from pre-captured screen
                if (_screenCapture != null)
                {
                    // Ensure we don't exceed bitmap bounds
                    x = Math.Max(0, Math.Min(x, _screenCapture.Width - 1));
                    y = Math.Max(0, Math.Min(y, _screenCapture.Height - 1));
                    width = Math.Min(width, _screenCapture.Width - x);
                    height = Math.Min(height, _screenCapture.Height - y);

                    System.Diagnostics.Debug.WriteLine($"[SelectionOverlay] Cropping: ({x}, {y}) {width}x{height} from bitmap {_screenCapture.Width}x{_screenCapture.Height}");

                    var croppedBitmap = _screenCapture.Clone(
                        new System.Drawing.Rectangle(x, y, width, height),
                        _screenCapture.PixelFormat);

                    // Copy to clipboard if enabled
                    if (App.Settings.CurrentSettings.CopyToClipboard)
                    {
                        ClipboardService.CopyImageToClipboard(croppedBitmap);
                    }

                    // Save to file
                    var savedPath = App.Screenshot.SaveScreenshot(croppedBitmap);
                    
                    System.Diagnostics.Debug.WriteLine($"[SelectionOverlay] Screenshot saved to: {savedPath}");

                    croppedBitmap.Dispose();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SelectionOverlay] Error: {ex.Message}");
                MessageBox.Show($"Failed to capture screenshot: {ex.Message}",
                    "ClipSnap", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _screenCapture?.Dispose();
                Close();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                _screenCapture?.Dispose();
                Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _screenCapture?.Dispose();
            base.OnClosed(e);
        }
    }
}