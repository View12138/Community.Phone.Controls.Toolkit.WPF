# Community.Phone.Controls.Toolkit

归档版 **Windows Phone Toolkit** 的 WPF 兼容移植，以单个 NuGet 包交付。移植在 WPF 允许的范围内保留原始 `Microsoft.Phone.Controls` / `Microsoft.Phone.Shell` 命名空间与 API 形态，并用 WPF 原生能力实现真实的 3D 投影、导航转场和 Metro 风格控件。

> **当前构建状态：** `main` 分支的 toolkit 工程无法完成编译 —— `Controls/ListPicker/ListPickerPage.xaml` 使用了不存在的 `shell:SystemTray.IsVisible` 附加属性（`Microsoft.Phone.Shell.SystemTray` 是无附加属性的静态类），标记编译报 `MC3072`；示例工程中的页面也使用同一写法。详见 [移植状态](docs/porting-status.md#构建与验证)。

## 包与程序集

包 ID 为 `Community.Phone.Controls.Toolkit`，包内包含两个程序集：

| 程序集 | 根命名空间 | 内容 |
|---|---|---|
| `Community.Phone.Controls.Toolkit` | `Microsoft.Phone.Controls` | 投影、转场、效果、toolkit 控件与主题 |
| `Community.Phone` | `Microsoft.Phone` | `Microsoft.Phone.Controls` 的页面/框架/导航类型，`Microsoft.Phone.Controls` 中的 `Pivot`、`LongListSelector`、`HyperlinkButton`，以及全部 `Microsoft.Phone.Shell` 类型 |

两个程序集共用 `Microsoft.Phone.Controls` 命名空间但内容不同，XAML 必须分别声明前缀（见[架构设计 §3](docs/architecture.md#3-命名空间与-xaml-前缀)）。使用方不需要额外安装兼容层包。

## 环境要求

- Windows
- .NET SDK 10.0 或更高版本
- 目标框架为 `net10.0-windows` 且启用 `UseWPF` 的工程

## 安装

```powershell
dotnet add package Community.Phone.Controls.Toolkit
```

## 快速开始

### 1. 合并主题字典

```xml
<Application x:Class="MyApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             Startup="OnStartup">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="/Community.Phone.Controls.Toolkit;component/Themes/Generic.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

该字典包含 Metro 调色板（`Theme.xaml`）、控件默认模板，以及应用 XAML 可直接引用的命名资源（`PhoneTextTitle1Style`、`PhoneTextNormalStyle`、`PhoneFontSizeNormal` 等）。控件模板本身由程序集主题字典自动查找，但命名资源需要这次合并。

### 2. 应用启动时应用主题

```csharp
using Microsoft.Phone.Shell;

public partial class App : Application
{
    private void OnStartup(object sender, StartupEventArgs e)
    {
        this.InitializeTheme();       // ThemeManager 的扩展方法
        new MainWindow().Show();
    }
}
```

`InitializeTheme()` 在应用资源树中查找 `/Community.Phone;component/Themes/Theme.xaml` 并写入当前调色板；找不到时抛 `Theme dictionary not found.`，因此必须在合并主题字典之后调用。`RequestedTheme`、`AccentColor`、`CustomAccentColor` 都是扩展属性，需要 `LangVersion` 支持 C# 14 扩展成员。完整机制见[主题系统](docs/theming.md)。

### 3. 用 `TransitionFrame` 承载页面

```xml
<Window x:Class="MyApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:phone="clr-namespace:Microsoft.Phone.Controls;assembly=Community.Phone.Controls.Toolkit">
    <phone:TransitionFrame x:Name="Frame" />
</Window>
```

```csharp
Frame.Navigate(new HomePage());
```

`TransitionFrame` 是 toolkit 类型（`phone:`）。页面与框架基类（`PhoneApplicationPage`、`PhoneApplicationFrame`）在兼容层，应用栏与系统栏在 `shell:`：

```xml
xmlns:phone="clr-namespace:Microsoft.Phone.Controls;assembly=Community.Phone.Controls.Toolkit"
xmlns:platform="clr-namespace:Microsoft.Phone.Controls;assembly=Community.Phone"
xmlns:shell="clr-namespace:Microsoft.Phone.Shell;assembly=Community.Phone"
```

`phone:` 用于 toolkit 程序集的类型：`TransitionFrame`、`TransitionService` 与各转场族、投影类型、`Rating`、`ToggleSwitch`、`ListPicker`、`HubTile`、`PhoneTextBox` 等。`platform:` 用于兼容层的 `PhoneApplicationPage`、`PhoneApplicationFrame` 以及其中的 `Pivot`、`LongListSelector`、`HyperlinkButton`。

### 4. 主题画刷的引用方式

主题画刷必须用 `DynamicResource` 引用，运行时切换主题/强调色才会生效：

```xml
<TextBlock Foreground="{DynamicResource PhoneForegroundBrush}" />
```

用 `StaticResource` 引用的画刷会保留 XAML 解析时的值。字体名、字号、`Thickness`、`Style` 等非颜色资源用 `StaticResource` 即可。

## 投影

WPF 的 `UIElement` 没有 `Projection` 属性。使用 `ProjectionHost`（内容仍是可交互的实时视觉树），或在任意元素上赋 `ProjectionService.Projection` 让服务按需包装：

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

```csharp
// 元素会按需被包进 ProjectionHost，清除投影时自动解包。
element.Projection = new PlaneProjection { RotationY = -35 };
```

`PlaneProjection` 继承 `Animatable`，可直接创建动画：

```csharp
projection.BeginAnimation(
    PlaneProjection.RotationYProperty,
    new DoubleAnimation(-80, 0, TimeSpan.FromMilliseconds(350))
    {
        EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6 }
    });
```

透视模型、几何与裁剪规则见[架构设计 §4](docs/architecture.md#4-投影子系统)。

## 导航与页面状态

```csharp
Frame.Navigate(new DetailPage(), new Dictionary<string, string> { { "id", "42" } });
```

```csharp
protected override void OnNavigatedTo(NavigationEventArgs e)
{
    base.OnNavigatedTo(e);
    string id = NavigationContext.QueryString["id"];
}
```

页面生命周期（`OnNavigatedTo`、`OnNavigatingFrom`、`OnNavigatedFrom`）与 `BackKeyPress` 由 `PhoneApplicationFrame` 触发；`PhoneApplicationPage.State` 由 frame 按页面实例保存；返回导航回到同一个页面实例（`KeepAlive = true`）。

## 转场

在页面 XAML 中声明：

```xml
<phone:TransitionService.NavigationInTransition>
    <phone:NavigationInTransition>
        <phone:NavigationInTransition.Forward>
            <phone:TurnstileTransition Mode="ForwardIn" />
        </phone:NavigationInTransition.Forward>
    </phone:NavigationInTransition>
</phone:TransitionService.NavigationInTransition>
```

出站转场取自**被离开的页面**，所以退场与进场需要各自声明（或在导航前用 `TransitionService.SetNavigationOutTransition` 设置）。每个转场播放的仍是原版工具箱的 storyboard（`Transitions/Storyboards/*.xaml`，以资源形式内嵌）。

## 应用栏与系统栏

```xml
<platform:PhoneApplicationPage.ApplicationBar>
    <shell:ApplicationBar IsVisible="True">
        <shell:ApplicationBarIconButton IconUri="/Assets/appbar.settings.png" Text="settings" Click="OnSettings" />
        <shell:ApplicationBarMenuItem Text="about" Click="OnAbout" />
    </shell:ApplicationBar>
</platform:PhoneApplicationPage.ApplicationBar>
```

应用栏由 frame 模板中的 `ApplicationBarPresenter` 绘制（图标槽、菜单、最小化模式、为页面预留高度）。`SystemTray` 只有状态 API（`IsVisible`、`Opacity`、`BackgroundColor`、`ForegroundColor`、`ProgressIndicator`），不绘制状态栏，也不是附加属性 —— 在 XAML 中写 `shell:SystemTray.IsVisible` 无法编译，请在代码中设置。

## 构建

```powershell
dotnet restore
dotnet build Community.Phone.Controls.Toolkit.slnx -c Release
dotnet pack src\Community.Phone.Controls.Toolkit\Community.Phone.Controls.Toolkit.csproj -c Release
```

包输出到 `artifacts/packages`（`PackageOutputPath`）。解决方案只包含两个库工程与示例工程，仓库内没有测试工程。

`build\verify-package.ps1` 会打包并从隔离的本地源还原、构建、运行一个全新的 WPF 消费工程；它引用 `tests/PackageConsumer/PackageConsumer.csproj`，该路径在当前仓库中不存在，因此脚本目前无法执行。

## 示例

```powershell
dotnet run --project samples\Community.Phone.Controls.Toolkit.Sample
```

`MainWindow` 承载一个 `TransitionFrame` 和底部三个导航按钮（返回、主页、搜索）。示例页面：

| 路径 | 内容 |
|---|---|
| `MainPage.xaml` | 示例索引（`LongListSelector` + `HyperlinkButton` 条目）与应用栏 Settings 按钮 |
| `Settings.xaml` | UI 文化、区域格式、`RequestedTheme`、强调色四个 `ListPicker` |
| `Samples/WPFControlsSample.xaml` | `WrapPanel`、`ToggleSwitch`、`Rating`、`PhoneTextBox`、`ListPicker`、`RecurringDaysPicker`、`ExpanderView`、`DatePicker`、`TimePicker`、`PerformanceProgressBar`、`RelativeAnimatingContentControl`、`HubTile` |
| `Samples/ListPickerSample.xaml` | 单/多选 `ListPicker` 与摘要 |
| `Samples/PivotSample.xaml` | `Pivot` 的拖拽、页眉点击、`IsLocked` |
| `Samples/HubTileSample.xaml` | 三项 `Pivot` + `HubTile` 与图片 |
| `Samples/LongListSelectorSample.xaml` | 分组列表、跳转列表、网格模式、`PersonDetail` 子页 |
| `Samples/ToggleSwitchSample.xaml`、`Samples/CustomMessageBoxSample.xaml` | 对应控件与消息框 |
| `Pages/ProjectionPage.xaml` | `ProjectionHost` 与投影动画 |
| `Pages/TransitionsPage.xaml` → `Pages/TransitionTargetPage.xaml` | 选择转场族并用它导航往返（`SampleNavigator` 同时设置两侧） |
| `Pages/FeatherPage.xaml` | `TurnstileFeatherEffect` 逐元素羽化 |
| `Pages/MenusPage.xaml` | `ContextMenu`、`CustomMessageBox`、`ApplicationBar` / `SystemTray` 状态 |

`MainPage` 的条目表沿用原版 `PhoneToolkitSample8` 的 25 条记录，其中只有上表中的页面存在；导航到其余 URI 会失败。

## 文档

- [架构设计](docs/architecture.md) —— 工程结构、命名空间策略、投影/导航/转场/效果/控件/主题实现
- [API 兼容矩阵](docs/api-compatibility.md) —— 原始类型到移植类型的映射、兼容级别、默认值、XAML 边界
- [移植状态](docs/porting-status.md) —— 按模块的实现状态与构建验证
- [主题系统](docs/theming.md) —— `Theme.xaml`、`ThemePalette`、`ThemeManager` 与资源映射
- [已知限制](docs/known-limitations.md) —— 未实现模块、行为差异、验证缺口、手动验证步骤
- [第三方声明](THIRD-PARTY-NOTICES.md)

## 未实现模块

`AutoCompleteBox`、`LongListMultiSelector`、`MultiselectList`、地图扩展、`TransferControl`、`SpeechTextBox`、`LockablePivot`（由 `Pivot.IsLocked` 取代）、`SlideInEffect`、`GestureListener` / `GestureService`、`LoopingSelector` 与 picker 页面均无实现。原因与替代方案见[已知限制](docs/known-limitations.md#1-不支持的控件与模块)。

## 许可证与归属

本项目以 **Microsoft Public License (MS-PL)** 分发，见 [LICENSE](LICENSE)。原始 toolkit 源码版权归 Microsoft Corporation，移植修改版权归各贡献者。包不依赖 Windows Phone SDK、Silverlight、`Microsoft.Phone` 或 `System.Windows.Interactivity`。
