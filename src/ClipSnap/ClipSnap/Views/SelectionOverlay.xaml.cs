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

        public SelectionOverlay()
        {
            InitializeComponent();
            
            // Cover all screens
            var virtualScreen = System.Windows.Forms.SystemInformation.VirtualScreen;
            Left = virtualScreen.X;
            Top = virtualScreen.Y;
            Width = virtualScreen.Width;
            Height = virtualScreen.Height;

            // Capture the screen before showing overlay
            _screenCapture = App.Screenshot.CaptureAllScreens();

            MouseLeftButtonDown += OnMouseLeftButtonDown;
            MouseLeftButtonUp += OnMouseLeftButtonUp;
            MouseMove += OnMouseMove;
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

            // Update dimension display
            DimensionText.Text = $"{(int)width} × {(int)height}";
            Canvas.SetLeft(DimensionBorder, x);
            Canvas.SetTop(DimensionBorder, y + height + 10);
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isSelecting || _selectionRectangle == null) return;

            _isSelecting = false;

            var currentPoint = e.GetPosition(this);

            // Calculate selection bounds relative to virtual screen
            var virtualScreen = System.Windows.Forms.SystemInformation.VirtualScreen;
            var x = (int)Math.Min(_startPoint.X, currentPoint.X);
            var y = (int)Math.Min(_startPoint.Y, currentPoint.Y);
            var width = (int)Math.Abs(currentPoint.X - _startPoint.X);
            var height = (int)Math.Abs(currentPoint.Y - _startPoint.Y);

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
                    
                    // Show notification (optional - could use toast)
                    System.Diagnostics.Debug.WriteLine($"Screenshot saved to: {savedPath}");

                    croppedBitmap.Dispose();
                }
            }
            catch (Exception ex)
            {
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