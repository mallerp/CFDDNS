# CFDDNS - 智能 Cloudflare DDNS 客户端

![C#](https://img.shields.io/badge/C%23-11-blueviolet) ![.NET](https://img.shields.io/badge/.NET-6.0-blue) ![Windows Forms](https://img.shields.io/badge/WinForms-orange) ![License](https://img.shields.io/badge/license-MIT-green)

CFDDNS 是一款功能强大、界面友好的 Windows 桌面应用程序，专为需要动态更新 Cloudflare DNS 记录的用户设计。它不需要手动执行的cmd脚本操作，而是一个拥有图形化界面、支持多账户、多域名管理的智能客户端。

## ✨ 功能亮点

*   **精美的图形界面**: 提供直观、现代化的用户界面，所有操作一目了然。
*   **多域名支持**: 可同时管理和更新多个域名（A 或 AAAA 记录）。
*   **多账户支持**: 支持为每个域名配置独立的 Cloudflare 账户凭据，轻松管理不同账户下的资产。
*   **智能更新**:
    *   启动时自动检测 IP 变化，并标记域名状态（"IP一致"或"待更新"）。
    *   仅在公网 IP 发生变化时才执行更新，避免不必要的 API 请求。
    *   实时显示域名在 Cloudflare 上的当前记录值。
*   **自动服务**: 可作为后台服务运行，按自定义的时间间隔（分钟）自动检查并更新 IP。
*   **状态透明**:
    *   主界面实时显示当前公网 IPv4 和 IPv6 地址。
    *   详细的操作日志，记录每一次 IP 获取、查询和更新操作，便于追踪和排错。
*   **持久化配置**:
    *   所有配置（包括全局设置和域名列表）均保存在本地 `config.json` 文件中，方便备份和迁移。
    *   程序的上次更新时间会被记录，即使重启程序也能看到。

## 🛠️ 技术架构

*   **开发语言**: C# 11
*   **框架**: .NET 6.0
*   **用户界面**: Windows Forms (WinForms)
*   **核心依赖**:
    *   `System.Text.Json`: 用于高效地处理和读写 JSON 配置文件。
    *   `HttpClient`: 用于与外部 IP 查询服务和 Cloudflare 官方 API 进行异步通信。
*   **项目结构**:
    *   `MainForm.cs`: 主窗口，承载所有界面交互和核心业务逻辑。
    *   `CloudflareClient.cs`: 封装了所有与 Cloudflare API v4 相关的操作（获取记录、更新记录）。
    *   `IpService.cs`: 负责从公共 API 获取本机的公网 IPv4 和 IPv6 地址。
    *   `Config.cs` & `ConfigManager.cs`: 定义了程序的配置数据结构，并管理配置文件的读写。
    *   `Program.cs`: 应用程序入口点。

## 📖 使用手册

1.  **首次运行**:
    *   直接运行 `CFDDNS.exe`。程序会自动在同目录下创建 `config.json` 文件。
2.  **全局设置**:
    *   点击主界面顶部的 **[全局设置]** 按钮。
    *   填入您的 Cloudflare **注册邮箱** 和 **Global API Key**。这是程序默认使用的凭据。
    *   设置一个您希望的**更新间隔**（以分钟为单位）。
    *   点击 **[保存]**。
3.  **添加域名**:
    *   点击 **[添加域名]** 按钮。
    *   **子域名**: 填入您要更新的完整域名，例如 `home.example.com`。
    *   **类型**: 选择 `A` (IPv4) 或 `AAAA` (IPv6)。
    *   **Zone ID**: 填入该域名所在根域名（如 `example.com`）的区域ID。
    *   **Record ID**: 填入该子域名DNS记录的ID。
    *   **(可选) 独立凭据**: 如果这个域名需要使用**不同于全局设置**的 Cloudflare 账户，请在此处填写对应的**邮箱**和**API Key**。如果留空，则使用全局设置。
    *   点击 **[保存]**。
4.  **管理域名**:
    *   **编辑**: 在列表中选中一个域名，然后点击 **[编辑域名]**。
    *   **删除**: 在列表中选中一个域名，然后点击 **[删除域名]**。
    *   **启用/禁用**: 直接点击域名左侧的复选框，可决定该域名是否参与自动更新。
5.  **开始 DDNS**:
    *   **手动更新**: 点击 **[立即更新]**，程序会立即为所有已启用的域名执行一次检查和更新流程。
    *   **自动更新**: 点击 **[启动服务]**，程序将根据您设定的时间间隔，在后台自动执行更新。按钮会变为 **[停止服务]**。

## 💻 开发手册

### 环境要求

*   .NET 6.0 SDK 或更高版本
*   Visual Studio 2022 或其他支持 .NET 6.0 的 IDE

### 编译项目

可以直接在项目根目录运行以下命令：
```shell
dotnet build
```

### 发布项目

项目内置了一个 `publish.bat` 脚本，用于一键打包发布。
在 PowerShell或者cmd 中运行：
```powershell
./publish.bat
```
脚本会自动创建一个 `publish` 文件夹，并将所有必要文件（包括 `README.md` 和 `images` 目录）复制进去，形成一个可直接分发的绿色版本。

## ❤️ 支持项目

如果您觉得这个项目对您有帮助，不妨请我喝杯咖啡，这将是我持续维护和改进的巨大动力！

| 支付宝 | 微信支付 |
| :---: | :---: |
| <img src="images/alipay_qr.jpg" alt="Alipay" width="200"> | <img src="images/wechat_qr.jpg" alt="WeChat Pay" width="200"> | 