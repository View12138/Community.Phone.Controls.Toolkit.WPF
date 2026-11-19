# 移植状态

按模块记录当前实现状态。本文件与[API 兼容矩阵](api-compatibility.md)的分工：那里逐类型列出 API 差异，这里给出模块级的范围与状态，并记录构建/运行事实。

## 状态定义

| 状态 | 含义 |
|---|---|
| **已实现** | 源码中存在完整实现，API 面可用。 |
| **部分实现** | 主体已实现，但有明确的功能或保真度缺口（见备注与[已知限制](known-limitations.md)）。 |
| **未实现** | 源码中不存在，不得声称可用。 |

仓库内没有自动化测试工程，因此本文件不按“测试是否通过”分级；状态由源码检查与构建结果得出，行为差异与验证缺口见[已知限制](known-limitations.md)。

## 构建与验证

| 检查项 | 命令 | 结果 |
|---|---|---|
| 解决方案编译 | `dotnet build Community.Phone.Controls.Toolkit.slnx -c Release` | 通过；0 错误，警告来自可空性注释（`CS8632`）、成员隐藏（`CS0108`）、未使用事件/字段（`CS0067`/`CS0414`/`CS0649`）、XML 注释（`CS1574`）与恒真比较（`CS8073`） |
| toolkit 工程编译 | 同上 | 通过 |
| 示例工程编译 | 同上 | 通过 |
| 打包 | `dotnet pack src\Community.Phone.Controls.Toolkit\Community.Phone.Controls.Toolkit.csproj -c Release` | 失败：`NU5019` —— `csproj` 打包项引用仓库中不存在的 `THIRD-PARTY-NOTICES_zh-CN.md`、`docs/en/*.md`、`docs/zh-CN/*.md` |
| 包消费验证 | `powershell -File build\verify-package.ps1` | 无法执行：脚本引用不存在的 `tests/PackageConsumer/PackageConsumer.csproj` |

`SystemTray` 是无视觉 chrome 的静态类，同时提供静态状态属性与 `IsVisibleProperty` 附加属性及 `GetIsVisible`/`SetIsVisible`；`ListPickerPage.xaml` 与示例页面中的 `shell:SystemTray.IsVisible` 均可正常标记编译。

打包阻塞来自随包文档路径，而非编译：`csproj` 中 `README.md`、`LICENSE`、`THIRD-PARTY-NOTICES.md` 存在，其余 `*-zh-CN.md` 与 `docs/en`、`docs/zh-CN` 目录不存在。

## 1. 投影

| 项目 | 状态 | 说明 |
|---|---|---|
| `Projection`、`PlaneProjection`、`Matrix3DProjection` | 已实现 | 位于 `Community.Phone`；12 个 `PlaneProjection` 依赖属性；`Animatable`；`ToMatrix3D` 复现 XAML 的 999 DIP 透视与符号约定；`Matrix3DProjection` 使用归一化矩阵 |
| `ProjectionHost` | 已实现 | 位于 `Community.Phone`；`Viewport3D` + `Viewport2DVisual3D`；`Content`、`Projection`、`PerspectiveDepth`（默认 999）、`Prefer3D`、`IsRenderingIn3D`；双面四边形；内容按布局槽裁剪，投影结果不裁剪 |
| `ProjectionService` | 已实现 | 位于 `Community.Phone`；附加属性 `Projection`、扩展成员 `element.Projection`、`EnsureProjectionHost`（复制布局属性）、`ReleaseProjectionHost`、`GetDirectProjectionHost`、`GetProjectionPropertyPath`；按需包装支持 `Panel`、`ItemsControl`、`ContentControl`、`Page` 父容器 |
| `ListBoxItem` 投影 | 已实现 | 默认主题模板 `Themes/Controls/ListBox.xaml` 内建名为 `ProjectionHost` 的部件；服务直接复用，不重父级化；自定义模板必须保留同名部件 |
| storyboard 路径 `(ProjectionService.Projection).(PlaneProjection.X)` | 已实现 | 供原版 storyboard 使用；storyboard 的 `xmlns:phone` 指向 `Community.Phone` |
| `Matrix3DProjection` 的复杂矩阵 | 部分实现 | `ToMatrix3D` 依次做“按尺寸归一化 → 应用矩阵 → 反归一化”；任意矩阵的视觉结果没有验证，示例未演示 |

## 2. 导航与 Shell

| 项目 | 状态 | 说明 |
|---|---|---|
| `PhoneApplicationFrame`（WPF `Frame`）、journal、`Navigate(page, parameters)` | 已实现 | 参数与 URI 查询串合并进 `NavigationContext` |
| 页面生命周期分发 | 已实现 | `Navigating`/`Navigated` 触发 `OnNavigatingFrom`、`OnNavigatedFrom`、`OnNavigatedTo`；`GoBack()` 先触发 `BackKeyPress` |
| 页面状态、`KeepAlive` | 已实现 | `State` 是内存字典，按页面实例保存；构造函数设置 `KeepAlive = true` |
| 方向检测 | 部分实现 | 保留 `Orientation` / `SupportedOrientations` 枚举与默认值；Frame 尺寸变化不触发方向推断，不强制旋转桌面窗口；页面/Frame 未加载时忽略方向变更回调；`OnOrientationChanged` 是可重写的受保护虚方法 |
| Tombstone / 真实 WP 生命周期 | 未实现 | 见[已知限制 §2](known-limitations.md#2-导航与页面模型) |
| `PhoneApplicationService` | 部分实现 | `State`、生命周期事件、idle 检测模式；无 tombstoning |
| `SystemTray` | 部分实现 | 只有状态 API，无视觉 chrome；同时提供静态属性与 `IsVisibleProperty` 附加属性 |
| `ApplicationBar`（API、chrome、菜单） | 已实现 | 原版 API 形状与限制；`ApplicationBarPresenter` 绘制 72/30 DIP 栏、96×72 图标槽、48 DIP 圆环、48 px 行高菜单 `Popup`；frame 按 `ReservedHeight` 预留高度 |
| `NavigationContext` | 已实现 | `Uri`、`ExtraData`、大小写不敏感 `QueryString`；可从相对 URI 解析查询串 |

## 3. 转场

| 族 | 状态 | 说明 |
|---|---|---|
| `TransitionFrame` | 已实现 | 两个 `ProjectionHost` 部件；退场完成后再进场；准备阶段关闭命中测试并打开 `Prefer3D`；无动画的转场在下一个 dispatcher 周期完成 |
| `Transition` | 已实现 | 自跟踪 begun/completed；可控时钟；`Stop()` 移除时钟；每次运行最多一次 `Completed` |
| `TurnstileTransition`（4 模式） | 已实现 | 原版 storyboard；`TurnstileCenterOfRotationX = -0.05`、`TurnstileDurationScale = 1.2` 为移植版调优常量 |
| `TurnstileFeatherTransition`（4 模式） | 已实现 | 目标从页面可视树收集；逐元素 `ProjectionHost`；40/50 ms 交错；清理后重绘；storyboard 由代码合成 |
| `SlideTransition`（8 模式） | 已实现 | 原版 `Slide*Fade*` storyboard；`RenderTransform` 位移与透明度 |
| `RotateTransition`（8 模式） | 已实现 | 原版 `Rotate*` storyboard |
| `SwivelTransition`（6 模式） | 已实现 | 原版 `Swivel*` storyboard |
| `RollTransition`（1 模式） | 已实现 | 原版 `Roll` storyboard |
| `FeatheredTransition` | 已实现 | 公共类型保留；`Begin()` 调用 `TurnstileFeatherEffect.ComposeStoryboard` |

共 27 个 storyboard XAML 资源；`TurnstileFeather` 的 4 个模式由代码合成，没有独立 XAML 文件。

## 4. 效果

| 项目 | 状态 | 说明 |
|---|---|---|
| `TiltEffect` | 已实现 | 通过 `ProjectionService` 赋投影；支点取第一个可视子元素；隧道式鼠标事件 + 触摸 manipulation；`ListBoxItem` 复用模板部件，其余无法包装的容器跳过 |
| `TurnstileFeatherEffect` | 已实现 | 目标从可视树收集；包装延迟到 dispatcher；`Prefer3D` + `CenterOfRotationX = -0.2`；恢复后 `InvalidateVisual()` |
| `SlideInEffect` | 已实现 | `LineIndex` 等附加属性；订阅宿主 Pivot 选择变化；有独立示例页 |

## 5. 控件

| 控件 | 状态 | 说明 |
|---|---|---|
| `WrapPanel` | 已实现 | 原始布局算法 |
| `ToggleSwitch` + `ToggleSwitchButton` | 已实现 | 视觉状态 + manipulation |
| `Rating` / `RatingItem` | 已实现 | 代码生成项目；半星模式以“星”为单位（偏离原版翻倍行为） |
| `PhoneTextBox` | 已实现 | hint、action icon、长度指示；占位符与光标对齐；输入法合成期间隐藏 |
| `ListPicker` / `ListPickerItem` / `ListPickerPage` | 已实现 | 普通/完整模式、原版应用栏、逐个翻转 |
| `AutoCompleteBox` | 已实现 | 过滤模式与谓词、文本补全、popup 布局、`SelectorSelectionAdapter`；有主题模板与示例页 |
| `LongListMultiSelector` / `LongListMultiSelectorItem` | 已实现 | 在 `LongListSelector` 上增加多选、`IsSelectionEnabled` 与选中项摘要；有主题模板与示例页 |
| `MultiselectList` / `MultiselectItem` | 部分实现 | 类型与选择逻辑存在，但 `Themes/` 下无默认模板；示例入口仅给出 WP8 弃用说明 |
| `ExpanderView` | 已实现 | 高度动画简化 |
| `HubTile` / `HubTileService` | 已实现 | 两倍高表面滑动 + 真实 `RotationX` 翻转；组冻结 |
| `ContextMenu` / `MenuItem` / `Separator` / `ContextMenuService` | 已实现 | 原版 `Popup` 实现与模板；右键 + 左键长按打开；页面缩放/淡入遮罩生效；无方向分支（示例页 `Samples/ContextMenuSample.xaml` 手工验证：4 个按钮各自连续打开两次、遮罩绘制、`IsZoomEnabled`/`IsFadeEnabled` 开关） |
| `CustomMessageBox` | 已实现 | WPF `Window` 承载；Swivel 弹入/关闭；不依赖 `Application` |
| `DatePicker` / `TimePicker` / `RecurringDaysPicker` | 已实现 | 值/格式/事件完整；通过宿主 Frame 导航到 `DatePickerPage` / `TimePickerPage`，页面使用 `LoopingSelector` |
| `LoopingSelector` / `LoopingSelectorItem` / `ILoopingSelectorDataSource` | 已实现 | `Microsoft.Phone.Controls.Primitives`；有主题模板；供 picker 页面使用 |
| `Panorama` / `PanoramaItem` | 已实现 | 位于 `Community.Phone`；WPF 指针事件与 storyboard 复刻视差与吸附；有主题字典，无独立示例页 |
| `PerformanceProgressBar`、`RelativeAnimatingContentControl` | 已实现 | 不确定进度与透明动画 |
| `Pivot` / `PivotItem` / `PivotHeadersControl` / `PivotHeaderItem` | 已实现 | 代码动画替代 DirectManipulation；鼠标拖拽 + 方向键；`IsLocked` |
| `LongListSelector` | 已实现 | 分组、页眉/页脚、列表/网格模式、跳转列表、`ScrollTo`、`ItemRealized`/`ItemUnrealized` |
| `HyperlinkButton` | 已实现 | 相对/pack URI 导航；外部 URI 交系统处理；`IInvokeProvider` |
| `ClipToBounds` | 已实现 | 附加属性 |
| 地图扩展、`TransferControl`、`SpeechTextBox`、`GestureListener`/`GestureService` | 未实现 | 源码中不存在 |
| `LockablePivot` | 未实现 | 无独立类型；`Samples/LockablePivotSample.xaml` 演示 `Pivot.IsLocked` |

## 6. 主题、本地化、打包

| 项目 | 状态 | 说明 |
|---|---|---|
| Metro 主题 + WPF 基础控件 WP 样式 | 已实现 | 两个程序集的 `Themes/Generic.xaml`；覆盖 toolkit 控件与常用 WPF 控件；未做与原版的并排像素对比 |
| 运行时主题/强调色切换 | 已实现 | `ThemeManager` + `ThemePalette` + `Theme.xaml`；详见[主题系统](theming.md) |
| 本地化资源 | 已实现 | 51 个 resx（50 个文化 + 中性）；`qps-ploc` 未打包 |
| `Community.Phone` 兼容层 | 已实现 | 独立程序集、独立主题字典；页面/框架、投影、Shell 类型均在此 |
| NuGet 包 | 部分实现 | 单包包含两个程序集的设计与 `IncludeCompatibilityLayerInPackage` 已就位；打包项引用的 `zh-CN` 文档与 `docs/en`、`docs/zh-CN` 目录不存在，`dotnet pack` 当前失败 |
| 示例应用 | 部分实现 | 页面与控件演示齐全；索引 25 条中 4 条（`GestureSample`、`MapsSample`、`SpeechTextBoxSample`、`TransferControlSample`）没有对应页面文件，在索引中禁用并加删除线；`lockablepivot` 指向 `Samples/LockablePivotSample.xaml` |

## 维护规则

行为、API 面或构建状态发生变化时，必须在同一次改动中更新本文件与[API 兼容矩阵](api-compatibility.md)。未实现的内容不得从“未实现”改为“已实现”，除非源码中确实存在实现。
