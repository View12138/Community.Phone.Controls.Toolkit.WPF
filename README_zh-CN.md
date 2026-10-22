# Community.Phone.Controls.Toolkit.WPF

这是归档的 **Windows Phone Toolkit** 的 WPF 兼容移植版，以一个 NuGet 包交付。该移植在 WPF 允许的范围内保留原有的 `Microsoft.Phone.Controls` / `Microsoft.Phone.Shell` 命名空间和 API 形态，并使用 WPF 原生能力实现真实的 3D 投影、导航转场和 Metro 风格控件。

[English README](README.md)

> **状态：preview（0.1.0-preview.1）。** 已实现的功能通过构建、自动化测试、可运行示例和全新 NuGet 消费工程验证。未实现模块在 [known-limitations.md](docs/zh-CN/known-limitations.md) 中明确列出，没有用“看似可用”的空实现冒充兼容。

## 已实现功能

- **投影**：`Projection`、`PlaneProjection`、`Matrix3DProjection`、`ProjectionHost`、`ProjectionService`。使用真实的 `Viewport3D` + `Viewport2DVisual3D` 渲染，支持命中测试、键盘焦点和动画。
- **导航**：`PhoneApplicationFrame`、`PhoneApplicationPage`、`NavigationContext`、页面状态、方向、返回键和页面生命周期分发。
- **转场**：`TransitionFrame`、`ITransition`、`NavigationInTransition` / `NavigationOutTransition`，以及 Roll、Rotate、Slide、Swivel、Turnstile、TurnstileFeather 系列（原 19 个 storyboard）。
- **效果**：`TiltEffect`、`TurnstileFeatherEffect`，基于真实投影管线。
- **控件**：`WrapPanel`、`ToggleSwitch`、`Rating`、`PhoneTextBox`、`ExpanderView`、`ListPicker`、`HubTile`、`ContextMenu`、`CustomMessageBox`、`DatePicker`、`TimePicker`、`RecurringDaysPicker`、`PerformanceProgressBar`，以及相关转换器和辅助类。
- **Shell**：`Microsoft.Phone.Shell.ApplicationBar` 移植了原版 API（`IApplicationBar`、图标按钮、菜单项、`Mode`、`StateChanged`、4/50 上限与 item 校验），由 frame 的 `ApplicationBarPresenter` 绘制；`SystemTray` 仍是 API/状态模型。`ListPicker` 完整模式页面的「完成/取消」使用真实 `ApplicationBar`。
- **本地化**：50 个真实文化加中性资源，由原始 designer/资源生成。
- **主题**：Metro 调色板、字体、文本样式、Frame 模板和控件模板位于 `Themes/Generic.xaml`。主题还为常用 WPF 控件提供隐式 WP 样式（`Button`、`TextBox`、`ListBox`/`ListBoxItem`、`ComboBox`/`ComboBoxItem`、`CheckBox`、`RadioButton`、`ScrollBar`、`ContextMenu`/`MenuItem`、`ProgressBar` 和 `ToolTip`）。

## 环境要求

- Windows
- .NET SDK 10.0 或更高版本
- 目标框架为 `net10.0-windows`（或兼容的 Windows TFM）的 WPF 工程

## 安装

```powershell
dotnet add package Community.Phone.Controls.Toolkit.WPF
```

包 ID 为 `Community.Phone.Controls.Toolkit.WPF`，程序集名称相同，根命名空间为 `Microsoft.Phone.Controls`。

## 快速开始

合并 toolkit 主题，使应用 XAML 可以使用 Metro 命名资源：

```xml
<Application x:Class="MyApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             StartupUri="MainWindow.xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary
                    Source="/Community.Phone.Controls.Toolkit.WPF;component/Themes/Generic.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

用 `TransitionFrame` 承载页面，并让页面继承 `PhoneApplicationPage`：

```xml
<Window x:Class="MyApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:phone="clr-namespace:Microsoft.Phone.Controls;assembly=Community.Phone.Controls.Toolkit.WPF">
    <phone:TransitionFrame x:Name="Frame" />
</Window>
```

```csharp
public MainWindow()
{
    InitializeComponent();
    Frame.Navigate(new HomePage());
}
```

## 投影

WPF 没有 `UIElement.Projection` 属性。请使用 `ProjectionHost`，其中内容仍然是可交互的实时视觉树：

```xml
<phone:ProjectionHost Width="320" Height="200">
    <Border Background="#FF1F1F1F">
        <TextBlock Text="Hello, Metro" Foreground="White" />
    </Border>
    <phone:ProjectionHost.Projection>
        <phone:PlaneProjection RotationY="-35" RotationX="8" />
    </phone:ProjectionHost.Projection>
</phone:ProjectionHost>
```

`PlaneProjection` 继承自 `Animatable`，可以直接创建动画：

```csharp
projection.BeginAnimation(
    PlaneProjection.RotationYProperty,
    new DoubleAnimation(-80, 0, TimeSpan.FromMilliseconds(350))
    {
        EasingFunction = new ExponentialEase
        {
            EasingMode = EasingMode.EaseOut,
            Exponent = 6
        }
    });
```

## 导航参数与状态

```csharp
Frame.Navigate(
    new DetailPage(),
    new Dictionary<string, string> { { "id", "42" } });
```

```csharp
protected override void OnNavigatedTo(NavigationEventArgs e)
{
    base.OnNavigatedTo(e);
    string id = NavigationContext.QueryString["id"];
}
```

`PhoneApplicationPage.State` 由 Frame 按页面实例保存；返回到同一个页面实例时状态仍然存在。

## 转场

```xml
<phone:TransitionService.NavigationInTransition>
    <phone:NavigationInTransition>
        <phone:NavigationInTransition.Forward>
            <phone:TurnstileTransition Mode="ForwardIn" />
        </phone:NavigationInTransition.Forward>
    </phone:NavigationInTransition>
</phone:TransitionService.NavigationInTransition>
```

需要自动进入/退出转场时使用 `TransitionFrame`，而不是普通 `PhoneApplicationFrame`。每个转场播放的都是原版工具箱的 storyboard（`Transitions/Storyboards/*.xaml`，以资源形式内嵌），角度、时长和缓动与原版一致。

## 构建、测试与打包

```powershell
dotnet restore
dotnet build Community.Phone.Controls.Toolkit.slnx -c Release
dotnet test Community.Phone.Controls.Toolkit.slnx -c Release
dotnet pack src\Community.Phone.Controls.Toolkit.WPF\Community.Phone.Controls.Toolkit.WPF.csproj -c Release
```

包输出到 `artifacts/packages`。

### 包消费验证

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File build\verify-package.ps1
```

该脚本会打包库，使用隔离的本地源和缓存还原一个全新 WPF 消费工程，构建（零警告）并运行，确认包内主题资源和控件可以加载。

## 验证状态

- Release 构建：0 errors（2026-10-22 重新验证）；仅有的警告来自未跟踪的进行中 `LongListSelector`/`ViewportControl`/`WindowHelper` 文件。
- 自动化测试：Release 与 Debug 下均 103 passed，0 failed（约 30 秒；覆盖投影、控件、转换器、效果、导航/转场和 Shell API）。交互测试覆盖投影动画时的 2D→3D 切换、忠实 XAML 的透视几何（999 DIP 深度、identity 1:1 映射、`RotationY` 方向）、Turnstile 进入/退出 storyboard、feather 转场、TiltEffect 只作用于自身控件、ToggleSwitch 滑块动画、Rating 指针输入、PhoneTextBox 焦点与编辑、`ApplicationBar` chrome（预留高度、槽位、菜单、`StateChanged`）以及 `ListPicker` 完整模式页面的应用栏。
- 包消费：全新工程从隔离本地源/缓存还原、零警告构建并运行成功。
- 示例：启动成功；UI 自动化逐个打开全部示例（Controls、Projection、Transitions → 目标页 → 返回、Feathering、Menus）并每次返回示例索引，未产生启动错误。
- 逐模块状态与证据见 [docs/zh-CN/porting-status.md](docs/zh-CN/porting-status.md)，最近一次交接见 [docs/zh-CN/handoff.md](docs/zh-CN/handoff.md)。

## 运行示例

```powershell
dotnet run --project src\Community.Phone.Controls.Toolkit.WPF.Sample
```

示例结构与原版 `PhoneToolkitSample8` 对齐：`MainPage` 列出示例，每个示例独立成页 —— `ControlsPage`（带 WP 样式的 WPF 基础控件 + 全部已实现工具包控件）、`ProjectionPage`、`TransitionsPage`（可选任意转场族，包括羽化，并用它导航）、`FeatherPage`、`MenusPage`（ContextMenu、CustomMessageBox、ApplicationBar/SystemTray 状态）。启动失败信息写入 `%TEMP%\community-phone-toolkit-sample-error.txt`。

## 文档

英文文档位于 `docs/en/`，中文文档位于 `docs/zh-CN/`。

- [交接文档 —— 新会话从这里开始](docs/zh-CN/handoff.md)（最新已验证状态、未提交修改、下一步）
- [移植状态](docs/zh-CN/porting-status.md)（按模块：已验证 / 已实现未验证 / 未实现）
- [原始功能审计](docs/zh-CN/porting-inventory.md)
- [架构设计](docs/zh-CN/architecture.md)
- [API 兼容矩阵](docs/zh-CN/api-compatibility.md)
- [迁移指南](docs/zh-CN/migration-guide.md)
- [已知限制](docs/zh-CN/known-limitations.md)
- [第三方声明](THIRD-PARTY-NOTICES_zh-CN.md)
- [中文文档索引](docs/zh-CN/README.md)

## 未实现模块

当前 preview **不实现** `AutoCompleteBox`、`LongListSelector`、`LongListMultiSelector`、`MultiselectList`、地图扩展、`TransferControl`、`SpeechTextBox`、`LockablePivot`、`SlideInEffect`、`GestureListener`/`GestureService` 或 looping-selector/picker pages。原因和替代方案见 [已知限制](docs/zh-CN/known-limitations.md)。这些内容不会被宣称为兼容。

## 许可证与归属

本项目以 **Microsoft Public License (MS-PL)** 分发，见 [LICENSE](LICENSE)。原始 toolkit 源码版权归 Microsoft Corporation；移植修改版权归各贡献者。包不依赖 Windows Phone SDK、Silverlight、`Microsoft.Phone` 或 `System.Windows.Interactivity`。
