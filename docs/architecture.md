# 架构设计

## 1. 目标与约束

把归档的 Windows Phone Toolkit 移植到现代 WPF，并在 WPF 允许的范围内保留原始公共 API。影响设计的关键约束：

- WPF 的 `UIElement` 没有 `Projection` 属性，也不能给 `Grid` 等既有框架类型追加 CLR/XAML 属性。
- WPF 自带 `Frame`、`Page`、`ContextMenu`、`WrapPanel`、`DatePicker`、`HeaderedItemsControl`，重新定义同名 `System.Windows.*` 类型会造成歧义并违反“不重新定义 WPF 框架类型”的约束。
- `PhoneApplicationFrame`、`PhoneApplicationService`、`SystemTray`、地图、后台传输、语音等平台服务在 WPF 中不存在。
- 原始 XAML 使用 Silverlight 资源键和动画路径，例如 `(UIElement.Projection).(PlaneProjection.RotationY)`。

因此移植版在**原始 `Microsoft.Phone.Controls` / `Microsoft.Phone.Shell` 命名空间**中提供兼容类型，对真实 3D 使用显式的 `ProjectionHost` 包装，并对框架相邻控件使用 WPF 原生基类。

## 2. 仓库与工程

```text
Community.Phone.Controls.Toolkit.slnx
├─ src/Community.Phone/                       兼容层（程序集 Community.Phone，根命名空间 Microsoft.Phone）
│  ├─ Controls/     Pivot、PivotItem、Panorama、PanoramaItem、LongListSelector、HyperlinkButton、ViewportControl、Primitives/*
│  ├─ Navigation/   PhoneApplicationFrame、PhoneApplicationPage、NavigationContext、方向类型
│  ├─ Projection/   Projection、PlaneProjection、Matrix3DProjection、ProjectionHost、ProjectionService、ProjectionMath
│  ├─ Shell/        ApplicationBar 及其呈现器、SystemTray、PhoneApplicationService、ThemeManager、ThemePalette
│  ├─ Globalization/  SortedLocaleGrouping
│  └─ Themes/       Theme.xaml（调色板）、Generic.xaml、Controls/{ApplicationBar,Button,ListBox,LongListSelector,Panorama,PhoneApplicationFrame,Pivot,ScrollBar,ScrollViewer,TextBlock,TextBox}.xaml
├─ src/Community.Phone.Controls.Toolkit/      toolkit 程序集（根命名空间 Microsoft.Phone.Controls）
│  ├─ Projection/   资源键与投影互操作辅助（投影类型在 Community.Phone）
│  ├─ Transitions/  ITransition、Transition、TransitionFrame、TransitionService、Storyboards/*.xaml、各转场族
│  ├─ Effects/      TiltEffect、TurnstileFeatherEffect、SlideInEffect
│  ├─ AutoCompleteBox/  AutoCompleteBox、AutoCompleteFilter(Mode/Predicate)、PopupHelper、SelectorSelectionAdapter
│  ├─ 控件目录/      ListPicker/、HubTile/、PhoneTextBox/、ToggleSwitch/、Primitives/、ContextMenu/、CustomMessageBox/、
│  │                ExpanderView/、ProgressBar/、DateTimePickers/、DateTimeConverters/、WrapPanel/、RatingControl/
│  ├─ Common/       ItemsControlExtensions、LengthConverter、TypeConverters、视觉树辅助
│  ├─ LocalizedResources/  51 个 ControlResources*.resx（50 个文化 + 中性资源）
│  └─ Themes/       Generic.xaml、Controls/*.xaml
└─ samples/Community.Phone.Controls.Toolkit.Sample/   可运行 WPF 示例
```

解决方案只包含这两个库工程与示例工程，**没有测试工程**。

职责划分：

- `Community.Phone` 重新实现 WPF 不提供的 `Microsoft.Phone` 平台表面：页面/框架/导航类型、投影类型、`Microsoft.Phone.Shell`（应用栏、系统栏、应用服务、主题），以及原 toolkit 中属于平台层的 `Pivot`、`LongListSelector`、`HyperlinkButton`、`Panorama`。它不引用 toolkit。
- `Community.Phone.Controls.Toolkit` 引用 `Community.Phone`，提供转场、效果与其余控件。
- 两者打进**一个** NuGet 包 `Community.Phone.Controls.Toolkit`。

两个工程都在所有配置下定义 `WINDOWS_PHONE;WP8`，移植源码用这些常量选择兼容命名空间与本地化资源。

## 3. 命名空间与 XAML 前缀

| 原始命名空间 | 移植命名空间 | 程序集 | 说明 |
|---|---|---|---|
| `Microsoft.Phone.Controls`（平台：页面、框架、导航、`Pivot`、`LongListSelector`、`HyperlinkButton`、`Panorama`） | 同名 | `Community.Phone` | `platform:` 前缀 |
| `Microsoft.Phone.Controls`（投影：`ProjectionHost`、`ProjectionService`、`PlaneProjection`、`Matrix3DProjection`） | 同名 | `Community.Phone` | `platform:` 前缀 |
| `Microsoft.Phone.Controls`（toolkit：转场、效果、控件） | 同名 | `Community.Phone.Controls.Toolkit` | `phone:` 前缀 |
| `Microsoft.Phone.Controls.Primitives`（`PivotHeadersControl`、`PivotHeaderItem`、`ViewportControl`） | 同名 | `Community.Phone` | `primitives:` 前缀 |
| `Microsoft.Phone.Controls.Primitives`（`ToggleSwitchButton`、`LoopingSelector`、`LoopingSelectorItem`、`ILoopingSelectorDataSource`、`ClipToBounds`） | 同名 | `Community.Phone.Controls.Toolkit` | `primitives:` 或 `phone:` 前缀 |
| `Microsoft.Phone.Controls.LocalizedResources` | 同名 | `Community.Phone.Controls.Toolkit` | `ControlResources` |
| `Microsoft.Phone.Shell` | 同名 | `Community.Phone` | `shell:` 前缀 |
| `Microsoft.Phone.Maps.Toolkit` | 未实现 | — | 没有 WPF 地图提供程序 |
| toolkit 的 `System.Windows.Controls.WrapPanel`、`LengthConverter` | `Microsoft.Phone.Controls`（`WINDOWS_PHONE` 分支） | toolkit | 避免重新定义 WPF 类型 |
| toolkit 的 `System.Windows.Controls.TimeTypeConverter` | `System.Windows.Controls` | toolkit | 无 `#if` 分支，保持全局命名空间 |
| toolkit 的 `System.Windows.HierarchicalDataTemplate` | WPF 原生 | — | 直接使用 `System.Windows.HierarchicalDataTemplate` |

因为 toolkit 类型与平台/投影类型共用 `Microsoft.Phone.Controls` 命名空间却位于不同程序集，同时使用两者的 XAML 需要多个前缀：

```xml
xmlns:phone="clr-namespace:Microsoft.Phone.Controls;assembly=Community.Phone.Controls.Toolkit"
xmlns:platform="clr-namespace:Microsoft.Phone.Controls;assembly=Community.Phone"
xmlns:shell="clr-namespace:Microsoft.Phone.Shell;assembly=Community.Phone"
```

注意 storyboard 资源内部使用的 `phone:` 是它自己的本地别名，指向 `assembly=Community.Phone`（投影类型所在程序集），与消费方 XAML 的前缀约定无关。

WPF 有 `System.Windows.Controls.ContextMenu`，移植版的 `Microsoft.Phone.Controls.ContextMenu` 直接使用 `phone:` 前缀选择。这类前缀修改属于**必须迁移**的部分，XAML 不能原样编译。

## 4. 投影子系统

投影类型编译在 `Community.Phone` 程序集。

### 4.1 公共模型

| 类型 | 基类 | 关键成员 |
|---|---|---|
| `Projection` | `Animatable` | `ProjectionChanged` 事件、`IsIdentity`、`ToMatrix3D(Size)` |
| `PlaneProjection` | `Projection` | `CenterOfRotationX/Y/Z`、`GlobalOffsetX/Y/Z`、`LocalOffsetX/Y/Z`、`RotationX/Y/Z`（均为依赖属性） |
| `Matrix3DProjection` | `Projection` | `ProjectionMatrix`，使用 0..1 归一化坐标 |
| `ProjectionHost` | `FrameworkElement` | `Content`、`Projection`、`PerspectiveDepth`、`Prefer3D`、`IsRenderingIn3D` |
| `ProjectionService` | 静态类 | 附加属性 `Projection`；`GetProjectionHost`、`GetDirectProjectionHost`、`EnsureProjectionHost`、`ReleaseProjectionHost`、`GetProjectionPropertyPath`；扩展成员 `element.Projection` / `element.ProjectionHost` |

`ProjectionHost` 标注 `[ContentProperty("Content")]`，因此内容元素可以直接写在 Host 内部。投影为非 identity 时 Host 通过 `Viewport3D` / `Viewport2DVisual3D` 渲染，identity 时走普通 2D 视觉树。

### 4.2 渲染方案

采用 **WPF `Viewport3D` + `Viewport2DVisual3D`**：把实时 2D 视觉树映射到 3D 平面，保留命中测试、文本、输入和焦点，由 GPU 合成。截图/`VisualBrush` 平面会丢失交互，只作静态快照；Composition/D3D 互操作需要额外宿主与依赖，不满足单包交付。

### 4.3 几何与透视

Host 创建一个以元素原点为中心的矩形网格：

```text
positions (Y up):  (-w/2, +h/2, 0), (+w/2, +h/2, 0), (+w/2, -h/2, 0), (-w/2, -h/2, 0)
texture coords:    (0,0), (1,0), (1,1), (0,1)
triangles:         0,2,1 与 0,3,2（正面）+ 0,1,2 与 0,2,3（反向绕序）
```

`PerspectiveCamera` 位于 `(0, 0, depth)`，`depth = PerspectiveDepth`，默认 `999` DIP。视场角由视口**宽度**推导：`fov = 2 * atan(viewportWidth / (2 * depth))`。WPF 的 `PerspectiveCamera.FieldOfView` 是水平视场角，垂直视场角由宽高比推导，因此按宽度推导能保证 identity 投影与布局槽位 1:1；按高度推导会让每个被投影元素按宽高比缩小。

999 DIP 是 XAML `PlaneProjection` 的固定透视（WinUI 用 `M34 = -1/999`），因此深度 `z` 处一点的实际缩放恒为 `999 / (999 - z)`，与元素尺寸无关。

`PlaneProjection.ToMatrix3D` 使用行向量约定：

```text
p' = (((p - center) + localOffset) * R) + center + globalOffset
R  = CreateRotationMatrix(RotationX, -RotationY, RotationZ)   （角度制）
center = ((CenterOfRotationX - 0.5) * width, (0.5 - CenterOfRotationY) * height, CenterOfRotationZ)
Y 分量取反，因为 WPF 3D 使用 +Y 向上而屏幕坐标使用 +Y 向下。
RotationY 取反，因为 XAML 中正的 RotationY 让右边缘靠近观察者，而 WPF 的 +Y 轴正向旋转让它远离。
```

矩阵赋给 `Viewport2DVisual3D.Transform`，裁剪与命中测试由 WPF 3D 管线处理。WPF 会剔除背面，而 XAML 投影从两侧可见，因此四边形同时带反向绕序三角形：超过 90° 的旋转（例如 `HubTile` 翻转的后半段）继续渲染镜像内容而不是消失。几何在尺寸或投影变化时重建。

**裁剪规则**：投影结果**不**裁剪到 Host 的布局槽（XAML 投影会让被放大的近端画到元素边界之外），3D 路径下 `Viewport3D.ClipToBounds = false`；被裁剪的是**内容** —— 表面带一个布局槽大小的 `RectangleGeometry` 裁剪，复现平台“把内容栅格化进元素边界”的规则。Host 只在 2D 渲染时给自身设置 `ClipToBounds`。

### 4.4 XAML 兼容边界

WPF 无法编译 `<Grid.Projection>`，因为 `Grid` 没有该属性。支持的形式：

```xml
<!-- 显式 Host。 -->
<platform:ProjectionHost>
    <Grid>...</Grid>
    <platform:ProjectionHost.Projection>
        <platform:PlaneProjection RotationY="-35" />
    </platform:ProjectionHost.Projection>
</platform:ProjectionHost>

<!-- 任意元素上的附加属性；元素按需被包装，投影清除时解包。 -->
<Grid platform:ProjectionService.Projection="{StaticResource MyProjection}">...</Grid>
```

代码中最短的形式是 `element.Projection = new PlaneProjection { RotationY = -35 };`；storyboard 路径为 `(ProjectionService.Projection).(PlaneProjection.RotationY)`，由 `ProjectionService.GetProjectionPropertyPath` 生成。

### 4.5 宿主包装规则

`ProjectionService` 只投影**直接宿主**，不复用祖先宿主：页面由转场 Frame 的 presenter 承载，复用祖先宿主会把整页一起投影。

- `ListBoxItem`：默认主题模板 `Themes/Controls/ListBox.xaml` 内含 `x:Name="ProjectionHost"` 的 `ProjectionHost`（包住 `Border` 与内容容器）。模板应用后，`EnsureProjectionHost` / `FindDirectHost` 通过 `Template.FindName` 复用该部件，不重父级化、不复制布局属性；清除投影后部件保留并回到 2D 路径。自定义 `ListBoxItem.Template` 必须保留同名部件。
- 其他元素：按需创建 `ProjectionHost`，复制尺寸、`Margin`、对齐、`Grid` 行列、`ZIndex`、`Dock`、`Canvas` 位置，并按父容器类型插回原位：`Panel.Children`、`ItemsControl.Items`、`ContentControl.Content` 或 `Page.Content`；投影清除时释放宿主并恢复布局属性。`ContentPresenter` 可以包装：重新挂载后 WPF 保留其 `TemplatedParent`、TemplateBinding 和具名模板部件。
- 由 `ItemsControl` 生成且模板没有该部件的 item 容器（如 `ComboBoxItem`）无法可靠地重父级化到新宿主；应把需要投影的内容放进模板内的 `ProjectionHost`。

`TiltEffect` 在控件加载时完成包装（倾斜关闭时解包），`TurnstileFeatherEffect` 在订阅时准备包装并延迟到 dispatcher 执行，因此指针按下与转场动画期间都不会改动视觉树。

### 4.6 渲染路径切换

投影为 identity 时 Host 走 2D 路径，否则走 3D 路径。`Prefer3D` 让 Host 在投影仍为 identity 时保持 3D 路径，使转场与效果在**准备阶段**完成路径切换，而不是等到动画首帧（此时文本栅格化方式会从 ClearType 变成 3D 纹理的灰度抗锯齿）。

`Viewport2DVisual3D` 把被投影可视对象的**绘制边界**映射到四边形上，而不是布局槽。若直接投影内容，绘制范围小于布局槽的元素（左对齐的 `CheckBox`、较短的 `TextBlock`）会被拉伸到整个平面。因此 Host 投影一个填满**布局槽**、绘制透明矩形并被裁剪的内部表面，平面几何与 `CenterOfRotation`/`LocalOffset`/`GlobalOffset` 都以布局槽为准。

投影动画每帧都会触发 `ProjectionChanged`，因此逐帧路径只做增量更新：`Viewport2DVisual3D.Transform` 复用同一个 `MatrixTransform3D` 并原地替换矩阵，相机只在 `FieldOfView`/`Position` 真正变化时才写依赖属性（重复赋值会让 3D 场景在变换之外再失效一次），矩阵未变时完全跳过。

## 5. 导航子系统

### 5.1 `PhoneApplicationFrame`

继承 WPF `Frame`，保留 WPF 的导航日志、`NavigationService`、`CanGoBack` 与 URI 导航，并增加：

| 成员 | 说明 |
|---|---|
| `Orientation`、`SupportedOrientations` | 依赖属性；`OrientationChanged` 事件；默认 `PortraitUp` / `PortraitOrLandscape`，不随 Frame 尺寸自动变化 |
| `IsPortrait` | 由 `Orientation` 推导 |
| `Navigate(PhoneApplicationPage, IDictionary<string,string>)` | 以页面实例导航并携带参数；参数与 URI 查询串合并进 `NavigationContext` |
| `GoBack()` | 先触发当前页面的 `RaiseBackKeyPress()`，未取消才执行 WPF 返回 |
| `ApplicationBar` | 当前页面的应用栏（只读） |

- 页面生命周期：`Navigating` 调用旧页面 `OnNavigatingFrom`；`Navigated` 调用旧页面 `OnNavigatedFrom` 与新页面 `OnNavigatedTo`；`OnOrientationChanged` 由 `Orientation` 变化触发。
- `Orientation` 与 `ApplicationBar` 的变更回调在元素未加载时直接返回，初始赋值不会提前分发事件或更新 chrome。
- 页面状态按页面实例保存在 Frame 的私有字典中，返回同一实例时恢复。
- 模板部件：`ApplicationBarHost`（按应用栏高度预留的行）与 `ApplicationBar`（`ApplicationBarPresenter`）。

WPF `Page` 没有 WP 的受保护导航虚方法，因此这些方法声明在 `PhoneApplicationPage` 中并由 Frame 触发；把页面放在移植 Frame 之外时 WPF 不会调用它们。

### 5.2 `PhoneApplicationPage`

继承 WPF `Page`：

| 成员 | 说明 |
|---|---|
| `State` | `IDictionary<string, object>`，按页面实例由 Frame 保存/恢复 |
| `NavigationContext` | `Uri`、`ExtraData`、大小写不敏感的 `QueryString` |
| `Orientation`、`SupportedOrientations`、`OrientationChanged` | 方向模型 |
| `ApplicationBar` | `IApplicationBar`；只接受移植版的 `ApplicationBar` 实现 |
| `NavigatedTo`、`NavigatingFrom`、`NavigatedFrom`、`BackKeyPress` | 公共事件，对应受保护虚方法 `OnNavigatedTo`、`OnNavigatingFrom`、`OnNavigatedFrom`、`OnBackKeyPress` |
| `OnOrientationChanged(OrientationChangedEventArgs)` | 受保护虚方法，触发 `OrientationChanged`；页面可重写它处理方向占位元素 |
| `Frame` | 所属的 `PhoneApplicationFrame` |
| `RaiseBackKeyPress()` | 触发 `BackKeyPress` 并返回是否被取消 |

构造函数设置 `KeepAlive = true`，返回导航回到离开时的页面实例；需要新实例时在页面构造函数中设置 `KeepAlive = false`。

### 5.3 应用栏

`Microsoft.Phone.Shell` 提供 `IApplicationBar`、`ApplicationBar`（`[ContentProperty("Buttons")]`）、`ApplicationBarIconButton`、`ApplicationBarMenuItem`、`ApplicationBarMode`、`ApplicationBarStateChangedEventArgs`，以及基础设施 `ApplicationBarItemList<T>`（4 个图标按钮、50 个菜单项上限与原版异常）、`ApplicationBarItemContainer` 家族（文本/图标/启用状态校验与点击派发）。

Windows Phone 由合成器绘制应用栏，移植版由 `ApplicationBarPresenter` 绘制：

| 元素 | 度量 |
|---|---|
| 应用栏高度 | `DefaultSize` 72 DIP，`MiniSize` 30 DIP |
| 图标槽 | 96×72，图标居上、12 px 标签在下 |
| 图标 chrome | 48 DIP 圆形按钮、2 DIP 描边、内部 32 DIP 图标 |
| 菜单 | 右上角三点按钮打开行高 48 px 的 `Popup` |

颜色来自 `BackgroundColor` / `ForegroundColor`，透明度来自 `Opacity`；frame 用呈现器的 `ReservedHeight` 设置 `ApplicationBarHost.Height`，因此隐藏或半透明的应用栏浮在页面之上而不压缩页面。

`SystemTray` 是无视觉 chrome 的静态类，同时提供静态状态属性与 `IsVisibleProperty` 附加属性（`GetIsVisible` / `SetIsVisible`）；附加属性只在宿主是 `PhoneApplicationPage`、`PhoneApplicationFrame`、`Window` 时同步到 `SystemTrayState.Current`。

### 5.4 `TransitionFrame`

继承 `PhoneApplicationFrame`，模板部件是名为 `FirstContentPresenter`、`SecondContentPresenter` 的两个 `ProjectionHost`（来自 `Community.Phone`，模板中需使用 `platform:` 前缀，不是 `ContentPresenter`）。

导航转场顺序：

1. `Navigating`：捕获旧页面，选择 `NavigationOutTransition.Forward/Backward`，取得 `ITransition`，准备旧 Host（关闭命中测试、非羽化转场同时打开 `Prefer3D`），启动退场。
2. `OnContentChanged`：把新页面放入非活动 Host，取 `NavigationInTransition`，并先强制执行一次布局，让羽化目标具备真实几何；退场仍在进行则暂存，否则立即启动进场。
3. 退场完成：停止/恢复旧转场，隐藏旧 Host，启动进场。
4. 进场完成：停止/恢复、恢复交互、触发 `EndTransition`。
5. `NavigationStopped`：恢复旧内容并取消已存的转场。

`TurnstileFeatherTransition` 例外：它只动画页面内部的元素，页面本身不动，因此页面宿主保持 2D 路径。若把页面也放进 3D 路径，整页会被额外光栅化进一个 `Viewport2DVisual3D`，与每个羽化元素各自的 3D 场景嵌套，实测（13 个目标）帧时间从约 12 ms 涨到约 23 ms，p95 从 29 ms 涨到 70 ms 并出现长帧卡顿。

`Transition` 保存/恢复 `CacheMode` 与 `IsHitTestVisible`，并自行跟踪状态，以适配 WPF 比 Silverlight 严格的 storyboard 控制 API：

- 时钟尚未创建时 `Storyboard.GetCurrentState()` / `GetCurrentTime()` / `Stop()` / `Pause()` 会抛异常，因此 `Transition` 自己跟踪是否已启动，未启动时返回 `Stopped` / `Zero`，对从未启动的转场调用 `Stop()` 只恢复元素状态。
- storyboard 以可控时钟启动（`Begin(containingObject, true)`），所有控制调用传入同一宿主对象。
- 可控时钟被停止或移除时 WPF 会再次触发 `Timeline.Completed`，因此每次运行最多触发一次 `Completed`；`Stop()` 会取消待完成事件并 `Storyboard.Remove` 时钟，使被动画的属性回到基础值。
- 动画缓存（`BitmapCache`）按转场实例创建，不共享静态实例：`Freezable` 属于创建它的 dispatcher。

没有动画的转场（例如页面无可羽化元素时的羽化转场）在下一个 dispatcher 周期完成，避免新内容永久不可交互。

### 5.5 转场族

27 个 storyboard XAML（`Transitions/Storyboards/*.xaml`）以 WPF 资源形式内嵌，按原版方式加载：`Application.GetResourceStream("/Community.Phone.Controls.Toolkit;component/Transitions/Storyboards/<Name>.xaml")` → 缓存文本 → `XamlReader.Load`。`Transitions.GetEnumStoryboard` 把 `<系列><模式>` 映射到文件，`Storyboard.SetTarget(storyboard, element)` 绑定到页面，工厂在返回 `Transition` 前设置投影（Slide 用 `RenderTransform`）。这些 XAML 内部的 `xmlns:phone` 指向 `assembly=Community.Phone`，用于解析投影类型。

storyboard 内唯一的改动是属性路径：Silverlight 的 `(UIElement.Projection).(PlaneProjection.RotationY)` 改为 `(phone:ProjectionService.Projection).(phone:PlaneProjection.RotationY)`；角度、关键时间与缓动保持原值。

| 系列 | 模式数 | storyboard 文件 |
|---|---|---|
| `RollTransition` | 1 | 1 |
| `RotateTransition` | 8 | 8 |
| `SlideTransition` | 8 | 8 |
| `SwivelTransition` | 6 | 6 |
| `TurnstileTransition` | 4 | 4 |
| `TurnstileFeatherTransition` | 4 | 0（由 `TurnstileFeatherEffect` 在代码中合成） |

`Transitions.cs` 中有两处仅移植版的常量：`TurnstileCenterOfRotationX = -0.05` 与 `TurnstileDurationScale = 1.2`（以 `Storyboard.SpeedRatio` 施加，原始关键时间不变），用于贴近观察到的 WP 8.1 系统动画；分别改回 `0` / `1` 即还原原工具包的几何与时序。

转场完成或停止时，内部逻辑会清除为转场临时附加的投影，使空闲页面回到纯 2D 路径。

## 6. 效果

### `TiltEffect`

附加属性 `IsTiltEnabled`、`SuppressTilt`，静态属性 `TiltableItems`、`UseLogarithmicEase`。按下时向上查找最近的 tiltable 祖先，缓存 container→control 变换后通过 `ProjectionService` 赋 `PlaneProjection`，由服务负责包装；旋转支点取控件的**第一个可视子元素**（模板根，即控件实际显示内容）的中心，按压点按该边界归一化，与原版的 `GetChild(control, 0)` 一致。

鼠标支持走隧道式 `PreviewMouseDown/Move/Up`，容器可以倾斜子按钮而不吞掉按钮点击；同时支持触摸 manipulation。返回动画以投影为目标并跟踪 storyboard 是否已应用；指针离开可见内容时暂停倾斜，交互结束清理全部逐次状态。

### `TurnstileFeatherEffect`

附加属性 `FeatheringIndex`，静态属性 `NonPermittedTypes` 与交错时间常数。合成 storyboard 时目标从页面的**可视树**收集（`FeatheringIndex` 稳定排序），因为进场页面在接入可视树之前就被合成。附加属性订阅只负责准备包装，且延迟到 `Loaded`/`SizeChanged` 之后的 dispatcher（布局过程中包装会引发布局循环）。

`TurnstileFeatherTransition` 开始时为屏幕内的目标确保 `ProjectionHost`，先打开 `Prefer3D` 再赋 `PlaneProjection`（`CenterOfRotationX = -0.2`），然后按 40–50 ms 交错组合 `RotationY` 与透明度动画。清理时恢复透明度、投影、渲染变换与 2D 路径，并对元素与包装器 `InvalidateVisual()`（走过 3D 路径的包装使用独立表面合成，不重绘会残留最后一帧）。包装保留到元素卸载，避免一次转场中反复重新父级化。没有 `ProjectionHost` 模板部件的 item 容器跳过。

### `SlideInEffect`

附加属性 `LineIndex`、`ParentPivot`、`ParentPivotItem`、`AttachedTransform`、`IsSubscribed`、`HasEventsAttached`。订阅宿主 Pivot 的选择变化，按行索引为 `PivotItem` 内的元素施加交错位移与透明度动画。实现位于 `Effects/SlideInEffect.cs`，有独立示例页。

## 7. 控件

控件使用 WPF 依赖属性与模板；`DefaultStyleKeyProperty.OverrideMetadata` 只在 `Community.Phone` 的少数类型（`PhoneApplicationFrame`、`ApplicationBarIconButtonView`、`ApplicationBarMenuItemView`、`ApplicationBarPresenter`）静态构造函数中调用，toolkit 控件普遍在实例构造函数中赋值 `DefaultStyleKey`。

| 控件 | 实现要点 |
|---|---|
| `WrapPanel` | 原始布局算法与 `ItemHeight`/`ItemWidth`/`Orientation` 语义 |
| `ToggleSwitch` / `ToggleSwitchButton` | 视觉状态契约、开关模板、manipulation |
| `Rating` / `RatingItem` | 代码生成星形项目、裁剪蒙版、拖动、方向、半星与零值选择；`AllowHalfItemIncrement` 时 `Value` 仍以“星”为单位 |
| `PhoneTextBox` | 内部 `TextBox` 使用双向 `Text` 绑定（`TemplateBinding` 是单向的）、hint、action icon、长度指示；占位符与光标起点一致，聚焦或输入法合成期间隐藏 |
| `ExpanderView` | 基于 `HeaderedItemsControl`，部件 `Presenter`/`ExpanderPanel`/`ItemsCanvas`，高度动画在代码中完成 |
| `HubTile` / `HubTileService` | 定时器驱动 `Expanded → Semiexpanded → Collapsed → Expanded → Flipped`；两倍瓦片高的表面用 `TranslateTransform` 滑到 `-H` / `-0.4566H` / `0`，翻转用 `PlaneProjection.RotationX`（`0→180`、`180→360`）并在表面侧立时换面；`GroupTag` 支持组冻结 |
| `ListPicker` / `ListPickerPage` | 普通模式对模板 Canvas 做高度/位移动画；完整模式通过宿主 frame 导航到 `ListPickerPage`（`PickerPageUri` 的 pack URI），页面用原版 `ApplicationBar` 确认/取消（图标来自 `Content/ApplicationBar.*.png`），项目逐个 `RotationX` 翻转展开；页面自带主题字典，等待列表就绪后再组合动画 |
| `ContextMenu` / `MenuItem` / `Separator` / `ContextMenuService` | 原版模板与视觉状态；自绘 `Popup`，`ContextMenuService` 只负责绑定所有者；右键与模拟长按打开；无横竖屏分支，遮罩缩放/淡入由代码动画驱动 |
| `CustomMessageBox` | `ContentControl` API，`Show()`/`Dismiss()` 由 WPF `Window` 承载，内容在 `ProjectionHost` 中播放 Swivel |
| `DatePicker` / `TimePicker` / `RecurringDaysPicker` | 保留 `Value`/`ValueString`/`Header`/`PickerPageUri` API；点击时通过宿主 `PhoneApplicationFrame` 导航到 `DatePickerPage` / `TimePickerPage`（主题中 `PickerPageUri` 的 pack URI），picker 页面使用 `LoopingSelector` 求值；`RecurringDaysPicker` 继承 `ListPicker` |
| `PerformanceProgressBar` | 内嵌 WPF `ProgressBar` 与 `ActualIsIndeterminate`，透明度淡入淡出 |
| `Pivot` / `PivotItem` | 容器在 `Grid` items panel 中堆叠，仅当前项可见；切换时进入项从 `±ActualWidth` 动画到 `0`（`QuarticEase`，0.25 s），离开项滑出后折叠；指针拖拽以 600 px/s 移动，超过半个视口切换；`IsLocked` 淡出页眉并拒绝修改项 |
| `PivotHeadersControl` / `PivotHeaderItem` | 页眉条在 `Canvas` 上循环排布（`VisualFirstIndex` 起，最后一个之后接第一个），0.5 s 滚动画布 |
| `Panorama` / `PanoramaItem` | 在 `Community.Phone` 中实现；`Title`/`TitleTemplate`/`HeaderTemplate`/`SelectedItem`/`SelectedIndex`/`DefaultItem`，用 WPF 指针事件与 storyboard 复刻WP 的吸附位置、时长与视差；主题字典 `Themes/Controls/Panorama.xaml` |
| `LongListSelector` | 内部 `ItemsControl` + `ContentPresenter` 容器；列表模式用 `VirtualizingStackPanel` 回收虚拟化，网格模式用固定单元格非虚拟化面板；跳转列表为 `Popup`；模板部件 `ViewportControl`（`ContentControl`）与 `VerticalScrollBar`（真实 `ScrollBar`） |
| `LongListMultiSelector` / `LongListMultiSelectorItem` | 在 `LongListSelector` 基础上增加 `SelectedItems`、`IsSelectionEnabled`、`EnforceIsSelectionEnabled`、`ItemInfoTemplate` 与 `IsSelectedChanged` 事件；有完整主题模板与示例页 |
| `MultiselectList` / `MultiselectItem` | 类型与选择逻辑已移植（`IsSelectionEnabled`、`ItemInfoTemplate`、`IsSelected`/`State`/`ContentInfo` 等），但 `Themes/` 下没有默认模板，示例入口只给出 WP8 弃用说明 |
| `AutoCompleteBox` | `Control` 派生，模板部件 `Text`/`Popup`/`Selector`；`MinimumPrefixLength`、`MinimumPopulateDelay`、`IsTextCompletionEnabled`、`FilterMode`、`ItemFilter`/`TextFilter`、`ValueMemberPath`、`MaxDropDownHeight`、`IsDropDownOpen`、`SelectedItem`、`Text`、`SearchText` 等 API；`Populating`/`Populated`/`DropDownOpening`/`DropDownOpened`/`DropDownClosing`/`DropDownClosed`/`SelectionChanged` 事件；`PopupHelper` 处理 WPF 与 WP 的 popup 布局差异；有主题模板与示例页 |
| `LoopingSelector` / `LoopingSelectorItem` / `ILoopingSelectorDataSource` | 在 `Primitives` 中实现，`DataSource`/`ItemTemplate`/`IsExpanded`；被日期/时间 picker 页面使用；有主题模板 |
| `HyperlinkButton` | 相对/pack URI 导航最近的 `NavigationService`/`Frame`，其他绝对 URI 通过可重写的 `OpenExternalUri`（默认 `Process.Start` + `UseShellExecute`）；`IInvokeProvider` 自动化对等体 |
| `ClipToBounds` | `IsEnabled` 附加属性，使用 WPF `Clip` |

## 8. 主题

两个程序集各自提供 `Themes/Generic.xaml`：

- `Community.Phone/Themes/Generic.xaml` 合并 `Theme.xaml`（唯一调色板字典）与 `Controls/{ApplicationBar,Button,ListBox,LongListSelector,Panorama,PhoneApplicationFrame,Pivot,ScrollBar,ScrollViewer,TextBlock,TextBox}.xaml`。
- `Community.Phone.Controls.Toolkit/Themes/Generic.xaml` 合并 `Community.Phone` 的 `Generic.xaml` 与自身 `Controls/*.xaml`（`AutoCompleteBox`、`Button`、`ComboBox`、`ContextMenu`、`CustomMessageBox`、`DateTimePicker`、`ExpanderView`、`HubTile`、`ListPicker`、`LongListMultiSelector`、`LoopingSelector`、`PhoneTextBox`、`Rating`、`ToggleSwitch`、`TransitionFrame`）。

主题覆盖 toolkit 控件，并为常用 WPF 控件提供隐式 WP 样式：`Button`、`ButtonBase`、`TextBox`、`ListBox`/`ListBoxItem`、`ComboBox`/`ComboBoxItem`、`CheckBox`、`RadioButton`、`ScrollBar`、`Thumb`、`ToggleButton`、`ProgressBar`、`ContextMenu`/`MenuItem`/`Separator`、`ToolTip`、`TextBlock` 与 `ContentControl`。`ListBoxItem` 模板内含名为 `ProjectionHost` 的部件，供投影服务直接复用（见 §4.5）。

三条 WPF 规则决定写法：

1. 库内 XAML 引用本程序集类型时，`clr-namespace:` **不写** `assembly=`：`MarkupCompilePass1` 只从编译源码认识本项目类型，写程序集名会让干净构建报 `MC3074`。
2. 主题字典里的 `StaticResource` 在**所属字典**（或应用程序）中解析，因此跨字典引用会失败；动画关键帧的取值使用字面量或内联画刷。
3. 运行时可切换主题要求消费方使用 `DynamicResource` 引用主题画刷（库内模板已经如此）。

库自己创建的页面（`ListPickerPage`、`DatePickerPage`、`TimePickerPage`）在挂到 frame 之前就解析样式，看不到 `Application.Resources`，因此它们在自己的 `Resources` 中合并 `Theme.xaml`，并在“尚未挂树”时求值的属性上使用 `DynamicResource`。日期/时间 picker 页面的 `VisibilityStates` 挂在 `LayoutRoot` 上，由 `VisualStateManager.GoToElementState(LayoutRoot, ...)` 播放；`DatePickerPage` 的投影也赋在 `LayoutRoot` 上。

调色板、映射表与切换机制见[主题系统](theming.md)。

## 9. 打包

单包工程 `src/Community.Phone.Controls.Toolkit/Community.Phone.Controls.Toolkit.csproj`：

- `PackageId` 为 `Community.Phone.Controls.Toolkit`，`PackageLicenseExpression` 为 `MS-PL`，`PackageReadmeFile` 为 `README.md`；启用 `GenerateDocumentationFile` 与符号包（`.snupkg`），输出到 `artifacts/packages`。
- toolkit 以 `PrivateAssets="all"` 引用 `Community.Phone`，并通过 `IncludeCompatibilityLayerInPackage` 目标把 `Community.Phone.dll`（及 pdb/xml）作为 `BuildOutputInPackage` 放入包中，因此不产生包依赖。
- `Transitions/Storyboards/*.xaml` 与 `Content/*.png` 作为 `Resource` 打包；本地化资源生成卫星程序集。

`dotnet build Community.Phone.Controls.Toolkit.slnx -c Release` 通过。当前 `csproj` 的打包项引用了仓库中不存在的 `THIRD-PARTY-NOTICES_zh-CN.md`，`docs/en/*.md` 与 `docs/zh-CN/*.md` 也不存在，`dotnet pack` 报 `NU5019`。`build/verify-package.ps1` 依赖的 `tests/PackageConsumer` 工程同样不存在。构建与打包事实见[移植状态](porting-status.md#构建与验证)。
