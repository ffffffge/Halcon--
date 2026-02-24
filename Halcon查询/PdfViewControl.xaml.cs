using Patagames.Pdf.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Media.Media3D;
using Patagames.Pdf.Enums;
using Patagames.Pdf;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Drawing.Imaging;
using System.Windows.Media.Animation;
using System.Diagnostics.Eventing.Reader;

namespace Halcon查询
{
    /// <summary>
    /// PdfViewControl.xaml 的交互逻辑
    /// </summary>
    public partial class PdfViewControl : System.Windows.Controls.UserControl
    {
        #region 依赖属性

        #region SelectPage For int
        public static readonly DependencyProperty SelectPageProperty = DependencyProperty.Register("SelectPage", typeof(int), typeof(PdfViewControl), new FrameworkPropertyMetadata(0, OnSelectPageChanged, CoerceSelectPageValue));
        public int SelectPage
        {
            get => (int)GetValue(SelectPageProperty);
            set => SetValue(SelectPageProperty, value);
        }
        private static void OnSelectPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (PdfViewControl)d;
            var newValue = (int)e.NewValue;
            var oldValue = (int)e.OldValue;
            if (control.PdfDocument == null || newValue >= control.pdfDocument.Pages.Count - 1 || newValue < 0)
                return;
            control.Dispatcher.Invoke(() =>
            {
                control.currentPage = newValue;
                control.RenderPage();
                control.UpdateUI();
            });
        }
        private static object CoerceSelectPageValue(DependencyObject d, object baseValue)
        {
            int Page;
            bool CanConvert = int.TryParse(baseValue.ToString(), out Page);

            // 示例：确保标题不为null，最大长度50
            if (CanConvert == false)
                return 0;

            if (Page < 0)
                return 0;

            return Page;
        }
        #endregion

        #region PdfPath For String
        public static readonly DependencyProperty PdfPathProperty = DependencyProperty.Register("PdfPath", typeof(string), typeof(PdfViewControl), new FrameworkPropertyMetadata("null", PdfPathChanged, CoercePdfPathValue));
        public string PdfPath
        {
            get => (string)GetValue(PdfPathProperty);
            set => SetValue(PdfPathProperty, value);
        }
        private static void PdfPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (PdfViewControl)d;
            var newValue = (string)e.NewValue;
            var oldValue = (string)e.OldValue;
            if (newValue == "null" || !File.Exists(newValue))
                return;
            control.Dispatcher.Invoke(() =>
            {
                control.LoadPdfFile(newValue);
            });
        }
        private static object CoercePdfPathValue(DependencyObject d, object baseValue)
        {
            string pdfPath = (string)baseValue;
            if (File.Exists(pdfPath))
            {
                return pdfPath;
            }
            else
                return "null";
        }
        #endregion

        #endregion

        private PdfDocument pdfDocument;
        public PdfDocument PdfDocument
        {
            get { return pdfDocument; }
            set
            {
                pdfDocument = value;

            }
        }

        private int currentPage = 0;//当前页面
        private System.Windows.Point _mousePosition;//缓存当前鼠标位置
        private System.Windows.Point _lastMousePosition;//上次鼠标位置
        private double _scale = 1.0;//缩放比例
        private bool _isDragging = false;//是否拖动标志位
        private const double ZoomStep = 0.1; // 缩放步长
        private const double MinZoom = 0.2;  // 最小缩放
        private const double MaxZoom = 5.0;   // 最大缩放
        public PdfViewControl()
        {
            InitializeComponent();

            //LoadPdfFile(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Halcon查询\\temp.pdf");
            //cbZoom.SelectedIndex = 3; // 默认选择100%
            //UpdateUI();

            // 添加键盘事件处理
            this.PreviewKeyDown += MainWindow_PreviewKeyDown;
            pdfImage.PreviewMouseWheel += MainWindow_MouseWheel;
            pdfImage.MouseLeftButtonDown += DisplayImage_MouseLeftButtonDown;
            pdfImage.MouseLeftButtonUp += DisplayImage_MouseLeftButtonUp;
            pdfImage.MouseMove += DisplayImage_MouseMove;
            this.Unloaded += UserControl_Unloaded;
        }

        #region 操作函数体
        // 打开PDF文件
        private void btnOpen_Click(object sender, RoutedEventArgs e)//禁用打开功能
        {
            try
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "PDF文件 (*.pdf)|*.pdf|所有文件 (*.*)|*.*",
                    DefaultExt = ".pdf"
                };

                if (dialog.ShowDialog() == true)
                {
                    LoadPdfFile(dialog.FileName);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"打开文件时出错: {ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //加载pdf文件函数
        private void LoadPdfFile(string filePath)
        {
            if (filePath.ToString() == null)
                return;
            try
            {
                // 释放之前的文档
                if (pdfDocument != null)
                {
                    pdfDocument.Dispose();
                    pdfDocument = null;
                }

                // 加载新文档
                pdfDocument = PdfDocument.Load(filePath);
                currentPage = 0;
                _scale = 1;

                // 更新界面
                UpdateUI();
                RenderPage();
                ApplyFinalZoom(_scale);

                tbStatus.Text = $"已加载: {System.IO.Path.GetFileName(filePath)}";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"加载PDF文件失败: {ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 渲染当前页面
        private void RenderPage()
        {
            if (pdfDocument == null || currentPage < 0 || currentPage >= pdfDocument.Pages.Count)
                return;

            try
            {
                // 获取页面尺寸
                var pageSize = pdfDocument.Pages[currentPage];
                double screenWidth = SystemParameters.PrimaryScreenWidth;
                double screenHeight = SystemParameters.PrimaryScreenHeight;

                var dpi = 96; // WPF的标准DPI

                //1英寸600像素大小的DPI创建图片
                int renderWidth = (int)(pageSize.Width / 72 * 600);
                int renderHeight = (int)(pageSize.Height / 72 * 600);

                Task.Run(() =>
                {
                    // 使用PDFium渲染页面
                    using (var stream = new MemoryStream())
                    using (var image = new PdfBitmap(renderWidth, renderHeight, true))
                    {
                        // 保存为位图流
                        image.FillRect(0, 0, renderWidth, renderHeight, FS_COLOR.White);
                        pageSize.Render(image, 0, 0, renderWidth, renderHeight, PageRotate.Normal, RenderFlags.FPDF_NONE);
                        image.GetImage().Save(stream, ImageFormat.Png);

                        stream.Position = 0;

                        // 转换为WPF ImageSource
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = stream;
                        bitmapImage.EndInit();
                        bitmapImage.Freeze(); // 提高性能

                        //FileStream testfile = new FileStream("C:\\Users\\19701\\Desktop\\文件\\等待的风\\test.png", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                        //stream.Position = 0;
                        //stream.CopyTo(testfile);
                        //testfile.Flush();
                        //testfile.Close();

                        Dispatcher.BeginInvoke(() =>
                        {
                            pdfImage.Source = bitmapImage;
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"渲染页面时出错: {ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //鼠标指针缩放
        private void PerformSmoothZoom(double targetScale, System.Windows.Point zoomCenter)
        {
            double startScale = _scale;
            if (targetScale != 1)
            {
                if (_scale >= 2)
                    _scale += ZoomStep * 5;
                else
                    _scale += ZoomStep;
                if (_scale > MaxZoom) _scale = MaxZoom;
            }
            else
            {
                if (_scale > 2)
                    _scale -= ZoomStep * 5;
                else
                    _scale -= ZoomStep;
                if (_scale < MinZoom) _scale = MinZoom;
            }

            //_scale = targetScale;

            // 创建动画
            //DoubleAnimation scaleAnimation = new DoubleAnimation
            //{
            //    From = startScale,
            //    To = targetScale,
            //    Duration = TimeSpan.FromMilliseconds(200),
            //    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            //};

            //scaleAnimation.Completed += (s, e) =>
            //{
            //    _scale = targetScale;
            ApplyFinalZoom(_scale);
            //};

            //// 计算动画过程中的中间滚动位置
            //scaleAnimation.CurrentTimeInvalidated += (s, e) =>
            //{
            //    double currentScale = ((DoubleAnimation)s).GetValue(DoubleAnimation.CurrentValueProperty) as double? ?? startScale;

            //    // 实时更新滚动位置
            UpdateScrollPosition(startScale, _scale, zoomCenter);
            //};

            //// 开始动画
            //var scaleTransform = new ScaleTransform(startScale, startScale);
            //scrollViewer.LayoutTransform = scaleTransform;
            //scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            //scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        }

        //更新ScrollView位置
        private void UpdateScrollPosition(double startScale, double currentScale, System.Windows.Point zoomCenter)
        {
            // 实时更新滚动位置，实现平滑缩放
            double contentX = scrollViewer.HorizontalOffset + zoomCenter.X;
            double contentY = scrollViewer.VerticalOffset + zoomCenter.Y;

            double newContentX = contentX * currentScale / startScale;
            double newContentY = contentY * currentScale / startScale;

            double newScrollX = newContentX - zoomCenter.X;
            double newScrollY = newContentY - zoomCenter.Y;

            // 边界检查
            newScrollX = Math.Max(0, Math.Min(newScrollX, scrollViewer.ScrollableWidth));
            newScrollY = Math.Max(0, Math.Min(newScrollY, scrollViewer.ScrollableHeight));

            scrollViewer.ScrollToHorizontalOffset(newScrollX);
            scrollViewer.ScrollToVerticalOffset(newScrollY);
        }

        //应用最终的缩放变换
        private void ApplyFinalZoom(double newScale)
        {
            var transform = new ScaleTransform(newScale, newScale);
            pdfImage.LayoutTransform = transform;
            _scale = newScale;

            pdfImage.UpdateLayout();
        }

        private void ZoomIn()
        {
            if (pdfDocument == null) return;

            _scale += ZoomStep;
            if (_scale > MaxZoom) _scale = MaxZoom;

            UpdateZoomUI();
            ApplyFinalZoom(_scale);
        }

        private void ZoomOut()
        {
            if (pdfDocument == null) return;

            _scale -= ZoomStep;
            if (_scale < MinZoom) _scale = MinZoom;
            UpdateZoomUI();
            ApplyFinalZoom(_scale);
        }
        #endregion

        #region 控件信息更改
        // 更新UI状态
        private void UpdateUI()
        {
            if (pdfDocument != null)
            {
                tbPageInfo.Text = $"第 {currentPage + 1} 页 / 共 {pdfDocument.Pages.Count} 页";

                // 更新按钮状态
                btnFirstPage.IsEnabled = currentPage > 0;
                btnPrevPage.IsEnabled = currentPage > 0;
                btnNextPage.IsEnabled = currentPage < pdfDocument.Pages.Count - 1;
                btnLastPage.IsEnabled = currentPage < pdfDocument.Pages.Count - 1;

                // 更新缩放按钮状态
                btnZoomIn.IsEnabled = _scale < MaxZoom;
                btnZoomOut.IsEnabled = _scale > MinZoom;
            }
            else
            {
                tbPageInfo.Text = "无文档";
                btnFirstPage.IsEnabled = false;
                btnPrevPage.IsEnabled = false;
                btnNextPage.IsEnabled = false;
                btnLastPage.IsEnabled = false;
                btnZoomIn.IsEnabled = false;
                btnZoomOut.IsEnabled = false;
            }
        }

        private void UpdateZoomUI()
        {
            tbZoomLevel.Text = $"缩放: {(_scale * 100):F0}%";

            // 更新下拉框选中项
            int zoomPercent = (int)(_scale * 100);
            string zoomText = $"{zoomPercent}%";

            foreach (ComboBoxItem item in cbZoom.Items)
            {
                if (zoomText == item.Content.ToString())
                {
                    cbZoom.SelectedItem = item;
                    break;
                }
            }
            UpdateUI();
        }
        #endregion

        #region 导航功能
        private void btnFirstPage_Click(object sender, RoutedEventArgs e)
        {
            if (pdfDocument == null) return;
            currentPage = 0;
            UpdateUI();
            RenderPage();
        }

        private void btnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (pdfDocument == null || currentPage <= 0) return;
            currentPage--;
            UpdateUI();
            RenderPage();
        }

        private void btnNextPage_Click(object sender, RoutedEventArgs e)
        {
            if (pdfDocument == null || currentPage >= pdfDocument.Pages.Count - 1) return;
            currentPage++;
            UpdateUI();
            RenderPage();
        }

        private void btnLastPage_Click(object sender, RoutedEventArgs e)
        {
            if (pdfDocument == null) return;
            currentPage = pdfDocument.Pages.Count - 1;
            UpdateUI();
            RenderPage();
        }

        // 缩放功能
        private void btnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            ZoomIn();
        }

        private void btnZoomOut_Click(object sender, RoutedEventArgs e)
        {
            ZoomOut();
        }

        // 下拉框缩放选择
        private void cbZoom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pdfDocument == null || cbZoom.SelectedItem == null) return;

            var selectedItem = (ComboBoxItem)cbZoom.SelectedItem;
            var content = selectedItem.Content.ToString();

            switch (content)
            {
                case "适合页面":
                    // 计算适合页面的缩放比例
                    if (pdfDocument != null && pdfImage.ActualHeight > 0)
                    {
                        var dpi = VisualTreeHelper.GetDpi(scrollViewer);
                        var pageSize = pdfDocument.Pages[currentPage];
                        var availableHeight = scrollViewer.ActualHeight * dpi.DpiScaleY; // 设备无关像素乘以DPI缩放比例
                        _scale = availableHeight / (pageSize.Height / 72.0 * 600);
                    }
                    break;

                case "适合宽度":
                    // 计算适合宽度的缩放比例
                    if (pdfDocument != null && pdfImage.ActualWidth > 0)
                    {
                        var dpi = VisualTreeHelper.GetDpi(scrollViewer);
                        var pageSize = pdfDocument.Pages[currentPage];
                        var availableWidth = scrollViewer.ActualWidth * dpi.DpiScaleX; // 设备无关像素乘以DPI缩放比例
                        _scale = availableWidth / (pageSize.Width / 72.0 * 600);
                    }
                    break;

                default:
                    // 解析百分比
                    if (content.EndsWith("%"))
                    {
                        var percentStr = content.TrimEnd('%');
                        if (double.TryParse(percentStr, out double percent))
                        {
                            _scale = percent / 100.0;
                        }
                    }
                    break;
            }
            UpdateZoomUI();
            ApplyFinalZoom(_scale);
        }
        #endregion

        #region 键盘快捷键支持
        private void MainWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (pdfDocument == null) return;

            switch (e.Key)
            {
                case Key.Left:
                case Key.Up:
                case Key.PageUp:
                    if (currentPage > 0)
                    {
                        currentPage--;
                        RenderPage();
                        UpdateUI();
                        e.Handled = true;
                    }
                    break;

                case Key.Right:
                case Key.Down:
                case Key.PageDown:
                    if (currentPage < pdfDocument.Pages.Count - 1)
                    {
                        currentPage++;
                        RenderPage();
                        UpdateUI();
                        e.Handled = true;
                    }
                    break;

                case Key.Home:
                    currentPage = 0;
                    RenderPage();
                    UpdateUI();
                    e.Handled = true;
                    break;

                case Key.End:
                    currentPage = pdfDocument.Pages.Count - 1;
                    RenderPage();
                    UpdateUI();
                    e.Handled = true;
                    break;

                case Key.Add:
                case Key.OemPlus:
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        ZoomIn();
                        e.Handled = true;
                    }
                    break;

                case Key.Subtract:
                case Key.OemMinus:
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        ZoomOut();
                        e.Handled = true;
                    }
                    break;

                case Key.D0:
                case Key.NumPad0:
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        _scale = 1.0;
                        UpdateZoomUI();
                        RenderPage();
                        e.Handled = true;
                    }
                    break;
            }
        }
        #endregion

        #region 鼠标快捷键支持
        private void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)// 鼠标滚轮缩放
        {
            if (pdfDocument == null || Keyboard.Modifiers != ModifierKeys.Control)
                return;

            System.Windows.Point currentPos = e.GetPosition(scrollViewer);
            if (e.Delta > 0)
            {
                PerformSmoothZoom(-1, currentPos);
            }
            else
            {
                PerformSmoothZoom(1, currentPos);
            }

            UpdateZoomUI();

            //禁止隧道事件进入Scrollview
            e.Handled = true;
        }

        private void DisplayImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)//按下鼠标事件
        {
            if (!_isDragging)
            {
                _lastMousePosition = e.GetPosition(scrollViewer);
                _isDragging = true;
            }
            else
                _isDragging = true;
            pdfImage.CaptureMouse();
            pdfImage.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void DisplayImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)//松开鼠标事件
        {
            _isDragging = false;
            pdfImage.ReleaseMouseCapture();
            pdfImage.Cursor = System.Windows.Input.Cursors.Arrow;
        }

        private void DisplayImage_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)//鼠标拖动事件
        {
            if (_isDragging)
            {
                System.Windows.Point currentPos = e.GetPosition(scrollViewer);
                double deltaX = _lastMousePosition.X - currentPos.X;
                double deltaY = _lastMousePosition.Y - currentPos.Y;
                _lastMousePosition = currentPos;

                if (Math.Abs(deltaX) < 1 && Math.Abs(deltaY) < 1)
                    return;

                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + deltaX);
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + deltaY);
            }
        }
        #endregion
        // 清理资源
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (pdfDocument != null)
            {
                pdfDocument.Dispose();
                pdfDocument = null;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //寻找适应宽度的缩放比例
            if (pdfDocument != null && pdfImage.ActualWidth > 0)
            {
                var dpi = VisualTreeHelper.GetDpi(scrollViewer);
                var pageSize = pdfDocument.Pages[currentPage];
                var availableWidth = (scrollViewer.ActualWidth - 50) * dpi.DpiScaleX; // 设备无关像素乘以DPI缩放比例
                _scale = availableWidth / (pageSize.Width / 72.0 * 600);
                UpdateZoomUI();
                ApplyFinalZoom(_scale);
            }
        }
    }
}
