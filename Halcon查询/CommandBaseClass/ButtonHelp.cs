using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace Halcon查询.CommandBaseClass
{

    /// <summary>
    /// 【新增】按钮辅助类，提供图片相关附加属性
    /// </summary>
    public static class ButtonHelper
    {
        #region ImageSource 附加属性
        /// <summary>
        /// 图片源附加属性，用于设置按钮显示的图片
        /// </summary>
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.RegisterAttached(
                "ImageSource",
                typeof(ImageSource),
                typeof(ButtonHelper),
                new PropertyMetadata(null, OnImageSourceChanged));

        public static ImageSource GetImageSource(DependencyObject obj)
        {
            return (ImageSource)obj.GetValue(ImageSourceProperty);
        }

        public static void SetImageSource(DependencyObject obj, ImageSource value)
        {
            obj.SetValue(ImageSourceProperty, value);
        }

        private static void OnImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // 当图片源改变时，可以在这里添加额外逻辑
            // 例如：更新按钮的ToolTip等
        }
        #endregion

        #region ImageWidth 附加属性
        /// <summary>
        /// 图片宽度附加属性
        /// </summary>
        public static readonly DependencyProperty ImageWidthProperty =
            DependencyProperty.RegisterAttached(
                "ImageWidth",
                typeof(double),
                typeof(ButtonHelper),
                new PropertyMetadata(16.0));

        public static double GetImageWidth(DependencyObject obj)
        {
            return (double)obj.GetValue(ImageWidthProperty);
        }

        public static void SetImageWidth(DependencyObject obj, double value)
        {
            obj.SetValue(ImageWidthProperty, value);
        }
        #endregion

        #region ImageHeight 附加属性
        /// <summary>
        /// 图片高度附加属性
        /// </summary>
        public static readonly DependencyProperty ImageHeightProperty =
            DependencyProperty.RegisterAttached(
                "ImageHeight",
                typeof(double),
                typeof(ButtonHelper),
                new PropertyMetadata(16.0));

        public static double GetImageHeight(DependencyObject obj)
        {
            return (double)obj.GetValue(ImageHeightProperty);
        }

        public static void SetImageHeight(DependencyObject obj, double value)
        {
            obj.SetValue(ImageHeightProperty, value);
        }
        #endregion

        #region ImageStretch 附加属性
        /// <summary>
        /// 图片拉伸方式附加属性（可选）
        /// </summary>
        public static readonly DependencyProperty ImageStretchProperty =
            DependencyProperty.RegisterAttached(
                "ImageStretch",
                typeof(Stretch),
                typeof(ButtonHelper),
                new PropertyMetadata(Stretch.Uniform));

        public static Stretch GetImageStretch(DependencyObject obj)
        {
            return (Stretch)obj.GetValue(ImageStretchProperty);
        }

        public static void SetImageStretch(DependencyObject obj, Stretch value)
        {
            obj.SetValue(ImageStretchProperty, value);
        }
        #endregion
    }
}
