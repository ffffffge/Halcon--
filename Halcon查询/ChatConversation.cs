// d:\Halcon查询\Halcon查询\ChatConversation.cs
using System;
using System.Collections.ObjectModel;
using MyCommands;

namespace Halcon查询
{
    /// <summary>
    /// 代表一个完整的对话会话（好比聊天软件里的一个"对话窗"）。
    /// 每个会话都有自己独立的消息列表和标题。
    /// </summary>
    public class ChatConversation : NotifyPropretyObject
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        private string _title = "新对话";
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ObservableCollection<ChatMessage> Messages { get; set; } = new ObservableCollection<ChatMessage>();

        /// <summary>
        /// 用于侧栏展示的简短摘要（取第一条用户消息的前20个字）
        /// </summary>
        public string Summary
        {
            get
            {
                foreach (var msg in Messages)
                {
                    if (msg.IsUser && !string.IsNullOrWhiteSpace(msg.Content))
                    {
                        string text = msg.Content.Trim();
                        return text.Length > 20 ? text.Substring(0, 20) + "..." : text;
                    }
                }
                return "新对话";
            }
        }

        /// <summary>
        /// 通知 UI 侧栏摘要需要刷新
        /// </summary>
        public void RefreshSummary()
        {
            OnPropertyChanged(nameof(Summary));
            OnPropertyChanged(nameof(Title));
        }
    }
}
