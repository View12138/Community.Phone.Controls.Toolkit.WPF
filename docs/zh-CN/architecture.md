# 架构设计

## 1. 目标与约束

本项目把归档的 Windows Phone Toolkit 移植到现代 WPF，并在 WPF 允许的范围内尽可能保留原始公共 API。影响设计的关键约束如下：

- WPF 的 `UIElement` 没有 `Projection` 属性，也不能给 `Grid` 等已存在的框架类型追加 CLR/XAML 属性。
- WPF 自带 `Frame`、`Page`、`ContextMenu`、`WrapPanel`、`DatePicker`、`HeaderedItemsControl` 和 `Application` 等类型。若在 `System.Windows.*` 中定义同名类型，会造成歧义，也违反“不重新定义 WPF 框架类型”的约束。
- Windows Phone 的 `PhoneApplicationFrame`、`PhoneApplicationService`、`SystemTray`、地图、后台传输、语音和 Pivot 等服务在 WPF 中不存在。
- 原始 XAML 使用 Silverlight 资源键和动画路径，例如 `(UIElement.Projection).(PlaneProjection.RotationY)`。

因此移植版采用**原始 `Microsoft.Phone.Controls` / `Microsoft.Phone.Shell` 命名空间中的兼容类型**，对真实 3D 使用显式的 `ProjectionHost` 包装，并对框架相邻控件使用 WPF 原生基类。

## 2. 仓库与工程

```text
Community.Phone.Controls.Toolkit.slnx
├─ src/Community.Phone.Controls.Toolkit.WPF/          （单一可分发程序集）
│  ├─ Projection/    Projection、PlaneProjection、Matrix3DProjection、ProjectionHost、ProjectionService
│  ├─ Navigation/    PhoneApplicationFrame、PhoneApplicationPage、NavigationContext、方向类型
│  ├─ Shell/         PhoneApplicationService、SystemTray、ApplicationBar
│  ├─ Transitions/   ITransition/Transition、TransitionFrame、TransitionService、各转场系列
│  ├─ Effects/       TiltEffect、TurnstileFeatherEffect
│  ├─ Controls/      WrapPanel、ToggleSwitch、Rating、PhoneTextBox、ExpanderView、HubTile、
│  │                 ListPicker、ContextMenu、CustomMessageBox、DatePicker/TimePicker、
│  │                 PerformanceProgressBar、转换器、辅助类
│  ├─ LocalizedResources/  51 个从原始本地化工程复制的 .resx
│  └─ Themes/Generic.xaml  Metro 调色板、字体、Frame 模板、控件模板
├─ src/Community.Phone.Controls.Toolkit.WPF.Sample/   可运行 WPF 示例
└─ tests/Community.Phone.Controls.Toolkit.WPF.Tests/  xUnit 测试（STA 辅助）
```

包 ID 为 `Community.Phone.Controls.Toolkit.WPF`。包中只包含一个主程序集及其卫星资源、主题字典、XML 文档、README、许可证和文档，不需要额外安装内部包。

## 3. 命名空间与 API 策略

| 原始命名空间 | 移植命名空间 | 原因 |
|---|---|---|
| `Microsoft.Phone.Controls` | `Microsoft.Phone.Controls` | 保留 toolkit API 名称。 |
| `Microsoft.Phone.Controls.Primitives` | `Microsoft.Phone.Controls.Primitives` | 保留 primitive API 名称。 |
| `Microsoft.Phone.Controls.LocalizedResources` | 同名 | 保留资源类。 |
| `Microsoft.Phone.Shell` | `Microsoft.Phone.Shell` | 保留系统栏/应用服务 API。 |
| `Microsoft.Phone.Maps.Toolkit` | 未实现 | 没有 WPF 地图提供程序。 |
| `System.Windows.Controls`（toolkit 自带的 `WrapPanel`、`LengthConverter`、`TimeTypeConverter`） | `Microsoft.Phone.Controls` 或 WPF 原生 | 避免重新定义 WPF 类型。`WrapPanel` 放在 `Microsoft.Phone.Controls`，WPF 原生 `WrapPanel` 保持不变。 |
| `System.Windows`（toolkit 的 `HierarchicalDataTemplate`） | WPF 原生 | 使用 `System.Windows.HierarchicalDataTemplate`。 |

WPF 有 `System.Windows.Controls.ContextMenu`；移植版的 `Microsoft.Phone.Controls.ContextMenu` 继承它。XAML 用户必须使用 `phone:` 前缀选择移植类型，这属于**需要迁移**，不能声称 XAML 原样兼容。

## 4. 投影子系统

### 4.1 公共模型

- `Projection : Animatable`：基类，提供 `ProjectionChanged`、`IsIdentity`、`ToMatrix3D(Size)`。
- `PlaneProjection : Projection`：依赖属性 `CenterOfRotationX/Y/Z`、`GlobalOffsetX/Y/Z`、`LocalOffsetX/Y/Z`、`RotationX/Y/Z`。
- `Matrix3DProjection : Projection`：`ProjectionMatrix` 使用 0..1 归一化坐标。
- `ProjectionHost : FrameworkElement`：承载一个 `Child`；投影为非 identity 时通过 `Viewport3D` / `Viewport2DVisual3D` 渲染，identity 时走普通 2D 视觉树。
- `ProjectionService`：附加 `Projection` 属性，以及 `EnsureProjectionHost` / `ReleaseProjectionHost` 辅助方法。设置在元素上的附加属性会作用于最近的 `ProjectionHost`；设置在 Host 自身上则直接生效。

### 4.2 渲染方案选择

评估了三条路线：

1. **WPF `Viewport3D` + `Viewport2DVisual3D`**：最终采用。它把实时 2D 视觉树映射到 3D 平面，可通过 `SetIsVisualHostMaterial` 保留命中测试，支持文本、输入和弹出层，并由 GPU 合成。
2. **截图/VisualBrush 平面**：被拒绝作为默认实现。它会丢失交互，只是静态快照，不符合“不得用截图替代交互控件树”的要求。
3. **Windows Composition/D3D 互操作**：因可移植性和打包复杂度被拒绝。需要 Windows App SDK/Composition 依赖和独立宿主策略；WPF 3D 已满足所需的交互和透视。

### 4.3 几何与透视

Host 创建一个以元素原点为中心的矩形网格：

```text
positions (Y up):  (-w/2, +h/2, 0), (+w/2, +h/2, 0), (+w/2, -h/2, 0), (-w/2, -h/2, 0)
texture coords:    (0,0), (1,0), (1,1), (0,1)
triangles:         0,2,1 and 0,3,2
```

`PerspectiveCamera` 位于 `(0, 0, distance)`，`distance = viewportHeight / (2 * tan(fov/2))`，默认 `fov = 57.6°`。这样 identity 投影在中心平面上近似 1:1，且透视随 Host 尺寸成比例。

`PlaneProjection.ToMatrix3D` 使用行向量约定：

```text
p' = (((p - center) + localOffset) * R) + center + globalOffset
R = Rz * Ry * Rx    （角度制）
center = ((CenterOfRotationX - 0.5) * width, (0.5 - CenterOfRotationY) * height, CenterOfRotationZ)
Y 偏移取反，因为 WPF 3D 使用 +Y 向上，而屏幕坐标使用 +Y 向下。
```

矩阵赋给 `Viewport2DVisual3D.Transform`。裁剪、背面和命中测试由 WPF 3D 管线处理。Host 在尺寸或投影变化时重新计算几何、相机距离和变换。

### 4.4 XAML 兼容边界

WPF 无法让 `<Grid.Projection>` 编译，因为 `Grid` 没有该属性。支持的形式是：

```xml
<!-- 显式 Host；保留实时交互和真实投影。 -->
<phone:ProjectionHost>
    <Grid>...</Grid>
    <phone:ProjectionHost.Projection>
        <phone:PlaneProjection RotationY="-35" />
    </phone:ProjectionHost.Projection>
</phone:ProjectionHost>

<!-- Host 上的附加属性；支持 storyboard 属性路径。 -->
<phone:ProjectionHost
    phone:ProjectionService.Projection="{StaticResource MyProjection}">
    <Grid>...</Grid>
</phone:ProjectionHost>
```

原始 `<Grid.Projection>` 形式**不兼容源码**，迁移时必须使用包装。该差异记录在兼容矩阵和已知限制中。

### 4.5 效果集成

`ProjectionService.EnsureProjectionHost` 可把 Panel 的直接子元素临时包进 `ProjectionHost`，并复制布局属性（尺寸、Margin、对齐、Grid 行列、ZIndex、Dock、Canvas 位置）。`TiltEffect` 和 `TurnstileFeatherEffect` 使用该能力。`ListBoxItem` 和 `ContentPresenter` 会被有意跳过，因为替换 item 容器会破坏生成和虚拟化。效果结束后释放包装。

## 5. 导航子系统

### 5.1 `PhoneApplicationFrame`

继承 WPF `Frame`，保留 WPF 的导航日志、`NavigationService`、`CanGoBack`、URI 导航等能力，并增加：

- `Orientation` / `SupportedOrientations` 依赖属性。
- `OrientationChanged` 事件；方向根据 Frame 尺寸推断（宽 > 高为横向）。
- 页面生命周期分发：`Navigating` 调用当前页面的 `OnNavigatingFrom`；`Navigated` 调用旧页面的 `OnNavigatedFrom` 和新页面的 `OnNavigatedTo`。
- `GoBack()` 先触发 `PhoneApplicationPage.BackKeyPress`。
- `Navigate(PhoneApplicationPage, IDictionary<string,string>)` 传递导航参数，与 URI 查询字符串合并为 `NavigationContext`，在 `OnNavigatedTo` 前赋给页面。
- 页面状态按页面实例保存在 Frame 中，返回到同一实例后仍然存在。

WPF `Page` 没有 WP 的 `OnNavigatedTo/OnNavigatedFrom/OnNavigatingFrom` 受保护虚方法，因此移植版在 `PhoneApplicationPage` 中声明这些虚方法，并由 Frame 触发。这样保留了 API 和确定顺序，但如果页面放在移植 Frame 之外，WPF 自身不会调用它们。

### 5.2 `PhoneApplicationPage`

继承 WPF `Page`，增加 `State`、`NavigationContext`、`Orientation`、`SupportedOrientations`、`ApplicationBar`、`NavigatedTo`、`NavigatingFrom`、`NavigatedFrom`、`BackKeyPress`、`OrientationChanged`。`NavigationContext` 暴露 `Uri`、`ExtraData` 和大小写不敏感的 `QueryString` 字典。

### 5.3 `TransitionFrame`

继承 `PhoneApplicationFrame`。模板有两个 `ProjectionHost` 部件，名称为 `FirstContentPresenter` 和 `SecondContentPresenter`。部件类型从 `ContentPresenter` 改为 `ProjectionHost`，以便对页面做 3D 投影。

导航转场顺序：

1. `Navigating`：捕获旧页面，选择 `NavigationOutTransition.Forward/Backward`，获取 `ITransition`，准备旧 Host，启动退出转场。
2. `OnContentChanged`：把新页面放入非活动 Host，获取 `NavigationInTransition`；如果退出转场仍在进行则暂存，否则立即启动进入转场。
3. 退出完成：停止/恢复旧转场，隐藏旧 Host，然后启动进入转场。
4. 进入完成：停止/恢复，恢复交互，触发 `EndTransition`。
5. `NavigationStopped`：恢复旧内容并取消已存转场。

`Transition` 保存/恢复 `CacheMode` 和 `IsHitTestVisible`；`ProjectionTransition` 还保存/恢复附加投影。WPF 的 `Storyboard.GetCurrentState()` 需要可控时钟的宿主对象，因此 `Transition` 自己跟踪 begun/completed 状态，运行时返回 `ClockState.Active`。`Completed` 每次运行最多触发一次；`Stop()` 会取消待完成事件，避免 `EndTransition` 重复触发。动画本身是带显式目标的普通 WPF `Storyboard`，测试通过观察实际 `PlaneProjection.RotationY` 验证。

`TransitionFrame` 还会：

- 防止重复的 `Navigating` / content-change 通知；
- 快速导航时清理已存转场；
- 在转场工厂或 `Begin` 失败时触发 `TransitionFailed`（包含异常、元素和退出/进入方向），回退到无转场导航，并恢复命中测试/Cache 状态，避免页面被隐藏或无法交互。

### 5.4 转场系列

原始 19 个 XAML storyboard 在 `Transitions.cs` 中用 `DoubleAnimation` / `DoubleAnimationUsingKeyFrames` 重新表达，时间、角度和缓动与原实现一致。Storyboard 直接以 `PlaneProjection` 对象为目标，避免 Silverlight 属性路径。

系列包括 Roll、Rotate（8 模式）、Slide（8 模式）、Swivel（6 模式）、Turnstile（4 模式）、TurnstileFeather（4 模式）。`TransitionService` 和 `NavigationTransition` / `NavigationInTransition` / `NavigationOutTransition` 保留原始附加属性形态。

## 6. 效果

### `TiltEffect`

保留 `IsTiltEnabled`、`SuppressTilt`、`TiltableItems`、`UseLogarithmicEase`。按下时向上查找最近的 tiltable 祖先，确保存在 `ProjectionHost`，创建 `PlaneProjection`，并应用原始归一化点 → RotationX/RotationY/GlobalOffsetZ 计算。鼠标支持通过隧道式 `PreviewMouseDown/Move/Up` 实现，使容器可以倾斜子按钮而不会吞掉按钮点击；同时支持触摸 manipulation。返回动画直接以投影为目标，并跟踪 Storyboard 是否已应用，避免 WPF `Storyboard` 状态异常，同时清理每次交互的状态。普通 WPF 控件按需包装；列表 item 容器不包装。

### `TurnstileFeatherEffect`

保留 `FeatheringIndex`、`NonPermittedTypes` 和原有交错时间常数。目标按 `PhoneApplicationPage` 注册。`TurnstileFeatherTransition` 开始时，为每个在屏幕内的目标确保 `ProjectionHost`，赋予中心 `CenterOfRotationX = -0.2` 的 `PlaneProjection`，并按原 40–50 ms 交错组合 RotationY/透明度动画。无法包装的目标（item 容器、content presenter）会跳过。清理时恢复透明度、移除投影并释放包装。

`SlideInEffect` 未实现，因为它依赖 WP `Pivot`/`PivotItem` 及其选择/manipulation 生命周期。文档中明确标记为不支持。

## 7. 控件与主题

已实现控件使用 WPF 依赖属性、模板和 `VisualStateManager`。`DefaultStyleKeyProperty.OverrideMetadata` 在**静态构造函数**中调用，确保每个类型只注册一次元数据。`Themes/Generic.xaml` 提供按类型键控的隐式样式；示例和测试还显式合并字典，使应用 XAML 可以访问命名 Metro 资源（`PhoneTextTitle1Style` 等）。

控件设计决策：

- **WrapPanel**：原始布局算法和 `ItemHeight/ItemWidth/Orientation` 语义。
- **ToggleSwitch / ToggleSwitchButton**：保留视觉状态契约和开关模板；保留 manipulation 与模板部件。
- **Rating / RatingItem**：保留星形项目生成、裁剪蒙版、拖动辅助、方向和半星/零值选择。
- **PhoneTextBox**：内部 TextBox 使用双向 `Text` 绑定（WPF `TemplateBinding` 是单向的），保留 hint、action icon 和长度指示。
- **ExpanderView**：基于 WPF `HeaderedItemsControl`；部件 `Presenter`、`ExpanderPanel`、`ItemsCanvas`；高度动画在代码中完成，因为 WPF 无法直接取得 Silverlight 模板部件。
- **HubTile**：定时器驱动 `Expanded → Semiexpanded → Collapsed → Expanded → Flipped` 状态循环；`HubTileService` 支持冻结组。
- **ListPicker**：保留原依赖属性面；普通模式使用 Popup，完整模式使用模态 Window；`PickerPageUri` 保留但不导航。
- **ContextMenu/MenuItem/Separator/ContextMenuService**：继承 WPF 菜单类型；右键打开。
- **CustomMessageBox**：`ContentControl` API，`Show()`/`Dismiss()` 由 WPF Window 承载。
- **DatePicker/TimePicker**：保留 Value/ValueString/Header/PickerPageUri API，用 WPF 日期/时间 UI 在模态 Window 中实现。
- **PerformanceProgressBar**：内嵌 WPF `ProgressBar` 和 `ActualIsIndeterminate`；简单透明度淡入淡出。
- **转换器/本地化**：保留原始格式化规则和 51 个 resx 文件（`qps-ploc` 伪语言不打包）。

## 8. 打包

单包工程。包含 `GenerateDocumentationFile`、包元数据（`PackageId`、描述、标签、MS-PL 许可证表达式）、README/LICENSE 打包、符号和本地 `artifacts/packages` 输出。包内含 `Community.Phone.Controls.Toolkit.WPF.dll`、卫星资源程序集、`Themes/Generic.xaml`（编译为 BAML）、XML 文档、README、LICENSE、第三方声明和中英文文档。

使用全新 WPF 消费工程做安装测试：从 `artifacts/packages` 添加包、构建、加载主题资源、实例化 `ProjectionHost` 和示例控件并运行。`build/verify-package.ps1` 使用隔离的本地源和 NuGet 缓存执行该流程，消费工程只有在加载主题和控件成功后才写入成功标记。

## 9. 测试策略

- **投影数学**：默认值、DP 往返、identity 矩阵、三轴旋转、旋转中心、local/global offset、元素尺寸缩放、归一化 `Matrix3DProjection`、动画驱动的变换变化。
- **投影渲染**：`RenderTargetBitmap` 验证 2D/3D 模式有可见像素、上下纹理方向正确、多个 Host 独立渲染、高 DPI 渲染，以及投影内容仍可获得键盘焦点。
- **命中测试**：`VisualTreeHelper.HitTest` 到达投影内容/3D 视觉。
- **控件**：主题资源加载、模板应用、DP 默认值/往返、项目生成、布局、选择、双向文本绑定、action-icon 事件、长度指示、Popup 选择/关闭、消息框生命周期、hint/长度、Expander 事件、日期时间格式化、HubTile 组冻结、RecurringDays 汇总、ContextMenu 打开/关闭。
- **效果**：TiltEffect 从子按钮启动真实投影、尊重 `SuppressTilt` 并恢复状态；TurnstileFeatherEffect 对屏幕内元素做 feather、创建/清理投影，并拒绝页面目标。
- **导航/转场**：前进/后退、导航参数、状态保持、返回键取消、快速导航、进入/退出编排及 begin/end 事件、转场失败回退、方向检测、Turnstile 状态和实际 `RotationY` 动画、系统栏/应用服务状态。
- **转换器**：On/Off、TileSize 尺寸、RelativeTime 分钟/小时格式。
- **示例运行**：示例作为真实 WPF 进程启动；启动错误写入 `%TEMP%\community-phone-toolkit-sample-error.txt`，并检查窗口稳定。
- **包消费**：`build/verify-package.ps1` 打包、隔离还原、零警告构建并运行全新消费工程。

需要窗口或 WPF Dispatcher 的测试通过 `StaTest.Run` 在专用 STA 线程上运行；动画通过真实的 dispatcher 消息泵推进，而不是只断言静态属性。
