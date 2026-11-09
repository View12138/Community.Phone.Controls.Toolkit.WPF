# API 兼容矩阵

本矩阵基于 [porting-inventory.md](porting-inventory.md) 的源码审计。“完全兼容”表示公共 API 和观察到的行为已实现并有测试覆盖；它**不**表示任意 Windows Phone XAML 可以原样编译——WPF 仍需要修改命名空间，投影还需要包装元素。

## 兼容级别

| 级别 | 含义 |
|---|---|
| **A. 完全兼容** | 在支持的场景中 API 和行为与原始实现一致，并有测试。 |
| **B. API 兼容、行为有差异** | 名称、DP、事件和总体行为存在，但 WPF 实现使用不同宿主或平台服务。 |
| **C. 需要迁移** | XAML 或 C# 必须修改（命名空间、包装、不同基类）。 |
| **D. 替代/包装** | 需要新的 WPF 专用类型来表达该功能。 |
| **E. 不支持** | 没有兼容实现；请使用替代方案或保留 WP 应用。 |

## 命名空间与类型映射

| WP 原始类型 | WPF 移植类型 | 级别 | 说明 |
|---|---|---|---|
| `Microsoft.Phone.Controls.PlaneProjection`（平台 `System.Windows.Media.PlaneProjection`） | `Microsoft.Phone.Controls.PlaneProjection` | C/D | 使用 `phone:` 前缀；赋给元素上的 `ProjectionService.Projection`（按需包装）或赋给 `ProjectionHost`。 |
| `Microsoft.Phone.Controls.Projection`（平台） | `Microsoft.Phone.Controls.Projection` | D | 新基类，继承 `Animatable`。 |
| `Microsoft.Phone.Controls.Matrix3DProjection`（平台） | `Microsoft.Phone.Controls.Matrix3DProjection` | C/D | 矩阵使用 0..1 归一化坐标。 |
| `UIElement.Projection` | `ProjectionHost.Projection` / `ProjectionService.Projection` | C/D | WPF 元素没有原地投影属性；赋值时会按需把元素包进 `ProjectionHost`，且 `(ProjectionService.Projection).(PlaneProjection.RotationY)` 可作为 storyboard 路径。 |
| `Microsoft.Phone.Controls.PhoneApplicationFrame` | 同名 | B/C | 继承 WPF `Frame`，增加页面参数导航。 |
| `Microsoft.Phone.Controls.PhoneApplicationPage` | 同名 | B/C | 继承 WPF `Page`；生命周期由 Frame 触发；返回导航保留页面实例（`KeepAlive = true`，需要新实例时设为 `false`）。 |
| `System.Windows.Navigation.NavigationContext` | `Microsoft.Phone.Controls.NavigationContext` | B/C | WPF 无等价类型；提供 `Uri`、`QueryString`、`ExtraData`。 |
| `Microsoft.Phone.Shell.PhoneApplicationService` | 同名 | B | 映射到 WPF 应用/窗口生命周期。 |
| `Microsoft.Phone.Shell.SystemTray` | 同名 | B | API/状态已实现；视觉 chrome 尚未完成。 |
| `Microsoft.Phone.Shell.ApplicationBar`（含 `IApplicationBar`、`IApplicationBarIconButton`、`IApplicationBarMenuItem`、`ApplicationBarIconButton`、`ApplicationBarMenuItem`、`ApplicationBarMode`、`ApplicationBarStateChangedEventArgs`） | 同名 | A/B | 保留原版 API 形状（最多 4 个图标按钮、最多 50 个菜单项、文本/图标校验、`StateChanged`）；应用栏由 frame 承载的 `ApplicationBarPresenter` 绘制，并像原版一样为页面预留空间。 |
| `Microsoft.Phone.Controls.TransitionFrame` | 同名 | B | 模板部件是 `ProjectionHost`，不是 `ContentPresenter`。 |
| `Microsoft.Phone.Controls.ITransition` 及转场类型 | 同名 | A/B | 相同 API 面；使用 WPF Storyboard 和状态跟踪。 |
| `Microsoft.Phone.Controls.ContextMenu` | 同名 | B/C | 继承 WPF `ContextMenu`。 |
| `Microsoft.Phone.Controls.WrapPanel` | 同名 | A | 与 WPF 自带 `System.Windows.Controls.WrapPanel` 并存。 |
| `Microsoft.Phone.Controls.Pivot`、`PivotItem`、`Microsoft.Phone.Controls.Primitives.PivotHeadersControl`、`PivotHeaderItem` | 同名 | A/B | 使用 `phone:` / `primitives:` 前缀；页眉基础类型保持公开，`PivotHeadersControl.VisualFirstIndex` 语义不变（左边缘的索引）。 |
| `System.Windows.Controls.HierarchicalDataTemplate`（toolkit） | `System.Windows.HierarchicalDataTemplate` | A | 使用 WPF 原生类型。 |
| `System.Windows.Controls.HeaderedItemsControl`（toolkit） | `System.Windows.Controls.HeaderedItemsControl` | A | 使用 WPF 原生类型。 |
| `System.Windows.Controls.LengthConverter` / `TimeTypeConverter`（toolkit） | 未重新定义 | E | 使用 WPF 类型转换器或在应用代码中解析。 |
| `Microsoft.Phone.Maps.Toolkit.*` | 未实现 | E | 没有 WPF 地图提供程序。 |
| `Microsoft.Phone.BackgroundTransfer` / `TransferControl` | 未实现 | E | 没有 WPF 等价服务。 |
| `Windows.Phone.Speech.Recognition` / `SpeechTextBox` | 未实现 | E | 没有 WPF 等价语音服务。 |
| 所有转场工厂 | 同名 | A/B | storyboard 直接使用原版 XAML 资源在运行时解析；角度、关键时间和缓动都来自原文件。工厂抛异常会向上传播，与原版一致（没有回退事件）。 |

## 投影与效果

| 原始类型 | 移植 | 级别 | XAML 修改 | 行为差异 |
|---|---|---|---|---|
| `PlaneProjection` | `PlaneProjection` | C/D | `phone:` 前缀；赋给元素或 `ProjectionHost` | 通过 `Viewport2DVisual3D` 做真实 3D；使用 XAML 固定的 999 DIP 透视深度；旋转、中心和偏移的正负与 XAML 一致。元素的内容会被裁剪到布局边界，但投影结果不会被裁剪（与平台一致）。接收投影的元素会按需被包进 `ProjectionHost`。 |
| `Matrix3DProjection` | `Matrix3DProjection` | C/D | 同上 | 矩阵归一化 0..1，转换为像素空间。 |
| `TiltEffect` | `TiltEffect` | C/D | `phone:TiltEffect.IsTiltEnabled` 可用于容器 | 普通控件按需包装；使用 `PreviewMouseDown/Move/Up`，容器可倾斜子按钮而不吞按钮点击；跳过列表 item 容器。 |
| `TurnstileFeatherEffect` | `TurnstileFeatherEffect` | C/D | `phone:TurnstileFeatherEffect.FeatheringIndex` | 目标必须可包装；跳过 item 容器/content presenter。 |
| `SlideInEffect` | 未实现 | E | — | 该效果在 Pivot 选中项变化时播放逐项动画；`Pivot` 已实现，效果本身尚未移植。 |

## 导航与转场

| 原始 | 移植 | 级别 | 说明 |
|---|---|---|---|
| `PhoneApplicationFrame` | 同名 | B/C | WPF journal/`NavigationService`；方向由尺寸推断。 |
| `PhoneApplicationPage` | 同名 | B/C | `State`、方向、事件；生命周期由 Frame 触发；返回导航保留现有实例（`KeepAlive = true`）。 |
| `TransitionFrame` | 同名 | B | 自定义模板，包含两个 `ProjectionHost`。 |
| `TransitionService.NavigationInTransition` | 同名 | A | 保留附加属性名。 |
| `TransitionService.NavigationOutTransition` | 同名 | A | 保留附加属性名。 |
| `NavigationInTransition` / `NavigationOutTransition` | 同名 | A | 保留 Forward/Backward DP。 |
| `TurnstileTransition` | 同名 | A | 保留 Mode 值。 |
| `TurnstileTransitionMode` | 同名 | A | ForwardIn/ForwardOut/BackwardIn/BackwardOut。 |
| `SlideTransition` | 同名 | A | 保留模式。 |
| `RotateTransition` | 同名 | A | 保留模式；保留 FlowDirection 调整。 |
| `SwivelTransition` | 同名 | A | 保留模式。 |
| `RollTransition` | 同名 | A | 单一模式。 |
| `TurnstileFeatherTransition` | 同名 | A/B | `Mode`、`BeginTime`；使用包装目标。 |
| `FeatheredTransition` | 同名 | A/B | 保留公共类型。 |

原始 27 个 `Transitions/Storyboards/*.xaml` 以 WPF 资源形式随包发布并在运行时加载，因此时长、关键时间、角度和缓动都是原值。文件内唯一的改动是投影属性路径（`(phone:ProjectionService.Projection).(phone:PlaneProjection.X)`），因为 WPF 没有 `UIElement.Projection`。

## 控件

| 控件 | 移植状态 | 级别 | 保留内容 | 差异 |
|---|---|---|---|---|
| `AutoCompleteBox` | 未实现 | E | — | 大型 Silverlight popup/选择基础设施未移植。 |
| `ContextMenu` / `MenuItem` / `Separator` | 已实现 | B/C | `ContextMenuService` 附加属性；`Owner`、`IsZoomEnabled`、`IsFadeEnabled`、`RegionOfInterest` | 继承 WPF 菜单类型；右键打开。 |
| `CustomMessageBox` | 已实现 | B | `Show`、`Dismiss`、`Dismissing`、`Dismissed`、按钮、标题/说明/消息、结果枚举 | 由 WPF `Window` 承载；视觉布局在代码中重建。 |
| `DatePicker` | 已实现 | B | `Value`、`ValueString`、`ValueStringFormat`、`ValueChanged`、`Header`、`PickerPageUri` | 在模态 Window 中使用 WPF `DatePicker`；picker page 未移植。 |
| `TimePicker` | 已实现 | B | 同上 | 在模态 Window 中使用小时/分钟 ComboBox。 |
| `RecurringDaysPicker` | 已实现 | A/B | 星期列表、多选、摘要 | 基于移植的 `ListPicker`；使用本地化星期名。 |
| `ExpanderView` | 已实现 | B | `Expander`、`ExpanderTemplate`、`NonExpandableHeader`、`HasItems`、`IsExpanded`、`Expanded`/`Collapsed` | WPF `HeaderedItemsControl`；简化高度动画；部件 `Presenter`/`ExpanderPanel`/`ItemsCanvas`。 |
| `HyperlinkButton` | 已实现（行为） | A/B | `NavigateUri`、`TargetName`、内容、Click、`CommonStates` 视觉状态、`ButtonBase` 键盘激活，以及带 `IInvokeProvider` 的超链接自动化对等体 | 相对/pack URI 导航最近的 `NavigationService`/`Frame`；绝对非 pack URI 交给系统默认处理程序（浏览器、邮件客户端等）。WPF 没有命名导航目标，`TargetName` 保留但不生效。默认模板把字符串内容渲染为下划线文本；非字符串内容需要自定义模板。 |
| `HubTile` | 已实现 | B | `Source`、`Title`、`Notification`、`Message`、`DisplayNotification`、`IsFrozen`、`GroupTag`、`Size`；`HubTileService` | 定时器驱动循环。仅模板层面的适配：滑动偏移由代码施加，翻转走 3D 路径（双面），`CenterOfRotationY` 取 `0.5`（原版 `0.25` 相对两倍高表面）。 |
| `ListPicker` | 已实现 | A/B | `SelectedIndex`、`SelectedItem`、`SelectedItems`、`SelectionMode`、`Header`、`FullModeHeader`、`FullModeItemTemplate`、`ItemCountThreshold`、`ExpansionMode`、`PickerPageUri`、`Open()`、`SelectionChanged`、`ListPickerMode` | 完整模式导航到原版 `ListPickerPage`（pack URI 来自 `PickerPageUri`），点 `Done`/返回时提交选择；页面操作用原版 `ApplicationBar`（勾/叉图标按钮）。普通模式与原版一致，对模板 Canvas 做高度动画。 |
| `LongListSelector`（WP8.1） | 已实现（行为） | A/B | `ItemsSource`（平铺或分组）、`IsGroupingEnabled`、`HideEmptyGroups`、`ItemTemplate`、`GroupHeaderTemplate`、`GroupFooterTemplate`、`ListHeader`/`ListFooter`（及模板）、`LayoutMode`、`GridCellSize`、`SelectedItem`、`ScrollTo`、`ItemRealized`/`ItemUnrealized`、`JumpListStyle`、`JumpListOpening`/`Closed`、`SelectionChanged`、`ManipulationState(Changed)`、模板部件 `ViewportControl`/`VerticalScrollBar` | 列表模式使用可回收虚拟化的 WPF items host；网格模式使用固定单元格、非虚拟化面板（跳转列表/图库规模较小）。分组对象必须实现 `IList`，与手机一致。点击组页眉会打开 `Popup` 跳转列表；原版 `PlaneProjection` 翻转动画由简单的 popup 取代。`ViewportControl` 是内容宿主（`ContentControl`，不是手机的滚动器），`VerticalScrollBar` 驱动内部 `ScrollViewer`。分组/项目绑定到真实数据对象，容器与手机一致为 `ContentPresenter`。`SelectedItem` 同时是依赖属性（附加）。 |
| `LongListMultiSelector`（WP8） | 未实现 | E | — | 依赖 WP8 列表基础设施。 |
| Maps 扩展 | 未实现 | E | — | 无 WPF 地图提供程序。 |
| `MultiselectList` | 未实现 | E | — | 未移植。 |
| `PhoneTextBox` | 已实现 | A/B | `Hint`、`HintStyle`、`ActualHintVisibility`、`LengthIndicatorVisible`、`LengthIndicatorThreshold`、`DisplayedMaxLength`、`ActionIcon`、`HidesActionItemWhenEmpty`、`ActionIconTapped` | 内部 TextBox 使用双向 Text 绑定。 |
| `Rating` / `RatingItem` | 已实现 | A/B | `FilledItemStyle`、`UnfilledItemStyle`、`RatingItemCount`、`Value`、`ReadOnly`、`AllowHalfItemIncrement`、`AllowSelectingZero`、`ShowSelectionHelper`、`Orientation`、`ValueChanged` | 星形项目在代码中生成；保留模板部件。 |
| `ToggleSwitch` / `ToggleSwitchButton` | 已实现 | A/B | `Header`、`HeaderTemplate`、`SwitchForeground`、`IsChecked`、`Checked`/`Unchecked`/`Indeterminate`/`Click`；开关模板部件 | WPF 视觉状态和 manipulation。 |
| `TransferControl` | 未实现 | E | — | 需要 WP 后台传输服务。 |
| `SpeechTextBox` | 未实现 | E | — | 需要 Windows Phone 语音。 |
| `PerformanceProgressBar` | 已实现 | B | `IsIndeterminate`、`ActualIsIndeterminate` | 不确定模板；简单淡入淡出。 |
| `RelativeAnimatingContentControl` | 已实现 | B | 公共 content control | 未移植 magic-number 相对动画重写。 |
| `WrapPanel` | 已实现 | A | `ItemHeight`、`ItemWidth`、`Orientation`、布局算法 | 测试未发现差异。 |
| `Pivot` / `PivotItem` | 已实现 | A/B | `HeaderTemplate`、`SelectedIndex`、`SelectedItem`、`Title`、`TitleTemplate`、`ItemContainerStyle`、`IsLocked`、`LoadingPivotItem`、`LoadedPivotItem`、`UnloadingPivotItem`、`UnloadedPivotItem`、`SelectionChanged`；模板部件 `PivotItemPresenter`、`HeadersListElement`、`TitleElement`；`PivotItem.Header` 与 `Position States` 视觉状态 | 各项在 `Grid` 中堆叠，只有当前项可见且可交互。鼠标拖拽（或超过半个视口的快速滑动）、点击页眉、方向键 Left/Right/Home/End 均可切换；进入项以 0.25 秒 `QuarticEase` 滑入、离开项滑出，页眉条以 0.5 秒滚动；拖拽跟随指针速度为原版 600 px/s。`Pivot.IsLocked` 会淡出页眉并拒绝修改项。 |
| `PivotHeadersControl` / `PivotHeaderItem` | 已实现 | A/B | `VisualFirstIndex`、`ItemContainerStyle`、`IsSelected`、`SelectionStates` 视觉状态 | 页眉条在 `Canvas` 上循环排布（最后一个页眉之后接第一个），以 0.5 秒 `QuarticEase` 滚动；手机版本由合成器移动同一条页眉带 |
| `LockablePivot` | 未实现 | E | — | 已由 `Pivot.IsLocked` 取代（手机 8.1 的 `Pivot` 本身即提供该属性）。 |
| `GestureListener` / `GestureService` | 未实现 | E | — | WPF 触摸/manipulation API 不同。 |
| `LoopingSelector` / `LoopingSelectorItem` | 未实现 | E | — | WP 日期/时间页内部使用；移植 picker 不需要。 |
| `ClipToBounds` | 已实现 | A | `IsEnabled` 附加属性 | 使用 WPF `Clip`。 |
| `LengthConverter` / `TimeTypeConverter` | 未重新定义 | E | — | 使用 WPF 原生转换器。 |

## 转换器与本地化

| 类型 | 状态 | 说明 |
|---|---|---|
| `DailyDateTimeConverter` | 已实现 | 原格式化和校验。 |
| `HourlyDateTimeConverter` | 已实现 | 原格式化和校验。 |
| `FullViewDateTimeConverter` | 已实现 | 原文化分支。 |
| `ListViewDateTimeConverter` | 已实现 | 原格式化。 |
| `ThreadDateTimeConverter` | 已实现 | 原格式化。 |
| `RelativeTimeConverter` | 已实现 | 原阈值和复数规则；使用内嵌 `ControlResources`。 |
| `OffOnConverter` | 已实现 | 使用本地化 On/Off。 |
| `ControlResources` | 已实现 | 51 个 resx 内嵌；50 个真实文化加中性资源。 |

## 重要依赖属性默认值

| 类型 | 属性 | 默认值 |
|---|---|---|
| `PlaneProjection` | `CenterOfRotationX/Y` | `0.5` |
| `PlaneProjection` | `CenterOfRotationZ`、旋转、偏移 | `0` |
| `Rating` | `RatingItemCount` | `5` |
| `Rating` | `Value` | `0` |
| `ToggleSwitch` | `IsChecked` | `false` |
| `ListPicker` | `SelectedIndex` | `-1` |
| `ListPicker` | `ItemCountThreshold` | `5` |
| `ListPicker` | `ExpansionMode` | `ExpansionAllowed` |
| `ExpanderView` | `IsExpanded` | `false` |
| `DatePicker` | `Value` | 构造时为今天 |
| `TimePicker` | `Value` | 构造时为当前时间 |
| `PerformanceProgressBar` | `IsIndeterminate` / `ActualIsIndeterminate` | `false` |

## XAML 兼容边界

| XAML 形式 | 结果 |
|---|---|
| `xmlns:phone="clr-namespace:Microsoft.Phone.Controls;assembly=Community.Phone.Controls.Toolkit.WPF"` | 所有移植类型必需。 |
| `<phone:ToggleSwitch ... />`、`<phone:Rating ... />`、`<phone:WrapPanel ... />` | 仅修改命名空间即可（以及应用使用的 WPF 资源键）。 |
| `<phone:HyperlinkButton NavigateUri="..." />` | 可用：相对 URI 导航最近帧；外部 URI 打开系统处理程序。`TargetName` 保留，但命名目标不受支持。 |
| 使用 `phone:` 前缀的 `<toolkit:TransitionService.NavigationInTransition>...` | 可用。 |
| `<Grid.Projection><PlaneProjection .../></Grid.Projection>` | WPF 不编译；改用 `ProjectionService.Projection`（按需包装）或 `ProjectionHost`。 |
| `<Grid phone:ProjectionService.Projection="...">` | 可用：元素会按需被包进 `ProjectionHost`（需为 Panel 的直接子元素，且该 Panel 不是 items panel），清除投影时自动解包。 |
| `<phone:ProjectionHost.Projection><phone:PlaneProjection .../></phone:ProjectionHost.Projection>` | 可用。 |
| 使用 `shell:SystemTray.IsVisible` 的原始 `PhoneApplicationPage` XAML | 使用移植 `shell` 命名空间可解析；视觉 chrome 有限。 |
| 原始 `shell:ApplicationBar` XAML | 可用：应用栏（含菜单、最小化模式与为页面预留的空间）由 frame chrome（`ApplicationBarPresenter`）渲染。 |
| 原始 `phone:PhoneApplicationPage` 的 `SupportedOrientations`/`Orientation` | 可解析并保留；不会强制旋转桌面窗口。 |
| `phone:LongListSelector` XAML | 可用；分组对象必须实现 `IList`，跳转列表是 WPF `Popup`。 |
| `phone:AutoCompleteBox`、地图、语音、传输 XAML | 当前 preview 不支持。 |

## 按功能的兼容注意事项

1. **投影与 3D**：真实 3D 渲染和动画已实现。原始 `<Grid.Projection>` 形式不编译，但把 `ProjectionService.Projection`（或 `element.Projection`）赋给任意元素时会按需把它包进 `ProjectionHost`，无需手动包装；模板中仍建议显式使用 Host。相机复现了 XAML 固定的 999 DIP 透视深度，因此缩放与透视收缩与 WP 一致；只有依赖“透视原点移出元素边界”的效果存在差异（见已知限制）。
2. **转场**：时序、角度和模式与原始 storyboard 一致。由于 WPF `GetCurrentState()` 需要可控宿主，移植版自行跟踪 storyboard 状态。
3. **导航**：保留 WPF `Frame`/`Page` 和 journal 行为，但 WP 页面生命周期由移植 Frame 触发；`PhoneApplicationService` 的 idle 检测策略没有 WPF 系统等价物。
4. **效果**：TiltEffect 和 TurnstileFeatherEffect 使用真实投影管线。无法包装的目标会被跳过：items panel 内的 item 容器（子元素由 WPF 管理）以及不是 Panel 直接子元素的元素；作为 Panel 子元素的 `ContentPresenter` 可以正常包装。
5. **控件**：DP 面和核心行为保留；WPF 没有对应 picker/popup 基础设施时，视觉细节和模态宿主不同。
6. **本地化**：包含原始 resx 资源；`ControlResources` 使用 `CurrentUICulture`。
7. **不支持模块**：AutoCompleteBox、LongListMultiSelector、MultiselectList、地图、传输、语音、`LockablePivot`、SlideInEffect 和手势 API 明确不支持，没有空壳。`LongListSelector` 已按上述 items-host 模型实现。

## 验证状态

| 证据 | 位置 |
|---|---|
| 构建（库、示例、测试） | `dotnet build -c Debug/Release` |
| 投影渲染/方向/命中测试 | `tests/.../ProjectionTests.cs` |
| 控件/API 测试 | `tests/.../ControlTests.cs`、`ShellTests.cs`、`ConverterTests.cs` |
| Pivot 测试 | `tests/.../PivotTests.cs` |
| 导航/转场测试 | `tests/.../NavigationTests.cs` |
| 效果测试 | `tests/.../EffectsTests.cs` |
| 示例运行冒烟 | `src/Community.Phone.Controls.Toolkit.WPF.Sample` + `%TEMP%\community-phone-toolkit-sample-error.txt` |
| 包安装测试 | `build/verify-package.ps1` + `tests/PackageConsumer`（隔离缓存，构建并运行） |

实现状态变化时必须同步更新本矩阵；没有构建/测试或明确手动证据的功能不得声称完成。
