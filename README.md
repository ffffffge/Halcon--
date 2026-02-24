<div align="center">
  <h1>📚 Halcon 函数查询工具</h1>
  <p>
    <strong>一个集成了现代 UI 和多模型 AI 智能助手的 Halcon 算子速查利器</strong>
  </p>
</div>

---

## 📖 项目简介

**Halcon 函数查询工具** 是一款基于 WPF (.NET 8) 开发的现代化桌面辅助应用程序，专为机器视觉算法工程师打造。它旨在解决 Halcon 庞大的算子库带来的查询难、记忆难问题，无论是日常编码还是排查问题，都能极大提高工作效率。

本项目不仅提供了**毫秒级本地数据库检索**和**内置算子手册 PDF 联动**，还创新性地引入了 **AI 智能副驾**面板。你可以在查阅算子的同时，随时呼出 AI 助手为你提供代码示例、算法思路或参数解释。

---

## ✨ 核心特性

- 🔍 **双剑合璧的极速搜索**
  - **按名称搜索**：支持算子名称的模糊或精确查找。
  - **按功能搜索**：忘记了名字？只需输入中文功能描述（如"标定"或"模板匹配"），立刻找到目标算子。
- 📑 **PDF 智能联动**
  - 深度集成 Pdfium 引擎，搜索结果可**一键跳转**到官方 PDF 手册中对应的详细讲解页，告别手动翻书。
- 🤖 **AI 智能副驾 (Multi-Agent Chat)**
  - 内置流畅的聊天面板，支持多会话管理与历史记录保存。
  - **全面兼容**各大主流 AI 模型：支持无缝切换 OpenAI、DeepSeek、阿里云通义千问、Google Gemini，以及本地私有化部署的 Ollama。
  - 采用流式（SSE）输出，给你沉浸式的对话体验。
- 🎨 **现代化 WPF UI 设计**
  - 采用 MVVM 架构模式，代码结构清晰。
  - 精致的交互动画与渐变色深色系侧边栏相得益彰。
  - 支持拖拽的分屏设计，左侧看文档，右侧问 AI。

---

## 📸 界面预览

*界面包含顶部的全局导航栏、左侧的搜索/PDF/文件浏览主功能区，以及右侧可自由折叠的 AI 智能助手面板。*

---

## 🚀 快速上手

### 运行环境
- Windows 系统 (Windows 10/11)
- [.NET 8.0 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)

### 编译与启动
1. 克隆本项目到本地：
   ```bash
   git clone https://github.com/ffffffge/Halcon--.git
   ```
2. 使用 **Visual Studio 2022** 打开 `Halcon查询.sln`。
3. 还原 NuGet 包（主要依赖 `Microsoft.Data.Sqlite`, `Microsoft.Web.WebView2`, `Pdfium.Net.SDK`）。
4. 编译并运行项目。

> **注意**：项目首次启动时，会自动将内置的 SQLite 数据库（`HalconSearch.db`）和 PDF 手册（`Halcon算子手册-1-672.pdf`）释放到本地 `文档` 文件夹下以供极速加载。

---

## ⚙️ AI 助手配置说明

想要使用右侧的 AI 智能副驾功能，你需要进行简单的 API 配置：

1. 点击 AI 聊天面板右下角的 **"⚙ 设置"** 按钮。
2. 在弹出的窗口中选择你拥有的服务商（如 `DeepSeek` 或是本机运行的 `Ollama`）。
3. 填入对应的 **API Key**（除了 Ollama 外通常需要）。
4. 系统会自动为你填入标准的接口地址（Endpoint）和推荐的模型名称（如 `deepseek-chat` 或 `gpt-4o-mini`），你也可以根据需要修改。
5. 点击保存后即可享受 AI 的实时帮助！

---

## 🏗️ 技术架构

- **框架**: C# / WPF / .NET 8.0
- **架构模式**: 纯正 MVVM (Model-View-ViewModel)
- **数据库**: SQLite (通过 `Microsoft.Data.Sqlite` 访问)
- **PDF 渲染组件**: `Patagames.Pdf.Net.Controls.Wpf`
- **AI 对话处理**: 统一封装基于 `HttpClient` 的 OpenAI-compatible 流式请求服务 (`MultiAgentChatService`)
- **其他**: 自定义命令绑定、全局弱引用事件总线 (`WeakEventBus`)

---

## 🤝 贡献与反馈

欢迎提交 Issue 和 Pull Request，一起让这个工具变得更好用！如果你觉得这个项目对你有帮助，不妨点个 ⭐ **Star** 支持一下~