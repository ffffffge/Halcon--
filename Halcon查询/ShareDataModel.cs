using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Halcon查询
{
    public class ShareDataModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }    
        }

        private int _PdfSeletPage;
        public int PdfSeletPage
        {
            get { return _PdfSeletPage; }
            set
            {
                _PdfSeletPage = value;
                OnPropertyChanged(nameof(PdfSeletPage));
            }
        }

        private Page _seletpage;
        public Page SeletPage//选中的页面
        {
            get { return _seletpage; }
            set
            {
                _seletpage = value;
                OnPropertyChanged(nameof(SeletPage));
            }
        }

        private string _HtmlContent;
        public string HtmlContent//详情查看页面的html.String
        {
            get { return _HtmlContent; }
            set
            {
                _HtmlContent = value;
                OnPropertyChanged(nameof(HtmlContent));
            }
        }
    }
}
