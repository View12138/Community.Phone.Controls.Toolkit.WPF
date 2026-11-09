# 交接文档 — Community.Phone.Controls.Toolkit

[English](../handoff.md) · 中文

**用途。** 这是新会话的入口：记录工作区里**真实成立**的东西——已验证的构建/测试/运行结果、已确认的设计决策、不能丢失的未提交修改，以及具体的下一步。它不重述原始任务计划（`.github/Guide.md`），也不替代完整 API 矩阵。

**最后更新：** 2026-11-09（本地时间），分支 `main`，工作区**有未提交修改** —— `LongListSelector` 移植（`Controls/LongListSelector/*`、`Themes/Controls/LongListSelector.xaml`、测试、示例页、文档）、`HyperlinkButton` 实现（导航、自动化对等体、导航上下文修复、测试）以及示例修复（路由探测、移植 `Settings.xaml`、可见的错误提示）已提交至 `7eddd7a`；返回导航的页面生命周期修复（`PhoneApplicationPage.KeepAlive`、`NavigationCachePage` 回归测试、文档）以及 2026-11-09 的重构（去掉所有工程/文件夹名中的 `.WPF`，把 `Microsoft.Phone.dll` 兼容类型移入新的 `Community.Phone` 工程）尚未提交。

---

## 1. 项目目标与验收标准

归档版 Windows Phone Toolkit 的 WPF 兼容移植，交付为单个 NuGet 包（`Community.Phone.Controls.Toolkit`，根命名空间 `Microsoft.Phone.Controls`，`net10.0-windows`）。

当前采用的验收标准：

1. **API 兼容** —— 原始 `Microsoft.Phone.Controls` / `Microsoft.Phone.Shell` 类型存在且成员/默认值兼容；所有差异都写入 `docs/*/api-compatibility.md` 与 `docs/*/known-limitations.md`。
2. **真实行为，不是空壳** —— 投影是真实的 `Viewport3D`/`Viewport2DVisual3D`，转场是真实 storyboard，控件是带 WP Metro 样式的真实 WPF 控件。
3. **证据** —— 解决方案 0 警告/0 错误、测试全绿、`build\verify-package.ps1` 通过（打包 + 全新消费工程）、示例可运行并覆盖功能。
4. **如实标注状态** —— 未实现模块明确列为未实现；没有验证手段的内容不得声称可用。

---

## 2. 当前已验证状态（2026-11-09）

| 检查项 | 命令 | 结果 |
|---|---|---|
| Release 构建 | `dotnet build Community.Phone.Controls.Toolkit.slnx -c Release` | **0 错误 / 2 个唯一警告** —— 只剩 `ViewportControl` 中刻意保留的公共事件警告；拆分兼容层时已删除 `WindowHelper` 的不可达代码警告。`WINDOWS_PHONE;WP8` 在所有配置下都定义 |
| Release 测试 | `dotnet test Community.Phone.Controls.Toolkit.slnx -c Release` | **142 通过 / 2 失败 / 0 跳过，共 144**，约 40 秒。两个失败与本次改动无关、且在基线上同样存在：`ControlTests.ListPicker_FullModeSingleSelectionUsesThePhoneItemStyle` 与 `ControlTests.ListPicker_HighlightedVisualStateUsesBrushResources`。新增的 `EffectsTests.TiltEffect_StaleStateFromAnotherThreadDoesNotThrow` 通过，且在 dispatcher 访问保护之前会失败 |
| Debug 测试 | `dotnet test Community.Phone.Controls.Toolkit.slnx -c Debug` | **140 通过 / 2 失败** —— Debug 下同样是那两个 `ListPicker` 断言失败，且在基线 `b55dc12` 的 Debug 下也一样失败（基线 `--filter ListPicker`：12 通过 / 2 失败） |
| 打包 + 消费 | `powershell -File build\verify-package.ps1` | **通过** —— `artifacts/packages/Community.Phone.Controls.Toolkit.0.1.0-preview.1.nupkg`（含 `.snupkg`）中同时包含 `Community.Phone.Controls.Toolkit.dll` 与 `Community.Phone.dll`；全新消费工程从本地源还原、零警告构建并运行，并验证兼容层的 frame 模板（`ApplicationBar` 模板部件） |
| 示例冒烟（UI 自动化） | 启动 `Community.Phone.Controls.Toolkit.Sample.exe` 并打开 Pivot 与 HubTile 示例 | **通过** —— 主页面列出新的 `Pivot` 条目，选中后打开 `PivotSample`（自动化树：活动的 `PivotItem` 及其内容、`SelectedIndex 0 of 3`、锁定复选框、返回按钮），`IsLocked` 可切换，返回按钮回到索引页；`HubTileSample`（三项 `Pivot` + 原版 hub tile）在 Release 与 Debug 下都能渲染文本和图片。进程存活且无新错误文件（`%TEMP%\community-phone-toolkit-sample-error.txt` 仍是 2026-10-23 的 ListPicker 遗留）。`HubTileSample` 最初抛 `Cannot locate resource 'images/dessert.jpg'`：把照片作为 `Resource` 加入后增量构建仍使用旧的 `.g.resources` 文件列表，重建即可，README 已注明 |

| 示例导航（UI 自动化） | 启动 `Community.Phone.Controls.Toolkit.Sample.exe`，调用索引条目与应用栏 **Settings** 按钮 | **通过** —— 索引正常渲染（应用标题、`samples`，25 条已实体化超链接中 8 条可用）；未移植条目置灰且无法调用（Invoke 模式不可用、页面不变）；`longlistselector` 条目打开 `LongListSelectorSample`（组页眉与城市条目）；导航栏返回按钮回到索引；**Settings** 打开已移植的设置页并显示两个文化选择器。进程保持存活且不写错误日志。本会话鼠标点击无法可靠送达，因此导航通过自动化 `Invoke` 模式驱动 |

测试清单（144 个用例）：`ControlTests` 36、`ProjectionTests` 32、`NavigationTests` 18、`TransitionStoryboardTests` 13 用例、`PivotTests` 12、`ShellTests` 11、`LongListSelectorTests` 8、`EffectsTests` 8、`HyperlinkButtonTests` 3、`ConverterTests` 3，以及 `StaTest`/`WpfTestHelper` 基础设施。

**2026-11-09 —— 工程命名与 `Community.Phone` 兼容层。** 所有工程名和文件夹名都去掉了 `.WPF`：解决方案为 `Community.Phone.Controls.Toolkit.slnx`，工程为 `Community.Phone`、`Community.Phone.Controls.Toolkit`、`Community.Phone.Controls.Toolkit.Sample`、`Community.Phone.Controls.Toolkit.Tests`。`Microsoft.Phone.dll` 表面（`Microsoft.Phone.Controls.PhoneApplicationFrame` / `PhoneApplicationPage` / `NavigationContext` / 方向类型，以及全部 `Microsoft.Phone.Shell` 类型）从 toolkit 工程移入 `src/Community.Phone`，由 toolkit 引用。`PhoneApplicationFrame` 模板移到 `src/Community.Phone/Themes/Generic.xaml`；toolkit 主题只保留 `TransitionFrame` 模板。单一 NuGet 包仍然同时包含两个程序集（没有第二个包）。同时使用两个程序集的 XAML 需要 `phone:`（toolkit）、`platform:`（`Community.Phone` 的页面/框架类型）和 `shell:`（`Community.Phone`）前缀；示例、测试、包消费工程、README 和中英文文档均已更新。`Transitions.StoryboardAssemblyName` 与所有 pack URI 都改为新程序集名。回归测试：`EffectsTests.TiltEffect_StaleStateFromAnotherThreadDoesNotThrow`（xUnit 执行顺序变化暴露了 `TiltEffect` 的跨线程清理问题；`StopTiltReturnStoryboardAndCleanup` 现在先检查 `Dispatcher.CheckAccess()` 再触碰静态 UI 状态）。

**2026-11-02 —— 返回导航回到现有页面实例。** `PhoneApplicationPage` 现在在构造函数中设置 `KeepAlive = true`。WPF 原本会在离开以 URI 导航的 `Page` 时丢弃实例，并在 `GoBack()` 时按 URI 重新构造它；这导致示例从 `PersonDetail` 返回时会重建 `LongListSelectorSample`（连同构造函数里的工作一起重做）。对象导航本来就会保留实例，现在 URI 导航与之保持一致；确实需要新实例的应用可以在页面构造函数中设置 `KeepAlive = false`。由 `NavigationTests.Frame_BackNavigationReturnsTheExistingPageInstance` 验证：改动前失败（返回时构造了第二个 `NavigationCachePage` 实例），改动后通过。其他导航行为不变：向前 URI 导航仍然创建新实例，`CanGoBack` 仍由 WPF journal 维护。

**2026-11-02 —— 示例不再静默失败。** 移除了示例应用里两条会走到死链的导航路径。`MainPage` 索引现在用 `Application.GetResourceStream` 逐条探测路由，把尚未移植的条目置灰禁用，因此点击不会再导航到不存在的资源并让应用直接退出；与实际文件位置不一致的路由也已修正（`/Samples/WPFControlsSample.xaml`、`/Pages/MenusPage.xaml`、`/Pages/FeatherPage.xaml`、`/Pages/TransitionsPage.xaml`、`/Samples/PivotSample.xaml`）。应用栏 **Settings** 按钮原先导航到本移植版并不存在的 `/Settings.xaml`；现已从 `PhoneToolkitSample8` 移植 `Settings.xaml`/`Settings.xaml.cs`（UI 文化与区域格式两个 `ListPicker`，配合 `App.UICultureOverride`/`RegionalCultureOverride`）。未处理异常现在弹出对话框并给出 `%TEMP%\community-phone-toolkit-sample-error.txt` 路径，不再无声关闭窗口。已用 UI 自动化验证：索引渲染正常（`WINDOWS PHONE TOOLKIT for WPF`、`samples`，25 条已实体化的超链接中 8 条可用），禁用链接无法调用且页面不变，LongListSelector 链接打开 `LongListSelectorSample`（组页眉与城市条目），导航栏返回按钮回到索引，**Settings** 打开含两个文化选择器的设置页，且没有错误日志。环境说明：本会话中鼠标点击无法可靠送达（导航只能通过自动化 `Invoke` 模式触发），`CopyFromScreen` 截图可能包含遮挡窗口，因此运行时证据来自自动化树与置顶窗口截图。新增 `LongListSelectorTests.LongListSelector_PaintsEveryRealizedItem`，用 `RenderTargetBitmap` 在 96 DPI 下渲染列表，要求视口内每个已实体化容器都画出像素，从而挡住“元素存在但什么都没画”这类缺陷。

**2026-10-30 —— `HyperlinkButton`。** 平台控件现在执行真实导航：相对 URI 与 `pack:` URI 导航最近的 `NavigationService`/祖先 `Frame`，其他绝对 URI 通过可重写的 `OpenExternalUri` 钩子（默认用 `Process.Start` + `UseShellExecute`，即浏览器/邮件客户端）。`TargetName` 保留，但 WPF 没有命名导航作用域。Click、Enter/Space 激活、`CommonStates` 视觉状态和 `IInvokeProvider` 超链接自动化对等体已实现。`NavigationContext.FromNavigationEventArgs` 现在能从相对 URI 解析查询串（WPF 会原样传递相对导航 URI，而 `Uri.Query` 对相对 URI 抛异常）；该缺陷曾让示例中的所有相对 URI 导航失败。UI 自动化已验证完整链路：调用索引页的 “a grouping items control with great performance” 超链接后进入 `LongListSelectorSample`，分组列表渲染出 A/B/D 组页眉与城市项目，`Toggle grid` 切换到网格模式，错误日志为空。

**2026-10-30 —— `LongListSelector`。** `Controls/LongListSelector/` 按 Windows Phone 8.1 内置 `Microsoft.Phone.Controls.LongListSelector` 参考实现补齐：`ItemsSource`（平铺或实现 `IList` 的分组）、`IsGroupingEnabled`、`HideEmptyGroups`、列表/组页眉与页脚、`ItemTemplate`、`LayoutMode`/`GridCellSize`、`SelectedItem`（提升为依赖属性）、键盘选择、`ScrollTo`、`ItemRealized`/`ItemUnrealized`、`ManipulationState`，以及跳转列表（`JumpListStyle` 打开组磁贴 `Popup`，选择后滚动到该组）。内部是 WPF 实现：内部 `ItemsControl` + `ContentPresenter` 容器，列表模式使用 `VirtualizingStackPanel` 回收虚拟化，网格模式使用固定单元格 `LongListSelectorGridPanel`，真实的 `ScrollBar` 模板部件绑定内部 `ScrollViewer`。分组和项目以真实数据对象推入容器，模板绑定与手机一致。`Common/LongListSelectorExtensions.GetItemsInViewPort` 与 `TiltEffect.StartTiltEffectOnLLS` 已适配 WPF 视觉树（不再有 Canvas/`ViewportControl` 祖先）。示例 `MainPage`（之前是空白页，且在恢复缺失的 `HyperlinkEmptyStyle`/`MultilineHyperlinkStyle` 之前根本无法加载）现在是可用的 `LongListSelector`，`Samples/LongListSelectorSample.xaml` 演示分组、跳转列表与网格模式。证据见 `LongListSelectorTests` 与 known-limitations 中的 LLS 条目。

**2026-10-27 —— `Pivot`。** `Controls/Pivot/` 现包含 `Pivot`、`PivotItem`、`PivotItemEventArgs`、`PivotHeadersControl`、`PivotHeaderItem` 和内部 `SelectedIndexChangedEventArgs`；`Themes/Controls/Pivot.xaml` 提供标题/页眉/项样式（数值取自手机主题）并合并进 `Generic.xaml`。手机控件由 DirectManipulation 与合成器驱动，因此移植版用代码动画并沿用原版数值：内容滑动 0.25 秒（`QuarticEase`）、页眉滚动 0.5 秒、指针跟踪 600 px/s。输入是指针拖拽加半视口阈值、点击页眉，以及 Left/Right/PageUp/PageDown/Home/End；`Pivot.IsLocked` 用 200 毫秒淡出页眉并拒绝修改项。有两个主题细节是必需的：`Themes/Controls/*.xaml` 必须各自合并 `Theme/Dark.xaml`（延迟 Setter 里的 `StaticResource` 只在所属字典内解析——否则 `PivotTitleStyle` 的前景色会变成 `DependencyProperty.UnsetValue` 并抛异常）；`PivotHeaderItem` 的容器样式需要 `BasedOn="{StaticResource {x:Type primitives:PivotHeaderItem}}"`，因为 WPF 在显式赋样式时会替换隐式样式。前进/后退的进入方向现在每次选中变化都按索引位移推断；原版保留上次的方向提示，使绑定 `SelectedIndex` 的滑动方向出错。`ControlTests.HubTile_PropertiesAndGroupFreeze` 在本轮之前就是失败的：它在从未布局过 tile 的情况下调用 `FreezeGroup`，而 tile 要在第一次 `SizeChanged` 时才加入 `HubTileService`，因此该测试改为把 tile 放进窗口。

`NavigationTests.TransitionFrame_FallsBackWhenTransitionFactoryThrows` 已随移植版的 `TransitionFailed` 事件一起删除：现在与原版一致，`TransitionElement` 工厂抛出的异常会向上传播，而不是降级成无转场导航。

**2026-10-27 —— `HubTile`。** 修复两个长期缺陷：`Source` 图片从不显示、翻转从不产生动画。当时生效的模板是 `Themes/Generic.xaml` 里的内联副本（它会遮蔽合并进来的 `Themes/Controls/HubTile.xaml`），而它从不移动滑动表面 —— 偏移来自视觉状态 storyboard 里绑定 `HeightConverter` 的值，而 WPF 会静默丢弃绑定模板父级的 storyboard 值，于是 `TitlePanel.Y` 一直是 `0`，图片带从未进入一瓦片高的视口。现在内联副本已删除（`Themes/Controls/HubTile.xaml` 是唯一模板，并合并 `Theme/Dark.xaml`），偏移改由代码施加：Expanded/Flipped 为 `-H`、Semiexpanded 为 `-0.4566H`、Collapsed 为 `0`，时长与缓动沿用原版 0.85 秒 `CubicEase` EaseOut。此后翻转超过 90° 仍然什么都不渲染，因为 WPF 会剔除背面，所以 `ProjectionHost` 的四边形同时带上反向绕序：XAML 投影从两侧都可见，翻转必须继续显示镜像内容（`ProjectionHost_RendersContentPastNinetyDegrees`）。另外三处 WPF 细节：滑动的 `TranslateTransform` 在 `OnApplyTemplate` 里创建，因为模板中声明的 `Freezable` 会被冻结、无法动画；视口套在普通 `Grid` 里，因为 `ProjectionService` 只包装父级是面板的元素（模板里是 `Border`）；`DiscreteObjectKeyFrame` 的值用 `{x:Static Visibility.Collapsed}`，因为模板 storyboard 里的普通字符串不会被转换成 `Visibility`。`CenterOfRotationY` 在移植版里取 `0.5`（原版 `0.25` × 两倍高表面，在宿主投影的一倍高平面上是同一条线）—— 用 `0.25` 时翻转后的表面会落在控件上方半个瓦片处。`ControlTests.HubTile_ShowsTheSourceImageAndFlipsToTheBackPanel` 校验 Expanded/Semiexpanded 偏移、`0→180` 旋转、正反面切换以及渲染像素。

**2026-10-27 —— 投影裁剪。** 手机版 Toolkit 会把投影结果画到元素边界之外：`HubTile` 翻转时被透视放大的近端会压到相邻瓦片上，而不是在瓦片边缘被切掉。移植版被裁剪了两次 —— `ProjectionHost` 把 `Viewport3D` 裁到宿主槽，`HubTile` 模板又加了 `Grid ClipToBounds="True"` —— 于是翻转看起来像在一扇窗后面进行。现在的规则与平台一致：*内容*被栅格化进布局槽，*投影结果*不裁剪。`Viewport3D.ClipToBounds` 为 `false`，内部 `ProjectedSurface` 带一个布局槽大小的 `RectangleGeometry` 裁剪（这同时保证子元素溢出时贴图映射仍一一对应 —— `HubTile` 的表面有两倍瓦片高、视口只有一倍高），宿主自己只在 2D 渲染（此时没有平面）时设置 `ClipToBounds`。模板保留外层普通 `Grid`（为 `ProjectionService` 提供面板父级）但不带裁剪。测试：`HubTile_FlipDrawsOutsideTheTileBounds`（滑动带仍在界内，45° 翻转画到界外）、`ProjectionHost_DoesNotClipTheProjectedResult`、`ProjectionHost_ClipsItsContentToTheLayoutSlot`。

已没有任何测试依赖操作系统指针。原 `ControlTests.Rating_MouseClickChangesValue`（移动物理光标，桌面不允许时自行跳过）改为 `Rating_PointerPositionUpdatesValue`，直接调用控件 `internal` 的 `UpdateValueFromMouse`，因此锁屏、被遮挡或虚拟化的桌面既不会造成失败，也不会被静默跳过。五个 `TransitionFrame_NavigatesWithEveryFamily` 用例以及效果/导航的等待不再固定 sleep 2000–3000 ms，而是通过 `WpfTestHelper.PumpUntil(condition, timeout)` 轮询可观察状态 —— 这也是测试耗时从约 45 秒降到约 30 秒的原因。

---

## 3. 已确认的架构决策（及原因）

完整细节：[docs/zh-CN/architecture.md](architecture.md) · [English](../en/architecture.md)。

1. **投影用 WPF `Viewport3D` + `Viewport2DVisual3D`** —— 内容保持实时可交互（命中测试、焦点、真实控件），截图平面方案会丢失交互；Windows Composition/D3D 互操作因打包复杂度被否决。
2. **忠实 XAML 的透视** —— 固定 `PerspectiveDepth = 999` DIP（WinUI `PlaneProjection` 用 `M34 = -1/999`），相机位于 `z = depth`，水平视场角由视口**宽度**推导。原因：WPF `PerspectiveCamera.FieldOfView` 是水平视场角、垂直视场角由宽高比推导；按高度推导会让所有被投影页面按宽高比缩小（480×800 → 288×480）。
3. **符号约定** —— `R = Rz·Ry·Rx`、行向量矩阵、Y 偏移取反、**RotationY 取反**。原因：XAML 中正 `RotationY` 让右边缘靠近观察者，而 WPF 的 +Y 正向旋转让它远离；RotationX/Z 因元素空间 Y 向下而已天然一致。已用 `TiltEffect` 按压方向与 WinUI/Uno 实现交叉验证。
4. **转场严格串行** —— 退场完成后才开始入场；`TransitionFrame` 在两个 `ProjectionHost` presenter 之间切换。与原版 `TransitionFrame` 一致。
5. **原版 storyboard XAML，运行时加载** —— 原版 27 个 `Transitions/Storyboards/*.xaml` 以 WPF 资源形式内嵌，用 `XamlReader` 解析（唯一改动是投影属性路径，需要 `phone:` 前缀）。原因：保持工具包权威的关键时间/角度/缓动逐字节一致，而不是用代码重新表达——之前的移植已经出现偏差（动画成 `-69°`/`45°` 而不是 `-80°`/`50°`）。
6. **渲染路径一致** —— `ProjectionHost.Prefer3D` 让 Host 在投影仍是 identity 时保持 3D 路径，使转场/效果在**准备阶段**切换路径而不是在动画首帧。转场与效果的宿主不再使用 `BitmapCache`（3D 管线本身会栅格化，额外 1× 缓存实测会明显变糊）。
7. **元素级效果使用自己的宿主** —— `ProjectionService.EnsureProjectionHost` 只复用**直接包含该元素**的宿主；使用祖先宿主会把整页一起投影（这是已修复的真实 bug，现有测试覆盖）。

---

## 4. 最近一次会话完成的工作

以下内容除特别标注外均为**已实现且已验证**。

### 4.1 投影几何（“转场和原版完全不一样”的根因修复）

- `Projection/ProjectionHost.cs`：把 FOV 旋钮替换为 `PerspectiveDepth`（999 DIP），`UpdateCamera` 改为按视口**宽度**推导水平视场角；新增 `Prefer3D`。
- `Projection/PlaneProjection.cs`：`ToMatrix3D` 中对 `RotationY` 取反（带注释），与 XAML 深度方向一致。
- 实测证据（离屏 `RenderTargetBitmap`，480×800）：`RotationY = −80`、`CenterOfRotationX = 0` 时铰链边高 800 px，自由边落在 x ≈ 133.7、高 ≈ 543 px，与解析值 `999/(999+472.7) = 0.6788` 完全一致；`RotationY = +50` 时自由边在 x ≈ 348 处放大 1.58 倍。屏幕 `CopyFromScreen` 实测静态 −60° 投影与解析梯形吻合（自由边 166 vs 解析 160）。
- identity 与 3D 的一致性实测：特征位置 100.00（2D）vs 100.01（3D）px —— 无位移、无缩放；剩余差异是 ClearType→灰度抗锯齿，以及（已移除的）位图缓存。

### 4.2 转场

- `Transitions/TransitionFrame.cs`：移除 `BitmapCache`，加入 `Prefer3D` 的准备/复位，并把非取消退场路径对齐原版（`CompleteTransition(..., null, ...)`）。
- 真实 turnstile 导航的运行期采样：退场 `0° → 45°` 用时 249 ms，随后入场 `−69° → 0°` 用时 265→592 ms —— 严格串行，与原版 storyboard 一致；动画期间 presenter 命中测试被禁用，结束后恢复。
- 所有转场族的数值/时长/缓动都与 `repo-other\...\Transitions\Storyboards\*.xaml` 比对（Roll、Rotate ×8、Slide ×8、Swivel ×6、Turnstile ×4）：完全一致。

### 4.3 效果

- `Effects/TiltEffect.cs`：只倾斜控件本身（不再整页）；在 `Loaded` 时完成包裹（倾斜关闭时解包），避免指针输入期间改动视觉树；在切换到 3D 之前先缓存 container→host 变换（3D 宿主无法做 2D 变换）；用 `Prefer3D` 取代 `BitmapCache`。旋转支点取**第一个可视子元素**（可见模板内容）的边界中心，按压点按该边界归一化，与原版工具箱的 `GetChild(control, 0)` 一致；`CenterOfRotation` 相对真正被投影的布局槽表达。回弹动画改为自行跟踪 begun 状态（`Storyboard.GetCurrentState()` 在没有包含时钟时会抛异常），并在清理时重置所有逐次状态，因此跨 UI 线程安全。由于 `ProjectionService` 会在赋值时按需包装，倾斜逻辑不再自己管理宿主：赋投影、让直接宿主保持 `Prefer3D`，清理时清除投影（由服务释放按需包装）。
- `Effects/TurnstileFeatherEffect.cs`：逐元素宿主现在真正生效（同一个祖先宿主 bug 的修复），并加入 `Prefer3D` 的准备/清理。
- `Projection/ProjectionHost.cs`：3D 场景改为投影一个内部“槽位尺寸”的 surface，而不是内容本身。`Viewport2DVisual3D` 会把可视对象的**绘制边界**映射到四边形上，绘制范围小于布局槽的内容会被拉伸——表现为 `CheckBox` 横向被拉宽（其模板只绘制左侧一条）以及羽化文字变形。surface 被固定为布局槽尺寸，并且平面几何与 `CenterOfRotation`/偏移都在 `UpdateScene` 中按布局槽（而不是 `Child.RenderSize`）计算，因此比宿主小的子元素会保持 2D 下的大小和位置。
- `Projection/ProjectionService.cs`：给元素赋 `Projection`（附加属性、`SetProjection` 或 `element.Projection` 扩展成员）时，投影会应用到**直接包含**该元素的宿主，必要时把元素按需包进 `ProjectionHost`（不再需要事先准备包装）。按需创建的包装会在投影被清除时释放，因此最终可视树不变；item 容器仍然跳过。`GetDirectProjectionHost` 暴露真正接收投影的宿主，storyboard 路径仍为 `(ProjectionService.Projection).(PlaneProjection.RotationY)`。

### 4.4 测试增改

- `ProjectionTests`：`ProjectionHost_KeepsTheProjectedPlaneOneToOneWithTheLayoutSlot`、`ProjectionHost_AppliesTheXamlPerspectiveDepth`、`PlaneProjection_RotationYMatchesTheXamlDepthDirection`（重命名），并更新两处符号断言；新增 `ProjectionService_WrapsAnElementThatReceivesAProjection`、`ProjectionService_AppliesAProjectionDeclaredBeforeTheElementLoads`、`ProjectionService_DoesNotProjectThePageHostOfAContainedElement`、`ProjectionHost_DoesNotStretchContentThatPaintsLessThanItsSlot`、`ProjectionHost_ProjectsTheLayoutSlotNotTheChildSize`。
- `EffectsTests`：`TiltEffect_DoesNotTiltThePageThatHostsTheElement`；`TiltEffect_WrapsAChildButtonAndAnimatesARealProjection` 现在还会检查 3D 渲染路径、回弹延迟以及恢复 2D；`TiltEffect_RotatesAboutTheVisibleContentNotTheLayoutSlot` 断言支点是模板根的边界中心（宽槽中左对齐的 `CheckBox`）。
- `NavigationTests`：`TransitionFrame_PlaysTurnstileTransition` 现在断言首帧与结束后的 `Prefer3D`/`IsRenderingIn3D`。

### 4.5 本轮：WP 对齐调优、羽化流畅度、Sample 重构

- **Turnstile 对齐 WP**（`Transitions/Transitions.cs`）：两个有据可查的常量 —— `TurnstileCenterOfRotationX = -0.05`（原工具包为 `0`，即正好在左边缘）、`TurnstileDurationScale = 1.2`（工具包：退场 0.25 s / 入场 0.35 s）。原因：用户对照 WP 8.1 模拟器，报告旋转轴略偏离左边缘、且动画比 WP 快。把它们设为 `0` / `1` 即可精确还原原工具包。storyboard 数值本身未改动。
- **羽化流畅度**（`Effects/TurnstileFeatherEffect.cs`）：被羽化元素在转场之前就包装进 `ProjectionHost`，避免动画期间重新挂载内容（重新挂载会让每个目标触发完整布局，造成可见卡顿）。**已在 §4.6 被取代**：包装现在延迟到 dispatcher，目标改为从可视树收集，因为原先在 `SizeChanged` 中的即时包装可能发生在布局过程中（见 §4.6 第 1 项）。
- **羽化退场不是羽化动画**：根因在示例而非库 —— 出站转场取自**被离开的页面**，而首页只声明了普通 turnstile 退场。重构后的示例在 `SampleNavigator` 中显式设置两侧，因此进入和离开羽化示例都使用 feather 族。
- **Sample 按 `PhoneToolkitSample8` 重构**：`MainPage` 列出示例，每个示例独立成页（`ControlsPage`、`ProjectionPage`、`TransitionsPage` + `TransitionTargetPage`、`FeatherPage`、`MenusPage`）；删除 `HomePage`/`DetailPage`；`App.xaml.cs` 从 `MainPage` 启动。`TransitionsPage` 可先选择转场族（Turnstile、Slide、Rotate、Swivel、Roll、feather）再导航，所有转场族都能直接做视觉对比。
- **实测帧率**（`CompositionTarget.Rendering`，480×800）：2D 基线约 150 fps、整页投影约 92–96 fps、6 个羽化元素约 64–68 fps、2D→3D 切换约 6 ms。

### 4.6 本轮：六个 WP 一致性问题（羽化回归、消息框 Swivel、转场族、Rating、PhoneTextBox、HubTile）

1. **羽化进场动画消失 + 进入后页面卡住（回归）** —— 用进程内探针（`TransitionFrame` + 按 XAML 顺序、在挂载父级之前设置 index 的页面）复现出两个根因：
   - 当页面**没有可羽化元素**时，羽化转场生成的是**空 storyboard**；空 storyboard 永不触发 `Completed`，于是 `TransitionFrame` 一直等待，新内容呈现器保持 `IsHitTestVisible = false`，页面看起来卡死（进场动画根本没开始）。修复：`Transitions/Transition.cs` 对没有动画的转场在下一个 dispatcher 周期完成（`CompleteWithoutAnimation`），保持异步语义。
   - `TurnstileFeatherEffect.ComposeStoryboard` 过去从附加属性订阅列表取目标，但进场页面在**接入可视树之前**就被合成，订阅回调尚未运行，即使修好第一个 bug 也仍然没有目标。修复：目标改为从页面的可视树收集（`CollectTargets`，按 index 稳定排序）；`TransitionFrame` 在合成前强制执行一次布局（`_newContentPresenter.UpdateLayout()`）；`Subscribe` 的包装改为 dispatcher 延迟（它由 `SizeChanged` 触发，在布局过程中重新挂载会引发布局循环）；新增 `Loaded` 处理；来自其它 dispatcher 的遗留目标直接丢弃而不是跨线程访问。
   - 修复后探针结果：六个元素依次 `−80° → 0°` 进场，动画结束恢复命中测试，退场为 `0 → −80°`。
   - 示例：当被离开的页面没有可羽化元素时，`SampleNavigator` 回退为 turnstile 退场（`HasFeatheredElements`），因此从首页进入羽化示例时退场是整页动画而不是静止。
2. **`CustomMessageBox` 的 Swivel 动画** —— 对话框内容承载在 `ProjectionHost` 中并复用真实转场管线：`ContentRendered` 时播放 `SwivelTransitionMode.BackwardIn`，`Dismiss` 时播放 `BackwardOut`（storyboard 完成后关闭窗口，另有 500 ms 安全计时器）。数值验证：弹出时 `RotationX −45° → 0°`，关闭时 `0° → 42°`（目标 60°），随后窗口关闭并触发 `Dismissed`。
3. **Transitions 示例的退场族** —— 新增库测试证明 Frame 会采用导航前刚设置的出站转场（`TransitionFrame_UsesAnOutTransitionSetJustBeforeNavigating`）。真正的缺口在**返回**导航：`Frame.GoBack()` 使用的是目标页 XAML 声明的（turnstile）入场，且自身没有退场。`SampleNavigator.GoBack(source, kind)` 现在对两侧都应用所选族（内部维护“从哪些页面导航过来”的栈），`TransitionsPage`/`TransitionTargetPage` 改用它。
4. **`Rating` 点击区域与显示不一致** —— 开启 `AllowHalfItemIncrement` 时控件把位置数翻倍后直接写入 `Value`，而 `Value` 的单位是“星”；裁剪遮罩和拖拽提示都按 `RatingItemCount` 缩放，于是点击 50% 处会填满整个控件。`PerformValueCalculation` 现在除以 2（已记录为对原工具包的偏离，原版存在同一缺陷），并用 1e-9 的 epsilon 让正好落在边界上的点击归属该星。测试：`Rating_HalfItemIncrementKeepsValueInItemUnits`。
5. **`PhoneTextBox` 光标/占位符与输入法** —— 占位符起点比光标靠左 7 px（2 px 边框内 `Margin="3,0,3,0"`，而 `PhoneTextBoxInnerMargin` 为 12）。现在占位符与光标起点完全一致（`Margin="10,0,10,0"`，实测占位符原点 12 px、文本边距 12 px）。占位符在编辑器获得键盘焦点时**以及**输入法合成期间（`TextCompositionManager.PreviewTextInputStart/Update`）隐藏，因此拼音输入时不再残留；`UpdateHintVisibility` 重新评估焦点与合成状态。
6. **`HubTile` 上滑卡顿且没有翻转** —— 模板按原版 WP8 模板重建：一块高度为瓦片两倍的表面，标题带、图片与背面用 `TranslateTransform` 平移（平移不会像原来的 `ScaleTransform` 那样每帧重新栅格化图片），翻转则由 `ProjectionHost` 上的 `PlaneProjection.RotationX` 动画驱动（`Expanded → Flipped` 为 `0 → 180`，返回为 `180 → 360`），在 0.185 s（表面正好侧立时）切换前景图片与背面面板。背面预先镜像（`ScaleTransform ScaleY=-1`），所以在 180° 时内容正立 —— 与原版 `RotationX=180` 的平面等价。`HubTile` 在翻转前把 host 切到 `Prefer3D`，结束后回到 2D。探针：`RotationX 0 → 180` 且背面可见、前景隐藏，翻回时 `180 → 0`，标题位移分别达到 `−H`、`−0.4566H`、`0`（Expanded/Semiexpanded/Collapsed）。

本轮新增测试：`NavigationTests.TransitionFrame_CompletesAFeatheredExitWithoutTargets`、`NavigationTests.TransitionFrame_AnimatesFeatheredEntryOnFirstLayout`、`NavigationTests.TransitionFrame_UsesAnOutTransitionSetJustBeforeNavigating`、`ControlTests.Rating_HalfItemIncrementKeepsValueInItemUnits`；`ControlTests.CustomMessageBox_ShowDismissRaisesLifecycleEvents` 现在会等待 Swivel 结束再断言 `Dismissed`。

### 4.7 本轮：回到原版 XAML storyboard 管线（以及它暴露的转场缺陷）

`Transitions.cs` 重新按原版工具箱的形态移植：原版 27 个 `Transitions/Storyboards/*.xaml` 作为 WPF 资源随包发布，并按原版的方式加载（`Application.GetResourceStream` + 文本缓存 + `XamlReader.Load`；WPF 需要 `XmlReader` 而不是字符串）。`Storyboard.SetTarget(storyboard, element)` 把动画绑定到整页，工厂负责设置投影（Slide 则是 `RenderTransform`），然后返回 `Transition`。storyboard 内部唯一的改动是属性路径：`(phone:ProjectionService.Projection).(phone:PlaneProjection.X)` 加 `xmlns:phone` —— WPF 能解析带前缀的路径（通过观察实际动画的 `PlaneProjection.RotationY` 验证）。

保留了两处仅移植版才有的内容，均已在源码中说明：

1. `ProjectionTransition` 在带投影的转场完成或停止时清除附加投影，使空闲页面回到纯 2D 路径（Silverlight 可以长期保留一个单位投影）。
2. Turnstile 的两个常量（`TurnstileCenterOfRotationX = -0.05`、`TurnstileDurationScale = 1.2`）；系数以 `Storyboard.SpeedRatio` 施加，原始关键时间不变。

在验证新管线时发现并修复了三个真实缺陷：

1. **完全没有任何转场。** `TransitionFrame.OnNavigating` 保留了原版的 `if (!e.IsNavigationInitiator) return;` 判断，但 WPF 对每次 `Frame.Navigate` 都报告 `IsNavigationInitiator == false`。于是 `_isForwardNavigation` 一直为 `false`，取到的是 `NavigationInTransition.Backward`（通常为 `null`），库和示例里的**所有**转场都被静默跳过 —— 这正是 `cd606b4` 里已经有 6 个导航测试失败的原因。修复：去掉该判断（WPF 没有 shell 发起的导航），并记录这处偏离。
2. **跨线程 `BitmapCache`。** `TransitionFrame` 保留了一个共享的 `static readonly BitmapCache` 并赋给呈现器。每个测试运行在自己的 STA 线程上，于是第二个测试从 `MediaSystem.AssertSameContext` 抛出 `ArgumentException`。修复：不再给呈现器加缓存 —— `Viewport2DVisual3D` 已经栅格化页面，架构说明本来也禁止再套一层缓存。
3. **过时的测试。** `TransitionFrame_FallsBackWhenTransitionFactoryThrows` 断言的计数器已不再递增（`TransitionFailed` 事件在上一轮被移除）；已删除该测试及其 `ThrowingTransitionElement` 辅助类型。

本轮新增的验证（全部自动化，`dotnet test -c Release`）：

- `TransitionStoryboardTests`（6 个）：每个族都通过公开的 `TransitionElement` 启动，并在运行时读取被动画的属性 —— Roll `RotationZ 0→45→90`、Rotate `90→0`、Swivel `RotationX −45→0`、Turnstile `RotationY −80→0`（并断言 `IsRenderingIn3D`）、Slide `TranslateTransform.X 200→0` 加透明度，以及 `ProjectionTransition` 在 `Stop()` 时清除投影。
- 通过真实 Frame 实测时长：turnstile 入场 **420 ms**、退场约 300 ms（0.35 s / 0.25 s × 1.2）。
- 渲染帧证据：`TransitionFrame` 转场中途的 `RenderTargetBitmap` 显示页面为透视梯形（绘制列：顶部 0–325、中间 0–399、底部 0–226），而稳定后是平整的 0–466。
- 示例冒烟：示例可启动，UI 自动化树列出所有示例入口，打开 Transitions 示例能正常导航且无错误日志。

本轮同步更新的文档（双语）：`architecture.md` §5.3/§5.4、`known-limitations.md` §5/§9、`api-compatibility.md`、`migration-guide.md`、`porting-inventory.md`、`porting-status.md` §3（所有转场族升级为 ✅ 并附新证据），以及两份 README（不再宣传 `TransitionFailed`）。

### 4.8 本轮：`Transition.GetCurrentState()` 抛 `InvalidOperationException`

转场源码被替换为原版工具包文件后（`Transition` 移到 `Transitions/Transitions/Transition.cs`），真实应用报错：

```
System.InvalidOperationException: 无法执行操作，因为未将指定的 Storyboard 应用到此交互控件的对象。
  at System.Windows.Media.Animation.Storyboard.GetCurrentState()
  at Microsoft.Phone.Controls.Transition.GetCurrentState()
  at Microsoft.Phone.Controls.TransitionFrame.EnsureStoppedTransition(...)
  at Microsoft.Phone.Controls.TransitionFrame.TransitionNewContent(...)
```

根因：Silverlight 的无参 `Storyboard` 控制方法在“时钟还不存在”时是安全的，WPF 会抛异常。`TransitionFrame` 在 `Begin()` **之前** 调用 `EnsureStoppedTransition(transition)` → `transition.GetCurrentState()`，因此每次导航的第一个转场都会崩溃。

`Transition` 现在保持原版 Silverlight 形态，只补上必要的 WPF 适配：

1. 跟踪 `_isBegun`：storyboard 未启动时 `GetCurrentState()`/`GetCurrentTime()` 返回 `Stopped`/`Zero`；没有时钟时 `Pause`/`Resume`/`Seek`/`SeekAlignedToLastTick`/`SkipToFill` 是空操作；对从未启动的转场调用 `Stop()` 只恢复元素状态。
2. `Begin()` 以**可控时钟**方式在元素上启动 storyboard（`Storyboard.Begin(containingObject, true)`），因为 WPF 的交互控制需要宿主对象；所有控制调用都传入该宿主。
3. `_isCompleted` 保证每次运行最多触发一次 `Completed`。可控时钟被停止/移除时 WPF 会再次触发 `Timeline.Completed`，而 Frame 正是在该事件里完成导航 —— 没有这个保护时 `NavigationTransition.EndTransition` 会触发两次（`TransitionFrame_PlaysExitAndEntryTransitionsAndRestoresInteractivity` 断言 1，实际 2）。`Stop()` 也会置位该标志，确保 Frame 已完成后再无待完成事件。
4. `Stop()` 现在还会 `Storyboard.Remove` 时钟，使被动画的属性回到基础值。
5. `Save()` 改为每个转场创建自己的 `BitmapCache`，不再使用共享的 `TransitionFrame.BitmapCacheMode` 静态实例：`Freezable` 属于创建它的 dispatcher，共享实例在第二个测试线程上会抛 `ArgumentException`（`MediaSystem.AssertSameContext`，以及 `Restore()` 中的 `Freezable.RemoveContextInformation`）。已从 `TransitionFrame` 删除这个不再使用的静态字段。

验证：

- `TransitionStoryboardTests` 新增三个回归测试：`Transition_ReportsStateAndTimeBeforeItBegins`（在 `Begin()` 之前探测状态/时间与全部控制方法 —— 即本次崩溃场景）、`Transition_RaisesCompletedOncePerRun`（多次 `Stop()` 后仍只有一次 `Completed`），以及 `TransitionFrame_NavigatesWithEveryFamily`（对 Turnstile/Slide/Rotate/Swivel/Roll 的 `[Theory]`，通过 Frame 跑退场+入场，断言两侧各一次 `BeginTransition`/`EndTransition`、页面可交互、投影已清除）。
- 真实 Frame 的进程内探针（turnstile 退场 + 入场）：`outBegun=1 outEnded=1 inBegun=1 inEnded=1`，两侧投影都被清除、命中测试恢复，无异常。
- 独立转场探针：`Stopped`→`Active`，时间正常推进，`Pause` 冻结（因 tick 粒度约 7 ms 溢出），`Resume` 继续，`Seek(50 ms)` 延迟到下一个 tick，`SeekAlignedToLastTick(10 ms)` 精确落在 10 ms，`SkipToFill` 只完成一次，`Stop` 清除投影。
- `dotnet test -c Release`：**89 通过 / 1 失败 / 0 跳过**（90 个）；失败的是既有问题 `CustomMessageBox_ShowDismissRaisesLifecycleEvents`（测试宿主里 `Application.Current` 为 null）。有一次运行中两个 `EffectsTests` 倾斜测试也失败 —— 它们在窗口于倾斜中途关闭时把 `TiltEffect` 的静态状态泄漏到其它 STA 线程；重跑即通过。

### 4.9 本轮：移植原版 WP8 `ListPicker`

早期简化版 `ListPicker`（WPF `Popup` + 完整模式模态 `Window`）已替换为**原版 WP8 源码**（`Controls/ListPicker/{ListPicker,ListPickerItem,ListPickerMode,ExpansionMode}.cs`、`ListPickerPage.xaml(.cs)`）与原版样式 `Themes/Controls/ListPicker.xaml`。`Themes/Generic.xaml` 不再自带 ListPicker 样式，改为合并 `Controls/ListPicker.xaml`。

WPF 适配（每条都是 WPF 的要求，不改变行为）：

1. 用 `ProjectionService`/`PlaneProjection` 替代 `UIElement.Projection`：页面的项目/标题入场动画直接给 `PlaneProjection` 对象做动画，元素本身仍由投影宿主承载。
2. `Tap`/`GestureEventArgs` → `OnMouseLeftButtonUp` + 鼠标进入/离开高亮；item 容器的处理器设置 `e.Handled`，因此普通模式下控件自身的处理器仍会展开选择器。
3. `Application.Current.RootVisual` → `Window.GetWindow(this)`（带视觉树回退），应用与测试里都能找到 frame。
4. `OnBackKeyPress` 重写 → 移植层的 `BackKeyPress` 事件（`e.Cancel = true`）；页面「完成/取消」走关闭动画后 `NavigationService.GoBack()`。
5. 不使用 `Extensions.RegisterNotification`：它每次调用都注册一个附加依赖属性，第二个 `ListPicker` 实例就会抛异常；改用 `IsEnabledChanged` 覆盖同样的通知。
6. `SelectionChangedEventArgs` 需要 `RoutedEvent`（`Selector.SelectionChangedEvent`），`ManipulationContainer` 需转成 `UIElement` 才能 `TransformToVisual`；新增 `Common/ItemsControlExtensions.GetItemsInViewPort` 供页面使用。
7. 主题文件：局部合并 `Dark.xaml`（拆分主题字典里的 `StaticResource` 彼此不可见），页面模板根属性使用 `DynamicResource`（同字典内前向引用）。
8. `PhoneTextBoxEditBackgroundColor` 是 `Color`，WPF 不接受用它给 `Border.Background` 做关键帧动画，因此高亮状态改用 `PhoneTextBoxEditBackgroundBrush`。
9. 完整模式创建 `ListPickerPage` 实例并交给 `Frame.Navigate(object)`（在 frame 的延迟/转场导航下更稳）；`PickerPageUri` 仍指向该页的 pack URI。打开期间宿主页的转场属性被保存并清空，与原版「弹出导航不做转场」一致。

两个只有在跑示例时才暴露的运行时缺陷，均已补回归测试：

- **首个渲染 tick 抛 `AnimationException`**：`SizeForAppropriateView` 在选中项尚未测量时就启动 canvas `Height` 动画，`DoubleAnimation` 没有起点、canvas 高度是 `Auto`（`NaN`）。Silverlight 容忍这种情况，WPF 直接抛异常。现在只有在高度动画的 from/to 都是真实数值时才启动故事板（`ListPicker_PageWithPickersLoadsWithoutAnimationErrors` 在修复前以该异常失败）。
- **鼠标悬停时抛 `AnimationException`**：高亮状态用 `Color` 资源给 `Background` 做动画（`ListPicker_HighlightedVisualStateUsesBrushResources` 在修复前失败）。

测试：`ListPicker_SelectionRoundTrips`、`…_ExpandedModeSelectsItemAndCollapses`、`…_OpenWithoutItemsExpands`、`…_FullModeWithoutFrameStaysUsable`、`…_FullModeNavigatesToPickerPageAndCommitsSelection`（现在还会断言页面在用户操作前保持打开）、`…_PageWithPickersLoadsWithoutAnimationErrors`、`…_HighlightedVisualStateUsesBrushResources`。

人工证据（示例 + UI 自动化）：Controls 页两个 picker 加载不再抛异常；点击 picker 展开、再点项目提交选择（宿主页绑定文本随之变化）；`RecurringDaysPicker` 打开完整模式页（`REPEATS` 标题、七行星期复选框、「完成/取消」命令栏 —— 自 §4.11 起为原版 `ApplicationBar`），点「完成」返回 Controls 页且无错误日志。

本轮同步更新的文档（双语）：`api-compatibility.md`（控件行）、`known-limitations.md` §6.3、`architecture.md` §5、`migration-guide.md` §6、`porting-status.md`（ListPicker 测试名 + 证据快照）。

### 4.10 本轮：移植原版 `Microsoft.Phone.Shell.ApplicationBar`

之前只有 `DependencyObject` 形式的 API 模型、没有呈现器。现在按反编译源码（`D:\repos\repo-other\WindowsPhone\Microsoft.Phone\Microsoft.Phone.Shell`）移植原版形状并适配 WPF，且真正绘制出来：

- **公共面**（`Shell/`）：`IApplicationBar`、`IApplicationBarIconButton`、`IApplicationBarMenuItem`、`ApplicationBar`（`[ContentProperty("Buttons")]`、`IsVisible` 默认 `true`、`Opacity` 截断到 0..1、`IsMenuEnabled`、`BackgroundColor`、`ForegroundColor`、`Mode`、`DefaultSize` 72、`MiniSize` 30、`Buttons`、`MenuItems`、`StateChanged`）、`ApplicationBarIconButton`（`IconUri`、`Text`、`IsEnabled`、`Click`）、`ApplicationBarMenuItem`（`Text`、`IsEnabled`、`Click`）、`ApplicationBarMode`、`ApplicationBarStateChangedEventArgs`。删除了移植自造的 `ApplicationBarItem` 基类与 `Command`/`CommandParameter`/`RaiseClick`：原版没有这些，命令通过 `Click` 接线。
- **基础设施**：`ApplicationBarItemList<T>`（原版 `IList` 实现，4/50 上限与原版异常消息）、`ApplicationBarItemContainer` + 图标/菜单容器 + 工厂（校验文本：非空、单行、去空白、≤255 字符；挂到应用栏后必须有图标；在 UI 线程派发点击），以及原版“预留空间”规则（可见、不透明度为 1、背景 alpha 为 255 或 0）。
- **WPF chrome**：新增 `ApplicationBarPresenter` 取代原生合成器（加法 API，已写入文档）。它绘制 72 DIP（最小化 30）的应用栏：96×72 图标槽（48×48 图标在上、12 px 标签在下）、右上角三点菜单按钮，以及行高 48 px 的 `Popup` 菜单；颜色/透明度跟随应用栏，禁用项变暗，支持键盘激活。图标 URI 按应用资源解析，缺失图标时按钮不显示图片。
- **Frame 接线**：两个 frame 模板都增加 `ApplicationBarHost`（按 `ReservedHeight` 预留的行）与 `ApplicationBar`（呈现器）。`PhoneApplicationFrame.OnApplyTemplate` 取得部件，`Navigated` 把新页面的应用栏推给呈现器；`PhoneApplicationPage.ApplicationBar`（改为 `IApplicationBar`，与原版一样拒绝其它实现）在页面活动期间转发变更。半透明或隐藏的应用栏浮在页面之上而不是压缩页面。
- **示例**：`MenusPage` 在 XAML 中声明应用栏（3 个图标按钮 + 3 个菜单项），`Images/appbar.*.rest.png` 是随资源打包的生成图标。

验证：`ShellTests`（10 个测试）覆盖点击、禁用项、上限与校验、预留高度规则、菜单打开/关闭与 `StateChanged`、frame 承载（host 与呈现器 72 DIP、页面布局在应用栏之上）、无应用栏以及拒绝外部实现；整包已全绿（见 §4.11）。示例像素证据：应用栏位于 frame 底部、高 72 DIP、颜色 `#1F1F1F`，三个槽位内有白色图标与标签，右上角有三个点；打开菜单并点击 `minimize` 后应用栏收起到 30 DIP 最小化模式，页面状态文本同步更新。

本轮同步更新的文档（双语）：`api-compatibility.md`（shell 映射 + XAML 行）、`known-limitations.md` §3.2、`architecture.md` §5.1/§5.2.1、`migration-guide.md` §6、`porting-status.md`（ApplicationBar 行 + 证据快照）。

### 4.11 本轮：`ListPickerPage` 回到原版 `ApplicationBar`，并修好测试回路

- **`ListPickerPage` 重新使用移植版 `ApplicationBar`。** 临时的 `CommandArea` `StackPanel`（手写的完成/取消条）已删除。`ListPickerPage.xaml` 现在声明 `<phone:PhoneApplicationPage.ApplicationBar><shell:ApplicationBar IsVisible="False">`，其中两个 `ApplicationBarIconButton` 指向 `Content/ApplicationBar.Check.png` 与 `Content/ApplicationBar.Cancel.png`（库资源，pack URI 为 `/Community.Phone.Controls.Toolkit;component/Content/...`）；代码后台就是 WP8 原版逻辑：`OnLoaded` 遍历 `ApplicationBar.Buttons`，用不区分大小写的 `DONE`/`CANCEL` 匹配，套上本地化的 `appbar_done`/`appbar_cancel` 文本并挂 `Click` 到 `Done_Click`/`Cancel_Click`；`OnNavigatedTo` 设置 `ApplicationBar.IsVisible = SelectionMode != SelectionMode.Single`。因此单选模式下 frame 完全不预留应用栏高度。
- **`CustomMessageBox.Show()` 不再需要 `Application`。** 主题背景色改为资源查找并回退到 `Colors.Black`，容忍 `MainWindow` 为 `null`，`OnWindowContentRendered` 对空 owner 做了保护，所以在普通窗口（或测试宿主）里也能显示。测试还必须在 `ShowDialog()` 的嵌套 dispatcher 循环**内部**关闭对话框，因此改用 `DispatcherTimer` 安排关闭。
- **测试回路的提速与确定性。** 新增 `WpfTestHelper.PumpUntil(predicate, timeout)` / `PumpFor`，并把占用时间最多的固定等待改为条件轮询：五个 `TransitionFrame_NavigatesWithEveryFamily`（5 × 3000 ms）、2000 ms 的倾斜测试以及八个 `NavigationTests` 等待。测试耗时从约 45 秒降到约 30 秒。`Rating` 测试不再移动物理光标（`Rating.UpdateValueFromMouse` 改为 `internal`；`Rating_PointerPositionUpdatesValue` 直接调用），因此没有任何测试依赖桌面是否允许指针落点。
- **并行化是有意保持关闭的。** 删掉 `[assembly: CollectionBehavior(DisableTestParallelization = true)]` 后整轮会挂起（>600 s），因为多组测试驱动共享的 WPF 单例（主题字典、`Popup`、storyboard XAML 缓存）；属性保留并附注释。真正有竞态的两个缓存（`Transitions._storyboardXamlCache`、`ApplicationBarIconLoader.Cache`）也顺手加了锁。

验证：两个新测试 —— `ListPicker_FullModePageUsesTheApplicationBarForSingleSelection`（应用栏隐藏、两个按钮、呈现器折叠、host 行高 0）与 `ListPicker_FullModePageCommitsFromTheApplicationBarDoneButton`（应用栏可见、呈现器 72 DIP、本地化文本、两个图标能从 pack URI 解析、点「完成」提交选择并返回宿主页面）。整包 **103/103**，Release 与 Debug 均通过。示例证据（UI 自动化 + 像素）：`RecurringDaysPicker` 页面显示 `REPEATS`、七行星期，frame 底部的应用栏为 `#1F1F1F`，`完成`/`取消` 两个标签（该次运行屏幕像素 y≈1258）上方各有白色图标（中心 x≈683 与 x≈827）；点击「完成」槽位返回 Controls 页。

本轮同步更新的文档（双语）：`known-limitations.md`（ListPicker 页面 chrome 现在使用真实 `ApplicationBar`）、`api-compatibility.md`（ListPicker 行）、`architecture.md` §9（条件等待）、`porting-status.md`（证据快照 + ListPicker/CustomMessageBox 行）。

### 4.12 本轮：干净构建失败，以及它掩盖的 `ListPickerPage` 回归

已提交的代码在"从零构建"时无法编译，编译通过后又有三个 `ListPickerPage` 测试失败。两者都已修复，最终**没有改动任何测试**。

- **干净构建报 `MC3074`**（"`clr-namespace:Microsoft.Phone.Controls;assembly=Community.Phone.Controls.Toolkit` 中不存在标记 `PhoneApplicationPage`"）。`MarkupCompilePass1` 只能通过 `@(Compile)` 认识本项目的类型；当库的页面用**程序集名**引用库自身时，在干净的 `obj` 上无法解析（增量构建残留的生成文件会掩盖这一点，所以只有在重新构建时才暴露）。修复：`ListPickerPage.xaml` 的 `xmlns:phone`/`xmlns:shell`/`xmlns:toolkit` **不带** `assembly=`，与库内主题 XAML 的写法一致。已删除库的 `obj`/`bin` 后验证。
- **picker 页面必须自带主题。** `ListPickerPage` 由 `ListPicker` 自己创建，因此页面在解析时尚未挂到任何可视化树上，之后又被放进 frame 的 `ProjectionHost`，那里继承资源不可达（与 `Viewport2DVisual3D` 阻断 `DataContext` 继承是同一个 WPF 限制）。页面侧的处理：`Resources` 用显式 `ResourceDictionary` 包住样式并合并 `Themes/Generic.xaml`（页面内所有样式/模板的 `StaticResource` 不再依赖应用级主题）；根元素与标题上在"尚未挂树"时求值的属性（`FontFamily`、`FontSize`、`Foreground`、`MainGrid.Background`）改用 `DynamicResource`；作为 `ObjectAnimationUsingKeyFrames` 值的 `TransparentBrush` 改为内联 `SolidColorBrush`（关键帧值不能用 `DynamicResource`，而 `StaticResource` 在模板所属字典里解析不到）。`Themes/Controls/ListPicker.xaml` 中同样的写法也一并改为内联画刷——当这份字典作为 WPF 的 *theme* 字典（而不是应用字典）加载时会出现完全相同的问题。
- **`AnimationForElement` 的目标设错**：打开/关闭动画把 `Storyboard` 的目标设成 `PlaneProjection`，而属性路径是从元素的 `ProjectionService.Projection` 附加属性开始的，于是 `Storyboard.Begin()` 抛 `InvalidOperationException`（"Projection 属性未指向 DependencyObject"）。现在目标改为元素本身，与原版 WP 代码一致（`(UIElement.Projection).(PlaneProjection.RotationX)`）。
- **`Extensions.IsExternalNavigation` 会解引用空的 `Uri`。** 移植版用页面实例导航，`NavigationEventArgs.Uri` 为 `null`，`ListPickerPage.OnNavigatedFrom` 在返回时抛 `NullReferenceException`。该扩展现在对 `null` 安全。

验证：删除库的 `obj`/`bin` 后 Release 构建 **0 错误、22 警告**（全部为既有警告，来自 `LongListSelector`/`ViewportControl`/`WindowHelper`）；`dotnet test` **Release 与 Debug 均 103/103**（约 28 秒）；`build\verify-package.ps1` 通过。本轮的示例屏幕验证无法进行：交互式桌面不可用（`CopyFromScreen` 报 "句柄无效"，UI 自动化点击也落不到窗口上）；该路径由三个 `ListPickerPage` 测试覆盖（驱动真实的 `TransitionFrame`）。

已放弃的方案（值得记住）：给测试进程创建 `Application`（即使放在单个长期存活的 STA 线程上），会让 `Application.Current.MainWindow` 指向上一个测试已经关闭的窗口，从而改变 `ListPicker` 查找 frame 的行为，导致四个测试失败；因此测试仍保持"每个测试一个 STA 线程、没有 `Application`"。

本轮同步更新的文档（双语）：本节、`porting-status.md`（证据快照、ListPicker 行）、`known-limitations.md`（页面自带主题、加载期属性使用 `DynamicResource`）、`architecture.md`（主题字典规则）。

### 4.13 本轮：`ListPicker` 完整模式保真度与普通模式的选中显示

来自示例反馈的五个问题：

1. **单选时完整模式关闭后仍显示第一项。** 普通模式显示依赖 Canvas 高度和 `ItemsPresenter` 的 `TranslateTransform.Y`，两者原本共用一个 `Storyboard`。高度动画的目标是 Canvas（`FrameworkElement`），能生效；位移动画的目标是模板里的 `TranslateTransform`（`Freezable`），时钟虽然创建了（`ClockState.Filling`），值却从未写进属性，`Y` 一直停在 `-4`（第一项的偏移）。`SelectedItem` 本身提交正确。修复：`SizeForAppropriateView` 改为直接在 Canvas 高度和 transform 上调用 `BeginAnimation`；非动画路径先清除动画再同步写入最终值（`Top`/`Y`）。离屏探针验证：选择第 4 项后 `TranslateTransform.Y = -160.5`，不再是 `-4`。
2. **多选复选框显示成桌面复选框**（白色填充、灰色边框）。`ListPickerPage` 用 `AnyFontSizeCheckBoxStyle` 替换主题样式，因此样式未设置的属性会回落到 WPF 主题样式，而不是手机的 `PhoneRadioCheckBox*` 画刷。修复：样式显式设置 `Background`（`PhoneRadioCheckBoxBrush`，透明）、`BorderBrush`（`PhoneRadioCheckBoxBorderBrush`，白色）、`Foreground` 与 `Padding`。探针实测：`#00FFFFFF` / `#FFFFFFFF`（修复前为 `#FFFFFFFF` / `#FF707070`）。
3. **完整模式的 item 没有依次翻转展开。** 两个原因。其一，`ListPicker` 的模板在首次布局前就应用，`OnLoaded` 里 `Picker.GetItemsInViewPort()` 返回 0 个 item，根本没有创建投影（Silverlight 会更早生成容器）。现在 `OnLoaded` 与 `UpdateVisualState` 都会先 `Picker.UpdateLayout()` 再执行 `SetupListItems`。其二，item 容器不能包进 `ProjectionHost`（`ProjectionService.EnsureProjectionHost` 为保护容器生成而拒绝 `ListBoxItem`/`ContentPresenter`），所以即使拿到 item，投影也没有承载对象。现在单选/多选的 item 模板内各有一个 `ProjectionHost`（内容放在其中），动画目标改为该 host（`(ProjectionHost.Projection).(PlaneProjection.RotationX)`）。探针时序：`-90 → 0`，保留原版 30 ms/350 ms 的逐个延迟。
4. **Accent 颜色完整模式页面出现桌面选中底色、行距过大。** 单选时页面没有设置 `ItemContainerStyle`，使用了系统 `ListBoxItem` 模板（蓝色选中底色、`MinHeight`），行高 118 DIP 而非 98。修复：为单选设置页面内的 `ListBoxItemStyle`（无底色、选中文字使用 accent 色、`FontSize=43`、Light 字体、无内边距、含 `ProjectionHost`）。页面的 `ListBox` 也使用页面内的 `ListBoxStyle`；否则系统 `ListBox` 模板的根不是 `GetItemsInViewPort` 期望的 `ScrollViewer`，这正是一次布局修复后仍然找不到 item 的原因。
5. **应用栏按钮样式。** 原来是在标签上方直接画 48 DIP 的图标；WP 会在图标外套一个圆形 chrome。`ApplicationBarIconButtonView` 现在绘制 48 DIP 圆环（2 DIP `ForegroundColor` 描边、半径 24），内部 32 DIP 图标，标签位于下方（72 DIP 槽位中圆环 4..52、标签 53..69）。

页面由控件在挂到 frame 之前解析，看不到 `Application.Resources`。`ListPickerPage.Resources` 现在用 `ResourceDictionary` 包住样式并合并 `Themes/Theme/Dark.xaml`，模板关键帧改用 `StaticResource`（这种场景下模板 storyboard 里的 `DynamicResource` 不生效；样式 setter 仍用 `DynamicResource`）。这也让页面在没有 `Application` 的测试里可验证。

测试：`ListPicker_FullModePageAnimatesEveryItemInView`、`…_FullModeSingleSelectionUsesThePhoneItemStyle`、`…_MultipleSelectionCheckBoxUsesThePhoneStyle`、`…_ReturnsFromFullModeShowsTheSelectedItem`、`ApplicationBarIconButtonDrawsThePhoneCircle`（同一变更加入；前四个在旧版本上失败，应用栏测试因没有圆形边框而失败）。Release **108/108**（约 29 秒）、Debug **108/108**（约 35 秒），库构建 0 错误，示例冒烟启动无错误文件。离屏探针（480×800 `RenderTargetBitmap`）：单选行高 98 DIP（无选中底色）、上述复选框颜色、依次旋转、应用栏圆环位于 `(297, 4)` 48×48、图标位于 `(305, 12)` 32×32。

本轮同步更新的文档（双语）：本节、`known-limitations.md` §3.2/§6.3/§7、`architecture.md` §5.2.1 与 ListPicker 设计决策、`porting-status.md`（证据快照、ListPicker 与 ApplicationBar 行）。

### 4.14 本轮：长列表 `ListPicker`（`GetItemsInViewPort` 抛 `NullReferenceException`）

items 很多的 `ListPicker`（示例里 105 个系统区域）在完整模式页面加载时崩溃：`ItemsControlExtensions.GetItemsInViewPort` 第 103 行 `NullReferenceException`。

- **空容器。** `ListBox` 默认虚拟化，缓存窗口之外的项目 `ItemContainerGenerator.ContainerFromIndex` 返回 `null`。第一个循环已做判空，第二个循环直接使用了容器。用 120 项的页面复现（同样的堆栈）并修复：遇到空容器跳过（改为逐个检查已实现容器是否与视口相交，而不是"走到第一个不可见项就停"），`TransformToVisual` 抛 `ArgumentException`/`InvalidOperationException` 时结束收集而不是向外抛。
- **滚动到选中项的请求被取消。** `Loaded` 里调用的 `ScrollIntoView` 会排队等到容器生成后才执行，而 `UpdateVisualState` 会重新设置当时的 `ScrollViewer.VerticalOffset`——此刻仍是 `0`，于是滚动请求被丢弃：页面停在顶部，而（尚未实现的）选中项远在下方。120 项时这也意味着翻转动画只覆盖前几行而不是可见行。探针实测：修复后页面会滚动到第 60 项（96 DIP 行高时 `VerticalOffset` 为 55，98 DIP 时为 49），动画集合与可见容器一致。
- **等待列表就绪。** 打开动画现在由 `BeginOpenAnimationWhenListIsReady` 准备：订阅 `LayoutUpdated`，在 dispatcher 轮次里重复 `ScrollIntoView`（第一次请求可能丢失），直到选中项的容器存在且与视口相交才开始（最多重试 50 轮；无选中项时回退到"任一可见容器"）。每次重新收集前会把上一次设置的 item 投影重置为 0，避免残留的 `-90` 让该行滚回视野时不可见。

测试：`ListPicker_FullModePageWithManyItemsScrollsToTheSelectionAndAnimatesIt`（120 项、选中第 60 项：不抛异常、把选中项滚入视口、对它做动画、且没有已实现容器停留在侧立状态）；`ListPicker_FullModePageAnimatesEveryItemInView` 改为等待（被推迟的）动画并采样最小观测角度，而不是只读一帧。完整套件 Release **109/109**（连跑两次，约 35 秒）；示例可构建并启动，无错误文件。

本轮同步更新的文档（双语）：本节、`known-limitations.md` §6.3、`architecture.md`（ListPicker 决策）、`porting-status.md`（证据快照与 ListPicker 行）。

### 4.15 本轮：`TurnstileFeatherEffect` 移植到本项目的投影模型

`Effects/TurnstileFeatherEffect.cs` 之前几乎是原版 WP 源码的逐字拷贝，在 WPF 上跑不起来：它走 `Application.Current.RootVisual`、动画目标写成 `(UIElement.Projection)` 这类 Silverlight 属性路径，且只通过附加属性订阅来发现目标。5 个羽化测试里有 4 个失败（先是 `ApplicationExtensions.get_RootVisual` 的 `NullReferenceException`，之后是跨线程 `Dispatcher.VerifyAccess`）。

- **目标收集** —— `GetTargetsToAnimate(FrameworkElement pageElement)` 改为在合成 storyboard 时按 `FeatheringIndex` 从页面可视树收集（保留原版对 ListBox/Pivot 的展开逻辑），不再依赖订阅顺序；`FeatheredTransition.Begin` 把被转场的页面传进来，收集结果做稳定排序，索引相同时保持树内顺序。
- **宿主** —— `TryAttachProjectionAndTransform` 用 `ProjectionService.EnsureProjectionHost` 包装元素，在改投影**之前**把 `host.Prefer3D` 置为 `true`，`GlobalOffsetY` 加到元素投影上，补偿用的位移则加到**包装器**上（包装器的表面按元素布局槽裁剪，位移加在元素上会把内容裁掉）。
- **线程** —— 只要 `Dispatcher.CurrentDispatcher` 变化就丢弃静态状态；一旦有待恢复的动画，延迟执行的 `EnsureProjectionHost` 直接跳过；静态缓动函数全部 `Freeze()`（线程亲和的 `Freezable` 不能带去下一个测试的 STA 线程）。`Element.Unloaded` 在元素仍位于 `ProjectionHost` 内时提前返回，避免"包装导致的卸载"又把包装拆掉。
- **重绘** —— `Prefer3D` 改回 `false` 后包装区域的屏幕上仍是**最后一帧 3D 画面**（示例中转场结束后页面标题一片空白）。`RestoreProjectionsAndTransforms` 在恢复投影与变换之后，对元素和包装器都调用 `InvalidateVisual()`。
- **属性路径** —— `RotationYPropertyPath` 改为由 `PlaneProjection.RotationYProperty` 经 `ProjectionService.GetProjectionPropertyPath` 生成，不再使用 Silverlight 字符串 `"(UIElement.Projection).(PlaneProjection.RotationY)"`。

证据：羽化测试 5/5 通过；完整套件 **128/130**，仅剩两个与本次改动无关、改动前就存在的 `ListPicker` 失败（`ControlTests.ListPicker_FullModeSingleSelectionUsesThePhoneItemStyle`、`…_HighlightedVisualStateUsesBrushResources`，在 HEAD 上同样失败）；Release 构建 0 错误 / 22 个既有警告；示例中 `FeatherPage` 能按序羽化，且转场结束后画面正常（重绘修复前后各有一张截屏对比）。

本轮同步更新的文档（双语）：本节、`porting-status.md`（两个羽化条目改为 ✅）、`known-limitations.md` §5.5（包装器生命周期与重绘失效）。

---

## 5. 未完成 / 未验证

明确**不**声称的内容：

1. **与 WP 8.1 模拟器的逐帧视觉对比** —— 模拟器在运行、通过 Virtual Machine Connection 查看，但本次会话无法“看图”；验证是数值化的（几何、时序、渲染帧几何）+ 与原 XAML/WinUI 数学比对。Turnstile 的调优（枢轴 `-0.05`、时长 ×1.2）来自用户的口头观察而非测量，因此仍**需要一次视觉确认**。现在每个族都有运行时属性测试（`TransitionStoryboardTests`），但只有 Turnstile 有渲染帧证据。
2. **各族缓动/像素一致性** —— storyboard 就是原版 XAML，数值天然精确；但 WPF 与 Silverlight 是否在每一帧都以完全相同的方式求值同一个 `ExponentialEase`/`SineEase`，没有测量。
3. **羽化透视原点** —— 原版 `TurnstileFeatherEffect` 会把每个元素的透视原点移到页面垂直中心（`GlobalOffsetY` + 补偿 RenderTransform）。本移植版因 `ProjectionHost` 与元素同尺寸且裁剪 3D 内容，让元素绕自身中心旋转；角度/延迟/时序与原版一致。已写入已知限制。
4. **Shell chrome** —— `ApplicationBar` 现在由 frame（`ApplicationBarPresenter`）绘制，但 `SystemTray` 仍是纯 API 模型，没有绘制 WP 状态栏。应用栏的圆形图标 chrome（48 DIP 圆环、2 DIP 描边、32 DIP 图标、12 px 标签）已实现并有测试覆盖，ListPicker 页面的应用栏也已与 WP 8.1 截图对比；应用栏不随页面转场动画进入，菜单是 WPF `Popup` 而不是原生表面，其余 WP8 度量（菜单滑入、省略号位置）仍是近似而非逐像素比对。
5. **未实现模块** —— `AutoCompleteBox`、`LongListMultiSelector`、`MultiselectList`、地图扩展、`TransferControl`、`SpeechTextBox`、`LockablePivot`、`SlideInEffect`、`GestureListener`/`GestureService`、`LoopingSelector`/picker 页面。已通过源码检索确认不存在（无对应 `class`）。
6. **示例覆盖缺口** —— 重构后的示例展示投影、全部转场族、羽化、所有已实现控件（包括 `LongListSelector` 的分组、跳转列表与网格模式）、ContextMenu、CustomMessageBox 以及 shell 状态（ApplicationBar/SystemTray API）；**未**展示 `Matrix3DProjection`，也没有真实 shell chrome（本来就没有绘制）。
7. **未做像素级验证的视觉细节** —— ListPicker 展开模式样式、ExpanderView 高度动画、PhoneTextBox 水印动画、HubTile 像素保真度：行为有测试（含 HubTile 翻转旋转量与 PhoneTextBox 占位符原点），像素保真度没有。ListPicker 完整模式页面（item chrome、间距、复选框颜色、依次翻转）及其应用栏已在 §4.13 与 WP 8.1 截图对比。

---

## 6. 阻塞与风险

1. **本轮改动未提交。** HEAD 是 `4c62a43`；原版 WP8 `ListPicker` 移植（`Controls/ListPicker/`、`Themes/Controls/ListPicker.xaml`、`Common/ItemsControlExtensions.cs`）、`ApplicationBar` 移植（`Shell/ApplicationBar*`）、frame/页面/模板接线、测试与示例改动以及全部文档更新只存在于工作区。任何破坏性 git 操作都会丢失它们。见 §10。
2. **视觉验证缺口。** 缺少目视手段时，保真度结论依赖几何/时序测量与原 XAML。后续最好能并排查看示例与模拟器。
3. **`docs/*/porting-inventory.md` 是 Phase 0 的大型审计文档**（英文版约 129 KB）。其中的模块状态是历史快照；当前状态以 `docs/porting-status.md` 为准。
4. **环境坑** —— `XDE.exe` 会卡死（用 `vmconnect.exe`）；`PrintWindow` 对 3D/VM 表面返回全黑（改用 `Topmost` 窗口 + `CopyFromScreen`）；显示为 150% DPI（注意 DIP↔像素换算）；示例运行时锁定 `bin` 输出，重建前先结束进程。
5. **羽化保真风险** —— 若严格要求一致，需要把羽化宿主改成“页面尺寸的 3D 场景”，属于设计级改动而非补丁。

---

## 7. 重要文件

| 路径 | 作用 |
|---|---|
| `src/Community.Phone.Controls.Toolkit/Projection/ProjectionHost.cs` | 2D↔3D 宿主、相机（`PerspectiveDepth`）、`Prefer3D`、槽位尺寸的投影 surface、场景更新 |
| `…/Projection/PlaneProjection.cs` | `ToMatrix3D`（符号约定、行向量顺序） |
| `…/Projection/ProjectionService.cs` | 附加投影（按需包裹、清除时释放）、`EnsureProjectionHost` / `FindDirectHost`、宿主包裹/解包 |
| `…/Projection/ProjectionMath.cs` | 旋转/平移辅助 |
| `…/Transitions/TransitionFrame.cs` | presenter 翻转、串行时序、准备/复位（3D + 命中测试，不加 bitmap cache） |
| `…/Transitions/Transitions.cs` | 加载原版 storyboard XAML 资源（`GetResourceStream` + `XamlReader`）、绑定整页目标、`ProjectionTransition` 清理、Turnstile 的 WP 对齐常量（`TurnstileCenterOfRotationX`、`TurnstileDurationScale`） |
| `…/Transitions/Storyboards/*.xaml` | 原版 27 个 storyboard（角度/关键时间/缓动为权威）；只有投影路径改写成 `phone:` 前缀 |
| `…/Transitions/Transitions/*.cs` | 各转场族的 `TransitionElement` |
| `…/Effects/TiltEffect.cs` | 元素级倾斜（通过 `ProjectionService`，不管理宿主）、以第一个可视子元素为支点、容器→控件变换缓存 |
| `…/Effects/TurnstileFeatherEffect.cs` | 逐元素羽化；目标从页面可视树收集；宿主包装延迟到 dispatcher |
| `…/Themes/Generic.xaml` | 唯一主题源（Metro 调色板、模板、WPF 基础控件 WP 样式）；含对齐 WP8 的 `HubTile` 模板（滑动表面 + `RotationX` 翻转）与 `PhoneTextBox`/`Rating` 布局 |
| `…/Controls/CustomMessageBox.cs` | 消息框窗口；内容承载在 `ProjectionHost`，弹出/关闭播放 `BackwardIn`/`BackwardOut` Swivel |
| `…/Controls/HubTile.cs` | 状态机；驱动标题位移、翻转（`Prefer3D`）与 3 秒脉冲 |
| `…/Controls/Rating.cs` | 以“星”为单位的命中测试（`AllowHalfItemIncrement` 除以 2）、裁剪遮罩 |
| `…/Controls/PhoneTextBox.cs` | 占位符可见性（焦点 + 输入法合成）、内部编辑器、长度提示 |
| `…/System/Shell/ApplicationBar.cs`（含 `IApplicationBar*`、`ApplicationBarIconButton`、`ApplicationBarMenuItem`、`ApplicationBarMode`、`ApplicationBarStateChangedEventArgs`） | 原版 `Microsoft.Phone.Shell` API 与状态 |
| `…/System/Shell/ApplicationBarItemList.cs`、`ApplicationBarItemContainer*.cs`、`BoolEventArgs.cs` | item 列表上限/异常、逐项校验与点击派发 |
| `…/System/Shell/ApplicationBarPresenter.cs`、`ApplicationBarItemView.cs`、`ApplicationBarIconButtonView.cs`、`ApplicationBarMenuItemView.cs` | WPF chrome：应用栏、图标槽、菜单按钮、菜单浮层、图标加载 || `…/Controls/ListPicker/ListPicker.cs` | 原版 WP8 控件：普通/展开模式 canvas 高度动画、完整模式页面导航、frame 查找、WPF 鼠标/返回键/选择适配 |
| `…/Controls/ListPicker/ListPickerPage.xaml(.cs)` | 原版完整模式页面：项目/标题入场动画经 `ProjectionService`，使用原版 `ApplicationBar`（check/cancel 图标按钮，图标来自 `Content/ApplicationBar.*.png`） |
| `…/Themes/Controls/ListPicker.xaml` | 原版 `ListPicker`/`ListPickerItem` 样式（由 `Generic.xaml` 合并，内含 `Dark.xaml`，并设置 `PickerPageUri`） |
| `src/…Sample/SampleNavigator.cs` | 为每个示例同时设置出站/入站转场，保证两侧使用同一转场族（修复“退场不是羽化”一类问题） |
| `src/…Sample/Pages/MainPage.xaml` | 示例索引，对应原版 `PhoneToolkitSample8` 的主页 |
| `src/…Sample/Pages/{Controls,Projection,Transitions,TransitionTarget,Feather,Menus}Page.xaml` | 每个示例一个页面 |
| `tests/Community.Phone.Controls.Toolkit.Tests/` | xUnit 套件（WPF 用 `StaTest`）；internal 对测试可见；等待可观察状态用 `WpfTestHelper.PumpUntil`/`PumpFor`（不用固定 sleep） |
| `build/verify-package.ps1` | 打包 Release + 全新消费冒烟 |
| `docs/en/` 与 `docs/zh-CN/` —— architecture、api-compatibility、known-limitations、migration-guide、porting-inventory | 双语正式文档 |
| `docs/porting-status.md` | 当前实现/验证状态（权威） |
| `.github/Guide.md` | 原始任务提示（历史，不代表当前状态） |

---

## 8. 建议的下一步（按顺序）

1. **提交本轮改动**（原版 WP8 `ListPicker` 移植及其两处动画修复尚未提交）：`git add -A`，提交信息概括原版源码移植、WPF 适配、高度动画与高亮画刷两处修复，并带 `Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>` 尾注。
2. **在模拟器里确认 Turnstile 调优**（Virtual Machine Connection），必要时调整 `Transitions.cs` 里的两个常量 —— 枢轴偏移与节奏只由这两处控制。
3. **目视确认其余修复**：羽化进场不再卡死（数值探针已确认）、消息框 Swivel、两侧都使用所选转场族、Rating 在 50%/80% 处的命中区域、PhoneTextBox 占位符与光标对齐、HubTile 翻转。以上均已数值验证，与模拟器的像素对比仍未做。
4. **决定 `Prefer3D` 的生命周期**：目前被丢弃的旧 presenter 会回到 2D，新 presenter 在动画完成后回到 2D。若将来出现“转场结束时文字突然变清晰”的跳变，可考虑把 `Prefer3D` 保持到下一次导航。
5. **只有隔离好状态后再考虑测试并行化** —— 并行运行会死锁，因为多组测试共享主题字典、`Popup` 与 storyboard 缓存；前提是把这些状态做成 per-test，而不是直接删掉 collection 属性。
6. **同步文档** —— 每次行为或验证状态变化时更新 `docs/porting-status.md` 与 `docs/handoff.md`（§2 结果、§5 状态）。

---

## 9. 信息优先级

1. **源代码 + 测试/构建结果 + Git 状态** —— 当前事实。
2. **`docs/handoff.md`（本文件）** —— 最近一次交接快照。
3. **`docs/*/architecture.md`、`docs/*/api-compatibility.md`、`docs/porting-status.md`** —— 已确认决策与状态。
4. **`docs/*/porting-inventory.md`** —— Phase 0 审计，历史。
5. **`.github/Guide.md`** —— 原始任务提示，历史。

---

## 10. Git 状态 —— 请保护好

- 分支：**`main`**，本工作区未配置远端跟踪。
- HEAD：**`83c24f6` "fix"** —— 包含 §4.9–§4.11 的全部内容（原版 WP8 `ListPicker` 移植、`ApplicationBar` 移植、`ListPickerPage` 应用栏、测试回路改动）以及本工作区其它进行中的工作。
- 交接时工作区：**未提交但很小** —— 只有 §4.12 的四个文件：`Common/Extensions.cs`、`Controls/ListPicker/ListPickerPage.xaml`、`Controls/ListPicker/ListPickerPage.xaml.cs`、`Themes/Controls/ListPicker.xaml`。

**已修改（本次工作）：**

```
docs/en/{api-compatibility,architecture,known-limitations,migration-guide,handoff,porting-status}.md
docs/zh-CN/{api-compatibility,architecture,handoff,known-limitations,migration-guide,porting-status}.md
src/Community.Phone/Navigation/PhoneApplicationFrame.cs   （应用栏 chrome 接线）
src/Community.Phone/Navigation/PhoneApplicationPage.cs    （ApplicationBar 改为 IApplicationBar）
src/Community.Phone/System/Shell/ApplicationBar.cs               （重写为原版 API）
src/Community.Phone/Themes/Generic.xaml                   （PhoneApplicationFrame 模板与应用栏宿主）
src/Community.Phone.Controls.Toolkit/Themes/Generic.xaml  （Metro 调色板和控件模板）
src/Community.Phone.Controls.Toolkit.Sample/Pages/MenusPage.xaml(.cs)      （使用移植版应用栏）
src/Community.Phone.Controls.Toolkit.Sample/Community.Phone.Controls.Toolkit.Sample.csproj  （Images 作为资源）
src/Community.Phone.Controls.Toolkit/Community.Phone.Controls.Toolkit.csproj  （Content/*.png 作为资源）
src/Community.Phone.Controls.Toolkit/Controls/ListPicker/ListPickerPage.xaml(.cs)  （用原版 ApplicationBar 取代命令区）
src/Community.Phone.Controls.Toolkit/Controls/CustomMessageBox/CustomMessageBox.cs （不再依赖 Application）
src/Community.Phone.Controls.Toolkit/Controls/Rating.cs                     （UpdateValueFromMouse 改为 internal）
src/Community.Phone.Controls.Toolkit/Transitions/Transitions.cs            （storyboard 缓存加锁）
src/Community.Phone/System/Shell/ApplicationBarPresenter.cs      （图标缓存加锁）
tests/Community.Phone.Controls.Toolkit.Tests/ControlTests.cs               （替换/新增 ListPicker 测试；Rating、CustomMessageBox、ListPickerPage）
tests/Community.Phone.Controls.Toolkit.Tests/ShellTests.cs                 （重写 ApplicationBar 测试）
tests/Community.Phone.Controls.Toolkit.Tests/NavigationTests.cs            （改为条件等待）
tests/Community.Phone.Controls.Toolkit.Tests/EffectsTests.cs               （改为条件等待）
tests/Community.Phone.Controls.Toolkit.Tests/TransitionStoryboardTests.cs  （改为条件等待）
tests/Community.Phone.Controls.Toolkit.Tests/WpfTestHelper.cs              （PumpUntil / PumpFor）
tests/Community.Phone.Controls.Toolkit.Tests/StaTest.cs                    （并行化保持关闭并附注释）
```

**已删除：**

```
src/Community.Phone.Controls.Toolkit/Controls/ListPicker.cs        （由 Controls/ListPicker/ 取代）
src/Community.Phone.Controls.Toolkit/Controls/ListPickerItem.cs
src/Community.Phone.Controls.Toolkit/Controls/ListPickerMode.cs
src/Community.Phone.Controls.Toolkit/Controls/ExpansionMode.cs
```

**新增（未跟踪，本次工作）：**

```
src/Community.Phone.Controls.Toolkit/Common/ItemsControlExtensions.cs
src/Community.Phone.Controls.Toolkit/Controls/ListPicker/          （ListPicker.cs、ListPickerItem.cs、ListPickerMode.cs、ExpansionMode.cs、ListPickerPage.xaml(.cs)）
src/Community.Phone.Controls.Toolkit/Themes/Controls/ListPicker.xaml
src/Community.Phone/System/Shell/IApplicationBar*.cs、ApplicationBarIconButton.cs、
    ApplicationBarMenuItem.cs、ApplicationBarMode.cs、ApplicationBarStateChangedEventArgs.cs、
    ApplicationBarItemList.cs、ApplicationBarItemContainer*.cs、ApplicationBarItemContainerFactory.cs、
    BoolEventArgs.cs、ApplicationBarPresenter.cs、ApplicationBarItemView.cs、
    ApplicationBarIconButtonView.cs、ApplicationBarMenuItemView.cs
src/Community.Phone.Controls.Toolkit.Sample/Images/appbar.*.rest.png
src/Community.Phone.Controls.Toolkit/Content/ApplicationBar.Check.png、ApplicationBar.Cancel.png
```

**工作区里还有不属于本次工作的内容**（他人在做的示例页）：`Sample/Data/`、`Sample/Samples/ListPickerSample.xaml(.cs)`、`MainPage.xaml(.cs)` 中新增的 `ListPicker` 入口、库 csproj 中重复的 `<RootNamespace>` 行，以及 `Community.Phone.Controls.Toolkit.Sample.csproj.user`。提交时不要动它们。

- **不要**执行 `git reset --hard`、`git checkout -- .`、`git clean -fd` 或强推：未跟踪的 `Controls/ListPicker/`、`Shell/ApplicationBar*` 与示例资源文件会丢失。
