using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Halcon查询
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public static ShareDataModel shareDataModel = new ShareDataModel();
        protected override void OnStartup(StartupEventArgs e)
        {
            // 在程序启动时加载嵌入的 DLL
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            base.OnStartup(e);
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // 根据请求的程序集名称进行处理
            string assemblyName = new AssemblyName(args.Name).Name + ".dll";

            string resourceName = $"YourApp.Resources.{assemblyName}";

            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream == null) return null;

                byte[] assemblyData = new byte[stream.Length];
                stream.Read(assemblyData, 0, assemblyData.Length);

                // 加载程序集
                return Assembly.Load(assemblyData);
            }

            return null;
        }
    }


    //转换器类型
    public class NameToImageConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return null;
            string ImageName = $"Halcon查询.Resource.{parameter}.png";
            Assembly assembly = Assembly.GetExecutingAssembly();
            try
            {
                using (Stream imagestream = assembly.GetManifestResourceStream(ImageName))
                {
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = imagestream;
                    image.EndInit();
                    image.Freeze();
                    return image;
                }
            }
            catch { return null; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class PercentageConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double actualWidth && parameter is string percentagestr)
            {
                double percentage = Convert.ToDouble(percentagestr);
                return (actualWidth - 80 - 100) * percentage / 100 - 10;
            }
            else
                return 300;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
