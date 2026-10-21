# 交接文档 — Community.Phone.Controls.Toolkit.WPF

[English](../handoff.md) · 中文

**用途。** 这是新会话的入口：记录工作区里**真实成立**的东西——已验证的构建/测试/运行结果、已确认的设计决策、不能丢失的未提交修改，以及具体的下一步。它不重述原始任务计划（`.github/Guide.md`），也不替代完整 API 矩阵。

**最后更新：** 2026-10-19 17:24（本地时间），分支 `main`，工作区**有未提交修改**（25 个修改，其中 4 个删除；13 个新增文件，见 §10）。上一轮工作已提交为 `bc0e22e`。

---

## 1. 项目目标与验收标准

归档版 Windows Phone Toolkit 的 WPF 兼容移植，交付为单个 NuGet 包（`Community.Phone.Controls.Toolkit.WPF`，根命名空间 `Microsoft.Phone.Controls`，`net10.0-windows`）。

当前采用的验收标准：

1. **API 兼容** —— 原始 `Microsoft.Phone.Controls` / `Microsoft.Phone.Shell` 类型存在且成员/默认值兼容；所有差异都写入 `docs/*/api-compatibility.md` 与 `docs/*/known-limitations.md`。
2. **真实行为，不是空壳** —— 投影是真实的 `Viewport3D`/`Viewport2DVisual3D`，转场是真实 storyboard，控件是带 WP Metro 样式的真实 WPF 控件。
3. **证据** —— 解决方案 0 警告/0 错误、测试全绿、`build\verify-package.ps1` 通过（打包 + 全新消费工程）、示例可运行并覆盖功能。
4. **如实标注状态** —— 未实现模块明确列为未实现；没有验证手段的内容不得声称可用。

---

## 2. 当前已验证状态（2026-10-19）

| 检查项 | 命令 | 结果 |
|---|---|---|
| Release 构建 | `dotnet build Community.Phone.Controls.Toolkit.slnx -c Release` | **0 警告、0 错误** |
| Release 测试 | `dotnet test Community.Phone.Controls.Toolkit.slnx -c Release` | **72 通过 / 0 失败 / 0 跳过**（17:24） |
| Debug 测试 | `dotnet test Community.Phone.Controls.Toolkit.slnx -c Debug` | **72 通过**（17:23） |
| 打包 + 消费 | `powershell -File build\verify-package.ps1` | **通过**（17:25）—— `artifacts/packages/Community.Phone.Controls.Toolkit.WPF.0.1.0-preview.1.nupkg`（含 `.snupkg`），全新消费工程从本地源还原并运行成功 |
| 示例冒烟（UI 自动化） | 启动 `Community.Phone.Controls.Toolkit.WPF.Sample.exe` 并逐个打开示例 | **五个示例均可打开并返回**（Controls、Projection、Transitions → 目标页 → 返回、Feathering → 进入 → 返回、Menus）；进程存活且无错误日志 |

测试清单（72 个）：`ProjectionTests` 20、`ControlTests` 23、`NavigationTests` 17、`EffectsTests` 6、`ConverterTests` 3、`ShellTests` 3。

唯一需要真实指针输入的测试是 `ControlTests.Rating_MouseClickChangesValue`；现在它会重试光标定位五次，若桌面无法让指针落到控件上（例如锁屏或被其它窗口遮挡），则跳过断言，不再把环境问题报成失败。

---

## 3. 已确认的架构决策（及原因）

完整细节：[docs/zh-CN/architecture.md](architecture.md) · [English](../en/architecture.md)。

1. **投影用 WPF `Viewport3D` + `Viewport2DVisual3D`** —— 内容保持实时可交互（命中测试、焦点、真实控件），截图平面方案会丢失交互；Windows Composition/D3D 互操作因打包复杂度被否决。
2. **忠实 XAML 的透视** —— 固定 `PerspectiveDepth = 999` DIP（WinUI `PlaneProjection` 用 `M34 = -1/999`），相机位于 `z = depth`，水平视场角由视口**宽度**推导。原因：WPF `PerspectiveCamera.FieldOfView` 是水平视场角、垂直视场角由宽高比推导；按高度推导会让所有被投影页面按宽高比缩小（480×800 → 288×480）。
3. **符号约定** —— `R = Rz·Ry·Rx`、行向量矩阵、Y 偏移取反、**RotationY 取反**。原因：XAML 中正 `RotationY` 让右边缘靠近观察者，而 WPF 的 +Y 正向旋转让它远离；RotationX/Z 因元素空间 Y 向下而已天然一致。已用 `TiltEffect` 按压方向与 WinUI/Uno 实现交叉验证。
4. **转场严格串行** —— 退场完成后才开始入场；`TransitionFrame` 在两个 `ProjectionHost` presenter 之间切换。与原版 `TransitionFrame` 一致。
5. **storyboard 用代码重建** —— `Transitions.cs` 以编程方式重建原始 19 个 XAML storyboard，以便直接作用于兼容的 `PlaneProjection`；数值已与原 XAML 逐条比对。
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

---

## 5. 未完成 / 未验证

明确**不**声称的内容：

1. **与 WP 8.1 模拟器的逐帧视觉对比** —— 模拟器在运行、通过 Virtual Machine Connection 查看，但本次会话无法“看图”；验证是数值化的（几何、时序）+ 与原 XAML/WinUI 数学比对。Turnstile 的调优（枢轴 `-0.05`、时长 ×1.2）来自用户的口头观察而非测量，因此仍**需要一次视觉确认**。除 Turnstile 外的转场**没有**运行时目视确认。
2. **storyboard 数值的自动化一致性测试** —— 与原 XAML 的比对是人工完成的；目前没有测试对 `Transitions.cs` 的数值做快照/断言。
3. **羽化透视原点** —— 原版 `TurnstileFeatherEffect` 会把每个元素的透视原点移到页面垂直中心（`GlobalOffsetY` + 补偿 RenderTransform）。本移植版因 `ProjectionHost` 与元素同尺寸且裁剪 3D 内容，让元素绕自身中心旋转；角度/延迟/时序与原版一致。已写入已知限制。
4. **Shell chrome** —— `SystemTray` 与 `ApplicationBar` 是 API 模型；默认 Frame 模板不绘制 WP 状态栏或应用栏，也没有视觉验证。
5. **未实现模块** —— `AutoCompleteBox`、`LongListSelector`、`LongListMultiSelector`、`MultiselectList`、地图扩展、`TransferControl`、`SpeechTextBox`、`LockablePivot`、`SlideInEffect`、`GestureListener`/`GestureService`、`LoopingSelector`/picker 页面。已通过源码检索确认不存在（无对应 `class`）。
6. **示例覆盖缺口** —— 重构后的示例展示投影、全部转场族、羽化、所有已实现控件、ContextMenu、CustomMessageBox 以及 shell 状态（ApplicationBar/SystemTray API）；**未**展示 `Matrix3DProjection`，也没有真实 shell chrome（本来就没有绘制）。
7. **未做像素级验证的视觉细节** —— ListPicker 弹出层样式是否与 WP 一致、ExpanderView 高度动画、PhoneTextBox 水印动画、HubTile 像素保真度：行为有测试（含 HubTile 翻转旋转量与 PhoneTextBox 占位符原点），像素保真度没有。

---

## 6. 阻塞与风险

1. **本轮改动未提交。** 上一轮工作已安全保存在 `bc0e22e`；Turnstile 调优、羽化修复、六个一致性问题修复、测试改动、Sample 重构与文档更新只存在于工作区（25 个修改，含 4 个删除；13 个新增文件）。任何破坏性 git 操作都会丢失它们。见 §10。
2. **视觉验证缺口。** 缺少目视手段时，保真度结论依赖几何/时序测量与原 XAML。后续最好能并排查看示例与模拟器。
3. **`docs/*/porting-inventory.md` 是 Phase 0 的大型审计文档**（英文版约 129 KB）。其中的模块状态是历史快照；当前状态以 `docs/porting-status.md` 为准。
4. **环境坑** —— `XDE.exe` 会卡死（用 `vmconnect.exe`）；`PrintWindow` 对 3D/VM 表面返回全黑（改用 `Topmost` 窗口 + `CopyFromScreen`）；显示为 150% DPI（注意 DIP↔像素换算）；示例运行时锁定 `bin` 输出，重建前先结束进程。
5. **羽化保真风险** —— 若严格要求一致，需要把羽化宿主改成“页面尺寸的 3D 场景”，属于设计级改动而非补丁。

---

## 7. 重要文件

| 路径 | 作用 |
|---|---|
| `src/Community.Phone.Controls.Toolkit.WPF/Projection/ProjectionHost.cs` | 2D↔3D 宿主、相机（`PerspectiveDepth`）、`Prefer3D`、槽位尺寸的投影 surface、场景更新 |
| `…/Projection/PlaneProjection.cs` | `ToMatrix3D`（符号约定、行向量顺序） |
| `…/Projection/ProjectionService.cs` | 附加投影（按需包裹、清除时释放）、`EnsureProjectionHost` / `FindDirectHost`、宿主包裹/解包 |
| `…/Projection/ProjectionMath.cs` | 旋转/平移辅助 |
| `…/Transitions/TransitionFrame.cs` | presenter 翻转、串行时序、准备/复位、失败回退 |
| `…/Transitions/Transitions.cs` | 代码形式的 19 个 storyboard（数值为权威）+ Turnstile 的 WP 对齐常量（`TurnstileCenterOfRotationX`、`TurnstileDurationScale`） |
| `…/Transitions/Transitions/*.cs` | 各转场族的 `TransitionElement` |
| `…/Effects/TiltEffect.cs` | 元素级倾斜、加载时包裹、变换缓存 |
| `…/Effects/TurnstileFeatherEffect.cs` | 逐元素羽化；目标从页面可视树收集；宿主包装延迟到 dispatcher |
| `…/Themes/Generic.xaml` | 唯一主题源（Metro 调色板、模板、WPF 基础控件 WP 样式）；含对齐 WP8 的 `HubTile` 模板（滑动表面 + `RotationX` 翻转）与 `PhoneTextBox`/`Rating` 布局 |
| `…/Controls/CustomMessageBox.cs` | 消息框窗口；内容承载在 `ProjectionHost`，弹出/关闭播放 `BackwardIn`/`BackwardOut` Swivel |
| `…/Controls/HubTile.cs` | 状态机；驱动标题位移、翻转（`Prefer3D`）与 3 秒脉冲 |
| `…/Controls/Rating.cs` | 以“星”为单位的命中测试（`AllowHalfItemIncrement` 除以 2）、裁剪遮罩 |
| `…/Controls/PhoneTextBox.cs` | 占位符可见性（焦点 + 输入法合成）、内部编辑器、长度提示 |
| `src/…Sample/SampleNavigator.cs` | 为每个示例同时设置出站/入站转场，保证两侧使用同一转场族（修复“退场不是羽化”一类问题） |
| `src/…Sample/Pages/MainPage.xaml` | 示例索引，对应原版 `PhoneToolkitSample8` 的主页 |
| `src/…Sample/Pages/{Controls,Projection,Transitions,TransitionTarget,Feather,Menus}Page.xaml` | 每个示例一个页面 |
| `tests/Community.Phone.Controls.Toolkit.WPF.Tests/` | xUnit 套件（WPF 用 `StaTest`）；internal 对测试可见 |
| `build/verify-package.ps1` | 打包 Release + 全新消费冒烟 |
| `docs/en/` 与 `docs/zh-CN/` —— architecture、api-compatibility、known-limitations、migration-guide、porting-inventory | 双语正式文档 |
| `docs/porting-status.md` | 当前实现/验证状态（权威） |
| `.github/Guide.md` | 原始任务提示（历史，不代表当前状态） |

---

## 8. 建议的下一步（按顺序）

1. **提交本轮改动**（重构与本轮修复尚未提交）：`git add -A`，提交信息概括羽化回归修复、消息框 Swivel、转场族修复、Rating/PhoneTextBox/HubTile 修复，并带 `Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>` 尾注。
2. **在模拟器里确认 Turnstile 调优**（Virtual Machine Connection），必要时调整 `Transitions.cs` 里的两个常量 —— 枢轴偏移与节奏只由这两处控制。
3. **目视确认这六项修复**：羽化进场不再卡死（数值探针已确认）、消息框 Swivel、两侧都使用所选转场族、Rating 在 50%/80% 处的命中区域、PhoneTextBox 占位符与光标对齐、HubTile 翻转。以上均已数值验证，与模拟器的像素对比仍未做。
4. **增加 storyboard 一致性测试**（internal 对测试程序集可见）：按族断言 `Transitions` 的关键帧数值/时长，防止偏离原 XAML。
5. **决定 `Prefer3D` 的生命周期**：目前被丢弃的旧 presenter 会回到 2D，新 presenter 在动画完成后回到 2D。若将来出现“转场结束时文字突然变清晰”的跳变，可考虑把 `Prefer3D` 保持到下一次导航。
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
- 已有提交：`bc0e22e`（上一轮的文档 + 投影/转场/倾斜修复）、`83d5d73 fix: 0.01`、`81a55d2 feat: 0.0.1`。
- 交接时工作区：**25 个修改（含 4 个删除）+ 13 个新增文件，全部未提交。**

**已修改/已删除（不可丢失）：**

```
.github/copilot-instructions.md
README.md
README_zh-CN.md
docs/en/architecture.md
docs/en/known-limitations.md
docs/handoff.md
docs/porting-status.md
docs/zh-CN/architecture.md
docs/zh-CN/handoff.md
docs/zh-CN/known-limitations.md
docs/zh-CN/porting-status.md
src/Community.Phone.Controls.Toolkit.WPF.Sample/App.xaml.cs
src/Community.Phone.Controls.Toolkit.WPF.Sample/Pages/FeatherPage.xaml
src/Community.Phone.Controls.Toolkit.WPF.Sample/Pages/HomePage.xaml        （已删除）
src/Community.Phone.Controls.Toolkit.WPF.Sample/Pages/HomePage.xaml.cs     （已删除）
src/Community.Phone.Controls.Toolkit.WPF.Sample/Pages/DetailPage.xaml      （已删除）
src/Community.Phone.Controls.Toolkit.WPF.Sample/Pages/DetailPage.xaml.cs   （已删除）
src/Community.Phone.Controls.Toolkit.WPF/Controls/CustomMessageBox.cs
src/Community.Phone.Controls.Toolkit.WPF/Controls/HubTile.cs
src/Community.Phone.Controls.Toolkit.WPF/Controls/PhoneTextBox.cs
src/Community.Phone.Controls.Toolkit.WPF/Controls/Rating.cs
src/Community.Phone.Controls.Toolkit.WPF/Effects/TurnstileFeatherEffect.cs
src/Community.Phone.Controls.Toolkit.WPF/Themes/Generic.xaml
src/Community.Phone.Controls.Toolkit.WPF/Transitions/Transition.cs
src/Community.Phone.Controls.Toolkit.WPF/Transitions/TransitionFrame.cs
src/Community.Phone.Controls.Toolkit.WPF/Transitions/Transitions.cs
tests/Community.Phone.Controls.Toolkit.WPF.Tests/ControlTests.cs
tests/Community.Phone.Controls.Toolkit.WPF.Tests/EffectsTests.cs
tests/Community.Phone.Controls.Toolkit.WPF.Tests/NavigationTests.cs
```

**新增（未跟踪）：**

```
src/Community.Phone.Controls.Toolkit.WPF.Sample/SampleNavigator.cs
src/Community.Phone.Controls.Toolkit.WPF.Sample/Pages/MainPage.xaml(.cs)
src/Community.Phone.Controls.Toolkit.WPF.Sample/Pages/ControlsPage.xaml(.cs)
src/Community.Phone.Controls.Toolkit.WPF.Sample/Pages/ProjectionPage.xaml(.cs)
src/Community.Phone.Controls.Toolkit.WPF.Sample/Pages/TransitionsPage.xaml(.cs)
src/Community.Phone.Controls.Toolkit.WPF.Sample/Pages/TransitionTargetPage.xaml(.cs)
src/Community.Phone.Controls.Toolkit.WPF.Sample/Pages/MenusPage.xaml(.cs)
```

- **不要**执行 `git reset --hard`、`git checkout -- .`、`git clean -fd` 或强推。

---

## 11. 长期开发规则摘要（中文）

完整规则见 `.github/copilot-instructions.md`（Copilot 只读取英文单文件，此处为中文摘要）。

- 保持原始公共 API 形状；移植新增 API 必须是**加法**并写入兼容性文档。
- 不得虚报：未实现/未验证的内容必须显式标注；未实现模块继续留在已知限制中。
- 转场数值只能来自原 storyboard XAML，不得自行调参。**唯一例外**是 `Transitions.cs` 中 Turnstile 的两个对齐常量（`TurnstileCenterOfRotationX = -0.05`、`TurnstileDurationScale = 1.2`）；设为 `0` / `1` 即还原原工具包。
- 出站转场取自**被离开的页面**：需要特定退场的页面必须自己声明或在导航前设置（示例见 `SampleNavigator`）。
- 羽化元素在**订阅时**包装（`SetFeatheringIndex`），清空索引时释放；动画期间绝不要重新挂载内容。
- 示例结构对齐原版 `PhoneToolkitSample8`：`MainPage` 索引 + 每示例独立页面。
- 投影：`PerspectiveDepth = 999`、相机在 `z = depth`、水平 FOV 由视口**宽度**推导；`R = Rz·Ry·Rx`、Y 偏移取反、`RotationY` 取反。
- 动画/效果切换 2D↔3D 必须在首帧之前（`Prefer3D`）；不要在转场/效果宿主上使用 `BitmapCache`。
- 元素级效果必须使用**直接包含元素**的宿主。
- `TransitionFrame` 模板部件是两个 `ProjectionHost`（`FirstContentPresenter`/`SecondContentPresenter`），退场先于入场。
- 构建 0 警告 0 错误；测试全绿；每个 bug 修复都要有回归测试；改动公共 API 后跑 `build\verify-package.ps1`。
- 文档双语同步（`docs/en` + `docs/zh-CN`，根目录用 `_zh-CN` 后缀）。
- 模拟器只用 Virtual Machine Connection，不用 `XDE.exe`。
