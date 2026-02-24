using MyCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Windows.Forms.Integration;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using Patagames.Pdf;
using Patagames.Pdf.Net;
using Patagames.Pdf.Net.Controls.Wpf;
using System.Drawing.Printing;

namespace Halcon查询
{
    public class PdfReaderViewModel : NotifyPropretyObject, IDisposable
    {
        private string m_pdfpath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Halcon查询\\temp.pdf";
        private string _PdfPath;
        private bool disposedValue;

        #region 依赖属性
        public string PdfPath
        {
            get
            {
                return _PdfPath;
            }
            set
            {
                _PdfPath = value;
                OnPropertyChanged(nameof(PdfPath));
            }
        }

        public int SelectPdfPage
        {
            get
            {
                return _SelectPdfPage;
            }
            set
            {
                _SelectPdfPage = value;
                OnPropertyChanged(nameof(SelectPdfPage));
            }
        }
        private int _SelectPdfPage;
        #endregion
        public PdfReaderViewModel()
        {
            string str = System.IO.Path.GetDirectoryName(m_pdfpath);
            if (!Directory.Exists(System.IO.Path.GetDirectoryName(m_pdfpath)))
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(m_pdfpath));
            }
            if (!File.Exists(m_pdfpath))
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream("Halcon查询.Resource.Halcon算子手册-1-672.pdf"))
                {
                    FileStream fileStream = new FileStream(m_pdfpath, FileMode.Create);
                    stream.Position = 0;
                    stream.CopyTo(fileStream);
                    fileStream.Flush();
                }
            }
            WeakEventBus.Default.Subscribe<SyncPdfPageEvent>(OnSyncPdfPage);

            PdfPath = m_pdfpath;
            SelectPdfPage = 0;
        }

        private void OnSyncPdfPage(SyncPdfPageEvent obj)
        {
            int page = obj.PageIndex + 5;
            SelectPdfPage = page;
        }

        #region 实现IDispose
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    WeakEventBus.Default.Unsubscribe<SyncPdfPageEvent>(OnSyncPdfPage);
                    // TODO: 释放托管状态(托管对象)
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~PdfReaderViewModel()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
