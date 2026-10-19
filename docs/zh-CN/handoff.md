# 交接文档 — Community.Phone.Controls.Toolkit.WPF

[English](../handoff.md) · 中文

**用途。** 这是新会话的入口：记录工作区里**真实成立**的东西——已验证的构建/测试/运行结果、已确认的设计决策、不能丢失的未提交修改，以及具体的下一步。它不重述原始任务计划（`.github/Guide.md`），也不替代完整 API 矩阵。

**最后更新：** 2026-10-19 16:00（本地时间），分支 `main`，工作区**有未提交修改**（21 个修改 + 8 个新增文件，见 §10）。

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
| Release 构建 | `dotnet build Community.Phone.Controls.Toolkit.slnx -c Release` | **0 警告、0 错误**（15:55:28） |
| Release 测试 | `dotnet test Community.Phone.Controls.Toolkit.slnx -c Release --no-build` | **68 通过 / 0 失败 / 0 跳过**（23 秒） |
| Debug 测试 | `dotnet test Community.Phone.Controls.Toolkit.slnx -c Debug` | **68 通过** |
| 打包 + 消费 | `powershell -File build\verify-package.ps1` | **通过**（16:00）—— `artifacts/packages/Community.Phone.Controls.Toolkit.WPF.0.1.0-preview.1.nupkg`（含 `.snupkg`），全新消费工程从本地源还原并运行成功 |
| 示例冒烟（UI 自动化） | 启动 `Community.Phone.Controls.Toolkit.WPF.Sample.exe` 并调用按钮 | Home → Detail（turnstile）→ Go back → Home → Feather page → Go back 全部成功；`Accent button` 点击前后及两次往返后始终 `644×66`；进程存活 |

测试清单（68 个）：`ProjectionTests` 20、`ControlTests` 22、`NavigationTests` 14、`EffectsTests` 6、`ConverterTests` 3、`ShellTests` 3。

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

- `Effects/TiltEffect.cs`：只倾斜控件本身（不再整页）；在 `Loaded` 时完成包裹（倾斜关闭时解包），避免指针输入期间改动视觉树；在切换到 3D 之前先缓存 container→host 变换（3D 宿主无法做 2D 变换）；用 `Prefer3D` 取代 `BitmapCache`。
- `Effects/TurnstileFeatherEffect.cs`：逐元素宿主现在真正生效（同一个祖先宿主 bug 的修复），并加入 `Prefer3D` 的准备/清理。

### 4.4 测试增改

- `ProjectionTests`：`ProjectionHost_KeepsTheProjectedPlaneOneToOneWithTheLayoutSlot`、`ProjectionHost_AppliesTheXamlPerspectiveDepth`、`PlaneProjection_RotationYMatchesTheXamlDepthDirection`（重命名），并更新两处符号断言。
- `EffectsTests`：`TiltEffect_DoesNotTiltThePageThatHostsTheElement`。
- `NavigationTests`：`TransitionFrame_PlaysTurnstileTransition` 现在断言首帧与结束后的 `Prefer3D`/`IsRenderingIn3D`。

---

## 5. 未完成 / 未验证

明确**不**声称的内容：

1. **与 WP 8.1 模拟器的逐帧视觉对比** —— 模拟器在运行、通过 Virtual Machine Connection 查看，但本次会话无法“看图”；验证是数值化的（几何、时序）+ 与原 XAML/WinUI 数学比对。除 Turnstile 外的转场**没有**运行时目视确认。
2. **storyboard 数值的自动化一致性测试** —— 与原 XAML 的比对是人工完成的；目前没有测试对 `Transitions.cs` 的数值做快照/断言。
3. **羽化透视原点** —— 原版 `TurnstileFeatherEffect` 会把每个元素的透视原点移到页面垂直中心（`GlobalOffsetY` + 补偿 RenderTransform）。本移植版因 `ProjectionHost` 与元素同尺寸且裁剪 3D 内容，让元素绕自身中心旋转；角度/延迟/时序与原版一致。已写入已知限制。
4. **Shell chrome** —— `SystemTray` 与 `ApplicationBar` 是 API 模型；默认 Frame 模板不绘制 WP 状态栏或应用栏，也没有视觉验证。
5. **未实现模块** —— `AutoCompleteBox`、`LongListSelector`、`LongListMultiSelector`、`MultiselectList`、地图扩展、`TransferControl`、`SpeechTextBox`、`LockablePivot`、`SlideInEffect`、`GestureListener`/`GestureService`、`LoopingSelector`/picker 页面。已通过源码检索确认不存在（无对应 `class`）。
6. **示例覆盖缺口** —— 示例展示了投影、turnstile/feather 导航、TiltEffect、WrapPanel、ToggleSwitch、Rating、PhoneTextBox、ListPicker、RecurringDaysPicker、ExpanderView、DatePicker、TimePicker、PerformanceProgressBar、RelativeAnimatingContentControl、HubTile、ContextMenu、CustomMessageBox；**未**展示 `Matrix3DProjection`、`SystemTray` 视觉、`ApplicationBar` 视觉。
7. **未做像素级验证的视觉细节** —— ListPicker 弹出层样式是否与 WP 一致、ExpanderView 高度动画、HubTile 计时、PhoneTextBox 水印动画：行为有测试，像素保真度没有。

---

## 6. 阻塞与风险

1. **尚未提交。** 最近的修复只存在于工作区（分支 `main`，21 个修改 + 8 个新增文件）。任何破坏性 git 操作都会丢失它们。见 §10。
2. **视觉验证缺口。** 缺少目视手段时，保真度结论依赖几何/时序测量与原 XAML。后续最好能并排查看示例与模拟器。
3. **`docs/*/porting-inventory.md` 是 Phase 0 的大型审计文档**（英文版约 129 KB）。其中的模块状态是历史快照；当前状态以 `docs/porting-status.md` 为准。
4. **环境坑** —— `XDE.exe` 会卡死（用 `vmconnect.exe`）；`PrintWindow` 对 3D/VM 表面返回全黑（改用 `Topmost` 窗口 + `CopyFromScreen`）；显示为 150% DPI（注意 DIP↔像素换算）；示例运行时锁定 `bin` 输出，重建前先结束进程。
5. **羽化保真风险** —— 若严格要求一致，需要把羽化宿主改成“页面尺寸的 3D 场景”，属于设计级改动而非补丁。

---

## 7. 重要文件

| 路径 | 作用 |
|---|---|
| `src/Community.Phone.Controls.Toolkit.WPF/Projection/ProjectionHost.cs` | 2D↔3D 宿主、相机（`PerspectiveDepth`）、`Prefer3D`、场景更新 |
| `…/Projection/PlaneProjection.cs` | `ToMatrix3D`（符号约定、行向量顺序） |
| `…/Projection/ProjectionService.cs` | 附加投影、`EnsureProjectionHost` / `FindDirectHost`、宿主包裹/解包 |
| `…/Projection/ProjectionMath.cs` | 旋转/平移辅助 |
| `…/Transitions/TransitionFrame.cs` | presenter 翻转、串行时序、准备/复位、失败回退 |
| `…/Transitions/Transitions.cs` | 代码形式的 19 个 storyboard（数值为权威） |
| `…/Transitions/Transitions/*.cs` | 各转场族的 `TransitionElement` |
| `…/Effects/TiltEffect.cs` | 元素级倾斜、加载时包裹、变换缓存 |
| `…/Effects/TurnstileFeatherEffect.cs` | 逐元素羽化 |
| `…/Themes/Generic.xaml` | 唯一主题源（Metro 调色板、模板、WPF 基础控件 WP 样式） |
| `src/…Sample/` | 可运行示例与手工验证入口 |
| `tests/Community.Phone.Controls.Toolkit.WPF.Tests/` | xUnit 套件（WPF 用 `StaTest`）；internal 对测试可见 |
| `build/verify-package.ps1` | 打包 Release + 全新消费冒烟 |
| `docs/en/` 与 `docs/zh-CN/` —— architecture、api-compatibility、known-limitations、migration-guide、porting-inventory | 双语正式文档 |
| `docs/porting-status.md` | 当前实现/验证状态（权威） |
| `.github/Guide.md` | 原始任务提示（历史，不代表当前状态） |

---

## 8. 建议的下一步（按顺序）

1. **提交工作区**（目前什么都没提交）：`git add -A`，提交信息概括转场/投影保真修复与倾斜宿主修复，并带 `Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>` 尾注。
2. **在模拟器里做目视核对**（Virtual Machine Connection）：示例 → Detail（turnstile）、Feather 页、"Animate projection"、以及 Slide/Rotate/Swivel/Roll（临时改页面 `TransitionService` 的族），对照 WP 系统转场检查方向、透视与时长。
3. **增加 storyboard 一致性测试**（internal 对测试程序集可见）：按族断言 `Transitions` 的关键帧数值/时长，防止偏离原 XAML。
4. **决定 `Prefer3D` 的生命周期**：目前被丢弃的旧 presenter 会回到 2D，新 presenter 在动画完成后回到 2D。若将来出现“转场结束时文字突然变清晰”的跳变，可考虑把 `Prefer3D` 保持到下一次导航。
5. **可选：羽化保真** —— 用页面尺寸的 3D 宿主恢复原始透视原点；这是设计改动，不是补丁。
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
- 已有提交：`83d5d73 fix: 0.01`、`81a55d2 feat: 0.0.1`。
- 交接时工作区：**21 个修改 + 8 个新增文件，全部未提交。**

**已修改（不可丢失）：**

```
README.md
README_zh-CN.md
docs/en/README.md
docs/en/api-compatibility.md
docs/en/architecture.md
docs/en/known-limitations.md
docs/en/porting-inventory.md
docs/zh-CN/README.md
docs/zh-CN/api-compatibility.md
docs/zh-CN/architecture.md
docs/zh-CN/known-limitations.md
docs/zh-CN/porting-inventory.md
src/Community.Phone.Controls.Toolkit.WPF/Effects/TiltEffect.cs
src/Community.Phone.Controls.Toolkit.WPF/Effects/TurnstileFeatherEffect.cs
src/Community.Phone.Controls.Toolkit.WPF/Projection/PlaneProjection.cs
src/Community.Phone.Controls.Toolkit.WPF/Projection/ProjectionHost.cs
src/Community.Phone.Controls.Toolkit.WPF/Projection/ProjectionService.cs
src/Community.Phone.Controls.Toolkit.WPF/Transitions/TransitionFrame.cs
tests/Community.Phone.Controls.Toolkit.WPF.Tests/EffectsTests.cs
tests/Community.Phone.Controls.Toolkit.WPF.Tests/NavigationTests.cs
tests/Community.Phone.Controls.Toolkit.WPF.Tests/ProjectionTests.cs
```

**新增（未跟踪）：**

```
.github/copilot-instructions.md
docs/architecture.md
docs/api-compatibility.md
docs/porting-inventory.md
docs/porting-status.md
docs/handoff.md
docs/zh-CN/porting-status.md
docs/zh-CN/handoff.md
```

- **不要**执行 `git reset --hard`、`git checkout -- .`、`git clean -fd` 或强推。
- 建议的第一次提交应包含以上全部内容（源码 + 测试 + 文档）；工作区里没有任何可丢弃的东西。

---

## 11. 长期开发规则摘要（中文）

完整规则见 `.github/copilot-instructions.md`（Copilot 只读取英文单文件，此处为中文摘要）。

- 保持原始公共 API 形状；移植新增 API 必须是**加法**并写入兼容性文档。
- 不得虚报：未实现/未验证的内容必须显式标注；未实现模块继续留在已知限制中。
- 转场数值只能来自原 storyboard XAML，不得自行调参。
- 投影：`PerspectiveDepth = 999`、相机在 `z = depth`、水平 FOV 由视口**宽度**推导；`R = Rz·Ry·Rx`、Y 偏移取反、`RotationY` 取反。
- 动画/效果切换 2D↔3D 必须在首帧之前（`Prefer3D`）；不要在转场/效果宿主上使用 `BitmapCache`。
- 元素级效果必须使用**直接包含元素**的宿主。
- `TransitionFrame` 模板部件是两个 `ProjectionHost`（`FirstContentPresenter`/`SecondContentPresenter`），退场先于入场。
- 构建 0 警告 0 错误；测试全绿；每个 bug 修复都要有回归测试；改动公共 API 后跑 `build\verify-package.ps1`。
- 文档双语同步（`docs/en` + `docs/zh-CN`，根目录用 `_zh-CN` 后缀）。
- 模拟器只用 Virtual Machine Connection，不用 `XDE.exe`。
