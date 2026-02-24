using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyCommands;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Security.Cryptography;

namespace Halcon查询
{
    public class MainWindowViewModel : NotifyPropretyObject, IDisposable
    {
        public CommandsBaseClass CutPageCommand
        {
            get; set;
        }


        #region 命令方法
        private void CutPageCommandAction(object obj)
        {
            switch (obj.ToString())
            {
                case "Search":
                    App.shareDataModel.SeletPage = searchView;
                    break;
                case "PdfView":
                    App.shareDataModel.SeletPage = pdfReaderView;
                    break;
                case "Detail":
                    App.shareDataModel.SeletPage = detailview;
                    break;
                default:
                    break;
            }
        }
        #endregion

        PdfReaderView pdfReaderView;
        SearchView searchView;
        DetailView detailview;
        // 智能副驾视图模型
        public AIChatViewModel ChatViewModel { get; private set; }
        private bool disposedValue;

        public MainWindowViewModel()
        {
            CutPageCommand = new CommandsBaseClass();
            CutPageCommand.ExecuteAction = CutPageCommandAction;

            pdfReaderView = new PdfReaderView();
            searchView = new SearchView();
            detailview = new DetailView();

            ChatViewModel = new AIChatViewModel();

            WeakEventBus.Default.Subscribe<NavigationEvent>(OnNavigationEvent);
            App.shareDataModel.SeletPage = searchView;


        }

        private void OnNavigationEvent(NavigationEvent obj)
        {
            CutPageCommandAction(obj.TargetViewName);
        }

        #region 实现IDispose
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    WeakEventBus.Default.Unsubscribe<NavigationEvent>(OnNavigationEvent);
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~MainWindowViewModel()
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
