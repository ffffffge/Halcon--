// d:\Halcon查询\Halcon查询\MultiAgentChatService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Halcon查询
{
    /// <summary>
    /// 真实的大模型 API 调用服务。
    /// 好比一个专业的翻译官：你把中文问题交给他，他帮你通过电话（HTTP）
    /// 连线远端的 AI 专家，然后把专家的回答一句一句翻译回来给你看。
    /// 
    /// 所有支持的供应商（OpenAI / DeepSeek / 千问 / Gemini / Ollama）
    /// 都走统一的 OpenAI-compatible chat/completions 协议。
    /// </summary>
    public class MultiAgentChatService
    {
        private static readonly HttpClient _httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(120)
        };

        /// <summary>
        /// 向大模型发送对话请求，并逐块（流式）返回 AI 的回复文本。
        /// </summary>
        /// <param name="settings">用户配置的 API 信息</param>
        /// <param name="messages">对话历史</param>
        /// <param name="onChunkReceived">每收到一小段文字时的回调</param>
        /// <param name="onStatusUpdate">Agent 状态更新回调</param>
        /// <param name="cancellationToken">取消令牌</param>
        public async Task<string> SendMessageAsync(
            AIChatSettings settings,
            List<Dictionary<string, string>> messages,
            Action<string>? onChunkReceived = null,
            Action<string>? onStatusUpdate = null,
            CancellationToken cancellationToken = default)
        {
            onStatusUpdate?.Invoke($"正在连接 {settings.ApiProvider}...");

            // 构建请求体（OpenAI 兼容格式）
            var requestBody = new
            {
                model = settings.ModelName,
                messages = messages,
                stream = true,
                max_tokens = 4096
            };

            string jsonBody = JsonSerializer.Serialize(requestBody);

            var request = new HttpRequestMessage(HttpMethod.Post, settings.ApiEndpoint)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };

            // 设置 Authorization Header
            // Gemini 使用 query parameter 形式的 key，其他用 Bearer token
            if (settings.ApiProvider == ApiProviderType.Gemini)
            {
                // Gemini OpenAI 兼容接口也支持 Bearer 形式
                request.Headers.Add("Authorization", $"Bearer {settings.ApiKey}");
            }
            else if (settings.ApiProvider != ApiProviderType.Ollama)
            {
                request.Headers.Add("Authorization", $"Bearer {settings.ApiKey}");
            }

            onStatusUpdate?.Invoke($"正在等待 {settings.ModelName} 响应...");

            try
            {
                var response = await _httpClient.SendAsync(request,
                    HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    string errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    return $"❌ API 返回错误 ({(int)response.StatusCode}):\n{TruncateError(errorBody)}";
                }

                onStatusUpdate?.Invoke("正在接收回复...");

                // 流式读取 SSE (Server-Sent Events)
                var fullResponse = new StringBuilder();
                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var reader = new StreamReader(stream);

                while (!reader.EndOfStream)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string? line = await reader.ReadLineAsync(cancellationToken);
                    if (string.IsNullOrEmpty(line)) continue;

                    // SSE 格式: "data: {...}" 
                    if (!line.StartsWith("data: ")) continue;

                    string data = line.Substring(6).Trim();

                    // 流结束标记
                    if (data == "[DONE]") break;

                    try
                    {
                        using var doc = JsonDocument.Parse(data);
                        var root = doc.RootElement;

                        if (root.TryGetProperty("choices", out var choices) &&
                            choices.GetArrayLength() > 0)
                        {
                            var firstChoice = choices[0];
                            if (firstChoice.TryGetProperty("delta", out var delta) &&
                                delta.TryGetProperty("content", out var content))
                            {
                                string? chunk = content.GetString();
                                if (!string.IsNullOrEmpty(chunk))
                                {
                                    fullResponse.Append(chunk);
                                    onChunkReceived?.Invoke(chunk);
                                }
                            }
                        }
                    }
                    catch (JsonException)
                    {
                        // 某些行可能不是合法 JSON，跳过
                        continue;
                    }
                }

                return fullResponse.ToString();
            }
            catch (TaskCanceledException)
            {
                return "⏳ 请求超时，请检查网络连接或尝试更换模型。";
            }
            catch (HttpRequestException ex)
            {
                return $"🌐 网络错误：{ex.Message}\n请检查 API Endpoint 配置是否正确。";
            }
            catch (Exception ex)
            {
                return $"❌ 未知错误：{ex.Message}";
            }
        }

        /// <summary>
        /// 将聊天历史转成 OpenAI messages 格式
        /// </summary>
        public static List<Dictionary<string, string>> BuildMessages(
            IEnumerable<ChatMessage> chatHistory, string systemPrompt = "")
        {
            var messages = new List<Dictionary<string, string>>();

            if (!string.IsNullOrWhiteSpace(systemPrompt))
            {
                messages.Add(new Dictionary<string, string>
                {
                    ["role"] = "system",
                    ["content"] = systemPrompt
                });
            }

            foreach (var msg in chatHistory)
            {
                // 跳过空消息和状态消息
                if (string.IsNullOrWhiteSpace(msg.Content)) continue;

                messages.Add(new Dictionary<string, string>
                {
                    ["role"] = msg.IsUser ? "user" : "assistant",
                    ["content"] = msg.Content
                });
            }

            return messages;
        }

        private static string TruncateError(string error)
        {
            if (error.Length > 500)
                return error.Substring(0, 500) + "...";
            return error;
        }
    }
}
