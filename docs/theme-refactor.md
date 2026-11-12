# 主题重构：单一 `Theme.xaml` + `ThemePalette` + 动态渐变切换

本文记录 `Microsoft.Phone.Shell.ThemeManager` 的重构结果：一次加载的主题资源字典、C# 配色定义、以及运行时就地改色 + 颜色渐变动画。

## 1. 修改的文件清单

| 文件 | 变更 |
| --- | --- |
| `src/Community.Phone/Themes/Theme.xaml` | **新增**。唯一主题资源字典：24 个 `Color`、30 个 `SolidColorBrush`、字体/字号/厚度/样式，以及主题相关的 Visibility/Opacity。 |
| `src/Community.Phone/Themes/LightTheme.xaml` | **删除**（内容并入 `Theme.xaml`，浅色值移入 `ThemePalette.Light`）。 |
| `src/Community.Phone/Themes/DarkTheme.xaml` | **删除**（同上，深色值移入 `ThemePalette.Dark`）。 |
| `src/Community.Phone/Themes/Generic.xaml` | 合并的第一份字典由 `DarkTheme.xaml` 改为 `Theme.xaml`。 |
| `src/Community.Phone/Shell/ThemePalette.cs` | **新增**。配色方案类型 + 内置 Dark/Light 两套 + `WithAccent`。 |
| `src/Community.Phone/Shell/ThemeManager.cs` | 重写。进程内唯一字典、映射表、`ApplyThemeWithAnimation`、动画/线程/多窗口处理；公开 API 不变。 |
| `src/Community.Phone.Controls.Toolkit/Themes/Generic.xaml` | `PhoneTextBox` 的 `Focused` 关键帧 2 处 `StaticResource` → `DynamicResource`。 |
| `samples/.../App.xaml` | `PhoneButtonBase` 模板的 5 处关键帧 `StaticResource` → `DynamicResource`。 |
| `samples/.../Settings.xaml` | 页级 `Foreground` 改为 `DynamicResource`。 |
| `samples/.../Settings.xaml.cs` | 修复编译错误（缺失 `;`），并补齐主题/强调色选择器的接线，使该页成为可手动验证的开关。 |

## 2. `ThemePalette`

`ThemePalette`（`src/Community.Phone/Shell/ThemePalette.cs`）用 `Color` 描述一整套配色，**不持有任何画刷**：

- 24 个颜色属性，与 `Theme.xaml` 中的 `Phone*Color` 资源一一对应；
- `IsDark`：驱动 `PhoneDarkThemeVisibility` / `PhoneLightThemeVisibility` 与对应的 opacity；
- `PivotUnselectedItemOpacity`：明暗主题取值本身不同（0.4 / 0.23144），属于配色的一部分；
- `WithAccent(Color)`：返回副本（强调色与基础色分开管理，切换强调色不重建调色板）；
- `Dark` / `Light` 两个静态只读实例。

### 配色表

| ThemePalette | 对应 Color 资源 | Dark | Light |
| --- | --- | --- | --- |
| `Background` | `PhoneBackgroundColor` | `#FF000000` | `#FFFFFFFF` |
| `Foreground` | `PhoneForegroundColor` | `#FFFFFFFF` | `#DE000000` |
| `Accent` | `PhoneAccentColor` | `#FF0050EF` | `#FF0050EF` |
| `Chrome` | `PhoneChromeColor` | `#FF1F1F1F` | `#FFDDDDDD` |
| `Border` | `PhoneBorderColor` | `#BFFFFFFF` | `#99000000` |
| `ContrastForeground` | `PhoneContrastForegroundColor` | `#FF000000` | `#FFFFFFFF` |
| `ContrastBackground` | `PhoneContrastBackgroundColor` | `#FFFFFFFF` | `#DE000000` |
| `Disabled` | `PhoneDisabledColor` | `#66FFFFFF` | `#4D000000` |
| `ProgressBarBackground` | `PhoneProgressBarBackgroundColor` | `#19FFFFFF` | `#19000000` |
| `TextCaret` | `PhoneTextCaretColor` | `#FF000000` | `#DE000000` |
| `TextBox` | `PhoneTextBoxColor` | `#BFFFFFFF` | `#26000000` |
| `TextBoxForeground` | `PhoneTextBoxForegroundColor` | `#FF000000` | `#DE000000` |
| `TextBoxEditBackground` | `PhoneTextBoxEditBackgroundColor` | `#FFFFFFFF` | `#00000000` |
| `TextBoxReadOnly` | `PhoneTextBoxReadOnlyColor` | `#77000000` | `#2E000000` |
| `TextBoxSelectionForeground` | `PhoneTextBoxSelectionForegroundColor` | `#FFFFFFFF` | `#FFFFFFFF` |
| `Subtle` | `PhoneSubtleColor` | `#99FFFFFF` | `#66000000` |
| `ButtonBasePressedForeground` | `PhoneButtonBasePressedForegroundColor` | `#FFFFFFFF` | `#FFFFFFFF` |
| `TextHighContrast` | `PhoneTextHighContrastColor` | `#FFFFFFFF` | `#DE000000` |
| `TextMidContrast` | `PhoneTextMidContrastColor` | `#99FFFFFF` | `#73000000` |
| `TextLowContrast` | `PhoneTextLowContrastColor` | `#73FFFFFF` | `#40000000` |
| `Semitransparent` | `PhoneSemitransparentColor` | `#AA000000` | `#AAFFFFFF` |
| `Inactive` | `PhoneInactiveColor` | `#33FFFFFF` | `#33000000` |
| `InverseInactive` | `PhoneInverseInactiveColor` | `#FFCCCCCC` | `#FFE5E5E5` |
| `InverseBackground` | `PhoneInverseBackgroundColor` | `#FFFFFFFF` | `#FFDDDDDD` |

强调色最终取值：`customAccentColor ?? ActualAccentColor`。`ActualAccentColor` 语义保持不变（`Auto` 时读系统强调色），`ThemeChanged` 报告的则是最终生效值。

## 3. `Theme.xaml`

只保留一份，`Themes/Generic.xaml` 中作为第一份合并字典，保证 `BasedOn="{StaticResource PhoneTextBlockBase}"` 这类跨字典静态查找仍然成立。内容为原两套字典的并集（两者除颜色、Visibility、Opacity、`PhonePivotUnselectedItemOpacity` 外完全一致），初始值取深色配色，使未调用 `InitializeTheme()` 时表现与旧版本一致。

注意：`Theme.xaml` 里声明的画刷**会被 WPF 冻结**（见第 7 节），它们的作用是"保证 key 永远可解析 + 提供初始值"，真正参与动画的是 `ThemeManager` 首次应用时接管的可变副本。

## 4. `ThemeManager`

公开 API 全部保留：`AvailableAccentColors`、`ThemeChanged`、`ActualTheme`、`ActualAccentColor`，以及 `Application` 扩展属性 `RequestedTheme`、`AccentColor`、`CustomAccentColor`、`InitializeTheme()`。新增：

- `ThemeTransitionDuration`（默认 300ms，`TimeSpan.Zero` 等价于关闭动画）；
- `EnableThemeTransitions`（全局开关）。

内部流程：`ApplyTheme` →（Dispatcher 校验）→ `ResolvePalette()` → `ApplyThemeWithAnimation(application, palette, animated)` → ① 定位/按需加载唯一字典 ② 非画刷资源 ③ Color 资源 ④ 画刷改色动画 → 触发 `ThemeChanged`。

字典定位：递归扫描 `Application.Resources` 与各 `Window.Resources` 的合并树，收集所有 `Source` 含 `/Themes/Theme.xaml` 的字典；一个都没有时才加载一次并插入 `Application.Resources.MergedDictionaries[0]`。除此之外**从不修改** `MergedDictionaries`，也不清空任何用户字典。

## 5. 主题资源映射表

### 画刷（`BrushMappings`，28 项）

| 调色板属性 | 画刷资源 |
| --- | --- |
| `Background` | `PhoneBackgroundBrush` |
| `Foreground` | `PhoneForegroundBrush`、`PhoneRadioCheckBoxBorderBrush`、`PhoneRadioCheckBoxCheckBrush` |
| `Accent` | `PhoneAccentBrush`、`PhoneTextBoxEditBorderBrush`、`PhoneRadioCheckBoxPressedBrush` |
| `Chrome` | `PhoneChromeBrush` |
| `Border` | `PhoneBorderBrush` |
| `ContrastForeground` / `ContrastBackground` | `PhoneContrastForegroundBrush` / `PhoneContrastBackgroundBrush` |
| `Disabled` | `PhoneDisabledBrush` |
| `ProgressBarBackground` | `PhoneProgressBarBackgroundBrush` |
| `Subtle` | `PhoneSubtleBrush` |
| `Semitransparent` | `PhoneSemitransparentBrush` |
| `TextCaret` | `PhoneTextCaretBrush` |
| `TextBox` / `TextBoxForeground` / `TextBoxEditBackground` | `PhoneTextBoxBrush` / `PhoneTextBoxForegroundBrush` / `PhoneTextBoxEditBackgroundBrush` |
| `TextBoxReadOnly` / `TextBoxSelectionForeground` | `PhoneTextBoxReadOnlyBrush` / `PhoneTextBoxSelectionForegroundBrush` |
| `ButtonBasePressedForeground` | `PhoneButtonBasePressedForegroundBrush` |
| `TextHighContrast` / `Mid` / `Low` | `PhoneTextHighContrastBrush` / `PhoneTextMidContrastBrush` / `PhoneTextLowContrastBrush` |
| `Inactive` / `InverseInactive` / `InverseBackground` | `PhoneInactiveBrush` / `PhoneInverseInactiveBrush` / `PhoneInverseBackgroundBrush` |

不参与映射：`PhoneRadioCheckBoxBrush`、`TransparentBrush`（两套主题均为 `Transparent`，保持原值）。

### 非画刷（`ValueMappings`，5 项，不可动画）

| 资源 | 取值 |
| --- | --- |
| `PhoneDarkThemeVisibility` / `PhoneLightThemeVisibility` | `IsDark ? Visible/Collapsed : Collapsed/Visible` |
| `PhoneDarkThemeOpacity` / `PhoneLightThemeOpacity` | `IsDark ? 1/0 : 0/1` |
| `PhonePivotUnselectedItemOpacity` | `palette.PivotUnselectedItemOpacity` |

`ColorMappings`（24 项）与画刷映射使用同一组调色板属性，同步更新 `Phone*Color`，保证按 `Color` key 取值的代码不落后于画刷。

## 6. `DynamicResource` 修改清单

| 文件 | 处数 | 内容 |
| --- | --- | --- |
| `src/Community.Phone/Themes/Theme.xaml` | 11 | 所有主题画刷的 `Style` Setter（原为 `StaticResource`）；`EmptyListHeaderStyle` 的 `Foreground` 由错误的 `PhoneTextMidContrastColor`（类型不匹配）改为 `PhoneTextMidContrastBrush` |
| `src/Community.Phone.Controls.Toolkit/Themes/Generic.xaml` | 2 | `PhoneTextBox` 模板 `FocusStates/Focused` 的 `DiscreteObjectKeyFrame` |
| `samples/.../App.xaml` | 5 | `PhoneButtonBase` 模板 `Pressed`/`Disabled` 的 `DiscreteObjectKeyFrame` |
| `samples/.../Settings.xaml` | 1 | 页级 `Foreground` |

保持 `StaticResource` 的部分：字体名、字号、`Thickness`、`Style`/`BasedOn` 等非颜色资源（不需要动态变化）。

## 7. 动画实现原理

1. **持久实例**：首次应用时，对每个映射 key 检查字典里的画刷。若 `IsFrozen`，创建一个同色可变副本，用一个 1ms 的 `HoldEnd` 动画"护卫"后写入字典（正在动画的 `Freezable` 不能被冻结），随后撤销护卫。此后该实例伴随应用整个生命周期。
2. **先定基色，再放动画**：`brush.Color = to`（基础值即最终目标），再用 `ColorAnimation(from, to, ThemeTransitionDuration) { FillBehavior = Stop }` + `HandoffBehavior.SnapshotAndReplace`。动画结束时时钟被丢弃，画刷回落到基础值——目标色。因此即使动画被打断，颜色也**不会**回到旧主题。
3. **插值起点**：`brush.Color` 读取的是当前**有效值**（含正在运行的动画），所以快速连续切换时新动画从画面上的中间色继续，不跳变、不叠加。
4. **同步启动**：先收集全部 `(brush, from, to)`，再统一 `BeginAnimation`，同一帧内启动，视觉上同步。
5. **关闭动画**：`EnableThemeTransitions == false` 或 `ThemeTransitionDuration == TimeSpan.Zero` 时，清掉动画并直接赋色；首次 `InitializeTheme()` 也不做动画（`hasApplied == false`）。
6. **失败兜底**：key 不存在、值不是 `SolidColorBrush`、画刷无法解冻时跳过该 key，绝不抛异常。

### 关于"冻结"（重要）

WPF 会把 `Application.Resources`（含其合并字典）中的 `Freezable` 强制冻结——应用级资源被称为可跨线程访问，必须不可变，`po:Freeze="False"` 也无法阻止。因此"XAML 里声明可变画刷"在 WPF 中不可行，第一次应用时的"接管 + 护卫"步骤是必需的：它把 XAML 里的冻结占位画刷换成可变实例，且只做一次。

## 8. 兼容性风险及未处理的资源类型

1. **`App.xaml` 解析期捕获的 `StaticResource` 画刷不会跟随切换。** 这是冻结机制的直接后果：`App.xaml` 在 `InitializeTheme()` 之前解析，静态引用拿到的是冻结占位画刷。库内与示例已全部改为 `DynamicResource`；第三方应用 XAML 若仍用 `StaticResource` 引用主题画刷，需改为 `DynamicResource`。`InitializeTheme()` 之后解析的页面/控件即使使用 `StaticResource` 也能正常工作（拿到的是接管后的可变实例）。
2. **关键帧里的画刷是快照。** `DiscreteObjectKeyFrame.Value` 若是 `Freezable` 会被冻结，所以进入某个视觉状态时拿到的是当时的颜色副本，切换主题后该状态需要重新进入（离开再进入）才会取新色，也不会参与渐变。改为 `DynamicResource` 后，至少在每次进入状态时重新解析，比原先固定为解析期颜色更好。
3. **`Theme.xaml` 中的画刷本身在应用级字典里始终是冻结的**（占位用）。若业务代码绕过 `ThemeManager` 直接取 `Application.Current.Resources["PhoneAccentBrush"]`，在 `InitializeTheme()` 之后拿到的是可变实例（如 `JumpListItemBackgroundConverter` 那样的构造函数缓存仍然有效）；但在 `InitializeTheme()` 之前取到的会是冻结占位。
4. **非颜色主题差异**：`PhoneDarkThemeVisibility`、`PhoneLightThemeVisibility`、`PhoneDarkThemeOpacity`、`PhoneLightThemeOpacity`、`PhonePivotUnselectedItemOpacity` 已按主题赋值，但只能是瞬时切换（非 `DependencyObject`，无法动画），且 `StaticResource` 消费者不会更新。
5. **`Phone*Color` 资源是瞬时切换**，不参与渐变；只有画刷有过渡。
6. **`LightTheme.xaml` / `DarkTheme.xaml` 已删除**：外部若直接按 URI 合并这两个文件会得到 `IOException`。这是"单一主题文件"的必然结果，升级时请改为合并 `Themes/Theme.xaml`（推荐）或继续合并 `Themes/Generic.xaml`。
7. **`ThemeChanged` 只在生效值发生变化时触发**（首次应用后）。系统主题事件的重复通知不会造成事件风暴。
8. 未处理：高对比度系统主题、`SystemParameters` 相关的系统画刷、`PhoneFontFamily*` 之类的字体回退（与本次主题切换无关）。

## 9. 编译与测试结果

```
dotnet build Community.Phone.Controls.Toolkit.slnx -t:Rebuild
    1450 个警告
    0 个错误
```

警告数与改动前一致（均为既有可空性告警），新增/修改文件无新增告警。

运行期验证（临时 WPF 夹具，独立进程，59 项断言全部通过）：字典只合并一次、切换前后 `MergedDictionaries` 与字典实例不变、浅/深色各画刷与调色板一致、`DynamicResource` 与初始化后 `StaticResource` 消费者实时跟随、动画中途处于中间色、快速连续切换 5 次后所有画刷落到目标色且实例未重建、关闭动画/零时长同步生效、`CustomAccentColor` 只影响强调色别名、第二个窗口共享同一套画刷、关闭窗口后主题资源仍有效、`Auto` 跟随系统主题、`PhoneTextBox` 模板关键帧在重新进入状态后取到新主题色。示例程序可正常启动并显示主窗口。

夹具位于仓库外的临时目录，未纳入版本控制。
