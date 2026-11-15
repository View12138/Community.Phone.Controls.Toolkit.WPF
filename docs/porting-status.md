# 移植状态

按模块记录当前实现状态。本文件与[API 兼容矩阵](api-compatibility.md)的分工：那里逐类型列出 API 差异，这里给出模块级的范围与状态，并记录构建/运行事实。

## 状态定义

| 状态 | 含义 |
|---|---|
| **已实现** | 源码中存在完整实现，API 面可用。 |
| **部分实现** | 主体已实现，但有明确的功能或保真度缺口（见备注与[已知限制](known-limitations.md)）。 |
| **未实现** | 源码中不存在，不得声称可用。 |

仓库内没有自动化测试工程，因此本文件不再按“测试是否通过”分级；状态由源码检查得出，行为差异与验证缺口见[已知限制](known-limitations.md)。

## 构建与验证

| 检查项 | 命令 | 结果 |
|---|---|---|
| `Community.Phone` 编译 | `dotnet build Community.Phone.Controls.Toolkit.slnx -c Release` | 通过；只有警告（可空性 `CS86xx`、未使用事件 `CS0067`、XML 注释 `CS1574` 等） |
| `Community.Phone.Controls.Toolkit` 编译 | 同上 | **失败**：`Controls/ListPicker/ListPickerPage.xaml(23,5)` 报 `MC3072` —— XML 命名空间 `clr-namespace:Microsoft.Phone.Shell;assembly=Community.Phone` 中不存在属性 `SystemTray.IsVisible` |
| 示例工程编译 | 同上 | 无法进行：依赖 toolkit 工程；且 `MainPage.xaml`、`Settings.xaml` 与 `Samples/*.xaml` 中的 9 处页面也使用 `shell:SystemTray.IsVisible`，会报同一错误 |
| 打包 | `dotnet pack src\Community.Phone.Controls.Toolkit\Community.Phone.Controls.Toolkit.csproj -c Release` | 无法进行：依赖编译成功；`csproj` 另外引用了不存在的 `README_zh-CN.md`、`THIRD-PARTY-NOTICES_zh-CN.md`、`docs/en/*.md`、`docs/zh-CN/*.md` |
| 包消费验证 | `powershell -File build\verify-package.ps1` | 无法进行：脚本引用不存在的 `tests/PackageConsumer/PackageConsumer.csproj` |

构建阻塞的根因：`Microsoft.Phone.Shell.SystemTray` 是静态类，只提供静态属性（`IsVisible`、`Opacity`、`BackgroundColor`、`ForegroundColor`、`ProgressIndicator`），没有附加依赖属性，因此不能作为 XAML 附加属性使用。该写法出现在 toolkit 的 `ListPickerPage.xaml` 与示例的 9 个页面中。

## 1. 投影

| 项目 | 状态 | 说明 |
|---|---|---|
| `Projection`、`PlaneProjection`、`Matrix3DProjection` | 已实现 | 12 个 `PlaneProjection` 依赖属性；`Animatable`；`ToMatrix3D` 复现 XAML 的 999 DIP 透视与符号约定；`Matrix3DProjection` 使用归一化矩阵 |
| `ProjectionHost` | 已实现 | `Viewport3D` + `Viewport2DVisual3D`；`Content`、`Projection`、`PerspectiveDepth`（默认 999）、`Prefer3D`、`IsRenderingIn3D`；双面四边形；内容按布局槽裁剪，投影结果不裁剪 |
| `ProjectionService` | 已实现 | 附加属性 `Projection`、扩展成员 `element.Projection`、`EnsureProjectionHost`（复制布局属性）、`ReleaseProjectionHost`、`GetDirectProjectionHost`、`GetProjectionPropertyPath`；item 容器不包装 |
| storyboard 路径 `(ProjectionService.Projection).(PlaneProjection.X)` | 已实现 | 供原版 storyboard 使用 |
| `Matrix3DProjection` 的复杂矩阵 | 部分实现 | `ToMatrix3D` 依次做“按尺寸归一化 → 应用矩阵 → 反归一化”；任意矩阵的视觉结果没有验证，示例未演示 |

## 2. 导航与 Shell

| 项目 | 状态 | 说明 |
|---|---|---|
| `PhoneApplicationFrame`（WPF `Frame`）、journal、`Navigate(page, parameters)` | 已实现 | 参数与 URI 查询串合并进 `NavigationContext` |
| 页面生命周期分发 | 已实现 | `Navigating`/`Navigated` 触发 `OnNavigatingFrom`、`OnNavigatedFrom`、`OnNavigatedTo`；`GoBack()` 先触发 `BackKeyPress` |
| 页面状态、`KeepAlive` | 已实现 | `State` 是内存字典，按页面实例保存；构造函数设置 `KeepAlive = true` |
| 方向检测 | 已实现 | 按 Frame 尺寸推断；不强制旋转桌面窗口 |
| Tombstone / 真实 WP 生命周期 | 未实现 | 见[已知限制 §2](known-limitations.md#2-导航与页面模型) |
| `PhoneApplicationService` | 部分实现 | `State`、生命周期事件、idle 检测模式；无 tombstoning |
| `SystemTray` | 部分实现 | 只有状态 API，无视觉 chrome；不是附加属性（见上方构建阻塞） |
| `ApplicationBar`（API、chrome、菜单） | 已实现 | 原版 API 形状与限制；`ApplicationBarPresenter` 绘制 72/30 DIP 栏、96×72 图标槽、48 DIP 圆环、48 px 行高菜单 `Popup`；frame 按 `ReservedHeight` 预留高度 |
| `NavigationContext` | 已实现 | `Uri`、`ExtraData`、大小写不敏感 `QueryString`；可从相对 URI 解析查询串 |

## 3. 转场

| 族 | 状态 | 说明 |
|---|---|---|
| `TransitionFrame` | 已实现 | 两个 `ProjectionHost` 部件；退场完成后再进场；准备阶段关闭命中测试并打开 `Prefer3D`；无动画的转场在下一个 dispatcher 周期完成 |
| `Transition` | 已实现 | 自跟踪 begun/completed；可控时钟；`Stop()` 移除时钟；每次运行最多一次 `Completed` |
| `TurnstileTransition`（4 模式） | 已实现 | 原版 storyboard；`TurnstileCenterOfRotationX = -0.05`、`TurnstileDurationScale = 1.2` 为移植版调优常量 |
| `TurnstileFeatherTransition`（4 模式） | 已实现 | 目标从页面可视树收集；逐元素 `ProjectionHost`；40/50 ms 交错；清理后重绘 |
| `SlideTransition`（8 模式） | 已实现 | 原版 `Slide*Fade*` storyboard；`RenderTransform` 位移与透明度 |
| `RotateTransition`（8 模式） | 已实现 | 原版 `Rotate*` storyboard |
| `SwivelTransition`（6 模式） | 已实现 | 原版 `Swivel*` storyboard |
| `RollTransition`（1 模式） | 已实现 | 原版 `Roll` storyboard |
| `ProjectionTransition` | 已实现 | 转场完成/停止时清除附加投影 |

## 4. 效果

| 项目 | 状态 | 说明 |
|---|---|---|
| `TiltEffect` | 已实现 | 通过 `ProjectionService` 赋投影；支点取第一个可视子元素；隧道式鼠标事件 + 触摸 manipulation；跳过 item 容器 |
| `TurnstileFeatherEffect` | 已实现 | 目标从可视树收集；包装延迟到 dispatcher；`Prefer3D` + `CenterOfRotationX = -0.2`；恢复后 `InvalidateVisual()` |
| `SlideInEffect` | 未实现 | Pivot 选中项动画效果未移植 |

## 5. 控件

| 控件 | 状态 | 说明 |
|---|---|---|
| `WrapPanel` | 已实现 | 原始布局算法 |
| `ToggleSwitch` + `ToggleSwitchButton` | 已实现 | 视觉状态 + manipulation |
| `Rating` / `RatingItem` | 已实现 | 代码生成项目；半星模式以“星”为单位（偏离原版翻倍行为） |
| `PhoneTextBox` | 已实现 | hint、action icon、长度指示；占位符与光标对齐；输入法合成期间隐藏 |
| `ListPicker` / `ListPickerItem` / `ListPickerPage` | 部分实现 | 普通/完整模式、原版应用栏、逐个翻转 |
| `ExpanderView` | 已实现 | 高度动画简化 |
| `HubTile` / `HubTileService` | 已实现 | 两倍高表面滑动 + 真实 `RotationX` 翻转；组冻结 |
| `ContextMenu` / `MenuItem` / `Separator` / `ContextMenuService` | 已实现 | 原版 `Popup` 实现与模板；右键 + 左键长按打开；页面缩放/淡入遮罩生效；无方向分支（示例页 `Samples/ContextMenuSample.xaml` 手工验证：4 个按钮各自连续打开两次、遮罩绘制、`IsZoomEnabled`/`IsFadeEnabled` 开关） |
| `CustomMessageBox` | 已实现 | WPF `Window` 承载；Swivel 弹入/关闭；不依赖 `Application` |
| `DatePicker` / `TimePicker` / `RecurringDaysPicker` | 部分实现 | 值/格式/事件完整；使用模态窗口，picker 页面未移植 |
| `PerformanceProgressBar`、`RelativeAnimatingContentControl` | 已实现 | 不确定进度与透明动画 |
| `Pivot` / `PivotItem` / `PivotHeadersControl` / `PivotHeaderItem` | 已实现 | 代码动画替代 DirectManipulation；鼠标拖拽 + 方向键；`IsLocked` |
| `LongListSelector` | 已实现 | 分组、页眉/页脚、列表/网格模式、跳转列表、`ScrollTo`、`ItemRealized`/`ItemUnrealized` |
| `HyperlinkButton` | 已实现 | 相对/pack URI 导航；外部 URI 交系统处理；`IInvokeProvider` |
| `ClipToBounds` | 已实现 | 附加属性 |
| `AutoCompleteBox`、`LongListMultiSelector`、`MultiselectList`、地图、`TransferControl`、`SpeechTextBox`、`LockablePivot`、`GestureListener`/`GestureService`、`LoopingSelector` | 未实现 | 源码中不存在 |

## 6. 主题、本地化、打包

| 项目 | 状态 | 说明 |
|---|---|---|
| Metro 主题 + WPF 基础控件 WP 样式 | 已实现 | 两个程序集的 `Themes/Generic.xaml`；覆盖 toolkit 控件与常用 WPF 控件；未做与原版的并排像素对比 |
| 运行时主题/强调色切换 | 已实现 | `ThemeManager` + `ThemePalette` + `Theme.xaml`；详见[主题系统](theming.md) |
| 本地化资源 | 已实现 | 51 个 resx（50 个文化 + 中性）；`qps-ploc` 未打包 |
| `Community.Phone` 兼容层 | 已实现 | 独立程序集、独立主题字典 |
| NuGet 包 | 部分实现 | 单包包含两个程序集的设计与 `IncludeCompatibilityLayerInPackage` 已就位；但当前无法编译，且 `csproj` 引用了不存在的随包文档 |
| 示例应用 | 部分实现 | 页面与控件演示齐全；索引 25 条中大部分没有对应页面文件；与库同样受 `SystemTray` XAML 问题影响 |

## 维护规则

行为、API 面或构建状态发生变化时，必须在同一次改动中更新本文件与[API 兼容矩阵](api-compatibility.md)。未实现的内容不得从“未实现”改为“已实现”，除非源码中确实存在实现。
