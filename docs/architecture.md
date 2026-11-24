# 架构设计

## 1. 目标与约束

把归档的 Windows Phone Toolkit 移植到现代 WPF，并在 WPF 允许的范围内保留原始公共 API。影响设计的关键约束：

- WPF 的 `UIElement` 没有 `Projection` 属性，也不能给 `Grid` 等既有框架类型追加 CLR/XAML 属性。
- WPF 自带 `Frame`、`Page`、`ContextMenu`、`WrapPanel`、`DatePicker`、`HeaderedItemsControl`，重新定义同名 `System.Windows.*` 类型会造成歧义并违反“不重新定义 WPF 框架类型”的约束。
- `PhoneApplicationFrame`、`PhoneApplicationService`、`SystemTray`、地图、后台传输、语音等平台服务在 WPF 中不存在。
- 原始 XAML 使用 Silverlight 资源键和动画路径，例如 `(UIElement.Projection).(PlaneProjection.RotationY)`。

因此移植版在**原始 `Microsoft.Phone.Controls` / `Microsoft.Phone.Shell` 命名空间**中提供兼容类型：投影由 `ShaderEffect` 直接作用于任意 `UIElement`，框架相邻控件使用 WPF 原生基类。

## 2. 仓库与工程

```text
Community.Phone.Controls.Toolkit.slnx
├─ src/Community.Phone/                       兼容层（程序集 Community.Phone，根命名空间 Microsoft.Phone）
│  ├─ Controls/     Pivot、PivotItem、Panorama、PanoramaItem、LongListSelector、HyperlinkButton、ViewportControl、Primitives/*
│  ├─ Navigation/   PhoneApplicationFrame、PhoneApplicationPage、NavigationContext、方向类型
│  ├─ Projection/   Projection、PlaneProjection、Matrix3DProjection、ProjectionService、ProjectionMath、HomographyMatrix
│  │                Rendering/Shader/  PerspectiveShaderEffect(.cs/.hlsl/.ps)、ShaderProjectionRenderer、ShaderResourceManager
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
├─ tests/Community.Phone.Tests/            投影数学、服务与离屏渲染测试（xunit）
├─ tools/ProjectionBenchmark/              投影性能基准（可切换到旧后端）
└─ samples/Community.Phone.Controls.Toolkit.Sample/   可运行 WPF 示例
```

着色器字节码（`.ps`）以 `Resource` 嵌入 `Community.Phone.dll`，由 `build\compile-shaders.ps1` 重新生成。

职责划分：

- `Community.Phone` 重新实现 WPF 不提供的 `Microsoft.Phone` 平台表面：页面/框架/导航类型、投影类型、`Microsoft.Phone.Shell`（应用栏、系统栏、应用服务、主题），以及原 toolkit 中属于平台层的 `Pivot`、`LongListSelector`、`HyperlinkButton`、`Panorama`。它不引用 toolkit。
- `Community.Phone.Controls.Toolkit` 引用 `Community.Phone`，提供转场、效果与其余控件。
- 两者打进**一个** NuGet 包 `Community.Phone.Controls.Toolkit`。

两个工程都在所有配置下定义 `WINDOWS_PHONE;WP8`，移植源码用这些常量选择兼容命名空间与本地化资源。

## 3. 命名空间与 XAML 前缀

| 原始命名空间 | 移植命名空间 | 程序集 | 说明 |
|---|---|---|---|
| `Microsoft.Phone.Controls`（平台：页面、框架、导航、`Pivot`、`LongListSelector`、`HyperlinkButton`、`Panorama`） | 同名 | `Community.Phone` | `platform:` 前缀 |
| `Microsoft.Phone.Controls`（投影：`ProjectionService`、`PlaneProjection`、`Matrix3DProjection`、`HomographyMatrix`、`ProjectionMath`） | 同名 | `Community.Phone` | `platform:` 前缀 |
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

投影类型编译在 `Community.Phone` 程序集。渲染后端是**基于 `ShaderEffect` 的平面单应变换**：一个元素只需要赋一个 `Projection`，不需要包装容器、不需要 `Viewport3D`，也不改变布局树。

### 4.1 公共模型

| 类型 | 基类 | 关键成员 |
|---|---|---|
| `Projection` | `Animatable` | `ProjectionChanged` 事件、`IsIdentity`、`ToMatrix3D(Size)` |
| `PlaneProjection` | `Projection` | `CenterOfRotationX/Y/Z`、`GlobalOffsetX/Y/Z`、`LocalOffsetX/Y/Z`、`RotationX/Y/Z`（均为依赖属性） |
| `Matrix3DProjection` | `Projection` | `ProjectionMatrix`，使用 0..1 归一化坐标 |
| `HomographyMatrix` | 结构 | 3×3 齐次变换：`Transform`、`TryTransform`、`TryInvert`、`TransformBounds`、`Multiply`、`GetOutputCoefficients` |
| `ProjectionMath` | 静态类 | `CreateHomography`、`GetPlaneDistance`、矩阵构造辅助、`DefaultPerspectiveDepth` |
| `PerspectiveShaderEffect` | `ShaderEffect` | 投影着色器（常量寄存器封装），由渲染器驱动 |
| `ProjectionService` | 静态类 | 附加属性 `Projection`、`PreferShader`、`PerspectiveDepth`、`HitTestPolicy`；`EffectConflictBehavior`、`EffectConflict` 事件；`GetProjectionPropertyPath`；扩展成员 `element.Projection` / `element.PreferShader` |

### 4.2 渲染方案

一个位于局部平面 `z = 0` 上的元素，经过任意 3D 平面变换与透视相机投影后，其二维映射**精确等于一个 3×3 Homography**。因此不需要构造 3D 场景：

```text
元素自身渲染 → 隐式输入纹理
输出像素 → 逆 Homography → 元素局部坐标 → 纹理 UV → 采样
```

- `UIElement.Effect` 被替换为 `PerspectiveShaderEffect`。WPF 把元素自身的内容作为着色器的隐式输入（sampler 0），不需要 `VisualBrush`，也不会递归。
- 元素保持原有父级、布局槽、命中测试、焦点与输入路由：**没有重新 Parent，没有复制布局属性**。
- 单一元素只创建一个 effect 实例；动画每帧只写 6 个 `Point4D` 常量与 Padding，不重新编译着色器，也不重新分配 effect。

不采用 `VisualBrush` + `Rectangle` 的原因：那需要一个额外的绘制层，会重复栅格化并在视觉树里插入兄弟节点；不采用 `RenderTargetBitmap` 快照的原因：它丢失实时内容与交互。`Viewport3D` 后端已移除：它为每个投影元素建立一个 3D 场景（相机、网格、`Viewport2DVisual3D`），是 ListBox 场景的主要成本。

### 4.3 数学：Matrix3D → Homography

`PlaneProjection.ToMatrix3D` 的语义不变（行向量约定）：

```text
p' = (((p - center) + localOffset) * R) + center + globalOffset
R  = CreateRotationMatrix(RotationX, -RotationY, RotationZ)   （角度制）
center = ((CenterOfRotationX - 0.5) * width, (0.5 - CenterOfRotationY) * height, CenterOfRotationZ)
```

元素局部坐标（原点左上、Y 向下）先映射到平面坐标（原点中心、Y 向上）：`X = x - W/2`、`Y = H/2 - y`。由于平面固定在 `z = 0`，裁剪坐标的三个分量都是 `(x, y, 1)` 的线性组合。

相机与旧后端一致：`PerspectiveCamera` 位于 `(0, 0, depth)`，水平视场角 `fov = 2 * atan(W / (2 * depth))`，`depth = PerspectiveDepth`（默认 999 DIP）。它把平面点映射为

```text
screenX = W/2 + depth * X' / (depth - Z')
screenY = H/2 - depth * Y' / (depth - Z')
```

乘以齐次权重 `W'` 后得到 `clipW = depth * W' - Z'`，于是

```text
H = A(局部→平面) · C(平面→裁剪)
```

`ProjectionMath.CreateHomography` 就是这个 3×3 乘积，并按 `H33` 归一化。它与旧后端用 `Matrix3D.Transform` + 相机公式逐点计算的结果一致（见 §4.9 的对照测试）。

约定与边界情况：

- 齐次矩阵只在相差一个标量意义下唯一。`CreateHomography` 会把符号规整为“元素中心处权重为正”，这样着色器可以用一个符号判断裁剪相机背后的像素。
- `depth - Z' <= 0`（平面到达或越过相机）时 `ProjectionMath.GetPlaneDistance` 返回非正值，渲染器直接输出全透明，不渲染镜像残影。
- 平面正对镜头（例如 `RotationY = 90°`）时行列式为 0，`TryInvert` 返回 false，渲染器输出全透明——与 3D 后端在 90° 时面积为 0 的表现一致。
- 超过 90° 的旋转继续显示镜像内容：逆映射本身对背面同样成立，着色器不做背面剔除。旧后端为此在网格里加了反向绕序三角形，新后端不需要。

### 4.4 着色器与坐标空间

`Projection/Rendering/Shader/PerspectiveShaderEffect.hlsl`（编译产物 `PerspectiveShaderEffect.ps`，`ps_2_0`，约 28 条指令）只做逆向纹理映射：

| 寄存器 | 内容 |
|---|---|
| `s0` | 元素自身的隐式输入纹理 |
| `c0`–`c2` | 逆 Homography 的三个输出分量系数 `(x, y, 常数)`，即 `GetOutputCoefficients(0..2)` |
| `c3` | 正向 Homography 的权重分量 + 启用标志（`w`） |
| `c4` | 输出偏移 `(padLeft, padTop)` 与填充后的尺寸 `(width, height)` |
| `c5` | 未投影元素尺寸 |

坐标链（每一步都必须显式换算，不能假设各坐标系一致）：

```text
uv（0..1，覆盖填充后的输出面）
  → padded = uv * OutputBounds.zw
  → 元素局部投影坐标 = padded + OutputBounds.xy      （xy 为负的 padding 偏移）
  → source = inverseH * 该点（含透视除法）
  → 纹理 uv = (source - OutputBounds.xy) / OutputBounds.zw
```

`source` 落在 `[0, SourceSize]` 之外、或正向权重 ≤ 0（相机背后）时，输出乘以 0；其余情况返回采样值。乘 0 而不是 `clip`，避免分支与空洞。

寄存器布局是行向量约定的直接结果：`HomographyMatrix` 的行是**输入分量**、列是**输出分量**，所以着色器每个常量取的是“某一输出分量的三个系数”。这一点在实现中曾经写反（取了 `M31/M32/M33`），症状是所有像素被权重为负的掩码吃掉；调用 `GetOutputCoefficients` 后不再有歧义。

### 4.5 ProjectionService：直接附加

```csharp
ProjectionService.SetProjection(element, new PlaneProjection { RotationY = 60 });
```

- 状态存放在 `ConditionalWeakTable<UIElement, ProjectionState>`：`ProjectionState` 只持有渲染器，渲染器持有元素与当前 effect。`ConditionalWeakTable` 是 ephemeron，值反向引用键不影响回收。
- 渲染器订阅元素尺寸变化（`FrameworkElement.SizeChanged`，非 `FrameworkElement` 用 `LayoutUpdated`）与 `Projection.ProjectionChanged`；两者都只做增量重算。
- 投影被清空时移除状态并恢复元素原来的 `Effect`。
- `PreferShader` 让元素在投影仍为 identity 时保留着色器路径，供从 0 开始的动画使用。
- `HitTestPolicy` 决定投影期间是否参与命中测试：默认 `Layout` 保持原生命中测试矩形，`Disabled` 在着色器挂载期间设置 `IsHitTestVisible = false` 并在摘除时恢复。

**Effect 冲突**：`UIElement.Effect` 是单一属性。默认策略是 `Replace`——投影接管 `Effect`，原有效果被保存、投影移除后恢复，并触发 `ProjectionService.EffectConflict` 事件（同时写 `Debug` 输出），不静默丢弃。`EffectConflictBehavior = Preserve` 时改为保留原有效果、跳过投影。WPF 没有效果链，因此无法同时渲染两个 `Effect`。

### 4.6 容器与内容裁剪

投影没有宿主类型：附加属性直接作用在元素上，元素既可以是叶子控件，也可以是承载内容的容器。需要“**裁剪内容、但保留投影溢出**”时，把它拆成两层普通元素：

```xml
<Grid platform:ProjectionService.Projection="{StaticResource Flip}">   <!-- 布局槽：投影在这里 -->
    <Grid ClipToBounds="True">                                          <!-- 内容裁剪 -->
        <StackPanel RenderTransform="...">...</StackPanel>
    </Grid>
</Grid>
```

`ClipToBounds` 只影响内层元素自己的渲染，不会裁掉外层元素的 effect 输出，因此翻转可以画到槽外，而滑出视口的内容仍然被隐藏。`HubTile` 的模板就是这个结构。

转场、`TiltEffect`、`TurnstileFeatherEffect`、`ListPicker` 都直接投影目标元素，指针按下与动画期间不再改动视觉树。

### 4.7 输出边界与裁剪

投影结果可以画到布局槽之外，所以每帧计算：

1. 四个角的投影坐标（`HomographyMatrix.TransformBounds`）；
2. 相对于元素边界的左/上/右/下扩张量；
3. `ShaderEffect.PaddingLeft/Top/Right/Bottom`（WPF 的原生机制，`ShaderEffect` 的属性是 protected，由 `PerspectiveShaderEffect` 内部写入）。

填充后的输出面每轴上限 `MaxOutputExtent = 4096` DIP，避免接近侧视时（bbox 无界）申请巨大的中间表面；超出部分被裁掉，但映射本身仍是逐像素精确的。

**父级裁剪不受影响**：`ClipToBounds`、父级 `Clip`、`ScrollViewer` 视口、元素自身 `Clip` 都会照常裁掉投影溢出部分。Padding 只扩大 effect 的输出区域，不能突破父级裁剪。

### 4.8 每帧路径与性能

动画每帧走同一条最短路径：`ProjectionChanged` → 重算矩阵（约 40 次浮点运算）→ 与上一帧比较（尺寸、输出矩形、逆矩阵、启用标志）→ 只在变化时写 6 个常量与 Padding。

- 不创建新的 `ShaderEffect`、不重新编译着色器、不注册事件、不强制 Measure/Arrange。
- identity 且未设置 `PreferShader` 时直接摘掉 effect，元素回到普通 2D 渲染（ClearType 不受影响）。
- 不使用 `CompositionTarget.Rendering` 轮询：只有真正变化的投影才更新。

基准工具 `tools/ProjectionBenchmark`（同一份代码可编译到任一后端）：

```text
dotnet run --project tools\ProjectionBenchmark -c Release -- --scenario 2d,single,10,50,100,list,transition --seconds 2
```

本机（WPF 软件合成环境，纯 2D 基线约 30 ms/帧，绝对值不代表 GPU 机器）实测，同一工作负载下旧 `Viewport3D` 后端与新后端的**每帧 CPU 与托管分配**：

| 场景 | 旧 CPU/帧 | 新 CPU/帧 | 旧分配/帧 | 新分配/帧 |
|---|---|---|---|---|
| 2D（100 个无投影元素） | 4.4 ms | 4.9 ms | 2.4 KB | 2.2 KB |
| 1 个投影元素 | 20.4 ms | 4.5 ms | 6.6 KB | 4.4 KB |
| 10 个 | 48.2 ms | 6.5 ms | 33 KB | 8.6 KB |
| 50 个 | 49.0 ms | 23.0 ms | 157 KB | 34 KB |
| 100 个 | 52.1 ms | 31.3 ms | 315 KB | 88 KB |
| 页面转场 | 3.1 ms | 6.1 ms | 200 KB | 47 KB |

结论只到这一步：投影元素越多，新后端的 CPU 与分配越低（旧后端每个元素一个 3D 场景）；单页面转场里新后端 CPU 略高（一次全页栅格化 + 着色器），但分配少四倍。帧间隔受软件合成限制，不作为 FPS 结论。

### 4.9 测试与对照

- `tests/Community.Phone.Tests`：Homography 数学（identity、单轴/组合旋转、偏移、旋转中心、不同尺寸、`Matrix3DProjection`、透视深度、正逆互验、奇异矩阵、相机背后、非有限输入），对照实现用 WPF 自己的 `Matrix3D.Transform` + 相机公式，容差 1e-5。
- 离屏渲染对照（开发期验证，脚本未入库）：同一投影分别用旧 `Viewport3D` 后端与新着色器渲染，四角包围盒误差 ≤ 1 px，像素差异 0.4%–2%（边缘抗锯齿）。
- UI 测试：任意元素直接投影、清除后恢复、identity 不栅格化、`PreferShader`、原有效果保存/恢复、`Preserve` 策略、尺寸变化重算、Storyboard 属性路径、父级 `ClipToBounds`、`ScrollViewer` 裁剪、ListBox 虚拟化、`Matrix3DProjection`。

### 4.10 XAML 兼容边界

```xml
<!-- 任意元素：附加属性，不改变布局树。 -->
<Grid platform:ProjectionService.Projection="{StaticResource MyProjection}">...</Grid>

<!-- 内联声明。 -->
<Border>
    <platform:ProjectionService.Projection>
        <platform:PlaneProjection RotationY="-35" />
    </platform:ProjectionService.Projection>
</Border>
```

storyboard 路径不变：`(ProjectionService.Projection).(PlaneProjection.RotationY)`，可用 `ProjectionService.GetProjectionPropertyPath` 生成。

注意：通过 `Style`/`ItemContainerStyle` 共享的 `Freezable` 会被 WPF 冻结，不能动画。需要动画时把 `PlaneProjection` 放进 `DataTemplate`（每个容器一个实例，例如绑定到页面上的一个 `double` 属性），或按容器在代码里创建。

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

继承 `PhoneApplicationFrame`，模板部件是名为 `FirstContentPresenter`、`SecondContentPresenter` 的两个 `ContentPresenter`；转场投影由被承载的页面承担。

导航转场顺序：

1. `Navigating`：捕获旧页面，选择 `NavigationOutTransition.Forward/Backward`，取得 `ITransition`，准备旧呈现器（关闭命中测试、对被投影的页面打开 `PreferShader`），启动退场。
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

`LongListSelector` 在 `TiltableItems` 中，但它不是被倾斜的元素：按下位置所属的**条目容器**决定倾斜目标。容器由选择器内部 items host 的 `ItemsControl.ContainerFromElement` 解析，因此列表头、组头、条目、组尾、列表尾各自命中自己的容器，而 WPF 额外插入的 `ScrollContentPresenter` 与 `ViewportControl` 的 `ContentPresenter` 不会被误判成条目容器（取最外层 content presenter 会退化成倾斜整个 items host）。目标取容器唯一的可视子元素（模板根），支点即该元素自身的中心——它已经是可见内容，不再向内取第一个子元素，否则行首的小图标会把支点拉偏。

鼠标支持走隧道式 `PreviewMouseDown/Move/Up`，容器可以倾斜子按钮而不吞掉按钮点击；同时支持触摸 manipulation。返回动画以投影为目标并跟踪 storyboard 是否已应用；指针离开可见内容时暂停倾斜，交互结束清理全部逐次状态。

### `TurnstileFeatherEffect`

附加属性 `FeatheringIndex`，静态属性 `NonPermittedTypes` 与交错时间常数。合成 storyboard 时目标从页面的**可视树**收集（`FeatheringIndex` 稳定排序），因为进场页面在接入可视树之前就被合成。附加属性订阅只负责准备包装，且延迟到 `Loaded`/`SizeChanged` 之后的 dispatcher（布局过程中包装会引发布局循环）。

`TurnstileFeatherTransition` 开始时为目标直接赋 `PlaneProjection`（`CenterOfRotationX = -0.2`）、打开 `PreferShader`，并给元素叠加一个 `TranslateTransform`（与投影组合），然后按 40–50 ms 交错组合 `RotationY` 与透明度动画。清理时恢复透明度、投影与渲染变换，并对元素 `InvalidateVisual()`（走过着色器路径的元素使用独立表面合成，不重绘会残留最后一帧）。

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
| `CustomMessageBox` | `ContentControl` API，`Show()`/`Dismiss()` 由 WPF `Window` 承载，内容直接播放 Swivel |
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

主题覆盖 toolkit 控件，并为常用 WPF 控件提供隐式 WP 样式：`Button`、`ButtonBase`、`TextBox`、`ListBox`/`ListBoxItem`、`ComboBox`/`ComboBoxItem`、`CheckBox`、`RadioButton`、`ScrollBar`、`Thumb`、`ToggleButton`、`ProgressBar`、`ContextMenu`/`MenuItem`/`Separator`、`ToolTip`、`TextBlock` 与 `ContentControl`。`ListBoxItem` 模板不再包含投影部件，投影直接作用于容器（见 §4.5）。

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
