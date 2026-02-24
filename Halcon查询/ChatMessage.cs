using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyCommands;

namespace Halcon查询
{
    public class ChatMessage : NotifyPropretyObject
    {
        private string _content = string.Empty;
        public string Content
        {
            get => _content;
            set
            {
                _content = value;
                OnPropertyChanged(nameof(Content));
            }
        }

        private bool _isUser;
        public bool IsUser
        {
            get => _isUser;
            set
            {
                _isUser = value;
                OnPropertyChanged(nameof(IsUser));
            }
        }

        private string _statusText = string.Empty;
        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                OnPropertyChanged(nameof(StatusText));
            }
        }

        public DateTime Timestamp { get; set; }

        public ChatMessage(string content, bool isUser, string statusText = "")
        {
            Content = content;
            IsUser = isUser;
            StatusText = statusText;
            Timestamp = DateTime.Now;
        }
    }
}
