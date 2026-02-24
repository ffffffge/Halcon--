// d:\Halcon查询\Halcon查询\AIChatSettings.cs
using System;
using System.IO;
using System.Text.Json;
using MyCommands;

namespace Halcon查询
{
    /// <summary>
    /// AI 大模型 API 供应商枚举
    /// </summary>
    public enum ApiProviderType
    {
        OpenAI,
        DeepSeek,
        千问,
        Gemini,
        Ollama
    }

    /// <summary>
    /// 用户自行配置的 AI API 连接信息。
    /// 好比你家的 WiFi 设置界面：需要填"运营商"、"密码"、"地址"才能上网。
    /// 这些配置会被保存到本地 JSON 文件中，下次启动时自动加载。
    /// </summary>
    public class AIChatSettings : NotifyPropretyObject
    {
        private static readonly string SettingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "HalconSearch",
            "ai_settings.json");

        private ApiProviderType _apiProvider = ApiProviderType.DeepSeek;
        public ApiProviderType ApiProvider
        {
            get => _apiProvider;
            set
            {
                _apiProvider = value;
                OnPropertyChanged(nameof(ApiProvider));
                // 根据供应商自动填充默认 Endpoint
                AutoFillEndpoint();
            }
        }

        private string _apiKey = string.Empty;
        public string ApiKey
        {
            get => _apiKey;
            set
            {
                _apiKey = value;
                OnPropertyChanged(nameof(ApiKey));
            }
        }

        private string _apiEndpoint = "https://api.deepseek.com/v1/chat/completions";
        public string ApiEndpoint
        {
            get => _apiEndpoint;
            set
            {
                _apiEndpoint = value;
                OnPropertyChanged(nameof(ApiEndpoint));
            }
        }

        private string _modelName = "deepseek-chat";
        public string ModelName
        {
            get => _modelName;
            set
            {
                _modelName = value;
                OnPropertyChanged(nameof(ModelName));
            }
        }

        /// <summary>
        /// 根据选择的供应商，自动填充默认 Endpoint 和 ModelName
        /// </summary>
        private void AutoFillEndpoint()
        {
            switch (_apiProvider)
            {
                case ApiProviderType.OpenAI:
                    ApiEndpoint = "https://api.openai.com/v1/chat/completions";
                    ModelName = "gpt-4o-mini";
                    break;
                case ApiProviderType.DeepSeek:
                    ApiEndpoint = "https://api.deepseek.com/v1/chat/completions";
                    ModelName = "deepseek-chat";
                    break;
                case ApiProviderType.千问:
                    ApiEndpoint = "https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions";
                    ModelName = "qwen-plus";
                    break;
                case ApiProviderType.Gemini:
                    ApiEndpoint = "https://generativelanguage.googleapis.com/v1beta/openai/chat/completions";
                    ModelName = "gemini-2.0-flash";
                    break;
                case ApiProviderType.Ollama:
                    ApiEndpoint = "http://localhost:11434/v1/chat/completions";
                    ModelName = "llama3";
                    break;
            }
        }

        /// <summary>
        /// 保存配置到本地 JSON 文件
        /// </summary>
        public void Save()
        {
            try
            {
                string? dir = Path.GetDirectoryName(SettingsFilePath);
                if (dir != null && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var data = new
                {
                    ApiProvider = _apiProvider.ToString(),
                    ApiKey = _apiKey,
                    ApiEndpoint = _apiEndpoint,
                    ModelName = _modelName
                };

                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AI设置] 保存失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 从本地 JSON 文件加载配置
        /// </summary>
        public static AIChatSettings Load()
        {
            var settings = new AIChatSettings();
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("ApiProvider", out var providerEl))
                    {
                        if (Enum.TryParse<ApiProviderType>(providerEl.GetString(), out var provider))
                            settings._apiProvider = provider;
                    }
                    if (root.TryGetProperty("ApiKey", out var keyEl))
                        settings._apiKey = keyEl.GetString() ?? string.Empty;
                    if (root.TryGetProperty("ApiEndpoint", out var endpointEl))
                        settings._apiEndpoint = endpointEl.GetString() ?? string.Empty;
                    if (root.TryGetProperty("ModelName", out var modelEl))
                        settings._modelName = modelEl.GetString() ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AI设置] 加载失败，使用默认值: {ex.Message}");
            }
            return settings;
        }
    }
}
