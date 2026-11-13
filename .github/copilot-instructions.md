# Copilot 说明 — Community.Phone.Controls.Toolkit

## 仓库定位

- 已归档 Microsoft **Windows Phone Toolkit**（MS-PL）的 WPF 兼容移植，以单个 NuGet 包发布。
- 包 ID `Community.Phone.Controls.Toolkit`，含两个程序集：toolkit `Community.Phone.Controls.Toolkit` 与 WP 兼容层 `Community.Phone`。根命名空间保持 `Microsoft.Phone.Controls` / `Microsoft.Phone.Shell`，目标框架 `net10.0-windows`，无第三方运行时依赖。
- 目标：WP 工具包应用只需改命名空间、包裹被投影的内容即可迁移；Metro 视觉、页面转场与 3D 投影应与 WP 8.1 一致。这是兼容移植，不是新 UI 框架。

## 不可妥协的原则

1. **保留原始公共 API 形态**（类型名、成员、语义、默认值）。新行为写进文档，而不是改名 API。
2. WPF 没有 `UIElement.Projection`。移植提供 `ProjectionHost` / `ProjectionService` / `PlaneProjection` / `Matrix3DProjection`；移植独有的 API 必须是**纯新增**，并记录在 `docs/api-compatibility.md`。
3. WP 平台基础设施（`PhoneApplicationFrame`、`PhoneApplicationPage`、`PhoneApplicationService`、`SystemTray`、`ApplicationBar`）在兼容命名空间中重新实现；差异必须明示，不得隐瞒。
4. **绝不把未实现或未验证的行为报告为已完成**；不支持的模块持续列在 `docs/known-limitations.md`。
5. 转场数值（角度、时长、延迟、缓动）取自 `Transitions/Storyboards/*.xaml`，照抄，不得自创或“调优”；唯一例外是 `Transitions.cs` 中的 `TurnstileCenterOfRotationX` 与 `TurnstileDurationScale`。

## 架构约束（不得破坏）

### 项目说明

- `src/Community.Phone` 是 `Microsoft.Phone` 兼容层，由 WindowsPhoneSDK 提供且WPF中不存在的，非 ToolKit 项目的内容
- `src/Community.Phone.Controls.Toolkit` 是 toolkit 程序集，引用 `Community.Phone`；不得反向引用，也不得把 toolkit 控件搬进兼容层。
- 两个程序集打进同一个 NuGet 包（csproj 的 `IncludeCompatibilityLayerInPackage`），不得拆成两个包。
- 共用 `Microsoft.Phone.Controls` 命名空间，XAML 需要多个前缀：`phone:`（toolkit 程序集）、`platform:`（页面/框架/Pivot/LLS/HyperlinkButton）、`shell:`（Shell）。
- 主题：`Community.Phone/Themes/Theme.xaml` 是唯一调色板字典，两个 `Themes/Generic.xaml` 各自合并本程序集需要的控件字典。

### 投影

- 相机 = XAML 固定透视：`ProjectionHost.PerspectiveDepth = 999` DIP，相机位于 `(0, 0, depth)`，**水平** FOV 由视口**宽度**推出 `2 * atan(viewportWidth / (2 * depth))`；用高度推导会按宽高比缩小投影内容。
- 矩阵为行向量：`p' = (((p - center) + localOffset) * R) + center + globalOffset`，`R = CreateRotationMatrix(RotationX, -RotationY, RotationZ)`。Y 偏移取反，**RotationY 取反**；RotationX/Z 已一致，不要“修正”。
- `Prefer3D` 让 3D 路径在投影仍为 identity 时保持启用，使转场/效果在首个动画帧前完成路径切换。不要在转场或效果宿主上重新引入 `BitmapCache`：`Viewport2DVisual3D` 已栅格化内容。
- 元素级效果必须投影**直接包含**该元素的宿主（`ProjectionService.EnsureProjectionHost`）；复用祖先宿主会投影整页。

### 框架、导航、转场

- `TransitionFrame` 的模板部件是名为 `FirstContentPresenter` / `SecondContentPresenter` 的 `ProjectionHost`，不是 `ContentPresenter`。
- 退场在进场开始**之前**完成；旧呈现器在启动前预置（`IsHitTestVisible = false`、`Prefer3D = true`），完成后恢复交互。
- 每个转场都必须完成：没有动画的 storyboard 在下一个 dispatcher 周期完成，否则新页面永久不可交互。
- 页面在接入可视树之前就被合成，`TransitionFrame` 先 `UpdateLayout()` 以便羽化找到目标。
- 出站转场取自被离开的页面；需要特定退场效果的页面自己声明，或在导航前 `TransitionService.SetNavigationOutTransition`。
- `TiltEffect` 在加载时包装可倾斜控件、禁用时解包；指针按下期间不得改动视觉树。
- `TurnstileFeatherEffect` 在合成 storyboard 时从页面可视树收集目标；包装延迟到 `Loaded`/`SizeChanged` 之后的 dispatcher。
- 有原版模板的控件照原样实现：`HubTile` 用 `TranslateTransform` 滑动两格高表面并用真实 `PlaneProjection.RotationX` 翻转；`Rating` 的 `Value` 以“星”为单位；`PhoneTextBox` 把提示文字放在插入符偏移处，聚焦或输入法组合时隐藏。
- `SystemTray` 是无附加属性的静态类；不得在 XAML 中写 `shell:SystemTray.IsVisible`。

## 工程规则

- 构建：`dotnet build Community.Phone.Controls.Toolkit.slnx -c Release`。
- 仓库内**没有测试工程**。新增行为需要手动验证（示例应用）并在 `docs/porting-status.md` 记录依据，不得虚构测试结果。
- 打包：`dotnet pack src\Community.Phone.Controls.Toolkit\Community.Phone.Controls.Toolkit.csproj -c Release`。`csproj` 目前引用了不存在的随包文档，`build\verify-package.ps1` 引用了不存在的 `tests/PackageConsumer`，修复后才能执行。
- 示例：`samples/Community.Phone.Controls.Toolkit.Sample` 是人工验证面，结构与原 `PhoneToolkitSample8` 对齐（`MainPage` 索引、每个示例一页、`SampleNavigator` 显式设置双向转场）。行为变更后示例须能演示该行为。
- 文档为简体中文，位于 `docs/`；同一次改动内更新受影响的文档。
- 不要手改生成的资源设计器文件。
- 如果是参照项目中的代码，请尽可能的保持不变，和参照代码一致，除非是需要兼容处理的地方
- 新增内容、方法内部、私有方法非必要不添加注释，只在必要的公开方法中添加简明注释，如果是参照代码，则不应该做除了兼容性以外的任何改动。 

## 文档优先级

源码与构建结果 → `docs/architecture.md`、`docs/api-compatibility.md`、`docs/porting-status.md`、`docs/theming.md`、`docs/known-limitations.md`。

## 参照项目

| 项目                                     | 参照项目                               | 参照项目路径                                                                 |
| ---------------------------------------- | -------------------------------------- | ---------------------------------------------------------------------------- |
| `Community.Phone`                        | `WindowsPhone`                         | D:\repos\repo-other\WindowsPhone                                             |
| `Community.PhoneControls.Toolkit`        | `Microsoft.Phone.Controls.Toolkit.WP8` | D:\repos\repo-other\WindowsPhoneToolkit\Microsoft.Phone.Controls.Toolkit.WP8 |
| `Community.PhoneControls.Toolkit.Sample` | `PhoneToolkitSample8`                  | D:\repos\repo-other\WindowsPhoneToolkit\PhoneToolkitSample8                  |
