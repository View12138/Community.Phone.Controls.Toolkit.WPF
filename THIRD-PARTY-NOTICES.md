# 第三方声明

## Windows Phone Toolkit（MS-PL）

本项目是归档的 Windows Phone Toolkit 的兼容移植：

- 原始仓库：Microsoft Windows Phone Toolkit（已归档）
- 原始许可证：Microsoft Public License (MS-PL)
- 原始版权：Microsoft Corporation

对于复制或改编的代码，移植版保留原始源文件头。完整 MS-PL 文本见 [LICENSE](LICENSE)。

## 运行时依赖

`Community.Phone.Controls.Toolkit` NuGet 包**没有 NuGet 包依赖**。它只使用随 .NET 提供的 WPF/.NET 框架程序集（`PresentationCore`、`PresentationFramework`、`WindowsBase` 及基础类库）。它不依赖：

- Windows Phone SDK
- Silverlight
- `Microsoft.Phone` / `Microsoft.Phone.Controls.Toolkit`
- `System.Windows.Interactivity`

## 仅构建期依赖

仓库内没有测试工程。构建只用 .NET SDK 自带的 MSBuild 与 WPF 目标，不存在需要随包分发的构建期 NuGet 包。

## 字体与图标

主题引用 Windows 提供的 Metro 时代系统字体（`Segoe UI`、`Segoe UI Light`）。包中不重新分发字体文件或第三方图标资源。
