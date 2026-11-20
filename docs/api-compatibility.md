# API 兼容矩阵

本矩阵逐类型列出移植版提供的公共 API 与原始 Windows Phone Toolkit / `Microsoft.Phone` 的对应关系。“完全兼容”指公共 API 与观察到的行为已实现；它**不**表示任意 Windows Phone XAML 可以原样编译 —— WPF 仍需要调整命名空间；投影类型可直接作用于任意 `UIElement`。

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
| `Microsoft.Phone.Controls.PhoneApplicationFrame` | 同名 | `Community.Phone` | B/C | 继承 WPF `Frame`；增加 `Orientation`、`SupportedOrientations`、`OrientationChanged`、`IsPortrait`、`ApplicationBar`、`Navigate(page, parameters)`、`GoBack()`（先触发 `BackKeyPress`）；方向不随尺寸自动变化。 |
| `Microsoft.Phone.Controls.PhoneApplicationPage` | 同名 | `Community.Phone` | B/C | 继承 WPF `Page`；`State`、`NavigationContext`、`ApplicationBar`、生命周期虚方法与对应事件；`OnOrientationChanged` 为受保护虚方法；`KeepAlive = true`。 |
| `System.Windows.Navigation.NavigationContext` | `Microsoft.Phone.Controls.NavigationContext` | `Community.Phone` | B/C | WPF 无等价类型；提供 `Uri`、`QueryString`（大小写不敏感）、`ExtraData`、静态 `Empty`。 |
| `Microsoft.Phone.Controls.PageOrientation` / `SupportedPageOrientation` | 同名 | `Community.Phone` | A | 枚举值一致。 |
| `Microsoft.Phone.Shell.PhoneApplicationService` | 同名 | `Community.Phone` | B | `Current`、`State`、`Launching`/`Activated`/`Deactivated`/`Closing` 事件、idle 检测模式枚举。 |
| `Microsoft.Phone.Shell.SystemTray` | 同名 | `Community.Phone` | B | **静态类**；静态属性 `IsVisible`、`Opacity`、`BackgroundColor`、`ForegroundColor`、`ProgressIndicator` 映射到 `SystemTrayState.Current`；另有 `IsVisibleProperty` 附加属性与 `GetIsVisible`/`SetIsVisible`，XAML 中 `shell:SystemTray.IsVisible` 可用（仅对页面/Frame/`Window` 生效）；不绘制状态栏。 |
| `Microsoft.Phone.Shell.SystemTrayState`、`ProgressIndicator` | 同名 | `Community.Phone` | D | 移植新增的状态载体（`INotifyPropertyChanged`）。 |
| `Microsoft.Phone.Shell.ApplicationBar`（含 `IApplicationBar`、`IApplicationBarIconButton`、`IApplicationBarMenuItem`、`ApplicationBarIconButton`、`ApplicationBarMenuItem`、`ApplicationBarMode`、`ApplicationBarStateChangedEventArgs`） | 同名 | `Community.Phone` | A/B | 保留原版 API（4 个图标按钮、50 个菜单项上限、文本/图标校验、`Opacity` 截断、`StateChanged`）；由 `ApplicationBarPresenter` 绘制。 |
| `Application.RequestedTheme`（平台） | `Application` 的 `RequestedTheme` 扩展属性 | `Community.Phone` | D | 由 `Microsoft.Phone.Controls.ApplicationExtensions` 提供，`application.RequestedTheme = ...` 立即应用调色板；另提供 `AccentColor`、`CustomAccentColor`、`InitializeTheme()`。 |
| —（移植新增） | `ApplicationTheme`、`AccentColor`、`AccentColors` | `Community.Phone` | D | `ApplicationTheme.Auto/Light/Dark`，`AccentColor.Auto` 加 20 个 Metro 强调色；`AccentColors.From` 返回对应 `Color`。 |
| —（移植新增） | `ThemePalette`、`ThemeChangedEventArgs`、`ThemeManager` | `Community.Phone` | D | 调色板记录类型、主题变更事件参数与切色服务；`ThemeManager` 另提供 `AvailableApplicationThemes`、`AvailableAccentColors`、`ActualTheme`、`ActualAccentColor`、`ThemeChanged`、`ResolveDictionary`、`GetSystemAccentColor`。 |
| `Microsoft.Phone.Controls.Pivot`、`PivotItem` | 同名 | `Community.Phone` | A/B | `HeaderTemplate`、`SelectedIndex`、`SelectedItem`、`Title`、`TitleTemplate`、`IsLocked`、四个 `*PivotItem` 事件、`SelectionChanged`。 |
| `Microsoft.Phone.Controls.Panorama`、`PanoramaItem` | 同名 | `Community.Phone` | B | `Title`/`TitleTemplate`、`HeaderTemplate`、`SelectedItem`/`SelectedIndex`、`DefaultItem`，`SelectionChanged`，`PanoramaItem.Header`/`HeaderTemplate`/`Orientation`；WP 手势改用 WPF 指针事件与 storyboard。 |
| `Microsoft.Phone.Controls.Primitives.PivotHeadersControl`、`PivotHeaderItem` | 同名 | `Community.Phone` | A/B | `VisualFirstIndex`、`IsSelected`、`SelectionStates`。 |
| `Microsoft.Phone.Controls.Primitives.ViewportControl` | 同名 | `Community.Phone` | B | `ContentControl`，作为 `LongListSelector` 的内容宿主与模板部件，不是手机的滚动器。 |
| `Microsoft.Phone.Controls.LongListSelector`（WP8.1） | 同名 | `Community.Phone` | A/B | 见[控件](#控件)。 |
| `Microsoft.Phone.Controls.HyperlinkButton` | 同名 | `Community.Phone` | A/B | 相对/pack URI 导航最近 frame；`TargetName` 保留但命名目标不受支持。 |
| `Microsoft.Phone.Maps.Toolkit.*` | 未实现 | — | E | 没有 WPF 地图提供程序。 |
| `Microsoft.Phone.BackgroundTransfer` / `TransferControl` | 未实现 | — | E | 没有 WPF 等价服务。 |
| `Windows.Phone.Speech.Recognition` / `SpeechTextBox` | 未实现 | — | E | 没有 WPF 等价语音服务。 |

## 投影与效果

投影类型位于 `Community.Phone` 程序集，XAML 使用 `platform:` 前缀。

| 原始类型 / 用法 | 移植类型 | 级别 | XAML 修改 | 行为差异 |
|---|---|---|---|---|
| `UIElement.Projection` | `ProjectionService.Projection`（附加属性） | C/D | 任意元素写 `platform:ProjectionService.Projection` | WPF 元素没有原地投影属性；附加属性直接作用于元素本身（不包装、不重 Parent、不改变布局），storyboard 路径为 `(ProjectionService.Projection).(PlaneProjection.RotationY)`。 |
| `System.Windows.Media.PlaneProjection` | `Microsoft.Phone.Controls.PlaneProjection` | C/D | `platform:` 前缀 | 平面投影按精确单应变换由 `ShaderEffect` 渲染；固定 999 DIP 透视深度；旋转、中心、偏移的正负与 XAML 一致。 |
| `System.Windows.Media.Matrix3DProjection` | `Microsoft.Phone.Controls.Matrix3DProjection` | C/D | 同上 | 矩阵使用 0..1 归一化坐标。 |
| — | `Projection`、`ProjectionService` | D | — | 移植新增：投影基类与附加属性服务（`Projection`、`PreferShader`、`PerspectiveDepth`、`HitTestPolicy`）。 |
| — | `HomographyMatrix`、`ProjectionMath.CreateHomography`/`GetPlaneDistance` | D | — | 移植新增：3×3 齐次变换与“`Matrix3D` → 单应”推导，可用于自建渲染器或验证。 |
| — | `PerspectiveShaderEffect` | D | — | 移植新增：驱动投影的 `ShaderEffect`，常量布局见架构文档 §4.4；正常使用由服务/宿主管理。 |
| — | `ProjectionService.EffectConflictBehavior`、`EffectConflict`、`PreferShader`、`HitTestPolicy` | D | `platform:ProjectionService.PreferShader`、`platform:ProjectionService.HitTestPolicy` | 移植新增：原有效果的冲突策略与事件、identity 期间保持着色器路径、投影期间的命中测试策略。 |
| `TiltEffect` | `TiltEffect`（toolkit） | C/D | `phone:TiltEffect.IsTiltEnabled`、`SuppressTilt`、`TiltableItems` | 直接给控件赋投影（不包装）；使用隧道式 `PreviewMouse*`，容器可倾斜子按钮而不吞点击。 |
| `TurnstileFeatherEffect` | `TurnstileFeatherEffect`（toolkit） | C/D | `phone:TurnstileFeatherEffect.FeatheringIndex` | 目标从页面可视树收集；直接投影 + `RenderTransform` 组合。 |
| `SlideInEffect` | `SlideInEffect`（toolkit） | C/D | `phone:SlideInEffect.LineIndex` | 订阅宿主 Pivot 的选择变化，按行索引交错位移/透明度动画；Pivot 自身无需该效果。 |

## 导航与转场

| 原始 | 移植 | 级别 | 说明 |
|---|---|---|---|
| `PhoneApplicationFrame` | 同名 | B/C | WPF journal / `NavigationService`；方向保留默认枚举值，不按尺寸推断。 |
| `PhoneApplicationPage` | 同名 | B/C | 生命周期由 Frame 触发；返回导航保留现有实例。 |
| `TransitionFrame` | 同名 | B | 自定义模板，部件是两个 `ContentPresenter`。 |
| `TransitionService.NavigationInTransition` / `NavigationOutTransition` | 同名 | A | 保留附加属性名与 `Forward`/`Backward` 依赖属性。 |
| `NavigationInTransition` / `NavigationOutTransition` / `NavigationTransition` | 同名 | A | 保留类型层次。 |
| `ITransition` / `Transition` | 同名 | A/B | 保留 API 面；用 WPF `Storyboard` 与自维护状态。 |
| `TransitionElement` | 同名 | A | 保留抽象基类与 `GetTransition` 契约。 |
| `TurnstileTransition` / `TurnstileTransitionMode` | 同名 | A | 4 个模式。 |
| `TurnstileFeatherTransition` / `TurnstileFeatherTransitionMode` | 同名 | A/B | 4 个模式；使用包装目标，storyboard 由代码合成。 |
| `SlideTransition` / `SlideTransitionMode` | 同名 | A | 8 个模式。 |
| `RotateTransition` / `RotateTransitionMode` | 同名 | A | 8 个模式。 |
| `SwivelTransition` / `SwivelTransitionMode` | 同名 | A | 6 个模式。 |
| `RollTransition` | 同名 | A | 单一模式。 |
| `FeatheredTransition` | 同名 | A/B | 保留公共类型。 |

27 个 `Transitions/Storyboards/*.xaml` 以 WPF 资源随包发布并在运行时加载，时长、关键时间、角度和缓动都是原值；文件内唯一的改动是投影属性路径。`Transitions.cs` 中的 `TurnstileCenterOfRotationX = -0.05` 与 `TurnstileDurationScale = 1.2` 是仅移植版的调优常量。`TurnstileFeather` 的 4 个模式没有独立 storyboard 文件。

## Shell 与主题

| 类型 | 状态 | 说明 |
|---|---|---|
| `ApplicationBar` | 实现 | `[ContentProperty("Buttons")]`、`IsVisible`、`Opacity`（截断到 0..1）、`IsMenuEnabled`、`BackgroundColor`、`ForegroundColor`、`Mode`、`DefaultSize`（72）、`MiniSize`（30）、`Buttons`、`MenuItems`、`StateChanged`。 |
| `ApplicationBarPresenter` | 实现（移植新增） | 绘制应用栏、图标槽、菜单按钮与 `Popup` 菜单；`ReservedHeight` 驱动页面预留高度。 |
| `SystemTray` | 状态实现 | 无视觉 chrome；`IsVisible` 同时提供静态属性与 `IsVisibleProperty` 附加属性，XAML 中 `shell:SystemTray.IsVisible` 可用。 |
| `PhoneApplicationService` | 状态实现 | `State`、生命周期事件、idle 检测模式；不复刻 tombstoning。 |
| `ThemeManager` | 实现（移植新增） | `AvailableApplicationThemes`、`AvailableAccentColors`、`ActualTheme`、`ActualAccentColor`、`ThemeChanged`；`ResolveDictionary`、`GetSystemAccentColor`。`RequestedTheme`、`AccentColor`、`CustomAccentColor`、`InitializeTheme()` 是 `ApplicationExtensions` 提供的 `Application` 扩展成员，不在 `ThemeManager` 上。 |
| `ThemePalette` | 实现（移植新增） | 24 个 `Phone*Color` 属性、5 个非颜色值、`PivotUnselectedItemOpacity`、`WithAccent`、`Dark`/`Light` 静态调色板、`ResourceKeys`。 |

## 控件

| 控件 | 状态 | 级别 | 保留内容 | 差异 |
|---|---|---|---|---|
| `ContextMenu` / `MenuItem` / `Separator` / `ContextMenuService` | 已实现 | B/C | `ContextMenuService` 附加属性；`Owner`、`IsZoomEnabled`、`IsFadeEnabled`、`VerticalOffset`、`IsOpen`、`RegionOfInterest`、`Opened`/`Closed`、`ItemContainerStyle`；原版模板部件 `OuterPanel`/`InnerGrid` 与 `Open`/`OpenReversed`/`Closed` 视觉状态 | 自绘 `Popup`（不继承 WPF `ContextMenu`）；WPF 无 `Hold` 手势，改为**右键**（移植新增，也可关闭菜单）与**左键长按 800 ms** 打开；**不区分横竖屏**，始终按竖屏全宽布局；页面快照用 `RenderTargetBitmap`（按 DPI 缩放取像素），背景缩放/淡入用 `BeginAnimation` 直接驱动 |
| `CustomMessageBox` | 已实现 | B | `Show`、`Dismiss`、`Dismissing`、`Dismissed`、`LeftButtonContent`、`RightButtonContent`、`IsLeftButtonEnabled`/`IsRightButtonEnabled`、`Title`（继承）、`Caption`、`Message`、`IsFullScreen`、`IsCenterScreen`、`CustomMessageBoxResult` | 由 WPF `Window` 承载；弹入/关闭播放 Swivel。 |
| `DatePicker` / `TimePicker` | 已实现 | B | `Value`、`ValueString`、`ValueStringFormat`、`ValueChanged`、`Header`、`HeaderTemplate`、`PickerPageUri` | 通过宿主 `PhoneApplicationFrame` 导航到 `DatePickerPage` / `TimePickerPage`（主题默认 pack URI）；页面用 `LoopingSelector` 求值。 |
| `RecurringDaysPicker` | 已实现 | A/B | 星期多选与摘要 | 继承 `ListPicker`；本地化星期名。 |
| `ExpanderView` | 已实现 | B | `Expander`、`ExpanderTemplate`、`NonExpandableHeader`、`NonExpandableHeaderTemplate`、`IsExpanded`、`HasItems`、`IsNonExpandable`、`Expanded`/`Collapsed`；部件 `Presenter`/`ExpanderPanel`/`ItemsCanvas` | 基于 `HeaderedItemsControl`；高度动画简化。 |
| `HyperlinkButton` | 已实现 | A/B | `NavigateUri`、`TargetName`、`Click`、`CommonStates`、`IInvokeProvider` | 相对/pack URI 导航最近 frame；外部 URI 交给系统处理程序；`TargetName` 不生效；非字符串内容需要自定义模板。 |
| `HubTile` / `HubTileService` | 已实现 | B | `Source`、`Title`、`Notification`、`Message`、`DisplayNotification`、`IsFrozen`、`GroupTag`、`State`、`Size`、`TileSize` | 定时器驱动状态循环；滑动偏移由代码施加，翻转走 3D 路径，`CenterOfRotationY` 为 `0.5`（原版 `0.25` 相对两倍高表面）。 |
| `ListPicker` / `ListPickerItem` | 已实现 | A/B | `SelectedIndex`、`SelectedItem`、`SelectedItems`、`SelectionMode`、`Header`、`HeaderTemplate`、`FullModeHeader`、`FullModeItemTemplate`、`ShadowItemTemplate`、`ItemCountThreshold`、`ExpansionMode`、`PickerPageUri`、`ListPickerMode`、`IsOpen`、`Open()`、`SelectionChanged`、`SummaryForSelectedItemsDelegate` | 完整模式导航到 `ListPickerPage`，用原版 `ApplicationBar` 确认/取消；普通模式直接对模板 Canvas 做高度/位移动画。 |
| `LongListSelector` | 已实现 | A/B | `ItemsSource`、`IsGroupingEnabled`、`HideEmptyGroups`、`ItemTemplate`、`GroupHeaderTemplate`、`GroupFooterTemplate`、`ListHeader`/`ListFooter`（及模板）、`LayoutMode`、`GridCellSize`、`SelectedItem`、`ScrollTo`、`ItemRealized`/`ItemUnrealized`、`JumpListStyle`、`JumpListOpening`/`JumpListClosed`、`SelectionChanged`、`ManipulationState`/`ManipulationStateChanged`、模板部件 `ViewportControl`/`VerticalScrollBar` | 列表模式回收虚拟化，网格模式固定单元格非虚拟化；跳转列表是 `Popup`；分组对象必须实现 `IList`。 |
| `LongListMultiSelector` / `LongListMultiSelectorItem` | 已实现 | A/B | `SelectedItems`、`IsSelectionEnabled`、`EnforceIsSelectionEnabled`、`ItemInfoTemplate`、`ItemContainerStyle`、分组与跳转列表 API、`ScrollTo`、`ContainerFromItem`、`ItemRealized`/`ItemUnrealized`、`SelectionChanged`、`IsSelectionEnabledChanged`、`IsSelectedChanged` | 基于 `LongListSelector` 增加多选；完整主题模板与示例页。 |
| `MultiselectList` / `MultiselectItem` | 类型已实现、无默认模板 | B | `IsSelectionEnabled`、`ItemInfoTemplate`、`ItemContainerStyle`、`SelectionChanged`、`IsSelectionEnabledChanged`；`MultiselectItem.IsSelected`/`State`/`HintPanelHeight`/`ContentInfo` | `Themes/` 下无默认样式，示例入口仅说明 WP8 已弃用；实际多选请使用 `LongListMultiSelector`。 |
| `AutoCompleteBox` | 已实现 | A/B | `MinimumPrefixLength`、`MinimumPopulateDelay`、`IsTextCompletionEnabled`、`FilterMode`、`ItemFilter`/`TextFilter`、`ItemTemplate`、`ItemContainerStyle`、`TextBoxStyle`、`MaxDropDownHeight`、`IsDropDownOpen`、`ItemsSource`、`SelectedItem`、`Text`、`SearchText`、`ValueMemberPath`、`InputScope`、`Populating`/`Populated`/`DropDownOpening`/`DropDownOpened`/`DropDownClosing`/`DropDownClosed`/`SelectionChanged` | `PopupHelper` 处理 WPF popup 布局；`InputScope` 隐藏 WPF `FrameworkElement.InputScope`（同名）；主题模板仅含 `Text`/`Popup`/`Selector` 部件，未声明 VisualStateGroup。 |
| `PhoneTextBox` | 已实现 | A/B | `Hint`、`HintStyle`、`ActualHintVisibility`、`LengthIndicatorVisible`、`LengthIndicatorThreshold`、`DisplayedMaxLength`、`ActionIcon`、`HidesActionItemWhenEmpty`、`ActionIconTapped` | 内部 `TextBox` 使用双向 `Text` 绑定。 |
| `Rating` / `RatingItem` | 已实现 | A/B | `FilledItemStyle`、`UnfilledItemStyle`、`RatingItemCount`、`Value`、`ReadOnly`、`AllowHalfItemIncrement`、`AllowSelectingZero`、`ShowSelectionHelper`、`Orientation`、`StrokeThickness`、`ValueChanged` | 星形项目在代码中生成；半星模式 `Value` 以“星”为单位。 |
| `ToggleSwitch` / `ToggleSwitchButton` | 已实现 | A/B | `Header`、`HeaderTemplate`、`SwitchForeground`、`IsChecked`、`Checked`/`Unchecked`/`Indeterminate`/`Click` | WPF 视觉状态与 manipulation。 |
| `Pivot` / `PivotItem` / `PivotHeadersControl` / `PivotHeaderItem` | 已实现 | A/B | 见上表 | 输入是鼠标拖拽 + 方向键；页眉条循环排布但只绘制一圈；`IsLocked` 是依赖属性。 |
| `Panorama` / `PanoramaItem` | 已实现 | B | `Title`/`TitleTemplate`、`HeaderTemplate`、`SelectedItem`/`SelectedIndex`、`DefaultItem`、`SelectionChanged`；`PanoramaItem.Header`/`HeaderTemplate`/`Orientation` | WP 手势改为 WPF 指针事件与 storyboard；无独立示例页。 |
| `PerformanceProgressBar` | 已实现 | B | `IsIndeterminate`、`ActualIsIndeterminate` | 内嵌 WPF `ProgressBar`；简单淡入淡出。 |
| `RelativeAnimatingContentControl` | 已存在 | B | 公共 `ContentControl` 派生类型 | 原版的相对动画重写未移植。 |
| `WrapPanel` | 已实现 | A | `ItemHeight`、`ItemWidth`、`Orientation`、布局算法 | 与 WPF 自带 `System.Windows.Controls.WrapPanel` 并存。 |
| `ClipToBounds` | 已实现 | A | `IsEnabled` 附加属性 | 使用 WPF `Clip`。 |
| `LoopingSelector` / `LoopingSelectorItem` / `ILoopingSelectorDataSource` | 已实现 | A/B | `DataSource`、`ItemTemplate`、`IsExpanded`、`IsExpandedChanged`、`Click`、`GetNext`/`GetPrevious`/`SelectedItem` | 位于 `Microsoft.Phone.Controls.Primitives`；供 picker 页面使用。 |
| 地图扩展 | 未实现 | E | — | 无 WPF 地图提供程序。 |
| `TransferControl` | 未实现 | E | — | 需要 WP 后台传输服务。 |
| `SpeechTextBox` | 未实现 | E | — | 需要 Windows Phone 语音。 |
| `LockablePivot` | 未实现 | E | — | 无独立类型；`Samples/LockablePivotSample.xaml` 演示 `Pivot.IsLocked`。 |
| `GestureListener` / `GestureService` | 未实现 | E | — | 使用 WPF 触摸/manipulation API。 |
| `LengthConverter` | 已提供 | A | 同名 `TypeConverter` | 位于 `Microsoft.Phone.Controls`（`WINDOWS_PHONE` 分支），不覆盖 WPF 类型。 |
| `TimeTypeConverter` | 已提供 | A | 同名 `TypeConverter` | 位于 `System.Windows.Controls`，不覆盖 WPF 类型。 |

没有独立示例页的已实现模块：`Panorama`（主题字典中提供样式，示例仅间接引用）。

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
| `ProjectionService` | `PerspectiveDepth` / `PreferShader` / `HitTestPolicy` | `999` / `false` / `Layout` |
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
| `xmlns:phone="clr-namespace:Microsoft.Phone.Controls;assembly=Community.Phone.Controls.Toolkit"` | toolkit 控件、转场、效果必需。 |
| `xmlns:platform="clr-namespace:Microsoft.Phone.Controls;assembly=Community.Phone"` | `PhoneApplicationPage`、`PhoneApplicationFrame`、`Panorama`、`Pivot`、`LongListSelector`、`HyperlinkButton` 以及全部投影类型必需。 |
| `xmlns:shell="clr-namespace:Microsoft.Phone.Shell;assembly=Community.Phone"` | `ApplicationBar`、`ApplicationBarIconButton`、`ApplicationBarMenuItem`、`SystemTray` 必需。 |
| `<phone:ToggleSwitch ... />`、`<phone:Rating ... />`、`<phone:WrapPanel ... />`、`<phone:AutoCompleteBox ... />` | 只需改命名空间。 |
| `<phone:HyperlinkButton NavigateUri="..." />` | 可用；`TargetName` 不受支持。 |
| `<phone:TransitionService.NavigationInTransition>...` | 可用。 |
| `<Grid.Projection><PlaneProjection .../></Grid.Projection>` | WPF 不编译；改用 `platform:ProjectionService.Projection`。 |
| `<Grid platform:ProjectionService.Projection="...">` | 可用：投影直接作用于该元素，任何 `UIElement` 都可以，不需要包装容器。 |
| `<platform:ProjectionService.Projection><platform:PlaneProjection .../></platform:ProjectionService.Projection>` | 可用：投影作用于该元素，不需要包装容器。 |
| `shell:SystemTray.IsVisible="..."` | 可用：`SystemTray` 提供 `IsVisibleProperty` 附加属性；挂在页面、Frame 或 `Window` 上时同步到状态载体。 |
| `shell:ApplicationBar` 声明在 `PhoneApplicationPage.ApplicationBar` 上 | 可用；由 frame 的呈现器渲染。 |
| `platform:PhoneApplicationPage` 的 `SupportedOrientations` / `Orientation` | 可解析并保留；不随窗口尺寸变化，不强制旋转桌面窗口。 |
| `phone:LongListSelector` | 属于 `Community.Phone`，需使用 `platform:` 前缀；分组对象必须实现 `IList`，跳转列表是 `Popup`。 |
| 地图、语音、传输 XAML | 不支持。 |

## 按功能的兼容注意事项

1. **投影与 3D**：平面投影使用单应变换着色器渲染，支持动画；`<Grid.Projection>` 不编译，但给元素赋 `platform:ProjectionService.Projection`（或 `element.Projection`）即可，元素不被包装。相机复现 XAML 固定的 999 DIP 透视深度；差异见[已知限制](known-limitations.md#4-投影)。投影类型在 `Community.Phone`，storyboard 内的 `phone:` 别名也指向该程序集。
2. **转场**：时序、角度与模式取自原始 storyboard 资源；WPF 的 storyboard 状态查询与 Silverlight 不同，移植版自行跟踪。
3. **导航**：保留 WPF `Frame`/`Page` 与 journal 行为；页面生命周期由移植 Frame 触发；`PhoneApplicationService` 的 idle 策略没有 WPF 系统等价物。
4. **效果**：`TiltEffect`、`TurnstileFeatherEffect` 与 `SlideInEffect` 使用真实投影/变换管线，并把投影直接赋给目标元素（不再包装）。
5. **控件**：依赖属性面与核心行为保留；WPF 缺少 picker/popup 基础设施时，视觉细节与模态宿主不同。
6. **本地化**：包含原始 resx 资源，`ControlResources` 使用 `CurrentUICulture`。
7. **主题**：`ThemeManager` 在运行时改色；库内模板使用 `DynamicResource`，应用 XAML 也应使用 `DynamicResource`。`Themes/Theme.xaml` 在应用级字典中会被 WPF 冻结，`ThemeManager` 首次应用时把它替换为可变副本。

实现状态或成员发生变化时必须同步更新本矩阵；没有实现依据的功能不得标记为已实现。构建与打包状态见[移植状态](porting-status.md#构建与验证)。
