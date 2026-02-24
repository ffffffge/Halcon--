// d:\Halcon查询\Halcon查询\AISettingsView.xaml.cs
using System.Windows;

namespace Halcon查询
{
    public partial class AISettingsView : Window
    {
        private readonly AIChatSettings _settings;

        public AISettingsView(AIChatSettings settings)
        {
            InitializeComponent();
            _settings = settings;

            // 加载现有配置到 UI
            ProviderComboBox.SelectedIndex = (int)_settings.ApiProvider;
            ApiKeyBox.Password = _settings.ApiKey;
            EndpointBox.Text = _settings.ApiEndpoint;
            ModelNameBox.Text = _settings.ModelName;
        }

        private void ProviderComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;

            // 根据选择自动填充默认值
            int idx = ProviderComboBox.SelectedIndex;
            switch (idx)
            {
                case 0: // OpenAI
                    EndpointBox.Text = "https://api.openai.com/v1/chat/completions";
                    ModelNameBox.Text = "gpt-4o-mini";
                    break;
                case 1: // DeepSeek
                    EndpointBox.Text = "https://api.deepseek.com/v1/chat/completions";
                    ModelNameBox.Text = "deepseek-chat";
                    break;
                case 2: // 千问
                    EndpointBox.Text = "https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions";
                    ModelNameBox.Text = "qwen-plus";
                    break;
                case 3: // Gemini
                    EndpointBox.Text = "https://generativelanguage.googleapis.com/v1beta/openai/chat/completions";
                    ModelNameBox.Text = "gemini-2.0-flash";
                    break;
                case 4: // Ollama
                    EndpointBox.Text = "http://localhost:11434/v1/chat/completions";
                    ModelNameBox.Text = "llama3";
                    break;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _settings.ApiProvider = (ApiProviderType)ProviderComboBox.SelectedIndex;
            _settings.ApiKey = ApiKeyBox.Password;
            _settings.ApiEndpoint = EndpointBox.Text;
            _settings.ModelName = ModelNameBox.Text;
            _settings.Save();

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
