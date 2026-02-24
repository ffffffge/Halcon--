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
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Halcon查询
{
    /// <summary>
    /// SearchView.xaml 的交互逻辑
    /// </summary>
    public partial class SearchView : Page
    {
        public SearchView()
        {
            InitializeComponent();
            this.DataContext =  new SearchViewModel();
        }
    }
}
