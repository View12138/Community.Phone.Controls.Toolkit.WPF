# 已知限制

本移植只覆盖归档 Windows Phone Toolkit 的一个子集。以下列出未实现功能、与平台的行为差异，以及尚未验证的部分。未出现在[API 兼容矩阵](api-compatibility.md)中标记为已实现的类型默认不受支持。

## 1. 不支持的控件与模块

| 原始模块 | 原因 / 替代方案 |
|---|---|
| `AutoCompleteBox` | 依赖 Silverlight popup、选择适配器与过滤基础设施；请使用应用专用的 WPF 自动完成控件。 |
| `LongListMultiSelector`（WP8） | 依赖 WP8 列表与选择视觉；可用 `ListView` 的 `SelectionMode="Multiple"`。 |
| `MultiselectList` | 未移植。 |
| 地图扩展（`MapChild`、`MapItemsControl`、`Pushpin` 等） | 没有 WPF 地图提供程序，依赖 `Microsoft.Phone.Maps`。 |
| `TransferControl` | 依赖 `Microsoft.Phone.BackgroundTransfer`。 |
| `SpeechTextBox` | 依赖 `Windows.Phone.Speech.Recognition`。 |
| `LockablePivot` | WP7 辅助控件由 `Pivot.IsLocked` 取代，移植版直接在 `Pivot` 上实现。 |
| `SlideInEffect` | Pivot 选中项变化时的逐项动画效果未移植。 |
| `GestureListener` / `GestureService` | WP 手势服务；请使用 WPF `Touch`/`Manipulation` 事件。 |
| `LoopingSelector` | WP picker 内部实现；移植的日期/时间 picker 使用 WPF 模态窗口。 |
| picker 页面（`DatePickerPage`、`TimePickerPage`） | picker 在模态窗口中完成等价的值编辑；`PickerPageUri` 保留但不导航。 |

## 2. 导航与页面模型

1. **基类差异**：`PhoneApplicationPage` 继承 WPF `Page`，`PhoneApplicationFrame` / `TransitionFrame` 继承 WPF `Frame`。WPF 中不存在的 Silverlight 导航 API 没有模拟。
2. **生命周期**：`OnNavigatedTo`、`OnNavigatingFrom`、`OnNavigatedFrom`、`OnOrientationChanged` 由 `PhoneApplicationFrame` 在 WPF 导航事件中调用。把页面放在移植 Frame 之外时，WPF 不会调用它们。
3. **方向**：保留 `PageOrientation` 与 `SupportedPageOrientation`，Frame 按尺寸推断横竖；桌面窗口不会被强制旋转。
4. **返回键**：`BackKeyPress` 与 `GoBack()` 可用，但没有硬件返回键；需要时绑定键盘快捷键（示例在窗口底部放了返回按钮）。
5. **`PhoneApplicationService`**：`State`、生命周期事件与 idle 检测模式映射到 WPF 应用/窗口语义；WP 的 tombstoning 与生命周期不复刻。
6. **状态持久化**：`PhoneApplicationPage.State` 是内存字典，没有自动序列化。
7. **Journal**：沿用 WPF `Frame` journal；`PhoneApplicationPage` 设置 `KeepAlive = true`，返回导航回到离开时的实例（相对 WP 默认 `NavigationCacheMode.Disabled` 的有意差异）。需要新实例时在构造函数中设置 `KeepAlive = false`。`NavigationCacheMode` 本身未移植。
8. **导航失败**：导航失败由 WPF 的 `NavigationFailed` 报告，不会像 WP 那样终止应用。

## 3. Shell

1. **系统栏**：`SystemTray` 只提供状态 API（`IsVisible`、`Opacity`、`BackgroundColor`、`ForegroundColor`、`ProgressIndicator`），默认 Frame 模板不绘制状态栏（信号、电池、时钟）。状态载体是 `SystemTrayState.Current`。
2. **`SystemTray` 不是附加属性**：它是静态类，因此 XAML 中的 `shell:SystemTray.IsVisible="True"` 无法编译。需要在代码中设置 `SystemTray.IsVisible = true;`。
3. **应用栏**：由 frame 的 `ApplicationBarPresenter` 绘制。差异：应用栏属于 frame 的 chrome，不随页面转场动画进入；菜单是 WPF `Popup`；图标 URI 按应用资源解析，缺失图标时按钮不显示图片而不是报错；默认颜色使用 Metro chrome 色（原生默认值无法从托管代码观察）。
4. **主题颜色**：库内模板使用 `DynamicResource`，运行时切换会就地更新。应用 XAML 中用 `StaticResource` 解析的主题画刷会保留解析时的值，动画关键帧里捕获的颜色只在模板重新应用时刷新；应用 XAML 请使用 `DynamicResource`。
5. **屏幕尺寸**：不强制 WP 布局常量（480×800、768×1280 等），示例使用桌面窗口尺寸。

## 4. 投影与 3D

1. **XAML 面**：WPF `UIElement` 没有 `Projection` 属性，也不能给密封框架类型增加属性元素。原始 `<Grid.Projection>` 需改为同一元素上的 `phone:ProjectionService.Projection`（按需包装）或显式 `ProjectionHost`。
2. **透视**：相机复现 XAML 固定的 999 DIP 透视深度与旋转/偏移符号，因此投影缩放与透视收缩与 WP 一致。差异在 `TurnstileFeatherEffect`：原版把每个羽化元素的透视原点平移到页面垂直中心，而 `ProjectionHost` 与元素同尺寸且裁剪 3D 内容，因此移植版让元素绕自身中心旋转；角度、错峰延迟与透明度时序一致。
3. **`Matrix3DProjection`**：矩阵按 0..1 归一化坐标解释再转换为像素空间；复杂矩阵没有验证。
4. **包装限制**：`ProjectionService` 只能包装某个 Panel 的直接子元素，且该 Panel 不能是 `ItemsControl` 的 items panel；item 容器（`ListBoxItem`、`ComboBoxItem` 等）跳过，因为 items panel 的子元素由 WPF 管理，直接修改会抛 `InvalidOperationException`。`ContentPresenter` 可以包装。元素级效果始终使用**直接包含**该元素的宿主，复用祖先宿主会把整页一起投影。
5. **布局**：Host 同时 measure/arrange 2D 子元素与隐藏的 3D viewport；投影固定尺寸内容时建议给 Host 显式 `Width`/`Height`。
6. **命中测试**：Host 与内容可用；嵌套 Host 和变换后的内容可能改变命中坐标。
7. **文本清晰度**：`Viewport2DVisual3D` 把内容作为纹理渲染，投影状态下 ClearType 变为灰度抗锯齿；几何保持 1:1。
8. **软件渲染**：无硬件加速时 3D 回退到软件渲染，性能与视觉质量下降。

## 5. 转场与效果

1. **Storyboard 状态**：WPF 对尚不存在的时钟调用无参 storyboard 控制方法会抛异常，`Pause`/`Resume`/`Seek`/`SkipToFill` 需要宿主对象。因此 `Transition` 以可控方式启动 storyboard、自行跟踪状态，并保证每次运行最多触发一次 `Completed`。`TiltEffect` 同样用 begun 标志代替状态查询。
2. **模板部件**：`TransitionFrame` 需要两个名为 `FirstContentPresenter`、`SecondContentPresenter` 的 `ProjectionHost` 部件；使用普通 `ContentPresenter` 的自定义模板不会运行转场。
3. **时序**：直接播放原版 storyboard XAML，因此时长、关键时间、角度与缓动是原值；WPF 与 Silverlight 对同一缓动函数的求值可能有细微差异，视觉接近但不保证逐帧一致。Turnstile 有两处调优常量（轴心 `-0.05`、时长 `×1.2`），设回 `0` / `1` 即还原原工具包。
4. **导航行为**：原版会忽略非应用发起的导航（如应用停用时由 shell 播放的退场）。WPF 对每次 `Frame.Navigate` 都报告 `IsNavigationInitiator == false`，也没有 shell 发起的导航，因此去掉该判断。转场工厂抛出的异常会向上传播（与原版一致），没有回退事件。
5. **出站转场属于被离开的页面**：想要特定退场动画的页面必须自己声明，或在导航前用 `TransitionService.SetNavigationOutTransition` 设置。
6. **`TurnstileFeatherEffect`**：不适合在效果活动期间重建视觉树的控件；无法包装的目标被跳过；页面没有可羽化元素时转场立即完成而不是阻塞导航。
7. **`TiltEffect`**：最大倾斜角度与回弹是近似值；触摸 manipulation 是比鼠标路径更完整的输入路径。
8. **`FeatheredTransition`**：原版面向 Pivot 内页面内容，移植版应用于转场 Frame 的页面宿主。

## 6. 控件

1. **视觉保真度**：控件模板用 WPF 原语近似 Metro 外观，部分专用 chrome（例如 WPF `DatePicker` 内的原生日历）与原版不同；功能 API 优先。
2. **`CustomMessageBox`**：独立顶层 `Window` 而不是页内浮层；Swivel 期间看不到其后的页面。
3. **`ListPicker`**：普通模式对模板 Canvas 直接做高度/位移动画（storyboard 无法驱动模板里的 `TranslateTransform`）；完整模式页面由控件自己创建，挂树前看不到 `Application.Resources`，因此在自身 `Resources` 中合并 `Theme.xaml`，模板关键帧使用 `StaticResource`；长列表只对屏幕内容器做动画，并在 dispatcher 轮次里重复 `ScrollIntoView` 直到选中项就绪。页面 XAML 目前因 `shell:SystemTray.IsVisible` 无法编译。
4. **`DatePicker` / `TimePicker`**：在模态窗口中使用 WPF 日期/时间 UI，未复刻 looping selector、picker 页面转场与应用栏。`ValueStringFormat` 是复合格式字符串（例如 `"{0:yyyy-MM-dd}"`）。
5. **`ExpanderView`**：高度动画简化；布局与 WP 可能存在差异；保留 `ItemsCanvas`/`ExpanderPanel` 部件名。
6. **`HubTile`**：滑动偏移由代码施加（WPF 不会应用绑定模板父级的 storyboard 值）；`TranslateTransform` 由代码创建（模板中声明的 `Freezable` 会被冻结）；视口外套一层普通面板（`ProjectionService` 只包装父级是面板的元素）；`CenterOfRotationY` 取 `0.5`（原版 `0.25` 相对两倍高表面的同一条线）。
7. **`PhoneTextBox`**：WP 内置文本选择与输入作用域边界在 WPF 中可能不同；水印动画的视觉细节未验证。
8. **`Rating`**：偏离原版——原工具包在 `AllowHalfItemIncrement` 时把翻倍后的位置数写入 `Value`，导致填充区域是指针位置的两倍；移植版以“星”为单位存储。自定义模板必须提供预期部件。
9. **`ContextMenu`**：自绘 `Popup`，不使用 WPF `ContextMenu`。WPF 没有 `Hold` 手势：右键直接打开，左键长按由 800 ms 计时器模拟（按下后移动超过拖动阈值或提前抬起即取消，松手后才弹出，因此不会在长按松手时误触菜单项）。横竖屏分支沿用原版布局，依赖 `PhoneApplicationFrame.Orientation`。
10. **`PerformanceProgressBar` / `RelativeAnimatingContentControl`**：原版的性能 workaround 与 magic-number 相对动画重写在 WPF 中没有意义，只保留外观与基本行为。
11. **`RecurringDaysPicker`**：基于 `ListPicker`，本地化星期名，不复刻 WP 视觉 chrome。
12. **`Pivot`**：输入是鼠标拖拽加 Left/Right/PageUp/PageDown/Home/End，而不是 DirectManipulation（在 pivot 上启用 `IsManipulationEnabled` 会让触摸输入不再到达子控件）；没有快速滑动速度判定，拖拽超过半个视口即切换；页眉条循环排布但只绘制一圈，页眉总宽小于视口时末尾留白；`IsLocked` 是依赖属性并用 200 ms 动画淡出页眉；没有自定义自动化对等体；只有当前项可见，离开项滑动结束后折叠。
13. **`LongListSelector`**：列表模式虚拟化，网格模式实现全部项目；跳转列表是普通 `Popup`，没有原版的 `RotationX` 翻转；分组对象必须实现 `IList`；模板部件 `ViewportControl` 是 `ContentControl` 而不是手机的滚动器，`VerticalScrollBar` 是真实 `ScrollBar`；`SelectedItem` 是依赖属性；选择没有视觉 chrome（与手机一致）。
14. **`HyperlinkButton`**：WPF 没有命名导航作用域，`TargetName` 保留但不生效；默认主题把字符串 `Content` 渲染为下划线文本，非字符串内容需要自定义模板。

## 7. 本地化

1. 原始 52 个 `.resx` 中内嵌 51 个（50 个真实文化 + 中性资源），`ControlResources` 由原始 designer 生成。
2. `qps-ploc` 伪语言未打包。
3. 资源文化由 `Thread.CurrentThread.CurrentUICulture`（及 WPF 文化流）决定；没有 WP 的语言重启或文化变化事件。
4. 应用栏菜单标签、系统栏文本等 WP 平台字符串不在 toolkit 资源中，移植版不负责本地化。

## 8. 打包与平台

1. 目标框架 `net10.0-windows` + `UseWPF`，不能被 WP/Silverlight 工程消费。
2. 包设计上包含两个编译程序集、主题资源、本地化卫星程序集、XML 文档与随包文档，不包含示例或测试。
3. 不依赖 WP SDK、`Microsoft.Phone`、Silverlight 或 `System.Windows.Interactivity`。
4. 包 ID 为 `Community.Phone.Controls.Toolkit`；根命名空间仍是 `Microsoft.Phone.Controls` / `Microsoft.Phone.Shell`。
5. **当前不可打包**：toolkit 工程编译失败（见[移植状态 §构建与验证](porting-status.md#构建与验证)），且 `csproj` 的打包项引用了仓库中不存在的 `README_zh-CN.md`、`THIRD-PARTY-NOTICES_zh-CN.md`、`docs/en/*.md`、`docs/zh-CN/*.md`。
6. 当前为 preview 质量，不是原始 `Microsoft.Phone.Controls.Toolkit.dll` 的二进制替换。

## 9. 验证缺口

1. 仓库内没有自动化测试工程；所有行为结论来自源码检查与示例运行，没有回归测试保护。
2. 与原始 WP 控件的视觉对比没有做（当前环境没有 WP 模拟器/设备）。
3. 转场只验证了“播放原版 storyboard 资源”这一实现事实，没有逐帧像素对比；只有 Turnstile 的轴心与时长为手动调优值。
4. 应用栏的栏高、槽位、圆形图标、菜单与预留空间由实现决定，仅 ListPicker 完整模式页面曾与 WP 8.1 截图对比；`SystemTray` 没有任何绘制。
5. 输入通过路由事件与合成事件覆盖，没有物理触摸设备，因此 `TiltEffect`、manipulation 与拖拽行为缺少真实输入验证。
6. 日期/时间 picker 的模态窗口交互、全部 50 个文化的资源、高 DPI/多显示器/软件渲染 3D 均未系统验证。

## 10. 手动验证步骤

### 示例应用

```powershell
dotnet run --project samples\Community.Phone.Controls.Toolkit.Sample
```

1. 窗口打开后应显示 `MainPage` 的示例索引（标题、`samples`、条目列表与应用栏 Settings 按钮）。
2. 点击 `transitions` 条目 → 选择转场族 → 导航到目标页 → 返回，确认两侧都使用所选族。
3. 进入 `featheredtransitions` 页面，确认六个元素依次羽化进场；离开时若源页没有羽化元素应回退为 turnstile 退场。
4. `ProjectionPage`：确认投影面板在真实 3D 中倾斜、持续旋转，内容仍可交互。
5. `WPFControlsSample`：逐个操作 `ToggleSwitch`、`Rating`、`PhoneTextBox`、`ListPicker`（展开与完整模式）、`RecurringDaysPicker`、`ExpanderView`、`DatePicker`、`TimePicker`、`PerformanceProgressBar`、`HubTile`。
6. `PivotSample`：拖拽、点击页眉、方向键切换，勾选 `IsLocked` 后确认无法切换。
7. `LongListSelectorSample`：确认组页眉与条目显示，点击组页眉打开跳转列表并滚动到该组，切换网格模式，进入 `PersonDetail` 后返回。
8. `Settings`：切换 UI 文化、区域格式、`RequestedTheme`（明/暗）与强调色，确认界面即时更新。

### TiltEffect

1. 在包含按钮的容器上设置 `phone:TiltEffect.IsTiltEnabled="True"`。
2. 按住按钮应向按压方向倾斜，释放后回弹；按钮仍能收到点击。

### TurnstileFeatherEffect

1. 给多个 `TextBlock` 设置 `phone:TurnstileFeatherEffect.FeatheringIndex`。
2. 导航到页面时元素按交错时序羽化进入；离开后包装被移除、视觉树恢复。

### 包消费

```powershell
dotnet pack src\Community.Phone.Controls.Toolkit -c Release -o artifacts\packages
dotnet new wpf -n Consumer -o artifacts\consumer
dotnet add artifacts\consumer package Community.Phone.Controls.Toolkit --source artifacts\packages
dotnet build artifacts\consumer
```

然后在 `MainWindow.xaml` 中加入 `ProjectionHost` 与移植控件并运行，验证包、主题资源与命名空间解析。当前需先修复编译与打包项（见 §8）。
