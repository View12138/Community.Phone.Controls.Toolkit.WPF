# Community.Phone.Controls.Toolkit

归档版 **Windows Phone Toolkit** 的 WPF 兼容移植，以单个 NuGet 包交付。移植在 WPF 允许的范围内保留原始 `Microsoft.Phone.Controls` / `Microsoft.Phone.Shell` 命名空间与 API 形态，用 WPF 原生能力实现真实的 3D 投影、导航转场和 Metro 风格控件。这是兼容移植，不是新 UI 框架。

## 包与程序集

包 ID 为 `Community.Phone.Controls.Toolkit`，包内包含两个程序集：

| 程序集 | 根命名空间 | 内容 |
|---|---|---|
| `Community.Phone.Controls.Toolkit` | `Microsoft.Phone.Controls` | 转场、效果、toolkit 控件与主题 |
| `Community.Phone` | `Microsoft.Phone` | `Microsoft.Phone.Controls` 的页面/框架/导航类型、`Pivot`、`LongListSelector`、`HyperlinkButton`、`Panorama`、投影类型，以及全部 `Microsoft.Phone.Shell` 类型 |

投影类型（`ProjectionHost`、`ProjectionService`、`PlaneProjection`、`Matrix3DProjection`）在 `Community.Phone` 程序集，与页面/框架共用 `platform:` 前缀；转场、效果和其余控件在 toolkit 程序集，使用 `phone:` 前缀。两个程序集共用 `Microsoft.Phone.Controls` 命名空间，XAML 必须分别声明前缀（见[架构设计 §3](docs/architecture.md#3-命名空间与-xaml-前缀)）。使用方不需要额外安装兼容层包。

## 环境要求

- Windows
- .NET SDK 10.0 或更高版本
- 目标框架为 `net10.0-windows` 且启用 `UseWPF` 的工程
- 主题与 `Application` 扩展成员使用 C# 14 扩展成员语法，工程需启用对应 `LangVersion`

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
using Microsoft.Phone.Controls;   // Application 的扩展成员
using Microsoft.Phone.Shell;
using System.Windows;

public partial class App : Application
{
    private void OnStartup(object sender, StartupEventArgs e)
    {
        this.InitializeTheme();
        new MainWindow().Show();
    }
}
```

`InitializeTheme()` 是 `Microsoft.Phone.Controls.ApplicationExtensions` 中定义的 `Application` 扩展成员；它在应用资源树中查找 `/Community.Phone;component/Themes/Theme.xaml` 并写入当前调色板，找不到时抛 `Theme dictionary not found.`，因此必须在合并主题字典之后调用。`RequestedTheme`、`AccentColor`、`CustomAccentColor` 同为 `Application` 的 C# 14 扩展成员。完整机制见[主题系统](docs/theming.md)。

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

页面与框架基类、应用栏与系统栏位于兼容层。使用这些类型时声明以下前缀：

```xml
xmlns:phone="clr-namespace:Microsoft.Phone.Controls;assembly=Community.Phone.Controls.Toolkit"
xmlns:platform="clr-namespace:Microsoft.Phone.Controls;assembly=Community.Phone"
xmlns:shell="clr-namespace:Microsoft.Phone.Shell;assembly=Community.Phone"
```

`phone:` 用于 toolkit 程序集的类型：`TransitionFrame`、`TransitionService` 与各转场族、`TiltEffect` / `TurnstileFeatherEffect` / `SlideInEffect`、`AutoCompleteBox`、`Rating`、`ToggleSwitch`、`ListPicker`、`HubTile`、`PhoneTextBox` 等。`platform:` 用于兼容层的 `PhoneApplicationPage`、`PhoneApplicationFrame`、`Panorama`、`Pivot`、`LongListSelector`、`HyperlinkButton` 以及投影类型。

### 4. 主题画刷的引用方式

主题画刷必须用 `DynamicResource` 引用，运行时切换主题/强调色才会生效：

```xml
<TextBlock Foreground="{DynamicResource PhoneForegroundBrush}" />
```

用 `StaticResource` 引用的画刷会保留 XAML 解析时的值。字体名、字号、`Thickness`、`Style` 等非颜色资源用 `StaticResource` 即可。

## 投影

WPF 的 `UIElement` 没有 `Projection` 属性。投影类型位于 `Community.Phone` 程序集，使用 `platform:` 前缀。`ProjectionHost` 的内容仍是可交互的实时视觉树；也可以在任意元素上赋 `ProjectionService.Projection`，由服务按需包装：

```xml
<platform:ProjectionHost Width="320" Height="200"
        xmlns:platform="clr-namespace:Microsoft.Phone.Controls;assembly=Community.Phone">
    <Border Background="#FF1F1F1F">
        <TextBlock Text="Hello, Metro" Foreground="White" />
    </Border>
    <platform:ProjectionHost.Projection>
        <platform:PlaneProjection RotationY="-35" RotationX="8" />
    </platform:ProjectionHost.Projection>
</platform:ProjectionHost>
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

出站转场取自**被离开的页面**，所以退场与进场需要各自声明（或在导航前用 `TransitionService.SetNavigationOutTransition` 设置）。每个转场播放的仍是原版工具箱的 27 个 storyboard（`Transitions/Storyboards/*.xaml`，以资源形式内嵌）；`TurnstileFeather` 模式的 storyboard 由代码合成。

## 应用栏与系统栏

```xml
<platform:PhoneApplicationPage.ApplicationBar>
    <shell:ApplicationBar IsVisible="True">
        <shell:ApplicationBarIconButton IconUri="/Assets/appbar.settings.png" Text="settings" Click="OnSettings" />
        <shell:ApplicationBarMenuItem Text="about" Click="OnAbout" />
    </shell:ApplicationBar>
</platform:PhoneApplicationPage.ApplicationBar>
```

应用栏由 frame 模板中的 `ApplicationBarPresenter` 绘制（图标槽、菜单、最小化模式、为页面预留高度）。`SystemTray` 是静态类，提供状态 API（`IsVisible`、`Opacity`、`BackgroundColor`、`ForegroundColor`、`ProgressIndicator`），并同时定义 `IsVisibleProperty` 附加属性与 `GetIsVisible`/`SetIsVisible`，因此 XAML 中 `shell:SystemTray.IsVisible="True"` 可用（仅对 `PhoneApplicationPage`、`PhoneApplicationFrame`、`Window` 生效）。它不绘制状态栏；可在代码中用 `SystemTray.IsVisible = true;` 设置。

## 构建

```powershell
dotnet restore
dotnet build Community.Phone.Controls.Toolkit.slnx -c Release
dotnet pack src\Community.Phone.Controls.Toolkit\Community.Phone.Controls.Toolkit.csproj -c Release
```

解决方案只包含两个库工程与示例工程，仓库内没有测试工程。构建通过，警告来自可空性、成员隐藏、未使用事件与 XML 注释等。

包输出到 `artifacts/packages`（`PackageOutputPath`）。`csproj` 的打包项引用了仓库中不存在的 `THIRD-PARTY-NOTICES_zh-CN.md`，`docs/en/*.md` 与 `docs/zh-CN/*.md` 目录也不存在，因此 `dotnet pack` 当前报 `NU5019` 失败。`build\verify-package.ps1` 引用不存在的 `tests/PackageConsumer/PackageConsumer.csproj`，同样无法执行。

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
| `Samples/AutoCompleteBoxSample.xaml` | 自动完成（`AutoCompleteBox`） |
| `Samples/ListPickerSample.xaml` | 单/多选 `ListPicker` 与摘要 |
| `Samples/LongListSelectorSample.xaml` | 分组列表、跳转列表、网格模式、`PersonDetail` 子页 |
| `Samples/LongListMultiSelectorSample.xaml` | 多选 long list；`?multiselect` 入口说明已弃用的 `MultiselectList` |
| `Samples/PivotSample.xaml` | `Pivot` 的拖拽、页眉点击、`IsLocked` |
| `Samples/HubTileSample.xaml` | `HubTile` 与图片 |
| `Samples/SlideInEffectSample.xaml` | `SlideInEffect` 逐项动画 |
| `Samples/ToggleSwitchSample.xaml`、`Samples/CustomMessageBoxSample.xaml` | 对应控件与消息框 |
| `Samples/DateTimePickerSample.xaml` | 日期/时间选择器及其 picker 页面 |
| `Pages/ProjectionPage.xaml` | `ProjectionHost` 与投影动画 |
| `Samples/TransitionsSample.xaml` → `Samples/NavigationTransitionSample1.xaml` | 选择转场族并用它导航往返（`SampleNavigator` 同时设置两侧） |
| `Samples/FeatheredTransitionsSample1.xaml` → `Samples/TurnstileFeatherEffectSample1.xaml` | 逐元素羽化进场与退场 |
| `Samples/TiltEffectSample.xaml`、`Samples/ContextMenuSample.xaml`、`Samples/ContextMenuPositionTest.xaml` | 倾斜效果、上下文菜单 |

`MainPage` 的条目表沿用原版 `PhoneToolkitSample8` 的 25 条记录，其中 `GestureSample`、`LockablePivotSample`、`MapsSample`、`SpeechTextBoxSample`、`TransferControlSample` 五项没有对应页面文件，导航会失败。

## 未实现模块

地图扩展、`TransferControl`、`SpeechTextBox`、`GestureListener` / `GestureService` 没有实现；没有独立的 `LockablePivot` 类型，由 `Pivot.IsLocked` 提供等价能力。`MultiselectList` 的类型与代码已移植，但没有默认主题模板。原因与替代方案见[已知限制](docs/known-limitations.md#1-不支持的控件与模块)。

## 文档

- [架构设计](docs/architecture.md) —— 工程结构、命名空间策略、投影/导航/转场/效果/控件/主题实现
- [API 兼容矩阵](docs/api-compatibility.md) —— 原始类型到移植类型的映射、兼容级别、默认值、XAML 边界
- [移植状态](docs/porting-status.md) —— 按模块的实现状态与构建验证
- [主题系统](docs/theming.md) —— `Theme.xaml`、`ThemePalette`、`ThemeManager` 与资源映射
- [已知限制](docs/known-limitations.md) —— 未实现模块、行为差异、验证缺口、手动验证步骤
- [第三方声明](THIRD-PARTY-NOTICES.md)

## 许可证与归属

本项目以 **Microsoft Public License (MS-PL)** 分发，见 [LICENSE](LICENSE)。原始 toolkit 源码版权归 Microsoft Corporation，移植修改版权归各贡献者。包不依赖 Windows Phone SDK、Silverlight、`Microsoft.Phone` 或 `System.Windows.Interactivity`。
