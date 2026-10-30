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
PhoneToolkit.WPF.slnx
├─ src/Community.Phone.Controls.Toolkit.WPF/          （单一可分发程序集）
│  ├─ Projection/    Projection、PlaneProjection、Matrix3DProjection、ProjectionHost、ProjectionService
│  ├─ Navigation/    PhoneApplicationFrame、PhoneApplicationPage、NavigationContext、方向类型
│  ├─ Shell/         PhoneApplicationService、SystemTray、ApplicationBar
│  ├─ Transitions/   ITransition/Transition、TransitionFrame、TransitionService、各转场系列
│  ├─ Effects/       TiltEffect、TurnstileFeatherEffect
│  ├─ Controls/      WrapPanel、ToggleSwitch、Rating、PhoneTextBox、ExpanderView、HubTile、
│  │                 ListPicker、Pivot（含 PivotHeadersControl/PivotHeaderItem）、ContextMenu、
│  │                 CustomMessageBox、DatePicker/TimePicker、PerformanceProgressBar、转换器、辅助类
│  ├─ LocalizedResources/  51 个从原始本地化工程复制的 .resx
│  └─ Themes/Generic.xaml  Metro 调色板、字体、Frame 模板、控件模板
├─ src/Community.Phone.Controls.Toolkit.WPF.Sample/   可运行 WPF 示例
└─ tests/Community.Phone.Controls.Toolkit.WPF.Tests/  xUnit 测试（STA 辅助）
```

包 ID 为 `Community.Phone.Controls.Toolkit.WPF`。包中只包含一个主程序集及其卫星资源、主题字典、XML 文档、README、许可证和文档，不需要额外安装内部包。

库保留了原工具包源码，这些文件通过 `#if WINDOWS_PHONE` 选择兼容命名空间（`Microsoft.Phone.Controls`）和本地化资源，因此 `WINDOWS_PHONE;WP8` 在**所有**配置下都必须定义；只有 Release 不定义时，`LengthConverter`、`TypeConverters`、`RoutedPropertyChangingEventArgs` 和 `InteractionHelper` 会编译失败。

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
triangles:         0,2,1 and 0,3,2（正面）；0,1,2 and 0,2,3（反向绕序）
```

`PerspectiveCamera` 位于 `(0, 0, depth)`，`depth = PerspectiveDepth`，默认 `999` DIP。视场角由视口**宽度**推导：`fov = 2 * atan(viewportWidth / (2 * depth))`。WPF 的 `PerspectiveCamera.FieldOfView` 是水平视场角，垂直视场角由视口宽高比推导，因此按宽度推导可以保证 identity 投影与布局槽位严格 1:1。若按高度推导，每个 3D 转场中的元素都会按宽高比缩小（480x800 的页面被渲染成 288x480），转场结束时再跳回全尺寸。

999 DIP 的深度是 XAML `PlaneProjection` 的固定透视（WinUI 的 `PlaneProjection` 以元素中心为基准使用 `M34 = -1/999`），因此深度 `z` 处一点的实际缩放恒为 `999 / (999 - z)`，与元素尺寸无关。这比“随尺寸成比例的相机近似”更接近 WP 的真实透视。

`PlaneProjection.ToMatrix3D` 使用行向量约定：

```text
p' = (((p - center) + localOffset) * R) + center + globalOffset
R = Rz * Ry * Rx    （角度制）
center = ((CenterOfRotationX - 0.5) * width, (0.5 - CenterOfRotationY) * height, CenterOfRotationZ)
Y 偏移取反，因为 WPF 3D 使用 +Y 向上，而屏幕坐标使用 +Y 向下。
RotationY 取反，因为 XAML 中正的 RotationY 会让右边缘靠近观察者，
而在 WPF 的 +Y 轴上正向旋转会让右边缘远离观察者。
```

矩阵赋给 `Viewport2DVisual3D.Transform`。裁剪与命中测试由 WPF 3D 管线处理。WPF 会剔除背面，而 XAML 投影从两侧都可见，所以四边形同时带上反向绕序的三角形：超过 90° 的旋转（例如 `HubTile` 翻转的后半段）仍然渲染镜像内容，而不是消失（`ProjectionHost_RendersContentPastNinetyDegrees`）。Host 在尺寸或投影变化时重新计算几何、相机视场角和变换。

投影结果**不会**被裁剪到 Host 的布局槽。XAML 投影旋转的是元素的栅格，并把结果原样画出，所以旋转元素被透视放大的近端会画到自身边界之外 —— `HubTile` 翻转会压到相邻瓦片，Turnstile 页面可以摆出 frame 之外。因此 3D 路径下 `Viewport3D.ClipToBounds` 为 `false`，Host 只在 2D 渲染时给自己的渲染加裁剪（`ClipToBounds`）。被裁剪的是**内容**：surface 带一个布局槽大小的 `RectangleGeometry` 裁剪，这复现了平台“把内容栅格化进元素边界”的规则 —— `HubTile` 的表面有两倍瓦片高、视口只有一倍高，滑出视口的那条带子因此被切掉 —— 同时让子元素溢出布局槽时贴图映射仍保持一一对应。`ProjectionHost_DoesNotClipTheProjectedResult` 与 `ProjectionHost_ClipsItsContentToTheLayoutSlot` 分别覆盖这两半。

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

<!-- 任意元素上的附加属性；元素会按需被包装。 -->
<Grid phone:ProjectionService.Projection="{StaticResource MyProjection}">...</Grid>
```

`ProjectionService` 把投影应用到**直接包含**该元素的宿主；若元素尚未被宿主包装，则按 `EnsureProjectionHost` 的规则（Panel 的直接子元素，且该 Panel 不是 items panel；item 容器跳过）按需创建宿主。这样创建的宿主会在投影被清除时自动释放，因此“赋投影 → 清除投影”不会改变最终的可视树。代码中最短的形式是 `element.Projection = new PlaneProjection { RotationY = -35 };`；storyboard 路径仍然是 `(ProjectionService.Projection).(PlaneProjection.RotationY)`。原始 `<Grid.Projection>` 形式依然**不兼容源码**，需用上面任一形式替代。

### 4.5 效果集成

`ProjectionService.EnsureProjectionHost` 可把 Panel 的直接子元素包进 `ProjectionHost`，并复制布局属性（尺寸、Margin、对齐、Grid 行列、ZIndex、Dock、Canvas 位置）。只有**直接包含该元素**的宿主才会被复用：页面本身由转场 Frame 的 presenter 承载，复用祖先宿主会把整页一起投影而不是只投影该元素。给元素赋投影（`ProjectionService.SetProjection`、`element.Projection` 或 XAML 附加属性）遵循同一规则：元素尚未被宿主包装时按需包装，并在投影被清除时释放这个按需创建的宿主。`TiltEffect` 在控件加载时完成包装（倾斜关闭时解包），因此指针按下期间不会修改视觉树；`TurnstileFeatherEffect` 在被羽化元素订阅时完成包装（清空 feathering index 时解包），因此转场动画期间不会重新挂载内容。item 容器（`ListBoxItem`、`ComboBoxItem`、自定义容器）会被跳过，因为 `ItemsControl` 的 items panel 子元素由 WPF 生成，直接修改会抛出 `InvalidOperationException`。`ContentPresenter` 与其他 Panel 子元素一样可以包装；重新挂载后 WPF 会保留它的 `TemplatedParent`、TemplateBinding 和具名模板部件，因此模板内容仍然正常。

由于**出站转场取自被离开的页面**，想要特定退场动画的页面必须自己声明（或在导航前动态设置）。示例在 `SampleNavigator` 中采用后者，使进入和离开羽化示例都使用 feather 族。

### 4.6 渲染路径切换

投影为 identity 时 Host 走 2D 路径，否则走 `Viewport3D` 路径。两条路径的**几何完全一致**（identity 投影与 2D 布局的偏差在 0.01 px 以内），但文本栅格化方式不同（ClearType 与 3D 纹理的灰度抗锯齿）。`ProjectionHost.Prefer3D` 让 Host 在投影仍是 identity 时就保持 3D 路径，因此 `TransitionFrame`、`TiltEffect`、`TurnstileFeatherEffect` 在**准备动画时**就切换渲染路径，而不是等到动画首帧。转场的 presenter 不再使用 `BitmapCache`：3D 管线本身就会栅格化内容，额外的 1x 缓存只会让它变糊。

`Viewport2DVisual3D` 把被投影可视对象的**绘制边界**（painted bounds）映射到四边形上，而不是布局槽。绘制范围小于布局槽的内容（左对齐的 `CheckBox`、较短的 `TextBlock`、任何被拉宽但只画一部分的元素）会被拉伸到整个投影平面（这曾让倾斜的 `CheckBox` 方框和文字横向被拉宽约 1.8 倍）。因此 Host 投影一个填满**布局槽**的内部 surface：它按布局槽尺寸绘制透明矩形，被裁剪到布局槽（见上），并被固定为布局槽尺寸（3D 可视宿主会按 desired size 布局它），平面几何以及 `CenterOfRotation`/`LocalOffset`/`GlobalOffset` 都以布局槽为准，而不是子元素碰巧被测量/排列出的大小。因此比宿主小的子元素会保持它在 2D 下的大小和位置。`ProjectionHost_DoesNotStretchContentThatPaintsLessThanItsSlot` 与 `ProjectionHost_ProjectsTheLayoutSlotNotTheChildSize` 覆盖这两点。

开发机实测帧率（`CompositionTarget.Rendering`，480×800 内容）：2D 基线约 150 fps、整页投影约 92–96 fps、6 个独立羽化元素约 64–68 fps，2D→3D 切换只花一帧（约 6 ms）。把羽化目标在**订阅时**提前包装可以把这部分开销移出转场过程，因为动画期间重新挂载元素会让每个目标触发一次完整布局。

## 5. 导航子系统

### 5.1 `PhoneApplicationFrame`

继承 WPF `Frame`，保留 WPF 的导航日志、`NavigationService`、`CanGoBack`、URI 导航等能力，并增加：

- `Orientation` / `SupportedOrientations` 依赖属性。
- `OrientationChanged` 事件；方向根据 Frame 尺寸推断（宽 > 高为横向）。
- 页面生命周期分发：`Navigating` 调用当前页面的 `OnNavigatingFrom`；`Navigated` 调用旧页面的 `OnNavigatedFrom` 和新页面的 `OnNavigatedTo`。
- `GoBack()` 先触发 `PhoneApplicationPage.BackKeyPress`。
- `Navigate(PhoneApplicationPage, IDictionary<string,string>)` 传递导航参数，与 URI 查询字符串合并为 `NavigationContext`，在 `OnNavigatedTo` 前赋给页面。
- 页面状态按页面实例保存在 Frame 中，返回到同一实例后仍然存在。
- 应用栏 chrome：模板承载 `ApplicationBarHost`（按应用栏高度预留的行）与 `ApplicationBar`（`ApplicationBarPresenter`）。`Navigated` 把新页面的应用栏推给呈现器；页面处于活动状态时修改 `ApplicationBar` 也会转发到呈现器。

WPF `Page` 没有 WP 的 `OnNavigatedTo/OnNavigatedFrom/OnNavigatingFrom` 受保护虚方法，因此移植版在 `PhoneApplicationPage` 中声明这些虚方法，并由 Frame 触发。这样保留了 API 和确定顺序，但如果页面放在移植 Frame 之外，WPF 自身不会调用它们。

### 5.2 `PhoneApplicationPage`

继承 WPF `Page`，增加 `State`、`NavigationContext`、`Orientation`、`SupportedOrientations`、`ApplicationBar`、`NavigatedTo`、`NavigatingFrom`、`NavigatedFrom`、`BackKeyPress`、`OrientationChanged`。`NavigationContext` 暴露 `Uri`、`ExtraData` 和大小写不敏感的 `QueryString` 字典。`ApplicationBar` 与原版一样是 `IApplicationBar` 类型，且只接受移植版的 `ApplicationBar` 实现（其它实现会抛异常）。

### 5.2.1 应用栏

`Microsoft.Phone.Shell` 移植了原版托管 API（`IApplicationBar`、`ApplicationBar`、`ApplicationBarIconButton`、`ApplicationBarMenuItem`、`ApplicationBarMode`、`ApplicationBarStateChangedEventArgs`）及其基础设施：`ApplicationBarItemList<T>` 保留每个列表的上限（4 个图标按钮、50 个菜单项）和原始异常，item 容器负责校验文本/图标/启用状态并派发点击，`ApplicationBar` 保留“只有可见且完全不透明的应用栏才预留页面空间”这条原版规则。

Windows Phone 由合成器绘制应用栏；移植版由新增的 `ApplicationBarPresenter` 绘制，并由 frame 模板承载（见 5.1）。呈现器订阅应用栏及其 item，构建图标按钮视图（96×72 槽位，48 DIP 圆形按钮——2 DIP 白色圆环内嵌 32 DIP 图标——下方 12 px 标签）、菜单按钮（右上角三个点）以及行高 48 px 的 `Popup` 菜单。颜色来自 `BackgroundColor`/`ForegroundColor`，透明度来自 `Opacity`；frame 用呈现器的 `ReservedHeight` 设置 `ApplicationBarHost.Height`，因此半透明或隐藏的应用栏会浮在页面之上而不是压缩页面。

### 5.3 `TransitionFrame`

继承 `PhoneApplicationFrame`。模板有两个 `ProjectionHost` 部件，名称为 `FirstContentPresenter` 和 `SecondContentPresenter`。部件类型从 `ContentPresenter` 改为 `ProjectionHost`，以便对页面做 3D 投影。

导航转场顺序：

1. `Navigating`：捕获旧页面，选择 `NavigationOutTransition.Forward/Backward`，获取 `ITransition`，准备旧 Host，启动退出转场。
2. `OnContentChanged`：把新页面放入非活动 Host，获取 `NavigationInTransition`；如果退出转场仍在进行则暂存，否则立即启动进入转场。
3. 退出完成：停止/恢复旧转场，隐藏旧 Host，然后启动进入转场。
4. 进入完成：停止/恢复，恢复交互，触发 `EndTransition`。
5. `NavigationStopped`：恢复旧内容并取消已存转场。

`Transition` 保存/恢复 `CacheMode` 和 `IsHitTestVisible`，并自己跟踪 begun/completed 状态。WPF 的 storyboard 控制 API 比 Silverlight 严格：

- 时钟还不存在时，`Storyboard.GetCurrentState()`/`GetCurrentTime()`/`Stop()`/`Pause()` 会**抛异常**，而 Silverlight 返回 `Stopped`/`Zero`。因此 `Transition` 自己跟踪 `IsBegun`，直接返回 `ClockState.Stopped`/`TimeSpan.Zero`，而不去查询一个尚不存在的时钟；对从未启动的转场调用 `Stop()` 只是恢复元素状态，不做任何停止操作。
- storyboard 以**可控时钟**启动（`Begin(containingObject, true)`），因为 WPF 的 `Pause`/`Resume`/`Seek`/`SkipToFill` 需要交互控制；所有控制调用都传入同一个宿主对象。
- 可控时钟被停止或移除时，WPF 会再次触发 `Timeline.Completed`，因此 `Transition` 每次运行最多触发一次 `Completed`；`Stop()` 会取消待完成事件，避免 `EndTransition` 重复触发（早期嵌套完成会导致事件翻倍）。
- `Stop()` 同时移除时钟，使被动画的属性回到基础值。
- 动画缓存改为每个转场各自创建，不再共享静态实例：`Freezable` 属于创建它的 dispatcher，共享的 `BitmapCache` 在第二个 UI 线程上会抛 `ArgumentException`（`MediaSystem.AssertSameContext`），也因此让缓存无法跨 WPF 测试线程使用。

动画本身是带显式目标的普通 WPF `Storyboard`，通过观察实际 `PlaneProjection.RotationY` 以及渲染转场中间帧（页面呈透视梯形）验证。

`Transitions.ProjectionTransition`（仅移植版）在带投影的转场完成或停止时清除附加投影，使空闲页面回到纯 2D 路径，而不是长期保留一个单位投影。`TransitionFrame` **不**给呈现器加 `BitmapCache`：`Viewport2DVisual3D` 已经对页面做过栅格化，再套一层缓存只会让动画变模糊。

原版有两处行为必须做适配：

- 原版 `Navigating` 会忽略非应用发起的导航（例如应用停用时由 shell 播放的退场）。WPF 对每次 `Frame.Navigate` 都报告 `IsNavigationInitiator == false`，也没有 shell 发起的导航，因此移植版去掉了该判断；保留它会屏蔽掉所有转场。
- 原版 `TransitionFrame` 不捕获转场工厂的异常，移植版保持一致：抛出异常的 `TransitionElement` 会让导航失败，而不是被静默降级。早期移植版曾提供 `TransitionFailed` 事件和无转场回退，为与原版行为完全一致已移除。

### 5.4 转场系列

原版的 27 个 storyboard XAML（`Transitions/Storyboards/*.xaml`）以 WPF 资源形式内嵌，并按原版工具箱的方式加载：`Application.GetResourceStream("/Community.Phone.Controls.Toolkit.WPF;component/Transitions/Storyboards/<Name>.xaml")` → 缓存文本 → `XamlReader.Load`（WPF 需要 `XmlReader`/流，不能直接传字符串）。`Transitions.GetEnumStoryboard` 把 `<系列><模式>` 映射到对应文件，`Storyboard.SetTarget(storyboard, element)` 把动画绑定到页面，工厂在返回 `Transition` 前设置投影（Slide 则是 `RenderTransform`）。

storyboard 内部唯一的改动是属性路径：Silverlight 写 `(UIElement.Projection).(PlaneProjection.RotationY)`，移植版写 `(phone:ProjectionService.Projection).(phone:PlaneProjection.RotationY)`（`xmlns:phone="clr-namespace:Microsoft.Phone.Controls;assembly=Community.Phone.Controls.Toolkit.WPF"`），因为 `Projection` 是投影宿主上的附加属性。角度、关键时间和缓动函数都是原值，WPF 能正确解析带前缀的路径。

系列包括 Roll、Rotate（8 模式）、Slide（8 模式）、Swivel（6 模式）、Turnstile（4 模式）、TurnstileFeather（4 模式）。`TransitionService` 和 `NavigationTransition` / `NavigationInTransition` / `NavigationOutTransition` 保留原始附加属性形态。

Turnstile 系列保留移植版仅有的两个常量偏差（均在 `Transitions.cs` 中说明）：`TurnstileCenterOfRotationX = -0.05` 与 `TurnstileDurationScale = 1.2`，后者以 `Storyboard.SpeedRatio` 施加，因此原始关键时间保持不变（把两个常量分别改回 `0`/`1` 即可精确还原原版几何与时长）。

## 6. 效果

### `TiltEffect`

保留 `IsTiltEnabled`、`SuppressTilt`、`TiltableItems`、`UseLogarithmicEase`。按下时向上查找最近的 tiltable 祖先，先缓存 container→control 变换，再通过 `ProjectionService` 赋 `PlaneProjection`——该服务会按需把控件包进 `ProjectionHost`，并在投影清除时移除包装，因此倾斜逻辑不再自己管理宿主。旋转支点取控件的**第一个可视子元素**（模板根，也就是控件实际显示的内容）的中心，按压点也按该边界归一化，与原版工具箱的 `GetChild(control, 0)` 一致；`CenterOfRotation` 以布局槽（即投影平面）为基准表达。随后应用原始归一化点 → RotationX/RotationY/GlobalOffsetZ 计算，指针离开可见内容时暂停倾斜。鼠标支持通过隧道式 `PreviewMouseDown/Move/Up` 实现，使容器可以倾斜子按钮而不会吞掉按钮点击；同时支持触摸 manipulation。返回动画直接以投影为目标，并跟踪 Storyboard 是否已应用，避免 WPF `Storyboard` 状态异常，同时清理每次交互的状态。普通 WPF 控件按需包装；列表 item 容器不包装。

### `TurnstileFeatherEffect`

保留 `FeatheringIndex`、`NonPermittedTypes` 和原有交错时间常数。合成 storyboard 时，目标从页面的**可视树**收集（`GetTargetsToAnimate` → `CollectFeatheringTargets`，按 index 稳定排序）—— 因为进场页面在接入可视树之前就被合成，此时附加属性的通知尚未运行；`TransitionFrame` 会在合成前对进场 presenter 强制执行一次布局，使目标具有真实几何。附加属性订阅只负责准备包装，并且延迟到 `Loaded`/`SizeChanged` 之后的 dispatcher（在布局过程中包装会引发布局循环）；包装触发的 `Unloaded` 既不会取消订阅，也不会拆掉包装。`TurnstileFeatherTransition` 开始时，为每个在屏幕内的目标确保 `ProjectionHost`，并在赋 `PlaneProjection`（`CenterOfRotationX = -0.2`）**之前**先打开 `host.Prefer3D`，再按原 40–50 ms 交错组合 RotationY/透明度动画；storyboard 通过由 `PlaneProjection.RotationYProperty` 生成的属性路径定位投影，因为 WPF 没有 `(UIElement.Projection)` 这种路径写法。页面中心的 `GlobalOffsetY` 加在元素投影上，补偿位移加在**包装器**上（包装器的表面按元素布局槽裁剪）。无法包装的目标（item 容器、content presenter）会跳过。清理时恢复透明度、投影、渲染变换和 2D 渲染路径，然后让元素与其包装器失效重绘 —— 走过 3D 路径的包装器使用独立表面合成，否则屏幕上会残留最后一帧 3D 画面。包装器本身保留到元素卸载时才释放，因此一个元素在一次转场中不会被重复重新父级化。

如果转场最终没有任何动画（例如对没有可羽化元素的页面做羽化），它会在下一个 dispatcher 周期立即完成，而不是阻塞 Frame —— 否则新页面会永远不可交互。

`SlideInEffect` 未实现，因为它依赖 WP `Pivot`/`PivotItem` 及其选择/manipulation 生命周期。文档中明确标记为不支持。

## 7. 控件与主题

已实现控件使用 WPF 依赖属性、模板和 `VisualStateManager`。`DefaultStyleKeyProperty.OverrideMetadata` 在**静态构造函数**中调用，确保每个类型只注册一次元数据。`Themes/Generic.xaml` 提供按类型键控的隐式样式；示例和测试还显式合并字典，使应用 XAML 可以访问命名 Metro 资源（`PhoneTextTitle1Style` 等）。

主题的写法受两条 WPF 规则约束。(1) 库内 XAML 若要使用库自身的类型，`clr-namespace:` **不写** `assembly=`：`MarkupCompilePass1` 只能从编译的源码中认识本项目的类型，在 xmlns 里写程序集名会让干净构建报 `MC3074`。(2) 主题字典里的 `StaticResource` 是在**所属字典**（当该字典被当作 WPF theme 字典使用时则是**应用程序**）中解析的，因此 `Themes/Controls/*.xaml` 各自合并 `Theme/Dark.xaml`，动画关键帧的值使用字面量而不是资源查找。库自己创建的页面（例如 `ListPickerPage`）会把自己的 `Resources` 合并 `Theme/Dark.xaml`——它在挂到 frame 之前就被解析，看不到 `Application.Resources`；因此它的模板关键帧使用 `StaticResource`（解析时从该字典取值），这种场景下模板 storyboard 里的 `DynamicResource` 不会生效。

控件设计决策：

- **WrapPanel**：原始布局算法和 `ItemHeight/ItemWidth/Orientation` 语义。
- **ToggleSwitch / ToggleSwitchButton**：保留视觉状态契约和开关模板；保留 manipulation 与模板部件。
- **Rating / RatingItem**：保留星形项目生成、裁剪蒙版、拖动辅助、方向和半星/零值选择。
- **PhoneTextBox**：内部 TextBox 使用双向 `Text` 绑定（WPF `TemplateBinding` 是单向的），保留 hint、action icon 和长度指示。
- **ExpanderView**：基于 WPF `HeaderedItemsControl`；部件 `Presenter`、`ExpanderPanel`、`ItemsCanvas`；高度动画在代码中完成，因为 WPF 无法直接取得 Silverlight 模板部件。
- **HubTile**：定时器驱动 `Expanded → Semiexpanded → Collapsed → Expanded → Flipped` 状态循环；`HubTileService` 支持冻结组。
- **ListPicker**：移植原版 WP8 实现——普通模式对模板 Canvas 做高度/位移动画，完整模式通过宿主 frame 导航到 `ListPickerPage`（pack URI 来自 `PickerPageUri`）；页面的「完成/取消」使用原版 `ApplicationBar`（圆形勾/叉图标按钮，图标来自库的 `Content` 资源，仅多选时显示）。完整模式的项目按原版依次做 `RotationX` 翻转展开；item 容器不能包进 `ProjectionHost`，因此由页面的单选/多选 item 模板承载 `ProjectionHost`，动画直接作用于它。普通模式的 Canvas 动画直接在模板部件上调用 `BeginAnimation`，因为 storyboard 无法驱动模板里的 `TranslateTransform`。页面会等列表就绪后再组合动画：`GetItemsInViewPort` 跳过虚拟化面板尚未生成的容器，页面则在 dispatcher 轮次里重复 `ScrollIntoView`，直到选中项出现在屏幕上（WPF 会丢弃 `Loaded` 期间发出的请求）。
- **ContextMenu/MenuItem/Separator/ContextMenuService**：继承 WPF 菜单类型；右键打开。
- **CustomMessageBox**：`ContentControl` API，`Show()`/`Dismiss()` 由 WPF Window 承载。
- **DatePicker/TimePicker**：保留 Value/ValueString/Header/PickerPageUri API，用 WPF 日期/时间 UI 在模态 Window 中实现。
- **PerformanceProgressBar**：内嵌 WPF `ProgressBar` 和 `ActualIsIndeterminate`；简单透明度淡入淡出。
- **Pivot / PivotItem / PivotHeadersControl / PivotHeaderItem**：手机版本由 DirectManipulation 与合成器驱动，原版 `PivotItem` 的位置状态不含 storyboard（主题里的 `Left`/`Center`/`Right` 状态都是空的）。移植版保持该契约并在代码中做动画：容器堆叠在 `Grid` items panel 中，只有当前项可见；切换时进入项从 `±ActualWidth` 动画到 `0`（`QuarticEase`，沿用原版 0.25 秒），离开项同时滑出并在动画结束后折叠。指针拖拽会以原版 600 px/s 移动 `ItemsPresenter` 与页眉画布，超过半个视口即切换。页眉条是放在 `Canvas` 上的 `ItemsControl`：`PivotHeadersControl` 为每个页眉设置 `Canvas.Left`（循环排布，从 `VisualFirstIndex` 开始，最后一个之后接第一个），并以原版 0.5 秒滚动画布；手机版本由合成器移动同一条页眉带。`Pivot.IsLocked` 用 200 毫秒淡出页眉并拒绝修改项。所有动画都通过 `DoubleAnimation.Completed` 收尾，使内容保持可交互并让排队的切换请求继续执行。
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

需要窗口或 WPF Dispatcher 的测试通过 `StaTest.Run` 在专用 STA 线程上运行；动画通过真实的 dispatcher 消息泵推进，而不是只断言静态属性。等待改为条件轮询（`WpfTestHelper.PumpUntil(predicate, timeout)`）而非固定 sleep，且没有测试依赖操作系统指针或物理输入设备。测试并行化保持关闭：多组测试驱动共享的 WPF 单例（主题字典、`Popup`、storyboard XAML 缓存），并行运行会死锁。
