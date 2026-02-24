using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;

namespace Halcon查询
{
    /// <summary>
    /// MainWindowView.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindowView : Window
    {
        public MainWindowView()
        {
            InitializeComponent();
            this.DataContext = new MainWindowViewModel();
        }

        #region 稍微改变控件透明度再还原，强制GPU重新绘制窗体，防止放大缩小行为导致的视觉渲染问题
        private void ForceVisualRedraw(Visual visual)
        {
            if (visual is UIElement uiElement)
            {
                //1.触发渲染更新
                Dispatcher.BeginInvoke(() =>
                {
                    // 微调属性触发重绘
                    var opacity = uiElement.Opacity;
                    if (Math.Abs(opacity - 1.0) < 0.001)
                    {
                        uiElement.Opacity = 0.999;
                        uiElement.Opacity = 1.0;
                    }
                    else if (opacity > 0.01) // 对于半透明元素
                    {
                        // 轻微调整不透明度
                        uiElement.Opacity = opacity * 0.999;
                        uiElement.Opacity = opacity;
                    }
                }, DispatcherPriority.Background);
            }
        }
        #endregion

        //订阅窗口大小改变事件用于防止放大缩小行为导致的视觉渲染问题
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ForceVisualRedraw(MainWindowGrid);
        }
    }

}
