// d:\Halcon查询\Halcon查询\AIChatViewModel.cs
using Halcon查询.CommandBaseClass;
using MyCommands;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Halcon查询
{
    public class AIChatViewModel : NotifyPropretyObject
    {
        #region 会话管理
        /// <summary>
        /// 所有会话列表（侧栏展示用）
        /// </summary>
        public ObservableCollection<ChatConversation> Conversations { get; set; } = new ObservableCollection<ChatConversation>();

        private ChatConversation? _currentConversation;
        /// <summary>
        /// 当前激活的会话
        /// </summary>
        public ChatConversation? CurrentConversation
        {
            get => _currentConversation;
            set
            {
                _currentConversation = value;
                OnPropertyChanged(nameof(CurrentConversation));
                OnPropertyChanged(nameof(CurrentMessages));
                OnPropertyChanged(nameof(CurrentTitle));
            }
        }

        /// <summary>
        /// 当前会话的消息列表（供聊天区绑定）
        /// </summary>
        public ObservableCollection<ChatMessage>? CurrentMessages => CurrentConversation?.Messages;

        /// <summary>
        /// 当前会话标题（顶部栏显示）
        /// </summary>
        public string CurrentTitle => CurrentConversation?.Title ?? "AI 智能副驾";
        #endregion

        #region 输入与状态
        private string _inputText = string.Empty;
        public string InputText
        {
            get => _inputText;
            set
            {
                _inputText = value;
                OnPropertyChanged(nameof(InputText));
                SendMessageCommand.RaiseCanExecuteChanged();
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
                SendMessageCommand.RaiseCanExecuteChanged();
            }
        }

        private bool _isSidebarVisible = true;
        public bool IsSidebarVisible
        {
            get => _isSidebarVisible;
            set
            {
                _isSidebarVisible = value;
                OnPropertyChanged(nameof(IsSidebarVisible));
                OnPropertyChanged(nameof(SidebarWidth));
            }
        }

        /// <summary>
        /// 侧栏宽度（折叠时为0）
        /// </summary>
        public GridLength SidebarWidth => IsSidebarVisible ? new GridLength(220) : new GridLength(0);
        #endregion

        #region 命令
        public AsyncRelayCommand SendMessageCommand { get; private set; }
        public CommandsBaseClass NewConversationCommand { get; private set; }
        public CommandsBaseClass SwitchConversationCommand { get; private set; }
        public CommandsBaseClass DeleteConversationCommand { get; private set; }
        public CommandsBaseClass ToggleSidebarCommand { get; private set; }
        public CommandsBaseClass OpenSettingsCommand { get; private set; }
        #endregion

        #region 设置
        public AIChatSettings Settings { get; private set; }
        #endregion

        public AIChatViewModel()
        {
            // 加载用户配置
            Settings = AIChatSettings.Load();

            // 初始化命令
            SendMessageCommand = new AsyncRelayCommand(OnSendMessageAsync, CanSendMessage);

            NewConversationCommand = new CommandsBaseClass();
            NewConversationCommand.ExecuteAction = _ => CreateNewConversation();

            SwitchConversationCommand = new CommandsBaseClass();
            SwitchConversationCommand.ExecuteAction = obj =>
            {
                if (obj is ChatConversation conv)
                    CurrentConversation = conv;
            };

            DeleteConversationCommand = new CommandsBaseClass();
            DeleteConversationCommand.ExecuteAction = obj =>
            {
                if (obj is ChatConversation conv)
                {
                    Conversations.Remove(conv);
                    if (CurrentConversation == conv)
                    {
                        CurrentConversation = Conversations.FirstOrDefault();
                        if (CurrentConversation == null)
                            CreateNewConversation();
                    }
                }
            };

            ToggleSidebarCommand = new CommandsBaseClass();
            ToggleSidebarCommand.ExecuteAction = _ => IsSidebarVisible = !IsSidebarVisible;

            OpenSettingsCommand = new CommandsBaseClass();
            OpenSettingsCommand.ExecuteAction = _ => OpenSettingsWindow();

            // 创建第一个默认会话
            CreateNewConversation();
        }

        private void CreateNewConversation()
        {
            var conv = new ChatConversation();
            conv.Messages.Add(new ChatMessage(
                "您好！我是 Halcon 智能副驾 🤖\n请在下方输入您的问题，例如：\n• find_shape_model 怎么用？\n• 帮我写一段模板匹配的代码",
                false));
            Conversations.Insert(0, conv);
            CurrentConversation = conv;
        }

        private bool CanSendMessage()
        {
            return !string.IsNullOrWhiteSpace(InputText) && !IsBusy;
        }

        private async Task OnSendMessageAsync()
        {
            if (CurrentConversation == null) return;

            string userMessage = InputText.Trim();
            InputText = string.Empty;

            // 添加用户消息
            CurrentConversation.Messages.Add(new ChatMessage(userMessage, true));

            // 如果是第一条用户消息，更新会话标题
            bool isFirstUserMsg = CurrentConversation.Messages.Count(m => m.IsUser) == 1;
            if (isFirstUserMsg)
            {
                CurrentConversation.Title = userMessage.Length > 15
                    ? userMessage.Substring(0, 15) + "..."
                    : userMessage;
                CurrentConversation.RefreshSummary();
            }

            IsBusy = true;

            var aiDraftMessage = new ChatMessage("", false, "Agent 深度解析中...");
            CurrentConversation.Messages.Add(aiDraftMessage);

            try
            {
                // 检查是否配置了 API Key（Ollama 除外，本地不需要 Key）
                if (string.IsNullOrWhiteSpace(Settings.ApiKey) && Settings.ApiProvider != ApiProviderType.Ollama)
                {
                    aiDraftMessage.StatusText = "";
                    aiDraftMessage.Content = "⚠️ 尚未配置 API Key。\n请点击左下角的 ⚙ 设置按钮，配置您的大模型 API 信息后即可开始对话。";
                }
                else
                {
                    // 构建对话历史（发送给大模型）
                    var apiMessages = MultiAgentChatService.BuildMessages(
                        CurrentConversation.Messages,
                        "你是 Halcon 机器视觉领域的智能助手。用户会问你关于 Halcon 算子的使用方法、参数说明、示例代码等问题。请用中文回答，必要时提供 HDevelop 代码示例。");

                    // 移除最后一条（空的 AI 草稿消息）以免发给 API
                    if (apiMessages.Count > 0 && apiMessages[apiMessages.Count - 1]["role"] == "assistant")
                        apiMessages.RemoveAt(apiMessages.Count - 1);

                    var service = new MultiAgentChatService();
                    string result = await service.SendMessageAsync(
                        Settings,
                        apiMessages,
                        onChunkReceived: chunk =>
                        {
                            // 在 UI 线程上更新消息内容（流式打字机效果）
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                aiDraftMessage.Content += chunk;
                            });
                        },
                        onStatusUpdate: status =>
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                aiDraftMessage.StatusText = status;
                            });
                        });

                    // 如果流式回调没有填充内容（某些 API 可能不支持 stream），用完整结果
                    if (string.IsNullOrEmpty(aiDraftMessage.Content))
                    {
                        aiDraftMessage.Content = result;
                    }

                    aiDraftMessage.StatusText = "";
                }
            }
            catch (Exception ex)
            {
                aiDraftMessage.StatusText = "";
                aiDraftMessage.Content = $"❌ 抱歉，出了点问题：{ex.Message}\n请检查网络连接或 API 配置。";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void OpenSettingsWindow()
        {
            var settingsWindow = new AISettingsView(Settings);
            settingsWindow.Owner = System.Windows.Application.Current.MainWindow;
            settingsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if (settingsWindow.ShowDialog() == true)
            {
                // 用户点了保存，重新加载配置
                Settings = AIChatSettings.Load();
                OnPropertyChanged(nameof(Settings));
            }
        }
    }
}
