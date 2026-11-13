# API 兼容矩阵

本矩阵逐类型列出移植版提供的公共 API 与原始 Windows Phone Toolkit / `Microsoft.Phone` 的对应关系。“完全兼容”指公共 API 与观察到的行为已实现；它**不**表示任意 Windows Phone XAML 可以原样编译 —— WPF 仍需要调整命名空间，投影还需要包装元素。

## 兼容级别

| 级别 | 含义 |
|---|---|
| **A. 完全兼容** | 在支持的场景中 API 与行为与原实现一致。 |
| **B. API 兼容、行为有差异** | 名称、依赖属性、事件和总体行为存在，但 WPF 实现使用不同宿主或平台服务。 |
| **C. 需要迁移** | XAML 或 C# 必须修改（命名空间、包装、不同基类）。 |
| **D. 替代/包装** | 需要新的 WPF 专用类型表达该功能。 |
| **E. 不支持** | 没有兼容实现。 |

## 平台类型

| 原始类型 | 移植类型 | 程序集 | 级别 | 说明 |
|---|---|---|---|---|
| `Microsoft.Phone.Controls.PhoneApplicationFrame` | 同名 | `Community.Phone` | B/C | 继承 WPF `Frame`；增加 `Orientation`、`SupportedOrientations`、`OrientationChanged`、`IsPortrait`、`ApplicationBar`、`Navigate(page, parameters)`、`GoBack()`（先触发 `BackKeyPress`）。 |
| `Microsoft.Phone.Controls.PhoneApplicationPage` | 同名 | `Community.Phone` | B/C | 继承 WPF `Page`；`State`、`NavigationContext`、`ApplicationBar`、生命周期虚方法与对应事件；`KeepAlive = true`。 |
| `System.Windows.Navigation.NavigationContext` | `Microsoft.Phone.Controls.NavigationContext` | `Community.Phone` | B/C | WPF 无等价类型；提供 `Uri`、`QueryString`（大小写不敏感）、`ExtraData`、静态 `Empty`。 |
| `Microsoft.Phone.Controls.PageOrientation` / `SupportedPageOrientation` | 同名 | `Community.Phone` | A | 枚举值一致。 |
| `Microsoft.Phone.Shell.PhoneApplicationService` | 同名 | `Community.Phone` | B | `Current`、`State`、`Launching`/`Activated`/`Deactivated`/`Closing` 事件、idle 检测模式枚举。 |
| `Microsoft.Phone.Shell.SystemTray` | 同名 | `Community.Phone` | B | **静态类**，`IsVisible`、`Opacity`、`BackgroundColor`、`ForegroundColor`、`ProgressIndicator` 映射到 `SystemTrayState.Current`；不绘制状态栏，也不是附加属性。 |
| `Microsoft.Phone.Shell.SystemTrayState`、`ProgressIndicator` | 同名 | `Community.Phone` | D | 移植新增的状态载体（`INotifyPropertyChanged`）。 |
| `Microsoft.Phone.Shell.ApplicationBar`（含 `IApplicationBar`、`IApplicationBarIconButton`、`IApplicationBarMenuItem`、`ApplicationBarIconButton`、`ApplicationBarMenuItem`、`ApplicationBarMode`、`ApplicationBarStateChangedEventArgs`） | 同名 | `Community.Phone` | A/B | 保留原版 API（4 个图标按钮、50 个菜单项上限、文本/图标校验、`Opacity` 截断、`StateChanged`）；由 `ApplicationBarPresenter` 绘制。 |
| `Application.RequestedTheme`（平台） | `ThemeManager` 的 `RequestedTheme` 扩展属性 | `Community.Phone` | D | `application.RequestedTheme = ...` 立即应用调色板；`ThemeManager` 是静态类，另提供 `AccentColor`、`CustomAccentColor`、`InitializeTheme()`。 |
| —（移植新增） | `ApplicationTheme`、`AccentColor`、`AccentColors` | `Community.Phone` | D | `ApplicationTheme.Auto/Light/Dark`，`AccentColor.Auto` 加 20 个 Metro 强调色；`AccentColors.From` 返回对应 `Color`。 |
| —（移植新增） | `ThemePalette`、`ThemeChangedEventArgs` | `Community.Phone` | D | 调色板记录类型与主题变更事件参数。 |
| `Microsoft.Phone.Controls.Pivot`、`PivotItem` | 同名 | `Community.Phone` | A/B | `HeaderTemplate`、`SelectedIndex`、`SelectedItem`、`Title`、`TitleTemplate`、`IsLocked`、四个 `*PivotItem` 事件、`SelectionChanged`。 |
| `Microsoft.Phone.Controls.Primitives.PivotHeadersControl`、`PivotHeaderItem` | 同名 | `Community.Phone` | A/B | `VisualFirstIndex`、`IsSelected`、`SelectionStates`。 |
| `Microsoft.Phone.Controls.Primitives.ViewportControl` | 同名 | `Community.Phone` | B | `ContentControl`，作为 `LongListSelector` 的内容宿主与模板部件，不是手机的滚动器。 |
| `Microsoft.Phone.Controls.LongListSelector`（WP8.1） | 同名 | `Community.Phone` | A/B | 见[控件](#控件)。 |
| `Microsoft.Phone.Controls.HyperlinkButton` | 同名 | `Community.Phone` | A/B | 相对/pack URI 导航最近 frame；`TargetName` 保留但命名目标不受支持。 |
| `Microsoft.Phone.Maps.Toolkit.*` | 未实现 | — | E | 没有 WPF 地图提供程序。 |
| `Microsoft.Phone.BackgroundTransfer` / `TransferControl` | 未实现 | — | E | 没有 WPF 等价服务。 |
| `Windows.Phone.Speech.Recognition` / `SpeechTextBox` | 未实现 | — | E | 没有 WPF 等价语音服务。 |

## 投影与效果

| 原始类型 / 用法 | 移植类型 | 级别 | XAML 修改 | 行为差异 |
|---|---|---|---|---|
| `UIElement.Projection` | `ProjectionHost.Projection` / `ProjectionService.Projection` | C/D | 显式 `ProjectionHost`，或在元素上写 `phone:ProjectionService.Projection`（按需包装） | WPF 元素没有原地投影属性；storyboard 路径为 `(ProjectionService.Projection).(PlaneProjection.RotationY)`。 |
| `System.Windows.Media.PlaneProjection` | `Microsoft.Phone.Controls.PlaneProjection` | C/D | `phone:` 前缀 | 通过 `Viewport2DVisual3D` 做真实 3D；固定 999 DIP 透视深度；旋转、中心、偏移的正负与 XAML 一致。 |
| `System.Windows.Media.Matrix3DProjection` | `Microsoft.Phone.Controls.Matrix3DProjection` | C/D | 同上 | 矩阵使用 0..1 归一化坐标。 |
| — | `Projection`、`ProjectionService`、`ProjectionHost` | D | — | 移植新增：投影基类、附加属性服务与 3D 宿主。 |
| `TiltEffect` | `TiltEffect` | C/D | `phone:TiltEffect.IsTiltEnabled`、`SuppressTilt`、`TiltableItems` | 普通控件按需包装；使用隧道式 `PreviewMouse*`，容器可倾斜子按钮而不吞点击；跳过 item 容器。 |
| `TurnstileFeatherEffect` | `TurnstileFeatherEffect` | C/D | `phone:TurnstileFeatherEffect.FeatheringIndex` | 目标必须可包装；跳过 item 容器；目标从页面可视树收集。 |
| `SlideInEffect` | 未实现 | E | — | `Pivot` 已实现，效果本身未移植。 |

## 导航与转场

| 原始 | 移植 | 级别 | 说明 |
|---|---|---|---|
| `PhoneApplicationFrame` | 同名 | B/C | WPF journal / `NavigationService`；方向由尺寸推断。 |
| `PhoneApplicationPage` | 同名 | B/C | 生命周期由 Frame 触发；返回导航保留现有实例。 |
| `TransitionFrame` | 同名 | B | 自定义模板，部件是两个 `ProjectionHost`。 |
| `TransitionService.NavigationInTransition` / `NavigationOutTransition` | 同名 | A | 保留附加属性名与 `Forward`/`Backward` 依赖属性。 |
| `NavigationInTransition` / `NavigationOutTransition` / `NavigationTransition` | 同名 | A | 保留类型层次。 |
| `ITransition` / `Transition` | 同名 | A/B | 保留 API 面；用 WPF `Storyboard` 与自维护状态。 |
| `TransitionElement` | 同名 | A | 保留抽象基类与 `GetTransition` 契约。 |
| `TurnstileTransition` / `TurnstileTransitionMode` | 同名 | A | 4 个模式。 |
| `TurnstileFeatherTransition` / `TurnstileFeatherTransitionMode` | 同名 | A/B | 4 个模式；使用包装目标。 |
| `SlideTransition` / `SlideTransitionMode` | 同名 | A | 8 个模式。 |
| `RotateTransition` / `RotateTransitionMode` | 同名 | A | 8 个模式。 |
| `SwivelTransition` / `SwivelTransitionMode` | 同名 | A | 6 个模式。 |
| `RollTransition` | 同名 | A | 单一模式。 |
| `FeatheredTransition` | 同名 | A/B | 保留公共类型。 |

原版 27 个 `Transitions/Storyboards/*.xaml` 以 WPF 资源随包发布并在运行时加载，时长、关键时间、角度和缓动都是原值；文件内唯一的改动是投影属性路径。`Transitions.cs` 中的 `TurnstileCenterOfRotationX = -0.05` 与 `TurnstileDurationScale = 1.2` 是仅移植版的调优常量。

## Shell 与主题

| 类型 | 状态 | 说明 |
|---|---|---|
| `ApplicationBar` | 实现 | `[ContentProperty("Buttons")]`、`IsVisible`、`Opacity`（截断到 0..1）、`IsMenuEnabled`、`BackgroundColor`、`ForegroundColor`、`Mode`、`DefaultSize`（72）、`MiniSize`（30）、`Buttons`、`MenuItems`、`StateChanged`。 |
| `ApplicationBarPresenter` | 实现（移植新增） | 绘制应用栏、图标槽、菜单按钮与 `Popup` 菜单；`ReservedHeight` 驱动页面预留高度。 |
| `SystemTray` | 状态实现 | 无视觉 chrome、无附加属性；XAML 中 `shell:SystemTray.IsVisible` 不可用。 |
| `PhoneApplicationService` | 状态实现 | `State`、生命周期事件、idle 检测模式；不复刻 tombstoning。 |
| `ThemeManager` | 实现（移植新增） | `AvailableApplicationThemes`、`AvailableAccentColors`、`ActualTheme`、`ActualAccentColor`、`ThemeChanged`；扩展属性 `RequestedTheme`、`AccentColor`、`CustomAccentColor`、方法 `InitializeTheme()`；`ResolveDictionary`、`GetSystemAccentColor`。 |
| `ThemePalette` | 实现（移植新增） | 24 个 `Phone*Color` 属性、`IsDark` 相关的 Visibility/Opacity 值、`PivotUnselectedItemOpacity`、`WithAccent`、`Dark`/`Light` 静态调色板、`ResourceKeys`。 |

## 控件

| 控件 | 状态 | 级别 | 保留内容 | 差异 |
|---|---|---|---|---|
| `ContextMenu` / `MenuItem` / `Separator` / `ContextMenuService` | 已实现 | B/C | `ContextMenuService` 附加属性；`Owner`、`IsZoomEnabled`、`IsFadeEnabled`、`RegionOfInterest` | 继承 WPF 菜单类型；右键打开；部分 WP zoom/fade 视觉未复刻。 |
| `CustomMessageBox` | 已实现 | B | `Show`、`Dismiss`、`Dismissing`、`Dismissed`、`LeftButtonContent`、`RightButtonContent`、`IsLeftButtonEnabled`/`IsRightButtonEnabled`、`Title`（继承）、`Caption`、`Message`、`IsFullScreen`、`IsCenterScreen`、`CustomMessageBoxResult` | 由 WPF `Window` 承载；弹入/关闭播放 Swivel。 |
| `DatePicker` / `TimePicker` | 已实现 | B | `Value`、`ValueString`、`ValueStringFormat`、`ValueChanged`、`Header`、`HeaderTemplate`、`PickerPageUri` | 在模态 `Window` 中用 WPF 日期/时间 UI；picker page 未移植。 |
| `RecurringDaysPicker` | 已实现 | A/B | 星期多选与摘要 | 继承 `ListPicker`；本地化星期名。 |
| `ExpanderView` | 已实现 | B | `Expander`、`ExpanderTemplate`、`NonExpandableHeader`、`NonExpandableHeaderTemplate`、`IsExpanded`、`HasItems`、`IsNonExpandable`、`Expanded`/`Collapsed`；部件 `Presenter`/`ExpanderPanel`/`ItemsCanvas` | 基于 `HeaderedItemsControl`；高度动画简化。 |
| `HyperlinkButton` | 已实现 | A/B | `NavigateUri`、`TargetName`、`Click`、`CommonStates`、`IInvokeProvider` | 相对/pack URI 导航最近 frame；外部 URI 交给系统处理程序；`TargetName` 不生效；非字符串内容需要自定义模板。 |
| `HubTile` / `HubTileService` | 已实现 | B | `Source`、`Title`、`Notification`、`Message`、`DisplayNotification`、`IsFrozen`、`GroupTag`、`State`、`Size`、`TileSize` | 定时器驱动状态循环；滑动偏移由代码施加，翻转走 3D 路径，`CenterOfRotationY` 为 `0.5`（原版 `0.25` 相对两倍高表面）。 |
| `ListPicker` / `ListPickerItem` | 已实现 | A/B | `SelectedIndex`、`SelectedItem`、`SelectedItems`、`SelectionMode`、`Header`、`HeaderTemplate`、`FullModeHeader`、`FullModeItemTemplate`、`ShadowItemTemplate`、`ItemCountThreshold`、`ExpansionMode`、`PickerPageUri`、`ListPickerMode`、`IsOpen`、`Open()`、`SelectionChanged`、`SummaryForSelectedItemsDelegate` | 完整模式导航到 `ListPickerPage`，用原版 `ApplicationBar` 确认/取消；普通模式直接对模板 Canvas 做高度/位移动画。 |
| `LongListSelector` | 已实现 | A/B | `ItemsSource`、`IsGroupingEnabled`、`HideEmptyGroups`、`ItemTemplate`、`GroupHeaderTemplate`、`GroupFooterTemplate`、`ListHeader`/`ListFooter`（及模板）、`LayoutMode`、`GridCellSize`、`SelectedItem`、`ScrollTo`、`ItemRealized`/`ItemUnrealized`、`JumpListStyle`、`JumpListOpening`/`JumpListClosed`、`SelectionChanged`、`ManipulationState`/`ManipulationStateChanged`、模板部件 `ViewportControl`/`VerticalScrollBar` | 列表模式回收虚拟化，网格模式固定单元格非虚拟化；跳转列表是 `Popup`；分组对象必须实现 `IList`。 |
| `PhoneTextBox` | 已实现 | A/B | `Hint`、`HintStyle`、`ActualHintVisibility`、`LengthIndicatorVisible`、`LengthIndicatorThreshold`、`DisplayedMaxLength`、`ActionIcon`、`HidesActionItemWhenEmpty`、`ActionIconTapped` | 内部 `TextBox` 使用双向 `Text` 绑定。 |
| `Rating` / `RatingItem` | 已实现 | A/B | `FilledItemStyle`、`UnfilledItemStyle`、`RatingItemCount`、`Value`、`ReadOnly`、`AllowHalfItemIncrement`、`AllowSelectingZero`、`ShowSelectionHelper`、`Orientation`、`StrokeThickness`、`ValueChanged` | 星形项目在代码中生成；半星模式 `Value` 以“星”为单位。 |
| `ToggleSwitch` / `ToggleSwitchButton` | 已实现 | A/B | `Header`、`HeaderTemplate`、`SwitchForeground`、`IsChecked`、`Checked`/`Unchecked`/`Indeterminate`/`Click` | WPF 视觉状态与 manipulation。 |
| `Pivot` / `PivotItem` / `PivotHeadersControl` / `PivotHeaderItem` | 已实现 | A/B | 见上表 | 输入是鼠标拖拽 + 方向键；页眉条循环排布但只绘制一圈；`IsLocked` 是依赖属性。 |
| `PerformanceProgressBar` | 已实现 | B | `IsIndeterminate`、`ActualIsIndeterminate` | 内嵌 WPF `ProgressBar`；简单淡入淡出。 |
| `RelativeAnimatingContentControl` | 已存在 | B | 公共 `ContentControl` 派生类型 | 原版的相对动画重写未移植。 |
| `WrapPanel` | 已实现 | A | `ItemHeight`、`ItemWidth`、`Orientation`、布局算法 | 与 WPF 自带 `System.Windows.Controls.WrapPanel` 并存。 |
| `ClipToBounds` | 已实现 | A | `IsEnabled` 附加属性 | 使用 WPF `Clip`。 |
| `AutoCompleteBox` | 未实现 | E | — | Silverlight popup/选择基础设施未移植。 |
| `LongListMultiSelector`（WP8） | 未实现 | E | — | 依赖 WP8 列表基础设施。 |
| `MultiselectList` | 未实现 | E | — | 未移植。 |
| Maps 扩展 | 未实现 | E | — | 无 WPF 地图提供程序。 |
| `TransferControl` | 未实现 | E | — | 需要 WP 后台传输服务。 |
| `SpeechTextBox` | 未实现 | E | — | 需要 Windows Phone 语音。 |
| `LockablePivot` | 未实现 | E | — | 由 `Pivot.IsLocked` 取代。 |
| `GestureListener` / `GestureService` | 未实现 | E | — | 使用 WPF 触摸/manipulation API。 |
| `LoopingSelector` / `LoopingSelectorItem` | 未实现 | E | — | WP picker 内部实现；移植 picker 使用 WPF 模态窗口。 |
| `LengthConverter` / `TimeTypeConverter` | 已提供 | A | 同名 `TypeConverter` | 位于 `Microsoft.Phone.Controls`，不覆盖 WPF 类型。 |

## 转换器与本地化

| 类型 | 状态 | 说明 |
|---|---|---|
| `DailyDateTimeConverter`、`HourlyDateTimeConverter`、`FullViewDateTimeConverter`、`ListViewDateTimeConverter`、`ThreadDateTimeConverter` | 已实现 | 原始格式化与文化分支；辅助类 `DateTimeFormatHelper`。 |
| `RelativeTimeConverter` | 已实现 | 原阈值与复数规则，使用内嵌 `ControlResources`。 |
| `OffOnConverter` | 已实现 | 使用本地化 On/Off 字符串。 |
| `TileSizeToWidthConverter`、`TileSizeToHeightConverter` | 已实现 | `TileSize` → 尺寸。 |
| `JumpListItemBackgroundConverter`、`JumpListItemForegroundConverter` | 已实现 | 跳转列表磁贴着色。 |
| `ControlResources` | 已实现 | 51 个 `ControlResources*.resx`：50 个文化 + 中性资源，由原始 designer 生成。 |
| `CultureInfoExtensions`、`SortedLocaleGrouping`、`RoutedPropertyChangingEventArgs<T>`、`WeakReferenceHelper` | 已实现 | 被已移植控件使用的辅助类型。 |

## 依赖属性默认值

| 类型 | 属性 | 默认值 |
|---|---|---|
| `PlaneProjection` | `CenterOfRotationX` / `CenterOfRotationY` | `0.5` |
| `PlaneProjection` | `CenterOfRotationZ`、旋转、偏移 | `0` |
| `ProjectionHost` | `PerspectiveDepth` / `Prefer3D` | `999` / `false` |
| `Rating` | `RatingItemCount` / `Value` / 布尔开关 | `5` / `0` / `false` |
| `ToggleSwitch` | `IsChecked` | `false` |
| `ListPicker` | `SelectedIndex` / `ItemCountThreshold` / `ListPickerMode` | `-1` / `5` / `Normal` |
| `ExpanderView` | `IsExpanded` / `HasItems` / `IsNonExpandable` | `false` |
| `DatePicker` / `TimePicker` | `Value` | 构造时为今天 / 当前时间 |
| `PerformanceProgressBar` | `IsIndeterminate` / `ActualIsIndeterminate` | `false` |
| `HubTile` | `State` / `Size` / `IsFrozen` | `Expanded` / `TileSize.Default` / `false` |
| `CustomMessageBox` | `IsLeftButtonEnabled` / `IsRightButtonEnabled` | `true` |
| `PhoneApplicationFrame` | `Orientation` / `SupportedOrientations` | `PortraitUp` / `PortraitOrLandscape` |
| `ApplicationBar` | `DefaultSize` / `MiniSize` / `IsVisible` | `72` / `30` / `true` |

## XAML 兼容边界

| XAML 形式 | 结果 |
|---|---|
| `xmlns:phone="clr-namespace:Microsoft.Phone.Controls;assembly=Community.Phone.Controls.Toolkit"` | toolkit 控件、投影、转场、效果必需。 |
| `xmlns:platform="clr-namespace:Microsoft.Phone.Controls;assembly=Community.Phone"` | `PhoneApplicationPage`、`PhoneApplicationFrame`、`NavigationContext`、`Pivot`、`LongListSelector`、`HyperlinkButton` 必需。 |
| `xmlns:shell="clr-namespace:Microsoft.Phone.Shell;assembly=Community.Phone"` | `ApplicationBar`、`ApplicationBarIconButton`、`ApplicationBarMenuItem` 必需。 |
| `<phone:ToggleSwitch ... />`、`<phone:Rating ... />`、`<phone:WrapPanel ... />` | 只需改命名空间。 |
| `<phone:HyperlinkButton NavigateUri="..." />` | 可用；`TargetName` 不受支持。 |
| `<phone:TransitionService.NavigationInTransition>...` | 可用。 |
| `<Grid.Projection><PlaneProjection .../></Grid.Projection>` | WPF 不编译；改用 `ProjectionService.Projection` 或 `ProjectionHost`。 |
| `<Grid phone:ProjectionService.Projection="...">` | 可用：元素按需被包进 `ProjectionHost`（需为 Panel 的直接子元素且该 Panel 不是 items panel）。 |
| `<phone:ProjectionHost.Projection><phone:PlaneProjection .../></phone:ProjectionHost.Projection>` | 可用。 |
| `shell:SystemTray.IsVisible="..."` | **不可用**：`SystemTray` 是无附加属性的静态类，标记编译失败；请在代码中设置。 |
| `shell:ApplicationBar` 声明在 `PhoneApplicationPage.ApplicationBar` 上 | 可用；由 frame 的呈现器渲染。 |
| `platform:PhoneApplicationPage` 的 `SupportedOrientations` / `Orientation` | 可解析并保留；不强制旋转桌面窗口。 |
| `phone:LongListSelector` | 可用；分组对象必须实现 `IList`，跳转列表是 `Popup`。 |
| `phone:AutoCompleteBox`、地图、语音、传输 XAML | 不支持。 |

## 按功能的兼容注意事项

1. **投影与 3D**：真实 3D 渲染与动画；`<Grid.Projection>` 不编译，但给任意元素赋 `ProjectionService.Projection`（或 `element.Projection`）会按需包装。相机复现 XAML 固定的 999 DIP 透视深度；依赖“透视原点移出元素边界”的效果存在差异（见[已知限制](known-limitations.md#4-投影与-3d)）。
2. **转场**：时序、角度与模式取自原始 storyboard 资源；WPF 的 storyboard 状态查询与 Silverlight 不同，移植版自行跟踪。
3. **导航**：保留 WPF `Frame`/`Page` 与 journal 行为；页面生命周期由移植 Frame 触发；`PhoneApplicationService` 的 idle 策略没有 WPF 系统等价物。
4. **效果**：`TiltEffect` 与 `TurnstileFeatherEffect` 使用真实投影管线。无法包装的目标被跳过：items panel 内的 item 容器、不是 Panel 直接子元素的元素。
5. **控件**：依赖属性面与核心行为保留；WPF 缺少 picker/popup 基础设施时，视觉细节与模态宿主不同。
6. **本地化**：包含原始 resx 资源，`ControlResources` 使用 `CurrentUICulture`。
7. **主题**：`ThemeManager` 在运行时改色；库内模板使用 `DynamicResource`，应用 XAML 也应使用 `DynamicResource`。`Themes/Theme.xaml` 在应用级字典中会被 WPF 冻结，`ThemeManager` 首次应用时把它替换为可变副本。

实现状态或成员发生变化时必须同步更新本矩阵；没有实现依据的功能不得标记为已实现。
