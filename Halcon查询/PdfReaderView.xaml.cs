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
using System.Windows.Forms.Integration;
using Microsoft.Web.WebView2.Core;

namespace Halcon查询
{
    /// <summary>
    /// PdfReaderView.xaml 的交互逻辑
    /// </summary>
    public partial class PdfReaderView : Page
    {
        public PdfReaderView()
        {
            InitializeComponent();
            PdfReaderViewModel pdfReaderViewModel = new PdfReaderViewModel();
            this.DataContext = pdfReaderViewModel;
        }
    }
}
