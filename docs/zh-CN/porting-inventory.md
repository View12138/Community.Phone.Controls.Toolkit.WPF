# 移植功能清单

本文是 `D:\repos\repo-other\WindowsPhoneToolkit` 归档 Windows Phone Toolkit 的 Phase 0 审计中文版。审计通过阅读解决方案、工程文件、共享源码、WP7/WP8 专用源码、XAML 主题、本地化资源和示例完成；不是仅根据 README 或文件名推断。

> 说明：类型名、命名空间、属性名和 API 签名保持英文，以便与源码一致。完整逐类型公共成员附录保留在英文版 [porting-inventory.md](../en/porting-inventory.md)；本文覆盖功能清单、模块状态、依赖关系、XAML 用法和验证结果。

## 仓库审计摘要

| 项目 | 结果 |
|---|---|
| 原始分支/提交 | `master`，`4e4fd5d`（Adding README） |
| 许可证 | Microsoft Public License (MS-PL) |
| 解决方案 | `PhoneToolkit.sln`、`Microsoft.Phone.Controls.Toolkit.WP7.sln` |
| Toolkit 工程 | `Microsoft.Phone.Controls.Toolkit`（共享源码）、`.WP7`、`.WP8`、`.Localization` |
| 示例工程 | `PhoneToolkitSample`（WP7）、`PhoneToolkitSample8`（WP8） |
| 阅读的源文件 | 245 个 `.cs`、82 个 `.xaml`、56 个 `.resx`、51 个 `.xlf`、21 个 `.png`、16 个 `.jpg` |
| 原始目标框架 | WP7.1（Silverlight 4 profile）、WP8（Silverlight 8） |
| 原始平台依赖 | `Microsoft.Phone`、`Microsoft.Phone.Controls`、`Microsoft.Phone.Interop`、`Microsoft.Xna.Framework`、`System.Device`、`Windows.Phone.Speech`、`Microsoft.Phone.Maps` |
| 原始主题 | `Microsoft.Phone.Controls.Toolkit.WP8\Themes\Generic.xaml`（150 KB），WP7 版本（152 KB） |
| 原始本地化资源 | `Microsoft.Phone.Controls.Toolkit.Localization\LocalizedResources\ControlResources.*`，51 个文化 |
| WP7/WP8 差异 | WP7 有独立 `GestureListener`/`ListPickerPage`；WP8 增加 `LongListMultiSelector`、地图、语音和更新后的 picker pages。共享文件链接进两个工程。 |

### 从工程文件确认的构建约束

- 原始工程是非 SDK 风格 MSBuild 工程，目标为 Windows Phone/Silverlight，不能直接用现代 .NET SDK 构建。
- 它们需要 `Microsoft.Phone.dll`、`Microsoft.Phone.Controls.dll`、WP SDK 工具和 XNA 程序集；本 WPF 移植不引用这些依赖。
- `Generic.xaml` 引用 WP 平台资源键（`PhoneAccentBrush`、`PhoneFontFamily*`、`PhoneFontSize*`、`PhoneBorderThickness` 等）和 WP 平台类型；移植版重新创建了这些资源。
- 原始示例是 WP Silverlight 应用，不是 WPF 应用；本仓库新建了 WPF 示例用于运行验证。

## 功能清单与模块状态

状态含义：**已实现** = 仓库中存在移植源码；**部分实现** = 已实现但有文档化差异；**WPF 原生** = 使用 WPF 框架类型；**未实现** = 当前 preview 没有兼容实现。

| 模块 | 原始源文件 | 公共面 | 移植状态 |
|---|---|---|---|
| Projection / 3D | 无原始 toolkit 类型；效果和转场使用平台 `UIElement.Projection`/`PlaneProjection` | `Projection`、`PlaneProjection`、`Matrix3DProjection`、`ProjectionHost`、`ProjectionService` | 已实现（真实 `Viewport2DVisual3D` 渲染；测试） |
| Navigation / page model | WP 平台 `PhoneApplicationFrame`、`PhoneApplicationPage`、`PhoneApplicationService`、`SystemTray` | `PhoneApplicationFrame`、`PhoneApplicationPage`、`NavigationContext`、生命周期、状态、方向、应用服务、系统栏状态 | 已实现子集（WPF Frame/Page；参数/状态/生命周期有测试；系统 chrome 和 idle 策略不同） |
| Navigation transitions | `Transitions\*`（19 个 XAML storyboard，36 个 C# 文件） | `ITransition`、`Transition`、`TransitionElement`、`NavigationTransition`、`TransitionFrame`、Slide/Rotate/Swivel/Turnstile/Roll/Feather | 已实现（代码化 storyboard；自动进入/退出；真实投影动画；失败回退；测试） |
| Effects | `Effects\*` | `TiltEffect`、`SlideInEffect`、`TurnstileFeatherEffect` | TiltEffect + TurnstileFeatherEffect 已实现并有自动化测试；SlideInEffect 未实现（需要 Pivot） |
| AutoCompleteBox | `AutoCompleteBox\*`（11 文件，约 174 KB） | `AutoCompleteBox`、过滤器、选择适配器、popup 辅助 | 未实现 |
| ContextMenu | `ContextMenu\*`（5 文件，约 74 KB） | `ContextMenu`、`ContextMenuService`、`MenuBase`、`MenuItem`、`Separator` | 部分实现（基于 WPF ContextMenu；右键打开测试） |
| CustomMessageBox | `CustomMessageBox\*`（4 文件） | `CustomMessageBox`、结果、`Dismissing`/`Dismissed` | 已实现（WPF Window 宿主） |
| DateTimeConverters | `DateTimeConverters\*`（7 文件） | 6 个转换器 + `DateTimeFormatHelper` | 已实现 |
| DateTimePickers | `DateTimePickers\*`（12 文件） | `DatePicker`、`TimePicker`、`DateTimePickerBase`、`RecurringDaysPicker`、picker pages、LoopingSelector 依赖 | 部分实现（WPF 日期/时间窗口；picker pages 和 LoopingSelector 未移植） |
| ExpanderView | `ExpanderView\*` | `ExpanderView` | 已实现（WPF HeaderedItemsControl；简化高度动画） |
| HubTile | `HubTile\*`（3 文件） | `HubTile`、`HubTileService`、`TileSize`、转换器 | 已实现（定时器驱动；时序近似） |
| ListPicker | `ListPicker\*`（4 文件） | `ListPicker`、`ListPickerItem`、模式、`ExpansionMode` | 部分实现（Popup/模态 Window；`PickerPageUri` 保留） |
| LongListSelector（WP7） | `LongListSelector\*`（8 文件，约 89 KB） | `LongListSelector`、item 类型、group、templated list box | 未实现 |
| LongListMultiSelector（WP8） | `LongListMultiSelector\*`（2 文件，约 57 KB） | `LongListMultiSelector`、item | 未实现 |
| Maps（WP8） | `Maps\*`（14 文件，约 76 KB） | `MapChild`、`MapExtensions`、`MapItemsControl`、`Pushpin`、`UserLocationMarker`、查询辅助 | 未实现（需要地图提供程序） |
| MultiselectList | `MultiselectList\*`（2 文件） | `MultiselectList`、`MultiselectItem` | 未实现 |
| PhoneTextBox | `PhoneTextBox\*` | `PhoneTextBox` | 已实现 |
| Pivot helper | `Pivot\LockablePivot.cs` | `LockablePivot` | 未实现 |
| Primitives | `Primitives\*`（5 文件） | `ClipToBounds`、`LoopingSelector`、`LoopingSelectorItem`、`ToggleSwitchButton` | ClipToBounds + ToggleSwitchButton 已实现；LoopingSelector 未实现 |
| ProgressBar | `ProgressBar\*`（2 文件） | `PerformanceProgressBar`、`RelativeAnimatingContentControl` | 部分实现（不确定模板；相对动画重写未移植） |
| Rating | `RatingControl\*`（2 文件） | `Rating`、`RatingItem` | 已实现 |
| ToggleSwitch | `ToggleSwitch\*`（2 文件） | `ToggleSwitch`、`OffOnConverter` | 已实现 |
| TransferControl | `TransferControl\*`（5 文件） | `TransferControl`、`TransferMonitor`、状态/类型、转换器 | 未实现（需要 WP 后台传输服务） |
| SpeechTextBox | `SpeechTextBox\*` | `SpeechTextBox` | 未实现（需要 Windows Phone 语音） |
| WrapPanel | `WrapPanel\*` | `WrapPanel` | 已实现 |
| GestureListener / GestureService | `Input\*`（WP7/WP8 变体） | 手势事件、监听器、服务 | 未实现 |
| Common/converters | `Common\*`、`Converters\*` | `CultureInfoExtensions`、`ClipToBounds`、`DateTimeWrapper`、长度转换器等 | 部分实现；`LengthConverter`/`TimeTypeConverter` 不重新定义 |
| Localization | `Localization\*` | `ControlResources` 和资源 | 已实现（51 个 resx；50 个真实文化加中性资源） |

## 公共类型清单（摘要）

完整逐类型公共成员附录见 [英文版 porting-inventory.md](../en/porting-inventory.md)。中文版保留关键类型和状态；API 名称不翻译。

| 命名空间 | 类型 | 原始源码 | 状态 |
|---|---|---|---|
| `Microsoft.Phone.Controls` | `AutoCompleteBox`、`AutoCompleteFilterMode` | `AutoCompleteBox\*` | 未实现 |
| `Microsoft.Phone.Controls` | `ContextMenu`、`ContextMenuService` | `ContextMenu\*` | 部分实现 |
| `Microsoft.Phone.Controls` | `CustomMessageBox`、`CustomMessageBoxResult`、`DismissingEventArgs`、`DismissedEventArgs` | `CustomMessageBox\*` | 已实现 |
| `Microsoft.Phone.Controls` | `DatePicker`、`TimePicker`、`DateTimePickerBase`、`DateTimeValueChangedEventArgs`、`RecurringDaysPicker` | `DateTimePickers\*` | 部分实现 |
| `Microsoft.Phone.Controls` | `DailyDateTimeConverter`、`HourlyDateTimeConverter`、`FullViewDateTimeConverter`、`ListViewDateTimeConverter`、`ThreadDateTimeConverter`、`RelativeTimeConverter` | `DateTimeConverters\*` | 已实现 |
| `Microsoft.Phone.Controls` | `ExpanderView` | `ExpanderView\*` | 已实现 |
| `Microsoft.Phone.Controls` | `HubTile`、`HubTileService`、`TileSize`、Tile 转换器 | `HubTile\*` | 已实现 |
| `Microsoft.Phone.Controls` | `ListPicker`、`ListPickerItem`、`ListPickerMode`、`ExpansionMode` | `ListPicker\*` | 部分实现 |
| `Microsoft.Phone.Controls` | `LongListSelector`、`LongListMultiSelector`、`MultiselectList` | `LongList*\`、`MultiselectList\*` | 未实现 |
| `Microsoft.Phone.Controls` | `PhoneTextBox` | `PhoneTextBox\*` | 已实现 |
| `Microsoft.Phone.Controls` | `Rating`、`RatingItem` | `RatingControl\*` | 已实现 |
| `Microsoft.Phone.Controls` | `ToggleSwitch`、`OffOnConverter` | `ToggleSwitch\*` | 已实现 |
| `Microsoft.Phone.Controls` | `WrapPanel` | `WrapPanel\*` | 已实现 |
| `Microsoft.Phone.Controls` | `GestureListener`、`GestureService`、手势事件参数 | `Input\*` | 未实现 |
| `Microsoft.Phone.Controls` | `LockablePivot` | `Pivot\LockablePivot.cs` | 未实现 |
| `Microsoft.Phone.Controls` | `Projection`、`PlaneProjection`、`Matrix3DProjection`、`ProjectionHost`、`ProjectionService` | 平台投影 + 新 WPF 适配 | 已实现 |
| `Microsoft.Phone.Controls` | `PhoneApplicationFrame`、`PhoneApplicationPage`、`NavigationContext`、方向类型 | 平台导航 + 新 WPF 适配 | 已实现子集 |
| `Microsoft.Phone.Controls` | `TransitionFrame`、`TransitionService`、`Transition`、各转场类型 | `Transitions\*` | 已实现 |
| `Microsoft.Phone.Controls` | `TiltEffect`、`TurnstileFeatherEffect` | `Effects\*` | 已实现 |
| `Microsoft.Phone.Shell` | `PhoneApplicationService`、`SystemTray`、`SystemTrayState`、`ProgressIndicator`、`ApplicationBar`、`ApplicationBarIconButton`、`ApplicationBarMenuItem` | WP 平台 Shell | 已实现 API/状态；视觉 chrome 部分待完成 |
| `Microsoft.Phone.Maps.Toolkit` | `MapChild`、`MapExtensions`、`MapItemsControl`、`Pushpin`、`UserLocationMarker` | `Maps\*` | 未实现 |
| `System.Windows.Controls` | toolkit 自带 `WrapPanel`、`LengthConverter`、`TimeTypeConverter` | `Common\*`、`WrapPanel\*` | 使用 WPF 原生或移植到 `Microsoft.Phone.Controls` |
| `System.Windows` | toolkit 自带 `HierarchicalDataTemplate` | `Data\*` | 使用 WPF 原生 |
| `Windows.Devices.Geolocation` | `GeocoordinateExtensions` | `Maps\*` | 未实现 |

## 仅移植版新增的兼容类型

这些类型在原始 toolkit 中没有相同命名空间，是 WPF 平台概念的兼容面。

| 移植类型 | 用途 | 兼容说明 |
|---|---|---|
| `Microsoft.Phone.Controls.NavigationContext` | 为页面提供 `Uri`、`QueryString`、`ExtraData` | 原始平台类型是 `System.Windows.Navigation.NavigationContext`；WPF 无等价类型。 |
| `Microsoft.Phone.Controls.ProjectionHost` | 对任意 WPF 内容应用投影 | 替代缺失的 `UIElement.Projection` 属性。 |
| `Microsoft.Phone.Controls.ProjectionService` | 附加投影和按需包装 Host | 新的 WPF 适配层，放在移植命名空间。 |
| `TransitionFrame.TransitionFailed` / `TransitionFailedEventArgs` | 报告转场工厂/动画失败和回退 | 新的诊断面，原始没有。 |
| `PhoneApplicationFrame.Navigate(PhoneApplicationPage, IDictionary<string,string>)` | 不通过 URI 传递页面参数 | 新的 WPF 友好重载；原始使用平台导航 URI。 |

## 依赖关系

```text
ProjectionHost / ProjectionService
        ▲        ▲
        │        │
   TiltEffect  TurnstileFeatherEffect
        ▲        ▲
        └── Transitions (TransitionFrame, storyboards)
                    ▲
             PhoneApplicationFrame
                    ▲
             PhoneApplicationPage / NavigationContext

Controls
 ├─ ToggleSwitch ── ToggleSwitchButton / OffOnConverter / ControlResources
 ├─ Rating ── RatingItem
 ├─ ListPicker ── ListPickerItem / ExpansionMode / ControlResources
 ├─ ExpanderView ── HeaderedItemsControl / ExpanderPanel / ItemsCanvas
 ├─ HubTile ── HubTileService / Tile 转换器
 ├─ DatePicker / TimePicker ── DateTimePickerBase / DateTimeFormatHelper
 ├─ ContextMenu ── WPF MenuBase / MenuItem / Separator / ContextMenuService
 └─ CustomMessageBox ── WPF Window

Localization
 └─ ControlResources ── 51 个 resx / 50 个真实文化 + 中性资源
```

内部依赖簇：

- `Transitions` 依赖 `Effects`（feather）、`Primitives` 和 `Common`。
- `ProjectionHost` 被转场、TiltEffect、TurnstileFeatherEffect 和示例使用。
- `PhoneApplicationFrame` 是 `TransitionFrame` 的基类；`TransitionFrame` 依赖 `ProjectionHost` 和 `TransitionService`。
- `ControlResources` 被 ToggleSwitch、ListPicker、DateTimePickers、转换器和本地化辅助使用。

## XAML 使用方式与示例统计

原始示例中的主要用法（按出现次数）：

| XAML 用法 | 次数 | 说明 |
|---|---|---|
| `phone:PhoneApplicationPage` | 55 | 页面基类型；需要项目命名空间映射。 |
| `toolkit:TiltEffect.IsTiltEnabled` | 19 | 应用于页面或列表容器。 |
| `toolkit:TurnstileFeatherEffect.FeatheringIndex` | 9 | 逐元素交错索引。 |
| `toolkit:TiltEffect.SuppressTilt` | 1 | 显式禁止某个控件的倾斜。 |
| `shell:ApplicationBar` / `ApplicationBarIconButton` / `ApplicationBarMenuItem` | 20 | 应用栏示例用法；API 模型已实现。 |
| `toolkit:ListPicker` | 多个 | 普通/完整模式、SelectionMode、Header、模板。 |
| `phone:LongListSelector` / `LongListMultiSelector` | 多个 | 当前 preview 不支持。 |
| `toolkit:ExpanderView`、`toolkit:HubTile`、`toolkit:Rating`、`toolkit:ToggleSwitch`、`toolkit:PhoneTextBox` | 多个 | 对应控件已实现。 |
| `toolkit:CustomMessageBox`（C# 构造） | 若干 | WPF Window 宿主。 |
| `toolkit:WrapPanel` | 若干 | 使用移植版或 WPF 原生 `WrapPanel`。 |

原始 `Generic.xaml` 定义了 25 个控件类型的隐式样式，并引用 40 多个 WP 平台资源键。移植版在 `Themes/Generic.xaml` 中重新创建了 Metro 调色板、字体、文本样式、Frame 模板和控件模板。

## 迁移边界

1. **可以原样编译的 XAML**：仅使用 WPF 原生类型和本包中与 WPF 无冲突的类型；仍需正确命名空间。
2. **只需改命名空间或少量属性**：`ToggleSwitch`、`Rating`、`PhoneTextBox`、`WrapPanel`、`ListPicker`、`PerformanceProgressBar` 等。
3. **必须改用包装控件**：所有 `<Grid.Projection>` / `UIElement.Projection` 用法必须改用 `ProjectionHost`；`TransitionFrame` 模板部件必须是 `ProjectionHost`。
4. **无法完全兼容**：AutoCompleteBox、LongListSelector、LongListMultiSelector、MultiselectList、地图、后台传输、语音、Pivot/LockablePivot、SlideInEffect、手势服务、picker pages。

## 测试与验证

当前仓库的验证结果：

| 证据 | 结果 |
|---|---|
| `dotnet build Community.Phone.Controls.Toolkit.slnx -c Release` | 0 warnings，0 errors |
| `dotnet test Community.Phone.Controls.Toolkit.slnx -c Release --no-build` | 57 passed，0 failed |
| `build\verify-package.ps1` | 打包成功；隔离缓存的 fresh consumer 零警告构建并运行成功 |
| 示例启动 | 进程 5 秒后仍存活；无 `%TEMP%\community-phone-toolkit-sample-error.txt` |
| 示例 UI 自动化 | 打开详情页、确认加载、返回首页，无启动错误 |
| 包内容 | 主程序集、XML 文档、50 个卫星资源程序集、主题/BAML、README、LICENSE、第三方声明、中英文文档 |

测试覆盖投影数学和渲染、高 DPI 渲染、投影动画、命中测试/焦点、控件模板/交互、转换器、导航参数/状态/返回键取消/快速导航、进入+退出转场、转场失败回退、TiltEffect、TurnstileFeatherEffect、系统栏/应用服务状态和包消费。视觉一致性和物理触摸仍是文档化的人工检查；见 [known-limitations.md](known-limitations.md)。

## 结论

- 已完成真实源码审计，并建立了功能清单、模块状态、依赖关系和 XAML 兼容边界。
- 已实现并验证 WPF 适配的核心基础设施、投影、导航、转场、效果和一组高价值控件。
- 未实现模块没有被空壳或静默降级伪装；全部列在已知限制中。
- 单一 NuGet 包、全新 WPF 消费工程、示例应用和自动化测试均有实际验证证据。
