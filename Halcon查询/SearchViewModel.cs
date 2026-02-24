using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Printing.IndexedProperties;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.VisualStyles;
using Halcon查询.CommandBaseClass;
using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using MyCommands;
using Patagames.Pdf.Net;
using static System.Windows.Forms.ListView;

namespace Halcon查询
{
    public class SearchResult : NotifyPropretyObject
    {
        #region 依赖属性
        private string _FuncName;
        public string FuncName
        {
            get
            {
                return _FuncName;
            }
            set
            {
                _FuncName = value;
                OnPropertyChanged(nameof(FuncName));
            }
        }
        private string _FuncCapacity;
        public string FuncCapacity
        {
            get { return _FuncCapacity; }
            set
            {
                _FuncCapacity = value;
                OnPropertyChanged(nameof(FuncCapacity));
            }
        }
        private int _PageIndex;
        public int PageIndex
        {
            get
            {
                return _PageIndex;
            }
            set
            {
                _PageIndex = value;
                OnPropertyChanged(nameof(PageIndex));
            }
        }

        private bool _CanJump;
        public bool CanJump
        {
            get
            {
                return _CanJump;
            }
            set
            {
                _CanJump = value;
                OnPropertyChanged(nameof(CanJump));
            }
        }
        private string _htmlContent;
        public string HtmlContent
        {
            get { return _htmlContent; }
            set
            {
                _htmlContent = value;
                OnPropertyChanged(nameof(HtmlContent));
            }
        }
        public SearchResult() { }
        public SearchResult CopyTo()
        {
            SearchResult result = new SearchResult();
            result._FuncName = _FuncName;
            result._FuncCapacity = _FuncCapacity;
            result._PageIndex = _PageIndex;
            result._CanJump = _CanJump;
            return result;
        }
        #endregion
    }
    class SearchViewModel : NotifyPropretyObject, IDisposable
    {
        //载入csv文件，获取的csv表格内容
        List<List<Tuple<string, string, int>>> CsvLibrary = new List<List<Tuple<string, string, int>>>();
        List<SearchResult> tempResults = new List<SearchResult>();
        string m_Databasepath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Halcon查询\\HalconSearch.db";

        #region 属性
        private ObservableCollection<SearchResult> _results;
        public ObservableCollection<SearchResult> results
        {
            get { return _results; }
            set
            {
                _results = value;
                ResultesPageInt = results.Count.ToString();
                OnPropertyChanged(nameof(results));
            }
        }

        private string _SearchInput = "";
        public string SearchInput
        {
            get
            { return _SearchInput; }
            set
            {
                _SearchInput = value;
                OnPropertyChanged(nameof(SearchInput));
            }
        }

        private string _resultesPageInt;
        public string ResultesPageInt
        {
            get
            {
                return _resultesPageInt;
            }
            set
            {
                _resultesPageInt = $"找到 {value} 个结果";
                if (value == "0")
                {
                    SearchViewVisibility = Visibility.Collapsed;

                }
                else
                {
                    SearchViewVisibility = Visibility.Visible;

                }
                OnPropertyChanged(nameof(ResultesPageInt));
            }
        }

        private Visibility _SearchViewVisibility;
        private bool disposedValue;

        public Visibility SearchViewVisibility
        {
            get
            {
                return (Visibility)_SearchViewVisibility;
            }
            set
            {
                _SearchViewVisibility = value;
                NoSearchResultViewVisivility = Visibility.Collapsed;
                OnPropertyChanged(nameof(SearchViewVisibility));
            }
        }

        public Visibility NoSearchResultViewVisivility
        {
            get
            {
                if (_SearchViewVisibility == Visibility.Collapsed)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
            set
            {
                OnPropertyChanged(nameof(NoSearchResultViewVisivility));
            }
        }
        #endregion

        #region 命令方法和执行函数
        public CommandsBaseClass SearchByNameCommand { get; set; }
        [Obsolete("该方法已废弃，禁止使用", true)]
        public void SearchByNameCommandActionAbandon(object param)
        {
            if (SearchInput == "")
                return;
            results.Clear(); tempResults.Clear();

            List<Task<List<SearchResult>>> tasks = new List<Task<List<SearchResult>>>();
            foreach (List<Tuple<string, string, int>> LibraryItem in CsvLibrary)
            {
                Task<List<SearchResult>> taskitem = new Task<List<SearchResult>>(() => SearchByIdCommandFunc(SearchInput, LibraryItem));
                tasks.Add(taskitem);
                taskitem.Start();
            }
            Task.WhenAll(tasks);

            foreach (Task<List<SearchResult>> taskItem in tasks)
            {
                if (taskItem.Result.Count != 0)
                    tempResults.AddRange(taskItem.Result);
            }
            foreach (SearchResult item in tempResults)
            {
                results.Add(item.CopyTo());
            }
            ResultesPageInt = results.Count.ToString();
        }
        public void SearchByNameCommandAction(object param)
        {
            if (SearchInput == "")
                return;
            results.Clear();

            string ConnectString = $"Data Source={m_Databasepath};";
            using (SqliteConnection sqliteconnection = new SqliteConnection(ConnectString))
            {
                sqliteconnection.Open();
                var selectCommand = sqliteconnection.CreateCommand();
                selectCommand.CommandText = "select * from HalconSearchTable where funcname like @funcname order by funcname";
                selectCommand.Parameters.AddWithValue("@funcname", "%" + SearchInput + "%");
                SqliteDataReader reader = selectCommand.ExecuteReader();
                while (reader.Read())
                {
                    SearchResult s = new SearchResult();
                    s.FuncName = reader["funcname"].ToString();
                    s.FuncCapacity = reader["FuncCapacity"].ToString();
                    s.HtmlContent = reader["funcdetails"].ToString();
                    s.PageIndex = int.Parse(reader["pdfpage"].ToString());
                    results.Add(s);
                }
            }
            //foreach (SearchResult item in tempResults)
            //{
            //    results.Add(item.CopyTo());
            //}
            ResultesPageInt = results.Count.ToString();
        }

        public CommandsBaseClass SearchByFuncCommand { get; set; }
        [Obsolete("该方法已废弃，禁止使用", true)]
        public void SearchByFuncCommandActionAbandon(object param)
        {
            if (SearchInput == "")
                return;
            results.Clear(); tempResults.Clear();

            List<Task<List<SearchResult>>> tasks = new List<Task<List<SearchResult>>>();
            foreach (List<Tuple<string, string, int>> LibraryItem in CsvLibrary)
            {
                Task<List<SearchResult>> taskitem = new Task<List<SearchResult>>(() => SearchByFuncCommandFunc(SearchInput, LibraryItem));
                tasks.Add(taskitem);
                taskitem.Start();
            }
            Task.WhenAll(tasks);

            foreach (Task<List<SearchResult>> taskItem in tasks)
            {
                if (taskItem.Result.Count != 0)
                    tempResults.AddRange(taskItem.Result);
            }
            foreach (SearchResult item in tempResults)
            {
                results.Add(item.CopyTo());
            }
            ResultesPageInt = results.Count.ToString();
        }
        public void SearchByFuncCommandAction(object param)
        {
            if (SearchInput == "")
                return;
            results.Clear();

            string ConnectString = $"Data Source={m_Databasepath};";
            using (SqliteConnection sqliteconnection = new SqliteConnection(ConnectString))
            {
                sqliteconnection.Open();
                var selectCommand = sqliteconnection.CreateCommand();
                selectCommand.CommandText = "select * from HalconSearchTable where funccapacity like @funccapacity or funckeyword like @funckeyword order by funcname";
                selectCommand.Parameters.AddWithValue("@funccapacity", "%" + SearchInput + "%");
                selectCommand.Parameters.AddWithValue("@funckeyword", "%" + SearchInput + "%");
                SqliteDataReader reader = selectCommand.ExecuteReader();
                while (reader.Read())
                {
                    SearchResult s = new SearchResult();
                    s.FuncName = reader["funcname"].ToString();
                    s.FuncCapacity = reader["FuncCapacity"].ToString();
                    s.HtmlContent = reader["funcdetails"].ToString();
                    s.PageIndex = int.Parse(reader["pdfpage"].ToString());
                    results.Add(s);
                }
            }

            ResultesPageInt = results.Count.ToString();
        }

        [Obsolete("该方法已废弃，禁止使用", true)]
        public List<SearchResult> SearchByIdCommandFunc(string param, List<Tuple<string, string, int>> tuples)
        {
            List<SearchResult> mresults = new List<SearchResult>();
            foreach (Tuple<string, string, int> item in tuples)
            {
                if (item.Item1.Trim().ToLower().Contains(SearchInput.Trim().ToLower()))
                {
                    SearchResult result = new SearchResult();
                    result.FuncName = item.Item1;
                    result.FuncCapacity = item.Item2;
                    result.PageIndex = item.Item3;
                    result.CanJump = true;
                    mresults.Add(result);
                }
            }
            return mresults;
        }
        [Obsolete("该方法已废弃，禁止使用", true)]
        public List<SearchResult> SearchByFuncCommandFunc(string param, List<Tuple<string, string, int>> tuples)
        {
            List<SearchResult> mresults = new List<SearchResult>();
            foreach (Tuple<string, string, int> item in tuples)
            {
                if (item.Item2.Trim().ToLower().Contains(SearchInput.Trim().ToLower()))
                {
                    SearchResult result = new SearchResult();
                    result.FuncName = item.Item1;
                    result.FuncCapacity = item.Item2;
                    result.PageIndex = item.Item3;
                    result.CanJump = true;
                    mresults.Add(result);
                }
            }
            return mresults;
        }


        public CommandsBaseClass ClearInputCommand { get; set; }
        public void ClearInputCommandAction(object param)
        {
            SearchInput = "";
        }


        public CommandsBaseClass explorerSearchCommand { get; set; }
        public void explorerSearchCommandAction(object param)
        {
            //获取edge浏览器,如果不存在就使用默认浏览器
            string explprerPath = "";
            if (File.Exists(@"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"))
                explprerPath = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";
            else
            {
                string name = @"HTTP\shell\open\command";
                using (RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey(name, false))
                {
                    if (registryKey != null)
                    {
                        string path = registryKey.GetValue(null) as string;
                        if (!string.IsNullOrEmpty(path))
                        {
                            // 清理路径字符串
                            if (path.Contains("\""))
                            {
                                path = path.Substring(path.IndexOf('"') + 1);
                                path = path.Substring(0, path.IndexOf('"'));
                            }
                            explprerPath = path;
                        }
                    }
                }
            }
            //创建uri，打开浏览器
            string uri = "https://www.baidu.com";
            if (SearchInput != "")
            {

                uri += @"/s?wd=Halcon函数：";
                uri += SearchInput;
            }
            System.Diagnostics.Process.Start(explprerPath, uri);
        }

        public CommandsBaseClass MouseDoubleClickCommand { get; set; }
        public void MouseDoubleClickCommandAction(object param)
        {
            //更新html
            SearchResult selectedListViewItemCollection = param as SearchResult;
            App.shareDataModel.HtmlContent = selectedListViewItemCollection.HtmlContent;
            //通知页面更改
            WeakEventBus.Default.Publish(new NavigationEvent { TargetViewName = "Detail" });
            //通知pdf同步
            WeakEventBus.Default.Publish(new SyncPdfPageEvent { PageIndex = selectedListViewItemCollection.PageIndex });
        }

        public CommandsBaseClass ListViewButtonCommand { get; set; }
        public void ListViewButtonCommandAction(object param)
        {
            //通知pdf更改
            SearchResult selectedListViewItem = param as SearchResult;
            WeakEventBus.Default.Publish(new SyncPdfPageEvent { PageIndex = selectedListViewItem.PageIndex });
            //页面更改
            WeakEventBus.Default.Publish(new NavigationEvent { TargetViewName = "PdfView" });
            //通知html更改
            App.shareDataModel.HtmlContent = selectedListViewItem.HtmlContent;
        }

        public bool CanListViewButtonCommandFunc(object param)
        {
            SearchResult selectedListViewItem = param as SearchResult;
            if (selectedListViewItem.PageIndex == 1)
                return false;
            else
                return true;
        }
        #endregion

        public SearchViewModel()
        {
            #region 把数据库文件扔到临时文件夹
            if (!Directory.Exists(System.IO.Path.GetDirectoryName(m_Databasepath)))
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(m_Databasepath));
            }
            if (!File.Exists(m_Databasepath))
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream("Halcon查询.Resource.HalconSearch.db"))
                {
                    FileStream fileStream = new FileStream(m_Databasepath, FileMode.Create);
                    stream.Position = 0;
                    stream.CopyTo(fileStream);
                    fileStream.Flush();
                }
            }
            #endregion

            #region 初始化命令
            SearchByNameCommand = new CommandsBaseClass();
            SearchByNameCommand.ExecuteAction = SearchByNameCommandAction;
            SearchByFuncCommand = new CommandsBaseClass();
            SearchByFuncCommand.ExecuteAction = SearchByFuncCommandAction;
            ClearInputCommand = new CommandsBaseClass();
            ClearInputCommand.ExecuteAction = ClearInputCommandAction;
            explorerSearchCommand = new CommandsBaseClass();
            explorerSearchCommand.ExecuteAction = explorerSearchCommandAction;
            MouseDoubleClickCommand = new CommandsBaseClass();
            MouseDoubleClickCommand.ExecuteAction = MouseDoubleClickCommandAction;
            ListViewButtonCommand = new CommandsBaseClass();
            ListViewButtonCommand.ExecuteAction = ListViewButtonCommandAction;
            ListViewButtonCommand.CanExecuteFunc = CanListViewButtonCommandFunc;
            #endregion

            #region 初始化类成员
            results = new ObservableCollection<SearchResult>();
            #endregion
        }

        #region IDispose实现
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    results.Clear();
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~SearchViewModel()
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
