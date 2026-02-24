using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
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

namespace Halcon查询
{
    /// <summary>
    /// DetailView.xaml 的交互逻辑
    /// </summary>
    public partial class DetailView : Page
    {
        public DetailView()
        {
            InitializeComponent();
            this.DataContext = new DetailViewModel();
            InitializeWebView2();
        }
        private async void InitializeWebView2()
        {
            // 创建无持久化的环境选项
            var options = new CoreWebView2EnvironmentOptions
            {
                // 禁用各种持久化
                AdditionalBrowserArguments =
                    "--disable-local-storage " +
                    "--disable-databases " +
                    "--disable-session-storage " +
                    "--disable-cache " +
                    "--disk-cache-size=0 " +
                    "--disable-application-cache " +
                    "--disable-background-networking " +
                    "--disable-sync"
            };

            // 使用内存模式文件夹
            string inMemoryFolder = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Temp",
                $"WV2_Memory_{Guid.NewGuid()}");

            var env = await CoreWebView2Environment.CreateAsync(
                userDataFolder: inMemoryFolder,
                browserExecutableFolder: null,
                options: options);

            await WebView.EnsureCoreWebView2Async(env);

            // 初始设置：强制亮色主题
            WebView.CoreWebView2.Profile.PreferredColorScheme =
                CoreWebView2PreferredColorScheme.Light;
        }
    }
}
