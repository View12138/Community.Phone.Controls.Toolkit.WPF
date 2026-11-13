# Copilot 说明 — Community.Phone.Controls.Toolkit

## 仓库定位

- 已归档 Microsoft **Windows Phone Toolkit**（MS-PL，原始源码 `D:\repos\repo-other\WindowsPhoneToolkit`）的 WPF 兼容移植，以单个 NuGet 包发布。
- 包名 `Community.Phone.Controls.Toolkit`，含两个程序集：工具包 `Community.Phone.Controls.Toolkit` 与 WP 兼容层 `Community.Phone`。根命名空间保持 `Microsoft.Phone.Controls` / `Microsoft.Phone.Shell`，目标框架 `net10.0-windows`，无第三方运行时依赖。
- 目标：WP 工具包应用只需改命名空间、包裹被投影内容即可迁移；Metro 视觉、页面过渡与 3D 投影应与 WP 8.1 一致。这是兼容移植，不是新 UI 框架。

## 不可妥协的原则

1. **保留原始公共 API 形态**（类型名、成员、语义、默认值）。新行为写进文档，而不是改名 API。
2. WPF 没有 `UIElement.Projection`。移植提供 `ProjectionHost` / `ProjectionService` / `PlaneProjection` / `Matrix3DProjection`；移植独有的 API 必须是**纯新增**，并在 `docs/*/api-compatibility.md` 中记录。
3. WP 平台基础设施（`PhoneApplicationFrame`、`PhoneApplicationPage`、`PhoneApplicationService`、`SystemTray`、`ApplicationBar`）在兼容命名空间中重新实现；差异（无墓碑化、无真实状态栏/电池等）必须明示，不得隐瞒。
4. **绝不把未实现或未验证的行为报告为已完成**；不支持的模块持续列在 `docs/en/known-limitations.md` 与 `docs/zh-CN/known-limitations.md`。
5. 过渡数值（角度、时长、延迟、缓动）取自 `Microsoft.Phone.Controls.Toolkit\Transitions\Storyboards\*.xaml`，照抄，不得自创或“调优”。

## 架构约束（不得破坏）

### 项目拆分

- `src/Community.Phone` 是 `Microsoft.Phone.dll` 兼容层：`Microsoft.Phone.Controls.PhoneApplicationFrame` / `PhoneApplicationPage` / `NavigationContext` / 方向类型，以及全部 `Microsoft.Phone.Shell` 类型（应用栏、系统托盘、应用服务）。它不依赖工具包。
- `src/Community.Phone.Controls.Toolkit` 是工具包程序集，引用 `Community.Phone`；不得反向引用，也不得把工具包控件搬进兼容层。
- 两个程序集打进同一个 NuGet 包（csproj 中的 `IncludeCompatibilityLayerInPackage`），不得拆成两个包。
- 因共享 `Microsoft.Phone.Controls` 命名空间，XAML 需要两个前缀：`phone:`（`assembly=Community.Phone.Controls.Toolkit`，工具包控件）、`platform:`（`assembly=Community.Phone`，页面/框架）；`shell:` 映射到 `Community.Phone`。
- 兼容层自带主题字典 `src/Community.Phone/Themes/Generic.xaml`（目前为 `PhoneApplicationFrame` 模板）；工具包主题仍在 `src/Community.Phone.Controls.Toolkit/Themes/Generic.xaml`。

### 投影 / PlaneProjection

- 相机 = XAML 固定透视：`ProjectionHost.PerspectiveDepth = 999` DIP（WinUI `M34 = -1/999`），相机位于 `(0, 0, depth)`，**水平** FOV 由视口**宽度**推出：`2 * atan(viewportWidth / (2 * depth))`。用高度推导会按宽高比缩小投影内容（历史缺陷：480×800 渲染成 288×480）。
- 矩阵为行向量：`p' = (((p - center) + localOffset) * R) + center + globalOffset`，`R = Rz * Ry * Rx`。Y 偏移取反（WPF 3D 为 +Y 向上，屏幕为 +Y 向下），**RotationY 取反**（XAML 正 `RotationY` 使右缘靠近观察者，正 WPF 旋转则远离）；RotationX/RotationZ 已一致，不要“修正”。
- `Prefer3D` 使 3D 渲染路径在投影仍为单位矩阵时保持启用，让过渡/特效在首个动画帧前完成路径切换。不要在过渡或特效宿主上重新引入 `BitmapCache`：`Viewport2DVisual3D` 已栅格化内容，第二次 1× 缓存只会让画面变糊（已实测）。
- 元素特效（倾斜、羽化）必须投影**直接包含**该元素的宿主（`ProjectionService.EnsureProjectionHost`）；复用祖先宿主会投影整页，因为页面本身由过渡框架的呈现器承载。
- 单位投影必须保持像素级精确：投影特征位置与 2D 布局误差 ≤ 0.01 px（`ProjectionTests` 回归测试）。

### 框架、导航、过渡

- `TransitionFrame` 的模板部件是名为 `FirstContentPresenter` / `SecondContentPresenter` 的 `ProjectionHost`，不是 `ContentPresenter`。
- 退出过渡在进入过渡开始**之前**完成；旧呈现器在 `Begin()` 前预置（`IsHitTestVisible = false`、`Prefer3D = true`），完成后恢复交互；非取消的退出路径用 `null` 完成被丢弃的呈现器（原版行为）。
- Turnstile 铰链靠近左边缘（`TurnstileCenterOfRotationX = -0.05`、`CenterOfRotationY = 0.5`）；正向退出 `0→50°`，正向进入 `−80→0°`，阶段乘以 `TurnstileDurationScale = 1.2`。`Transitions.cs` 中这两个常量是**唯一**获准偏离原工具包之处（设为 `0`/`1` 即完全还原），不得改动故事板数值本身。
- 页面的**退出**过渡取自被离开的页面；需要特定退出效果的场景必须在该页面上声明或在导航前设置（示例在 `SampleNavigator` 中这样做）。
- `TiltEffect` 在加载时包裹可倾斜控件、禁用时解包；指针按下期间不得改动可视化树。
- `TurnstileFeatherEffect` 在组装故事板时从页面**可视化树**收集目标（附加属性订阅只负责准备 `ProjectionHost` 包裹层，在 `Loaded`/`SizeChanged` 后由派发器延后执行）。过渡动画进行中或布局过程内不得重新挂载内容。
- 每个过渡都必须完成：没有动画的故事板（如页面没有羽化元素）在下一次派发器回合完成，否则框架永久等待、新页面无法交互。
- 新页面在接入可视化树之前组装：`TransitionFrame` 先调用 `_newContentPresenter.UpdateLayout()`，以便依赖真实几何的特效找到目标。
- 有原 WP8 模板的控件照原样实现：`HubTile` 用 `TranslateTransform` 滑动两格高的表面，并用真实 `PlaneProjection.RotationX` 翻转（`0→180→360`，侧立时换面，翻转使用 `Prefer3D`）；`Rating` 即使启用 `AllowHalfItemIncrement` 也以项为单位存 `Value`；`PhoneTextBox` 把提示文字放在插入符偏移处，聚焦或输入法组合时隐藏。

## 工程规则

- 构建：`dotnet build Community.Phone.Controls.Toolkit.slnx -c Release` 必须保持 **0 警告 / 0 错误**。
- 测试：xUnit，WPF 测试经 `StaTest.Run` 运行。全量：`dotnet test Community.Phone.Controls.Toolkit.slnx -c Release`（当前 144 个测试：142 通过，另 2 个 `ListPicker` 断言在基线上同样失败）。每个缺陷修复都要附一个修复前会失败的回归测试。
- 打包：`build\verify-package.ps1` 打 `Release` 包，然后用本地源全新还原/构建/运行消费方；公共 API 或行为变更后需重跑。
- 示例：`src/Community.Phone.Controls.Toolkit.Sample` 是人工/视觉验证面，镜像原 `PhoneToolkitSample8`（`MainPage` 列出样例、每个样例一页、`SampleNavigator` 显式设置双向过渡）。行为变更后示例仍须能演示该行为且能正常启动。
- 文档双语：英文在 `docs/en/`，中文在 `docs/zh-CN/`；根级文档使用 `_zh-CN` 后缀；同一次变更内两种语言一起更新。
- `Themes/Generic.xaml` 是各程序集的模板源（工具包：Metro 配色与控件模板；兼容层：框架模板）。不要手改生成的资源设计器文件或 `repo-other` 下的原 WP 工程。
- 保持库无依赖，仅在确有并已记录的需求时新增包。

## 验证环境

- WP 8.1 模拟器只能通过 **Virtual Machine Connection**（`vmconnect.exe`）查看，绝不用 `XDE.exe`（本机会挂起）；不要重置或删除模拟器 VM。
- 离屏证据：`RenderTargetBitmap`（使用 96 DPI 与 144 DPI）。屏上证据：`Topmost` 窗口的 `CopyFromScreen`；`PrintWindow` 对 3D 与 VM 表面返回全黑。本机为 150% DPI，注意 DIP↔像素换算。
- 重建前先停止运行中的示例，否则会锁定 `bin\Debug\...\*.resources.dll`。

## 文档优先级

`源码 + 测试结果 + git 状态` → `docs/handoff.md`（最新交接）→ `docs/*/architecture.md`、`docs/*/api-compatibility.md`、`docs/porting-status.md` → `docs/*/porting-inventory.md`（Phase 0 审计，历史）→ `.github/Guide.md`（原始任务提示，历史）。
