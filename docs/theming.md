# 主题系统

主题由三部分组成：单一调色板字典 `Themes/Theme.xaml`、C# 侧的配色描述 `ThemePalette`、运行时切色的 `ThemeManager`。切换主题或强调色时只改颜色值并播放颜色动画，不重建窗口、页面或控件。

## 1. 组成

| 文件 | 作用 |
|---|---|
| `src/Community.Phone/Themes/Theme.xaml` | 唯一主题字典：`Phone*Color` 颜色、`Phone*Brush` 画刷、字体/字号/厚度、文本样式，以及 `PhoneDarkTheme*` / `PhoneLightTheme*` 的 Visibility 与 Opacity |
| `src/Community.Phone/Themes/Generic.xaml` | 合并 `Theme.xaml` 与 `Controls/{Button,LongListSelector,PhoneApplicationFrame,Pivot}.xaml` |
| `src/Community.Phone/Shell/ThemePalette.cs` | `ThemePalette` 记录：24 个颜色属性 + 5 个非颜色值，`Dark` / `Light` 两个静态实例、`WithAccent`、`GetValue(key)`、`ResourceKeys` |
| `src/Community.Phone/Shell/ThemeManager.cs` | `ThemeManager` 静态类：应用调色板、定位字典、系统主题/强调色读取、`ThemeChanged` |
| `src/Community.Phone/Shell/AccentColor.cs` | `AccentColor` 枚举（`Auto` + 20 个 Metro 强调色）与 `AccentColors` 颜色表 |
| `src/Community.Phone/Shell/ApplicationTheme.cs` | `ApplicationTheme` 枚举（`Auto` / `Light` / `Dark`） |

`ThemePalette` 只描述颜色，不持有画刷；画刷实例由 `Theme.xaml` 声明，`ThemeManager` 负责把它们更新到新的调色板。

## 2. 调色板

| `ThemePalette` 属性 | 资源键 | Dark | Light |
|---|---|---|---|
| `PhoneBackgroundColor` | `PhoneBackgroundColor` | `#FF000000` | `#FFFFFFFF` |
| `PhoneForegroundColor` | `PhoneForegroundColor` | `#FFFFFFFF` | `#DE000000` |
| `PhoneAccentColor` | `PhoneAccentColor` | `#FF0050EF`（可被强调色覆盖） | 同左 |
| `PhoneChromeColor` | `PhoneChromeColor` | `#FF1F1F1F` | `#FFDDDDDD` |
| `PhoneBorderColor` | `PhoneBorderColor` | `#BFFFFFFF` | `#99000000` |
| `PhoneContrastForegroundColor` | `PhoneContrastForegroundColor` | `#FF000000` | `#FFFFFFFF` |
| `PhoneContrastBackgroundColor` | `PhoneContrastBackgroundColor` | `#FFFFFFFF` | `#DE000000` |
| `PhoneDisabledColor` | `PhoneDisabledColor` | `#66FFFFFF` | `#4D000000` |
| `PhoneProgressBarBackgroundColor` | `PhoneProgressBarBackgroundColor` | `#19FFFFFF` | `#19000000` |
| `PhoneTextCaretColor` | `PhoneTextCaretColor` | `#FF000000` | `#DE000000` |
| `PhoneTextBoxColor` | `PhoneTextBoxColor` | `#BFFFFFFF` | `#26000000` |
| `PhoneTextBoxForegroundColor` | `PhoneTextBoxForegroundColor` | `#FF000000` | `#DE000000` |
| `PhoneTextBoxEditBackgroundColor` | `PhoneTextBoxEditBackgroundColor` | `#FFFFFFFF` | `#00000000` |
| `PhoneTextBoxReadOnlyColor` | `PhoneTextBoxReadOnlyColor` | `#77000000` | `#2E000000` |
| `PhoneTextBoxSelectionForegroundColor` | `PhoneTextBoxSelectionForegroundColor` | `#FFFFFFFF` | `#FFFFFFFF` |
| `PhoneSubtleColor` | `PhoneSubtleColor` | `#99FFFFFF` | `#66000000` |
| `PhoneButtonBasePressedForegroundColor` | `PhoneButtonBasePressedForegroundColor` | `#FFFFFFFF` | `#FFFFFFFF` |
| `PhoneTextHighContrastColor` | `PhoneTextHighContrastColor` | `#FFFFFFFF` | `#DE000000` |
| `PhoneTextMidContrastColor` | `PhoneTextMidContrastColor` | `#99FFFFFF` | `#73000000` |
| `PhoneTextLowContrastColor` | `PhoneTextLowContrastColor` | `#73FFFFFF` | `#40000000` |
| `PhoneSemitransparentColor` | `PhoneSemitransparentColor` | `#AA000000` | `#AAFFFFFF` |
| `PhoneInactiveColor` | `PhoneInactiveColor` | `#33FFFFFF` | `#33000000` |
| `PhoneInverseInactiveColor` | `PhoneInverseInactiveColor` | `#FFCCCCCC` | `#FFE5E5E5` |
| `PhoneInverseBackgroundColor` | `PhoneInverseBackgroundColor` | `#FFFFFFFF` | `#FFDDDDDD` |

非颜色值（不可动画）：

| 资源键 | 取值 |
|---|---|
| `PhoneDarkThemeVisibility` / `PhoneLightThemeVisibility` | `IsDark ? Visible/Collapsed : Collapsed/Visible` |
| `PhoneDarkThemeOpacity` / `PhoneLightThemeOpacity` | `IsDark ? 1/0 : 0/1` |
| `PhonePivotUnselectedItemOpacity` | Dark `0.4`，Light `0.23144` |

## 3. 资源映射

`ThemeManager` 遍历 `ThemePalette.ResourceKeys`（58 个键）：颜色键与画刷键都映射到调色板的同一组属性，因此按 `Phone*Color` 或 `Phone*Brush` 取值的代码都能拿到新主题。

画刷分组：

| 调色板属性 | 画刷资源 |
|---|---|
| `PhoneForegroundColor` | `PhoneForegroundBrush`、`PhoneRadioCheckBoxBorderBrush`、`PhoneRadioCheckBoxCheckBrush` |
| `PhoneAccentColor` | `PhoneAccentBrush`、`PhoneTextBoxEditBorderBrush`、`PhoneRadioCheckBoxPressedBrush` |
| `PhoneContrastForegroundColor` / `PhoneContrastBackgroundColor` | `PhoneContrastForegroundBrush` / `PhoneContrastBackgroundBrush` |
| `PhoneTextBoxColor` / `PhoneTextBoxForegroundColor` / `PhoneTextBoxEditBackgroundColor` / `PhoneTextBoxReadOnlyColor` / `PhoneTextBoxSelectionForegroundColor` | `PhoneTextBoxBrush` / `PhoneTextBoxForegroundBrush` / `PhoneTextBoxEditBackgroundBrush` / `PhoneTextBoxReadOnlyBrush` / `PhoneTextBoxSelectionForegroundBrush` |
| `PhoneTextHighContrastColor` / `Mid` / `Low` | `PhoneTextHighContrastBrush` / `PhoneTextMidContrastBrush` / `PhoneTextLowContrastBrush` |
| 其余（背景、chrome、border、disabled、progress、caret、subtle、semitransparent、inactive、inverse） | 同名 `Phone*Brush` |

`PhoneRadioCheckBoxBrush` 与 `TransparentBrush` 两套主题取值相同，不参与映射。

## 4. 公开 API

```csharp
using Microsoft.Phone.Shell;

ThemeManager.AvailableApplicationThemes;   // Auto / Light / Dark
ThemeManager.AvailableAccentColors;        // Auto + 20 个强调色
ThemeManager.ActualTheme;                  // Auto 解析为当前 Windows 应用主题
ThemeManager.ActualAccentColor;            // Auto 解析为系统强调色（忽略 CustomAccentColor）
ThemeManager.ThemeChanged += (s, e) => { }; // ThemeChangedEventArgs(Theme, AccentColor)

this.InitializeTheme();                          // 应用当前主题（不播放动画）
Application.Current.RequestedTheme = ApplicationTheme.Light;
Application.Current.AccentColor = AccentColor.Cobalt;
Application.Current.CustomAccentColor = Colors.Orange;   // 优先于 AccentColor，null 清除
```

`RequestedTheme`、`AccentColor`、`CustomAccentColor`、`InitializeTheme()` 是 `Application` 的 C# 14 扩展成员，值变化时立即应用调色板。

最终强调色为 `CustomAccentColor ?? ActualAccentColor`，`ThemeChanged` 报告的是最终生效的 `Theme` 与 `Color`，且只在生效值变化时触发。

## 5. 应用流程

`ThemeManager.ApplyTheme` 按以下顺序执行：

1. 校验在 UI 线程（否则用 `Dispatcher.BeginInvoke` 重新调度）。
2. `ResolvePalette()`：按 `ActualTheme` 取 `ThemePalette.Light` / `Dark`，再用 `WithAccent(customAccentColor ?? ActualAccentColor)`。
3. 定位字典：从 `Application.Resources`（或传入的 `FrameworkElement.Resources`）递归扫描合并树，找 `Source` 含 `/Community.Phone;component/Themes/Theme.xaml` 的字典。**找不到时抛 `Exception("Theme dictionary not found.")`** —— 应用必须先在 `App.xaml` 中合并主题字典。
4. 遍历 `ResourceKeys` 写入新值。
5. 若生效主题或强调色变化，触发 `ThemeChanged`。

系统主题跟随：静态构造函数订阅 `SystemEvents.UserPreferenceChanged`；当 `RequestedTheme == Auto` 或 `AccentColor == Auto` 且类别为 `General`/`VisualStyle`/`Color` 时重新应用。`GetSystemTheme()` 读注册表 `Software\Microsoft\Windows\CurrentVersion\Themes\Personalize\AppsUseLightTheme`（0 为深色，读取失败时回退深色）；`GetSystemAccentColor()` 读 `SOFTWARE\Microsoft\Windows\DWM\ColorizationColor`（失败时回退 `AccentColors.Cobalt`）。

## 6. 颜色动画

1. **可变副本**：WPF 会把 `Application.Resources`（含合并字典）中的 `Freezable` 强制冻结。首次应用时若目标画刷 `IsFrozen`，创建一个同色可变副本，用 1 ms `HoldEnd` 动画“护卫”后写入字典（正在动画的 `Freezable` 不会被冻结），随后撤销护卫；该实例此后伴随应用生命周期。
2. **用动画持有目标色**：`ColorAnimation(from, to, duration) { FillBehavior = HoldEnd }` 以 `HandoffBehavior.SnapshotAndReplace` 启动，时间线一直持有目标色（画刷的基础值保持旧色）。`from` 读取画刷当前**有效**颜色（含正在运行的动画），因此快速连续切换会从画面上的中间色继续，不跳变、不叠加。
3. **同步启动**：先收集全部 `(brush, from, to)` 再统一 `BeginAnimation`，同一帧内启动。
4. **不播放动画的情况**：`InitializeTheme()`（首次应用）与值为 `Color` 以外的资源改动直接赋值；动画时长为 `ThemeManager.ThemeTransitionDuration`（内部常量 300 ms）。
5. **失败兜底**：键不存在、值不是 `SolidColorBrush` 时跳过该键，不抛异常。

## 7. 消费方规则

1. **必须合并主题字典**：`InitializeTheme()` 不会自己创建字典；没有合并时抛 `Theme dictionary not found.`。示例在 `App.xaml` 中合并 `/Community.Phone.Controls.Toolkit;component/Themes/Generic.xaml`（它间接合并 `Community.Phone` 的字典）。
2. **主题画刷用 `DynamicResource`**：库内模板已全部使用 `DynamicResource`；应用 XAML 中用 `StaticResource` 引用的画刷会保留解析时的（冻结的）占位值，不跟随切换。`InitializeTheme()` 之后解析的页面即使使用 `StaticResource` 也能拿到接管后的可变实例。
3. **关键帧里的画刷是快照**：`DiscreteObjectKeyFrame.Value` 若是 `Freezable` 会被冻结，切换主题后需要重新进入该视觉状态才会取新色，也不参与渐变。
4. **非颜色资源瞬时切换**：`Phone*ThemeVisibility`、`Phone*ThemeOpacity`、`PhonePivotUnselectedItemOpacity` 不是 `DependencyObject`，无法动画，`StaticResource` 消费者不会更新；`Phone*Color` 同样瞬时切换，只有画刷有过渡。
5. **不要在应用级资源外复制调色板**：`ThemeManager` 只更新它定位到的那一份 `Theme.xaml` 字典；自行复制一份画刷不会跟随切换。
6. 未覆盖：高对比度系统主题、`SystemParameters` 系统画刷、`PhoneFontFamily*` 字体回退。

## 8. 自建页面的注意事项

由库自己创建的页面（`ListPickerPage`）在挂到 frame 之前就解析样式，看不到 `Application.Resources`，也不会走应用级主题切换：

- 页面在自身 `Resources` 中合并 `/Community.Phone;component/Themes/Theme.xaml` 携带颜色。
- 尚未挂树时求值的属性（`FontFamily`、`FontSize`、`Foreground`、背景画刷）使用 `DynamicResource`。
- 模板 storyboard 里的关键帧值使用 `StaticResource` 或内联画刷：这种场景下关键帧不能使用 `DynamicResource`，`StaticResource` 也只在所属字典内解析。
