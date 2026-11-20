# Copilot 说明 — Community.Phone.Controls.Toolkit

## 仓库定位

- 已归档 Microsoft **Windows Phone Toolkit**（MS-PL）的 WPF 兼容移植，以单个 NuGet 包发布。
- 包 ID `Community.Phone.Controls.Toolkit`，含两个程序集：toolkit `Community.Phone.Controls.Toolkit` 与 WP 兼容层 `Community.Phone`。根命名空间保持 `Microsoft.Phone.Controls` / `Microsoft.Phone.Shell`，目标框架 `net10.0-windows`，无第三方运行时依赖。
- 目标：WP 工具包应用只需改命名空间、包裹被投影的内容即可迁移；Metro 视觉、页面转场与 3D 投影应与 WP 8.1 一致。这是兼容移植，不是新 UI 框架。

## 不可妥协的原则

1. **保留原始公共 API 形态**（类型名、成员、语义、默认值）。新行为写进文档，而不是改名 API。
2. WPF 没有 `UIElement.Projection`。移植提供 `ProjectionService`（附加属性）/ `PlaneProjection` / `Matrix3DProjection`（编译在 `Community.Phone` 程序集，XAML 用 `platform:` 前缀）；移植独有的 API 必须是**纯新增**，并记录在 `docs/api-compatibility.md`。
3. WP 平台基础设施（`PhoneApplicationFrame`、`PhoneApplicationPage`、`PhoneApplicationService`、`SystemTray`、`ApplicationBar`）在兼容命名空间中重新实现；差异必须明示，不得隐瞒。
4. **绝不把未实现或未验证的行为报告为已完成**；不支持的模块持续列在 `docs/known-limitations.md`。
5. 转场数值（角度、时长、延迟、缓动）取自 `Transitions/Storyboards/*.xaml`，照抄，不得自创或“调优”；例外是 `Transitions.cs` 中的 `TurnstileCenterOfRotationX` 与 `TurnstileDurationScale`，以及由 `TurnstileFeatherEffect` 在代码中合成的 feather storyboard。

## 架构约束（不得破坏）

### 项目说明

- `src/Community.Phone` 是 `Microsoft.Phone` 兼容层，由 WindowsPhoneSDK 提供且WPF中不存在的，非 ToolKit 项目的内容；平台层的页面/框架、投影类型、`Panorama`、`Pivot`、`LongListSelector`、`HyperlinkButton` 与 Shell 类型在此。
- `src/Community.Phone.Controls.Toolkit` 是 toolkit 程序集，引用 `Community.Phone`；不得反向引用，也不得把 toolkit 控件搬进兼容层。
- 两个程序集打进同一个 NuGet 包（csproj 的 `IncludeCompatibilityLayerInPackage`），不得拆成两个包。
- 共用 `Microsoft.Phone.Controls` 命名空间，XAML 需要多个前缀：`phone:`（toolkit 程序集：转场、效果、控件）、`platform:`（兼容层：页面/框架/投影/Panorama/Pivot/LLS/HyperlinkButton）、`shell:`（Shell）。storyboard 资源内部的 `phone:` 别名指向 `Community.Phone`，与消费方约定无关。
- 主题：`Community.Phone/Themes/Theme.xaml` 是唯一调色板字典，两个 `Themes/Generic.xaml` 各自合并本程序集需要的控件字典。

### 投影

- 渲染后端是**单应变换 + `ShaderEffect`**：元素局部平面 → `Matrix3D` → 3×3 齐次矩阵（`ProjectionMath.CreateHomography`）→ `PerspectiveShaderEffect` 逆映射采样。不创建 `Viewport3D`，不重 Parent。
- 相机 = XAML 固定透视：默认深度 999 DIP，相机位于 `(0, 0, depth)`，**水平** FOV 由视口**宽度**推出 `2 * atan(viewportWidth / (2 * depth))`；用高度推导会按宽高比缩小投影内容。
- 矩阵为行向量：`p' = (((p - center) + localOffset) * R) + center + globalOffset`，`R = CreateRotationMatrix(RotationX, -RotationY, RotationZ)`。Y 偏移取反，**RotationY 取反**；RotationX/Z 已一致，不要“修正”。
- `HomographyMatrix` 的行是输入分量、列是输出分量。取“某个输出分量的三个系数”必须用 `GetOutputCoefficients`，不要直接把 `M31/M32/M33` 当成权重行（曾因此让所有像素被掩码吞掉）。
- 着色器常量：`c0..c2` 逆矩阵三个输出分量、`c3` 正向右量 + 启用标志、`c4` 输出偏移与填充尺寸、`c5` 元素尺寸。`uv` 覆盖**填充后**的输出面，`OutputBounds.xy` 是负偏移，`position = padded + xy`、`texUv = (source - xy) / zw`。
- `ShaderEffect.PaddingLeft..` 是 protected，由 `PerspectiveShaderEffect` 内部写入；输出面每轴上限 4096 DIP。Padding 不能突破父级裁剪。
- 每帧只复用同一个 effect 并写常量；identity 且未设置 `PreferShader` 时摘掉 effect，回到普通 2D 渲染。
- 元素级效果（转场、`TiltEffect`、`TurnstileFeatherEffect`、`ListPicker`）直接把投影赋给目标元素，没有宿主/包装层。
- 需要“裁内容、不裁投影”时用“外层投影元素 + 内层 `ClipToBounds` 容器”（`HubTile` 模板就是这种结构），不要把投影和裁剪放在同一个元素上。
- `UIElement.Effect` 是单一属性：投影会接管它，原效果被保存/恢复并触发 `EffectConflict`；`EffectConflictBehavior.Preserve` 可改为跳过投影。不要静默丢弃原有效果。
- `Style`/`ItemContainerStyle` 里共享的 `Freezable` 会被 WPF 冻结、无法动画；`ListBoxItem` 的投影放 `DataTemplate` 或按容器创建。
- 着色器字节码 `Projection/Rendering/Shader/*.ps` 以 `Resource` 嵌入 `Community.Phone.dll`；改 HLSL 后跑 `build/compile-shaders.ps1`（需要 Windows SDK 的 `fxc.exe`）。

### 框架、导航、转场

- `TransitionFrame` 的模板部件是名为 `FirstContentPresenter` / `SecondContentPresenter` 的 `ContentPresenter`；转场投影由被承载的页面承担。
- 退场在进场开始**之前**完成；旧呈现器在启动前预置（`IsHitTestVisible = false`、对页面设 `PreferShader`），完成后恢复交互。
- 每个转场都必须完成：没有动画的 storyboard 在下一个 dispatcher 周期完成，否则新页面永久不可交互。
- 页面在接入可视树之前就被合成，`TransitionFrame` 先 `UpdateLayout()` 以便羽化找到目标。
- 出站转场取自被离开的页面；需要特定退场效果的页面自己声明，或在导航前 `TransitionService.SetNavigationOutTransition`。
- `TiltEffect` 在加载时包装可倾斜控件、禁用时解包；指针按下期间不得改动视觉树。
- `TurnstileFeatherEffect` 在合成 storyboard 时从页面可视树收集目标；包装延迟到 `Loaded`/`SizeChanged` 之后的 dispatcher。
- 有原版模板的控件照原样实现：`HubTile` 用 `TranslateTransform` 滑动两格高表面并用真实 `PlaneProjection.RotationX` 翻转；`Rating` 的 `Value` 以“星”为单位；`PhoneTextBox` 把提示文字放在插入符偏移处，聚焦或输入法组合时隐藏。
- `SystemTray` 是静态类，同时提供静态状态属性和 `IsVisibleProperty` 附加属性（`GetIsVisible`/`SetIsVisible`）；XAML 中 `shell:SystemTray.IsVisible` 可用，但只在宿主为 `PhoneApplicationPage`、`PhoneApplicationFrame`、`Window` 时同步状态。它不绘制状态栏。

## 工程规则

- 构建：`dotnet build Community.Phone.Controls.Toolkit.slnx -c Release`，当前通过（0 错误，若干警告）。
- 测试：`tests/Community.Phone.Tests`（xunit，`dotnet test tests/Community.Phone.Tests`）覆盖投影数学、服务行为与离屏渲染；其余模块仍需手动验证（示例应用）并在 `docs/porting-status.md` 记录依据，不得虚构测试结果。
- 性能：`tools/ProjectionBenchmark` 可对同一工作负载测新旧后端（`-p:CommunityPhoneProject=<另一个 Community.Phone.csproj>`）。
- 打包：`dotnet pack src\Community.Phone.Controls.Toolkit\Community.Phone.Controls.Toolkit.csproj -c Release` 当前因 `csproj` 引用不存在的 `THIRD-PARTY-NOTICES_zh-CN.md`、`docs/en/*.md`、`docs/zh-CN/*.md` 报 `NU5019`；`build\verify-package.ps1` 引用了不存在的 `tests/PackageConsumer`，修复后才能执行。
- 示例：`samples/Community.Phone.Controls.Toolkit.Sample` 是人工验证面，结构与原 `PhoneToolkitSample8` 对齐（`MainPage` 索引、`SampleNavigator` 显式设置双向转场）。索引 25 条中 4 条没有页面文件的条目用 `Supported=false` 禁用并加删除线；`lockablepivot` 指向 `Samples/LockablePivotSample.xaml`，演示 `Pivot.IsLocked`。行为变更后示例须能演示该行为。
- 文档为简体中文，位于 `docs/`；同一次改动内更新受影响的文档。
- 不要手改生成的资源设计器文件。
- 如果是参照项目中的代码，请尽可能的保持不变，和参照代码一致，除非是需要兼容处理的地方
- 新增内容、方法内部、私有方法非必要不添加注释，只在必要的公开方法中添加简明注释，如果是参照代码，则不应该做除了兼容性以外的任何改动。 
- 有任何不确定得地方及时询问用户。

## 文档优先级

源码与构建结果 → `docs/architecture.md`、`docs/api-compatibility.md`、`docs/porting-status.md`、`docs/theming.md`、`docs/known-limitations.md`。

## 参照项目

| 项目                                     | 参照项目                               | 参照项目路径                                                                 |
| ---------------------------------------- | -------------------------------------- | ---------------------------------------------------------------------------- |
| `Community.Phone`                        | `WindowsPhone`                         | D:\repos\repo-other\WindowsPhone                                             |
| `Community.PhoneControls.Toolkit`        | `Microsoft.Phone.Controls.Toolkit.WP8` | D:\repos\repo-other\WindowsPhoneToolkit\Microsoft.Phone.Controls.Toolkit.WP8 |
| `Community.PhoneControls.Toolkit.Sample` | `PhoneToolkitSample8`                  | D:\repos\repo-other\WindowsPhoneToolkit\PhoneToolkitSample8                  |
