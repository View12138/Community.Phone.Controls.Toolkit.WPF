# 迁移指南：Windows Phone Toolkit → WPF

## 1. 安装

```powershell
dotnet add package Community.Phone.Controls.Toolkit.WPF
```

添加命名空间：

```xml
xmlns:phone="clr-namespace:Microsoft.Phone.Controls;assembly=Community.Phone.Controls.Toolkit.WPF"
xmlns:shell="clr-namespace:Microsoft.Phone.Shell;assembly=Community.Phone.Controls.Toolkit.WPF"
```

原始 WP XAML 使用 `assembly=Microsoft.Phone` 和 `assembly=Microsoft.Phone.Controls.Toolkit`。这些程序集名称已不存在；在可能的情况下保留了命名空间和类型名，但必须修改 `assembly=` 值。

## 2. 合并主题资源

包内 `Themes/Generic.xaml` 包含 Metro 颜色、字体、文本样式、Frame 模板和控件模板。隐式控件样式会从程序集主题自动加载，但应用 XAML 使用的**命名**资源（`PhoneTextTitle1Style`、`PhoneAccentBrush` 等）应合并到 `App.xaml`：

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary
                Source="/Community.Phone.Controls.Toolkit.WPF;component/Themes/Generic.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

## 3. 应用启动与导航

用 WPF `Window` 承载 `TransitionFrame`，替换 WP 的 `PhoneApplicationFrame` 启动：

```xml
<Window x:Class="MyApp.MainWindow" ...>
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

页面继承 `phone:PhoneApplicationPage`：

```xml
<phone:PhoneApplicationPage
    x:Class="MyApp.HomePage"
    xmlns:phone="clr-namespace:Microsoft.Phone.Controls;assembly=Community.Phone.Controls.Toolkit.WPF">
    ...
</phone:PhoneApplicationPage>
```

`PhoneApplicationPage` 暴露 `State`、`NavigationContext`、`Orientation`、`SupportedOrientations`、`NavigatedTo`、`NavigatingFrom`、`NavigatedFrom`、`BackKeyPress` 和 `ApplicationBar`。生命周期方法由 `PhoneApplicationFrame`/`TransitionFrame` 触发，因此页面应放在这些 Frame 内。

使用 Frame 重载传递参数：

```csharp
Frame.Navigate(
    new DetailPage(),
    new Dictionary<string, string> { { "id", "42" }, { "mode", "edit" } });
```

在 `OnNavigatedTo` 中读取：

```csharp
protected override void OnNavigatedTo(NavigationEventArgs e)
{
    base.OnNavigatedTo(e);
    string id = NavigationContext.QueryString["id"];
}
```

移植版会把 URI 查询字符串和传入字典合并（字典值优先）到 `page.NavigationContext`，同时提供 `NavigationContext.Uri` 和 `ExtraData`。`PhoneApplicationPage.State` 由 Frame 按页面实例保存，返回到同一页面时仍然存在。

## 4. 投影 XAML 迁移

### 4.1 原始 Silverlight 形式（WPF 不编译）

```xml
<Grid>
    <Grid.Projection>
        <PlaneProjection RotationY="-35" />
    </Grid.Projection>
</Grid>
```

WPF `Grid` 没有 `Projection` 属性。C#/XAML 也没有能力给密封框架类型追加属性元素。

### 4.2 推荐的 WPF 形式

```xml
<phone:ProjectionHost Width="320" Height="200">
    <Grid Background="#FF1F1F1F">
        <TextBlock Text="Projected content" Foreground="White" />
    </Grid>
    <phone:ProjectionHost.Projection>
        <phone:PlaneProjection
            CenterOfRotationX="0.5"
            CenterOfRotationY="0.5"
            RotationY="-35" />
    </phone:ProjectionHost.Projection>
</phone:ProjectionHost>
```

Host 使用真实 `Viewport3D`/`Viewport2DVisual3D`，因此内容可交互，不是截图。

### 4.3 附加属性形式

```xml
<phone:ProjectionHost
    phone:ProjectionService.Projection="{StaticResource MyProjection}">
    <Grid>...</Grid>
</phone:ProjectionHost>
```

同一个属性也可以用在任意元素上，元素会按需被包装：

```xml
<Grid phone:ProjectionService.Projection="{StaticResource MyProjection}">...</Grid>
```

```csharp
myGrid.Projection = new PlaneProjection { RotationY = -35 };
myGrid.Projection = null;   // 释放按需创建的包装
```

按需包装要求元素是某个 `Panel` 的直接子元素，且该 Panel 不是 `ItemsControl` 的 items panel。item 容器（`ListBoxItem`、`ComboBoxItem`、自定义容器）会被跳过，因为 items panel 的子元素由 WPF 管理；`ContentPresenter` 与其他 Panel 子元素一样可以包装。投影被清除时包装会自动释放，因此最终的可视树与原来一致。模板中或元素不是 panel 子元素时，请显式使用 `ProjectionHost`。

### 4.4 动画

Silverlight storyboard 使用：

```xml
Storyboard.TargetProperty="(UIElement.Projection).(PlaneProjection.RotationY)"
```

WPF 中直接对 `PlaneProjection` 对象做动画：

```csharp
DoubleAnimation animation = new DoubleAnimation
{
    From = -35,
    To = 0,
    Duration = TimeSpan.FromSeconds(0.35),
    EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6 }
};
projection.BeginAnimation(PlaneProjection.RotationYProperty, animation);
```

`Projection` 继承 `Animatable`，所以 `BeginAnimation` 可直接使用。`Storyboard` 也可以把元素本身作为目标，路径为 `(ProjectionService.Projection).(PlaneProjection.RotationY)`，与 Silverlight 一致。

### 4.5 `Matrix3DProjection`

`ProjectionMatrix` 按 Silverlight 的归一化 0..1 约定解释，并由 Host 转换到像素空间。复杂矩阵请做视觉验证。

## 5. 导航转场

修改命名空间后，原始页面声明可用：

```xml
<phone:TransitionService.NavigationInTransition>
    <phone:NavigationInTransition>
        <phone:NavigationInTransition.Backward>
            <phone:TurnstileTransition Mode="BackwardIn" />
        </phone:NavigationInTransition.Backward>
        <phone:NavigationInTransition.Forward>
            <phone:TurnstileTransition Mode="ForwardIn" />
        </phone:NavigationInTransition.Forward>
    </phone:NavigationInTransition>
</phone:TransitionService.NavigationInTransition>

<phone:TransitionService.NavigationOutTransition>
    <phone:NavigationOutTransition>
        <phone:NavigationOutTransition.Backward>
            <phone:TurnstileTransition Mode="BackwardOut" />
        </phone:NavigationOutTransition.Backward>
        <phone:NavigationOutTransition.Forward>
            <phone:TurnstileTransition Mode="ForwardOut" />
        </phone:NavigationOutTransition.Forward>
    </phone:NavigationOutTransition>
</phone:TransitionService.NavigationOutTransition>
```

需要这些转场时请使用 `TransitionFrame`（不是普通 `PhoneApplicationFrame`）。`TransitionFrame` 通过两个 `ProjectionHost` 部件 `FirstContentPresenter` 和 `SecondContentPresenter` 呈现页面。直接 `Frame.Navigate(page)` 会同时执行进入和退出转场。

如果转场工厂抛出异常，异常会从导航中向上抛出，行为与原版工具箱一致 —— 没有回退事件，因此请自行校验自定义 `TransitionElement` 实现。

每个转场播放的都是原版 storyboard：工具箱从自身资源加载 `Transitions/Storyboards/<系列><模式>.xaml`，因此角度、关键时间和缓动保持不变。仅有的两个获准偏差是 turnstile 的两个常量（`CenterOfRotationX = -0.05`、时长系数 `1.2`），均在 `Transitions.cs` 中说明，可分别改回工具箱原值 `0` 和 `1`。

## 6. 控件迁移

### 仅需修改命名空间的典型控件

这些控件保留公共 API；把 `toolkit:` 改为 `phone:`，并修改命名空间中的程序集：

```xml
<phone:ToggleSwitch Header="Wi-Fi" />
<phone:Rating Width="220" Value="3" AllowHalfItemIncrement="True" />
<phone:PhoneTextBox Hint="Search" LengthIndicatorVisible="True" />
<phone:ListPicker Header="Choose" SelectedIndex="0" />
<phone:ExpanderView Header="Details">...</phone:ExpanderView>
<phone:HubTile Title="News" Message="Back" />
<phone:WrapPanel ItemWidth="80">...</phone:WrapPanel>
<phone:PerformanceProgressBar IsIndeterminate="True" />
<phone:ContextMenuService.ContextMenu>
    <phone:ContextMenu>
        <phone:MenuItem Header="Open" Click="OnOpen" />
        <phone:Separator />
    </phone:ContextMenu>
</phone:ContextMenuService.ContextMenu>
```

### 有宿主/行为差异的控件

| 控件 | 迁移说明 |
|---|---|
| `ContextMenu` | 继承 WPF `ContextMenu`；右键打开。`ContextMenuService.ContextMenu` 是附加属性。 |
| `CustomMessageBox` | 构造后调用 `Show()`；由 WPF `Window` 承载，`Dismiss()` 触发相同事件。 |
| `DatePicker` / `TimePicker` | 保留 `Value`、`ValueString`、`ValueChanged`、`Header`。picker UI 是 WPF 模态窗口，不是 WP picker page；`PickerPageUri` 保留但不导航。`ValueStringFormat` 是复合格式字符串，例如 `"{0:yyyy-MM-dd}"`。 |
| `ListPicker` | 普通（展开）模式行为不变；完整模式通过宿主 frame 导航到 `ListPickerPage`，使用 `PickerPageUri` 中的 pack URI。页面的「完成/取消」与原版一致，声明在 `ApplicationBar` 里（勾/叉图标按钮；多选时显示、单选时隐藏）。 |
| `ExpanderView` | 模板部件是 `Presenter`、`ExpanderPanel`、`ItemsCanvas`；高度动画在代码中完成。 |
| `HubTile` | 定时器驱动状态循环；支持 `HubTileService.FreezeGroup/UnfreezeGroup`。 |
| `PhoneTextBox` | 内部 TextBox 使用双向文本绑定；保留 hint/action/length indicator 行为。 |
| `Rating` | 星形项目在代码中生成并使用 `RatingItem`；模板部件名称与原始一致。 |
| `TiltEffect` | 在容器上设置 `phone:TiltEffect.IsTiltEnabled="True"`。普通控件会临时包进 `ProjectionHost`；列表 item 容器不包装。 |
| `TurnstileFeatherEffect` | 在可包装元素上设置 `phone:TurnstileFeatherEffect.FeatheringIndex`。item 容器/content presenter 会被跳过。 |

## 7. 效果与手势

### TiltEffect

```xml
<StackPanel phone:TiltEffect.IsTiltEnabled="True">
    <Button Content="Tilt me" />
</StackPanel>
```

按下时创建 `PlaneProjection` 并动画。除触摸 manipulation 外，也支持鼠标输入；鼠标使用隧道式预览事件，因此不会吞掉按钮点击。

### TurnstileFeatherEffect

```xml
<TextBlock Text="First" phone:TurnstileFeatherEffect.FeatheringIndex="0" />
<TextBlock Text="Second" phone:TurnstileFeatherEffect.FeatheringIndex="1" />
```

目标必须可被 `ProjectionHost` 包装：它是某个 Panel 的直接子元素，且该 Panel 不是 `ItemsControl` 的 items panel。item 容器本身（`ListBoxItem`）无法包装，因为 items panel 的子元素由 WPF 管理；请把 feathering index 放到 item 模板内部的内容上。

### 未迁移

- `SlideInEffect` 依赖 WP `Pivot`，未实现。
- `GestureListener`/`GestureService` 未实现；使用 WPF 鼠标/触摸/manipulation 事件或专用手势库。
- 地图、后台传输、语音未实现；请定义应用自己的服务接口，把平台相关代码隔离在共享 UI 之外。

## 8. 系统栏与应用栏

`ApplicationBar` 已完整移植：可以在 XAML 中声明（`<phone:PhoneApplicationPage.ApplicationBar><shell:ApplicationBar .../></phone:PhoneApplicationPage.ApplicationBar>`），也可以在代码中赋值；frame 会在页面底部绘制它并为页面预留空间。图标按钮需要 `IconUri`（应用相对路径，例如 `/Images/appbar.save.rest.png`）和 `Text`；一个应用栏最多 4 个图标按钮和 50 个菜单项，其 item 会触发 `Click`。`SystemTray.IsVisible` 是附加属性，状态对象已实现，但默认 Frame 模板仍未绘制完整的 Windows Phone 状态栏；需要时请自行承载该 chrome。

## 9. 资源与主题迁移

原始 WP 平台提供 `PhoneAccentBrush`、`PhoneForegroundBrush`、`PhoneBackgroundBrush`、`PhoneFontFamily*`、`PhoneFontSize*`、`PhoneText*Style` 等资源。移植版在 `Themes/Generic.xaml` 中提供这些键。如果应用定义了自己的主题资源，请放在 toolkit 字典之后合并，以便覆盖：

```xml
<ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source="/Community.Phone.Controls.Toolkit.WPF;component/Themes/Generic.xaml" />
    <ResourceDictionary Source="MyTheme.xaml" />
</ResourceDictionary.MergedDictionaries>
```

## 10. 验证清单

1. 工程以 `net10.0-windows`（或选择的 Windows TFM）和 `UseWPF` 构建。
2. 如果使用命名 Metro 资源，`App.xaml` 合并了 `Themes/Generic.xaml`。
3. 页面继承 `PhoneApplicationPage`，并放在 `PhoneApplicationFrame`/`TransitionFrame` 内。
4. 投影赋给元素（按需包装）或显式 `ProjectionHost`；验证命中测试和输入。
5. 导航转场实际运行；注意无效 storyboard 属性值导致的异常。
6. 控件以预期主题和模板部件渲染。
7. 不支持的模块用应用专用 WPF 实现替代，而不是假定其可用。

## 11. 常见迁移陷阱

- `xmlns:toolkit` 仍指向旧程序集；删除它并使用 `xmlns:phone`。
- 区分 WPF 的 `System.Windows.Controls.WrapPanel` 和移植版的 `Microsoft.Phone.Controls.WrapPanel`；明确使用 `phone:` 前缀。
- 同时导入 `Microsoft.Phone.Controls` 和 `System.Windows.Controls` 时，`ContextMenu` 会产生歧义；在 C# 中限定类型。
- `DatePicker` 同样有歧义；使用别名或限定类型。
- 不要在截图/VisualBrush 投影中放交互控件；移植版使用实时 `Viewport2DVisual3D`。
- 不要在 `DiscreteObjectKeyFrame` 中用字符串动画 `Visibility` 或 `Margin`；使用 `<Visibility>` 和 `<syswin:Thickness>` 对象（移植主题已经这样处理）。
