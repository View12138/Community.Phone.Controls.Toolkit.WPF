# 已知限制

本移植只针对归档 Windows Phone Toolkit 的一个可验证子集。以下列表明确说明哪些功能未实现、哪些行为有差异，避免把未完成功能误认为已完成。如果某个功能没有在 [api-compatibility.md](api-compatibility.md) 中标记为已实现，请默认它不受支持。

## 1. 不支持的控件与模块

| 原始模块 | 状态 | 原因/替代方案 |
|---|---|---|
| `AutoCompleteBox` | 未实现 | Silverlight popup、选择适配器、过滤和键盘基础设施；包内没有 WPF 替代。请使用应用专用 WPF 自动完成控件。 |
| `LongListSelector`（WP7） | 未实现 | 大型虚拟化列表，含分组、跳转列表和 item 动画。可使用 `ListBox`/`ListView` + `CollectionViewSource` 分组。 |
| `LongListMultiSelector`（WP8） | 未实现 | 依赖 WP8 列表基础设施和选择视觉。可使用 `ListView` 的 `SelectionMode="Multiple"`。 |
| `MultiselectList` | 未实现 | 未移植。 |
| `Maps` / `MapExtensions` | 未实现 | 没有 WPF 地图提供程序；依赖 `Microsoft.Phone.Maps`。 |
| `TransferControl` | 未实现 | 依赖 `Microsoft.Phone.BackgroundTransfer`。 |
| `SpeechTextBox` | 未实现 | 依赖 `Windows.Phone.Speech.Recognition`。 |
| `LockablePivot` / Pivot 集成 | 未实现 | 依赖 WP `Pivot` 控件。 |
| `SlideInEffect` | 未实现 | 依赖 `Pivot` 及其选择事件。 |
| `GestureListener` / `GestureService` | 未实现 | WP 触摸手势服务，含 flick/垂直/水平检测。请使用 WPF `Touch`/`Manipulation` 事件或专用手势库。 |
| `LoopingSelector` | 未实现 | WP picker 内部实现；移植的日期/时间 picker 使用 WPF 模态窗口。 |
| Picker pages（`DatePickerPage`、`TimePickerPage`） | 未实现 | picker 使用自己的模态窗口完成等价的值编辑。`PickerPageUri` 保留但不导航。 |
| `LengthConverter`、`TimeTypeConverter` | 未重新定义 | 使用 WPF 类型转换或在应用代码中解析。 |

## 2. 导航与页面模型

1. **WPF 基类**：`PhoneApplicationPage` 继承 WPF `Page`，`PhoneApplicationFrame`/`TransitionFrame` 继承 WPF `Frame`。WPF 中不存在的 Silverlight 导航 API 不会被模拟。
2. **生命周期**：`OnNavigatedTo`、`OnNavigatingFrom`、`OnNavigatedFrom`、`OnOrientationChanged` 声明为 protected virtual，但 WPF `Page` 不会调用它们。`PhoneApplicationFrame` 从 WPF 导航事件触发这些方法。若页面放在移植 Frame 之外，生命周期方法不会被调用。
3. **方向**：保留 `PageOrientation` 和 `SupportedPageOrientation`，Frame 根据尺寸推断横向/纵向。桌面窗口不会被强制旋转，也不会复刻 WP 自动方向策略。
4. **返回键**：`BackKeyPress` 和 `NavigationService.GoBack()` 可用，但没有硬件返回键。需要时请绑定键盘快捷键或命令。
5. **`PhoneApplicationService`**：`Launching`、`Activated`、`Deactivated`、`Closing`、`State`、`Current` 和 idle 检测在可能时映射到 WPF 应用/窗口事件。WP 的 tombstoning 和生命周期语义不会被复刻；WPF 进程/窗口生命周期不同。
6. **状态持久化**：`PhoneApplicationPage.State` 是内存字典，没有自动 tombstone 序列化。
7. **Journal**：保留 WPF `Frame` journal 行为，但 Silverlight 页面实例缓存和导航模式不完全相同。
8. **导航失败**：移植 Frame 会触发 `NavigationFailed`；导航失败不会像 WP 那样自动终止应用。

## 3. Shell chrome

1. **系统栏**：`SystemTray.IsVisible`、`SystemTray.ProgressIndicator`、`SystemTray.Opacity`、`SystemTray.BackgroundColor`、`SystemTray.ForegroundColor` 及状态变化事件以 API 模型实现。默认 Frame 模板**不会**绘制完整的 Windows Phone 状态栏（信号、电池、时钟、全屏行为）。需要视觉 chrome 的应用应自行渲染。
2. **应用栏**：`ApplicationBar`、`ApplicationBarIconButton`、`ApplicationBarMenuItem`、`IsVisible`、`IsMenuEnabled`、`BackgroundColor`、`ForegroundColor`、`Opacity` 以及命令/点击事件已建模。当前预览的 Frame 模板**不包含**默认视觉呈现器；`Mode`/`StateChanged` 也不存在。示例使用普通 WPF 按钮演示命令接线。
3. **主题颜色**：提供 Metro 颜色和文本样式资源，但没有自动明/暗主题切换和 WP accent 颜色集成。应用可以覆盖资源键。
4. **屏幕尺寸**：不强制 WP 布局常量（800×480、480×800、768×1280 等）。示例使用桌面窗口尺寸。

## 4. 投影与 3D

1. **XAML 面**：WPF `UIElement` 没有 `Projection` 属性，也不能给密封框架类型添加属性元素。移植版提供 `ProjectionHost` 和 `ProjectionService`；原始 `<Grid.Projection>` 必须重写。
2. **投影透视**：相机复现了 XAML 固定的 999 DIP 透视深度以及旋转/偏移的正负，因此投影缩放与透视收缩与 WP 一致。剩余差异是 `TurnstileFeatherEffect`：原版会把每个被羽化元素的透视原点平移到页面的垂直中心（`GlobalOffsetY` 加一个补偿用的 RenderTransform）。`ProjectionHost` 的尺寸等于被包裹元素的尺寸，并且会裁剪 3D 内容，因此本移植版让每个元素围绕自身的垂直中心旋转。旋转角度、错峰延迟和透明度时序与原版完全一致。
3. **`Matrix3DProjection`**：矩阵按 0..1 归一化坐标解释，再转换为像素空间。复杂矩阵应做视觉验证；并非所有 Silverlight 矩阵约定都有测试。
4. **包装限制**：`ProjectionService` 可包装 Panel 直接子元素和部分 content control。它有意不包装 `ListBoxItem` 或 `ContentPresenter`，因为替换这些元素会破坏 item 生成和模板化。需要逐 item 投影的效果会跳过这类目标。元素级效果始终使用**直接包含该元素**的宿主：复用祖先宿主会把整页一起投影，因为页面本身是由转场 Frame 的 presenter 承载的。
5. **布局**：Host 同时 measure/arrange 2D 子元素和隐藏的 3D viewport。期望尺寸取子元素的期望尺寸；投影固定尺寸内容时建议给 Host 设置显式 `Width`/`Height`。
6. **命中测试**：已验证 Host 和内容。嵌套 Host 和变换后的内容可能改变命中坐标，应用交互需自行验证。
7. **文本清晰度**：`Viewport2DVisual3D` 把内容作为纹理渲染，因此元素处于投影状态时 ClearType 子像素渲染会变成灰度抗锯齿。几何保持 1:1（identity 投影与 2D 布局的偏差在 0.01 px 以内）；转场在准备阶段设置 `ProjectionHost.Prefer3D`，让渲染路径切换发生在首帧之前而不是首帧上。不再使用位图缓存：WPF 3D 管线本身就会栅格化内容，额外的 `BitmapCache` 只会让它变糊。
8. **软件渲染**：没有硬件加速时 3D 会回退到软件渲染，性能和视觉质量可能下降；这是 WPF/平台限制。

## 5. 转场与效果

1. **Storyboard 状态**：WPF `Storyboard.GetCurrentState()` 在没有包含时钟时会抛出异常。移植版的 `Transition` 自己跟踪 begun 状态并返回 `Active`/`Stopped`，从而保持已实现场景的 `ITransition` 契约。
2. **模板部件**：`TransitionFrame` 需要两个名为 `FirstContentPresenter`、`SecondContentPresenter` 的 `ProjectionHost` 部件。使用普通 `ContentPresenter` 的自定义 Frame 模板不会运行转场。
3. **转场时序**：时长、关键时间、角度和缓动从原始 storyboard 复制，但 WPF 与 Silverlight 缓动实现可能有细微差异。视觉接近，不是逐位一致。
4. **TurnstileFeatherEffect**：使用逐元素包装和交错动画；无法包装的目标会跳过。清理会恢复原始视觉树，但不适合在效果活动期间重建视觉树的控件。
5. **TiltEffect**：控件在加载时被包装进 `ProjectionHost`，因此只倾斜该控件本身，且不会在指针按下期间修改视觉树；倾斜关闭时会移除包装。支持鼠标输入（通过隧道的 preview 事件），跳过列表 item 容器。倾斜作用于整个控件，不是单个 item 容器；最大倾斜/回弹行为是近似值。鼠标倾斜在通过已启用容器路由的 mouse-up 时结束；触摸 manipulation 是更完整的输入路径。
6. **SlideInEffect**：未实现（需要 Pivot）。
7. **FeatheredTransition**：已实现，但原始 `TurnstileFeatherTransition` 面向 Pivot 内页面内容；移植版将其应用于转场 Frame 的页面宿主。

## 6. 控件

1. **视觉保真度**：控件模板用 WPF 原语近似 Metro 外观。主题还为常用 WPF 控件提供隐式 WP 样式（`Button`、`TextBox`、`ListBox`/`ListBoxItem`、`ComboBox`/`ComboBoxItem`、`CheckBox`、`RadioButton`、`ScrollBar`、`ContextMenu`/`MenuItem`、`ProgressBar` 和 `ToolTip`）。部分专用 WPF chrome（例如 WPF `DatePicker` 内的原生日历）和 WP 专用视觉状态仍有差异；功能 API 优先。
2. **`CustomMessageBox`**：由 WPF `Window` 承载，是独立顶层窗口而不是页内浮层。保留 `Dismiss()` 和结果/事件契约。
3. **`ListPicker`**：普通模式使用 WPF `Popup`，列表项使用 WP 悬停/选中样式；完整模式使用模态 `Window`。不会导航到 `ListPickerPage`，因此 `PickerPageUri`、完整模式页面生命周期和 WP 转场行为未复刻。支持 `SelectedItems` 和 `SelectionMode`。
4. **`DatePicker`/`TimePicker`**：在模态窗口中使用 WPF `DatePicker`/小时分钟选择器。未复刻 WP 的 looping selector、picker page 转场和日期时间 picker 应用栏。支持 `ValueStringFormat`、`ValueString`、`ValueChanged` 和 `Header`。`ValueStringFormat` 是复合格式字符串（例如 `"{0:yyyy-MM-dd}"`）。
5. **`ExpanderView`**：高度动画简化；任意嵌套内容的布局可能与 WP 不同。保留原始 `ItemsCanvas`/`ExpanderPanel` 部件名。展开区使用 18 像素内容缩进，避免竖线与文本相贴。
6. **`HubTile`**：实现了定时器驱动的 hub 循环，但原始 WP 平面投影翻转视觉和精确时序是近似的。支持 `HubTileService` 组冻结/解冻。
7. **`PhoneTextBox`**：实现了 hint、action icon 和长度指示行为。内部编辑器现在可见，并会从模板控件接收焦点；WP 内置文本选择和输入作用域边界在 WPF 中仍可能不同。
8. **`Rating`**：项目在代码中生成；半星和零值选择已实现。除触摸 manipulation 外，也支持鼠标点击/拖动选择。保留模板部件名，但自定义模板必须提供预期部件。
9. **`ToggleSwitch`**：支持 header、switch foreground、checked/unchecked/indeterminate 和 click。状态变化时滑块和背景使用动画过渡；系统 accent 集成仍是近似。
10. **`ContextMenu`**：继承 WPF `ContextMenu`；`ContextMenuService.ContextMenu` 是附加属性。保留 `IsZoomEnabled`、`IsFadeEnabled`、`RegionOfInterest` 和 `Owner`，但部分 WP zoom/fade 视觉未复刻。
11. **`PerformanceProgressBar`**：实现不确定模式，使用简单淡入淡出；原始 WP 性能 workaround 在 WPF 中没有意义。
12. **`RelativeAnimatingContentControl`**：控件存在，但原始 magic-number 相对动画重写没有移植。
13. **`WrapPanel`**：移植原始布局算法并测试。WPF 自带 `WrapPanel` 是独立类型。
14. **`RecurringDaysPicker`**：基于移植的 `ListPicker`；提供本地化星期名，但未复刻 WP 视觉 chrome。

## 7. 本地化

1. 原始 52 个 `.resx` 中有 51 个内嵌，`ControlResources` 由原始 designer 生成。提供 50 个真实文化加中性资源。
2. `qps-ploc` 伪语言未打包，因为 NuGet 会规范化其大小写并产生警告；它不是面向用户的文化。
3. 使用 `Thread.CurrentThread.CurrentUICulture`（以及 WPF 文化流）决定资源文化。没有语言重启或 WP 文化变化事件。
4. WP 平台提供的一些字符串（如应用栏菜单标签、系统栏文本）不在 toolkit 资源中，移植版不负责本地化。

## 8. 打包与平台

1. 包目标为 `net10.0-windows` + `UseWPF`，不能被 WP/Silverlight 工程消费。
2. 包包含编译程序集、主题资源、本地化资源、许可证、README、第三方声明、XML 文档和中英文文档；不包含示例应用或测试。
3. 不依赖 WP SDK、`Microsoft.Phone`、Silverlight 或 `System.Windows.Interactivity`。
4. 包 ID 为 `Community.Phone.Controls.Toolkit.WPF`；程序集名相同，根命名空间为 `Microsoft.Phone.Controls`。
5. 当前为 preview 质量，不是原始 `Microsoft.Phone.Controls.Toolkit.dll` 的二进制直接替换。

## 9. 测试与验证缺口

以下内容尚未完全自动化：

1. 与原始 WP 控件的视觉对比（当前环境没有 WP 模拟器/设备）。
2. 完整转场 Frame 的逐帧视觉验证（导航测试验证投影状态变化，不验证渲染帧）。
3. 应用栏和系统栏视觉（仅 API 测试；视觉呈现器待完成）。
4. Rating（桌面鼠标点击）以及 ToggleSwitch/PhoneTextBox 交互已覆盖真实指针/键盘输入；TiltEffect 仍使用合成路由事件，且当前环境没有物理触摸设备。
5. 日期/时间 picker 模态窗口交互（API/值测试；没有 UI 自动化）。
6. 全部 50 个打包文化（验证资源存在；代表性文化有转换器测试）。
7. 高 DPI、多显示器、软件渲染 3D 行为。
8. 物理触摸硬件（合成鼠标路径和 manipulation 代码已覆盖；当前环境没有触摸设备）。

## 10. 手动验证步骤

### 示例应用

```powershell
dotnet run --project src\Community.Phone.Controls.Toolkit.WPF.Sample
```

1. 确认窗口打开并显示 Home 页。
2. 点击 **Open detail (turnstile)** 确认进入转场执行；点击 **Go back** 确认返回转场执行。
3. 点击 **Animate projection**；状态行应显示 `RotationX` 变化，投影面板在真实 3D 中倾斜；面板同时持续进行 `RotationY` 旋转。
4. 切换 ToggleSwitch 并确认滑块有动画；在 Rating 星星上点击/拖动；在 hint 文本框输入；打开 ListPicker 并检查悬停/选中样式；展开 ExpanderView；打开 ContextMenu；显示 CustomMessageBox；检查 DatePicker、TimePicker、RecurringDaysPicker、PerformanceProgressBar、WrapPanel、HubTile 和 WPF 基础控件。
5. 关闭后检查 `%TEMP%\community-phone-toolkit-sample-error.txt`；不应存在或应为空。

### TiltEffect

1. 在包含按钮的容器上添加 `phone:TiltEffect.IsTiltEnabled="True"`。
2. 按住按钮；应向按压方向倾斜，释放后回弹。
3. 确认按钮仍能收到点击。

### TurnstileFeatherEffect

1. 给多个 `TextBlock` 添加 `phone:TurnstileFeatherEffect.FeatheringIndex`。
2. 导航到页面；元素应按交错时序 feather 进入。
3. 导航离开；包装应被移除，视觉树恢复。

### 包消费

```powershell
dotnet pack src\Community.Phone.Controls.Toolkit.WPF -c Release -o artifacts\packages
dotnet new wpf -n Consumer -o artifacts\consumer
dotnet add artifacts\consumer package Community.Phone.Controls.Toolkit.WPF --source artifacts\packages
dotnet build artifacts\consumer
```

然后在 `MainWindow.xaml` 中加入 `ProjectionHost` 和移植控件并运行，验证包、主题资源和命名空间解析。仓库内推荐直接运行：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File build\verify-package.ps1
```

## 11. 后续路线候选

按优先级大致排序：

1. 基于 WPF 虚拟化的 `LongListSelector`/`LongListMultiSelector`。
2. 使用 WPF Popup/过滤的 `AutoCompleteBox`。
3. Frame 模板中的应用栏和系统栏视觉呈现器。
4. 更精确的 WP 投影校准和视觉回归测试。
5. 使用 WPF 触摸/manipulation 的 `GestureService`。
6. 基于 WPF Pivot-like 控件的 `SlideInEffect`。
7. 所有控件模板的视觉状态和主题一致性。
8. 模态 picker、消息框和转场的自动化 UI 测试。
