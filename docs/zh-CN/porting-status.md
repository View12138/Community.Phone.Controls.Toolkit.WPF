# 移植状态 — Community.Phone.Controls.Toolkit.WPF

[English](../porting-status.md) · 中文

**本文档是什么。** 按模块记录的**当前**实现/验证状态，是本项目的权威状态表。内容来自源码树、测试套件和实测运行，而不是原始计划。Phase 0 审计（`docs/*/porting-inventory.md`）记录的是原版工具包**有什么**，不代表本移植版**现在能做什么**。

**状态定义**

| 状态 | 含义 |
|---|---|
| ✅ **已验证** | 已实现，且有自动化测试或实测数据（会引用具体数字）。 |
| 🟡 **已实现、未验证** | 已实现且示例或测试可达，但至少有一个方面（通常是像素保真度或时序）没有证据。 |
| ⛔ **未实现** | 没有兼容实现，不得声称可用。 |

**证据快照（2026-10-19）：** Release 构建 0 警告 / 0 错误 · Debug 与 Release 均 68/68 测试通过 · `build\verify-package.ps1` 通过 · 示例 UI 自动化冒烟通过。

---

## 1. 投影

| 项目 | 状态 | 证据 / 说明 |
|---|---|---|
| `Projection` 基类 | ✅ 已验证 | `PlaneProjection_HasSilverlightDefaults`、`…_DependencyPropertiesRoundTrip`；`Animatable`、`IsIdentity`、`ProjectionChanged` |
| `PlaneProjection` 矩阵 | ✅ 已验证 | `…_IdentityMatrixLeavesCenterAndCornersUnchanged`、`…_RotationXAndZRespectTheCoordinateSystem`、`…_CenterOfRotationMovesThePivot`、`…_LocalOffsetIsRotatedButGlobalOffsetIsNot`、`…_UsesElementSizeForCentersAndScales`、`…_RotationYMatchesTheXamlDepthDirection` |
| 透视模型（999 DIP 深度、按宽度推导 FOV） | ✅ 已验证 | `ProjectionHost_AppliesTheXamlPerspectiveDepth`（−80° 时铰链边 800 px、自由边 x≈134 / 543 px）、`ProjectionHost_KeepsTheProjectedPlaneOneToOneWithTheLayoutSlot`（200×400 槽位渲染出 199–200 px 宽，而不是 60%） |
| 2D↔3D 切换与 `Prefer3D` | ✅ 已验证 | `ProjectionHost_RendersContentInTwoAndThreeDimensions`、`ProjectionHost_Enters3DWhenIdentityProjectionIsAnimated`、`TransitionFrame_PlaysTurnstileTransition`（断言首帧与结束后的 `Prefer3D`/`IsRenderingIn3D`） |
| 命中测试、焦点、纹理方向 | ✅ 已验证 | `ProjectionHost_ForwardsHitTestingToProjectedContent`、`…_KeepsProjectedContentFocusable`、`…_PreservesTextureOrientation` |
| 多宿主 / 高 DPI | ✅ 已验证 | `…_RendersIndependentProjectionsForMultipleHosts`、`…_RendersAtHighDpiWithoutChangingLayoutUnits` |
| storyboard 定位（`(UIElement.Projection).(PlaneProjection.X)` 路径） | ✅ 已验证 | `Storyboard_AnimatesProjectionTarget`、`ProjectionHost_UpdatesTheSceneWhenTheProjectionIsAnimated` |
| `Matrix3DProjection` | 🟡 已实现、未验证 | `Matrix3DProjection_TranslatesInNormalizedCoordinates` 只覆盖平移；其他矩阵约定与视觉一致性未测；示例无演示 |
| identity 像素级一致 | ✅ 已验证 | 2026-10-19 实测：1 px 特征线在 2D 为 100.00、3D 为 100.01 px；无位移、无缩放。剩余差异是 ClearType→灰度抗锯齿 |

## 2. 导航

| 项目 | 状态 | 证据 / 说明 |
|---|---|---|
| `PhoneApplicationFrame`（基于 WPF `Frame`）、journal、`Navigate(page, parameters)` | ✅ 已验证 | `Frame_NavigatesAndRaisesPageLifecycle`、`Frame_NavigationContextCarriesParameters`、`Frame_RapidNavigationKeepsTheLatestPage`；示例导航 |
| `PhoneApplicationPage`、生命周期（`OnNavigatedTo/From`、`OnBackKeyPress`）、`State` | ✅ 已验证 | `Frame_NavigatesAndRaisesPageLifecycle`、`Frame_StateSurvivesBackNavigation`、`Frame_BackKeyPressCanCancelBackNavigation` |
| 方向检测 | ✅ 已验证 | `Frame_DetectsOrientationFromSize` |
| Tombstone / 真实 WP 生命周期 | ⛔ 未实现 | `State` 是内存字典；见 `known-limitations.md` §2 |
| `PhoneApplicationService` 状态 API | ✅ 已验证 | `SystemTrayAndApplicationServiceExposeState`；idle/deactivation 策略未实现 |
| `SystemTray`（状态 API） | ✅ 已验证 | `SystemTrayStatePublishesPropertyChanges`；**不绘制视觉 chrome** |
| `ApplicationBar`（命令/点击） | ✅ 已验证 | `ApplicationBarButtonsRaiseClicksAndExecuteCommands`、`ApplicationBarDisabledItemsDoNotRaiseEvents`；Frame 模板**没有视觉呈现器** |
| `NavigationContext` 查询参数/附加数据 | ✅ 已验证 | `Frame_NavigationContextCarriesParameters` |

## 3. 转场

| 族 | 状态 | 证据 / 说明 |
|---|---|---|
| `TransitionFrame`（退场→入场串行、presenter 翻转、交互恢复、失败回退） | ✅ 已验证 | `TransitionFrame_PlaysExitAndEntryTransitionsAndRestoresInteractivity`、`…_HostsInitialContentInTheOutgoingPresenter`、`…_RapidNavigationDoesNotLeaveTransitionsActive`、`…_FallsBackWhenTransitionFactoryThrows`、`…_PlaysTurnstileTransition` |
| `TurnstileTransition`（4 模式） | ✅ 已验证 | 上述测试 + 2026-10-19 运行期采样：退场 0→45° 用时 249 ms，入场 −69°→0° 用时 265–592 ms；几何按 999 DIP 解析模型实测 |
| `TurnstileFeatherTransition` | 🟡 已实现、未验证 | `TurnstileFeatherEffect_AnimatesFeatheredElements`、`…_FeathersElementsInOrderAndCleansUp`、`…_RejectsPageAsATarget` 覆盖动画与清理；原版“页面中心透视原点”**未**复刻（已文档化），且没有视觉对比 |
| `SlideTransition`（8 模式） | 🟡 已实现、未验证 | 数值已与原 XAML 人工逐条比对（全部 8 个）；仅通过 `ITransition` 测试间接执行；无运行期/视觉实测 |
| `RotateTransition`（8 模式） | 🟡 已实现、未验证 | 同 Slide（数值已比对，未在运行时观察） |
| `SwivelTransition`（6 模式） | 🟡 已实现、未验证 | 同 Slide |
| `RollTransition`（1 模式） | 🟡 已实现、未验证 | 同 Slide |
| storyboard 数值一致性测试 | ⛔ 缺失 | 没有对 `Transitions.cs` 数值的自动化快照；与原 XAML 的比对是人工的 |

## 4. 效果

| 项目 | 状态 | 证据 / 说明 |
|---|---|---|
| `TiltEffect` | ✅ 已验证 | `TiltEffect_WrapsAChildButtonAndAnimatesARealProjection`、`TiltEffect_DoesNotTiltThePageThatHostsTheElement`、`TiltEffect_DoesNotWrapWhenSuppressed`；按压方向已按 XAML 约定做解析验证 |
| `TurnstileFeatherEffect` | 🟡 已实现、未验证 | 同羽化转场的测试；逐元素宿主已可用，但透视原点与原版不同 |
| `SlideInEffect` | ⛔ 未实现 | 依赖 Pivot |

## 5. 控件

| 控件 | 状态 | 证据 / 说明 |
|---|---|---|
| `WrapPanel` | ✅ 已验证 | `WrapPanel_WrapsChildrenAndHonorsItemSize` |
| `ToggleSwitch` + `ToggleSwitchButton` | ✅ 已验证 | `ToggleSwitch_HasTemplateAndDependencyProperties`、`…_AnimatesThumbTranslation`、`…_SwitchButtonDrivesEvents` |
| `Rating` | ✅ 已验证 | `Rating_CreatesConfiguredNumberOfItems`、`…_MouseClickChangesValue`、`…_ValueChangesRaiseEventAndUpdateItems` |
| `PhoneTextBox` | ✅ 已验证（行为） | `PhoneTextBox_ShowsHintWhenEmpty`、`…_TemplatePartsAndTwoWayText`、`…_InnerEditorIsVisibleAndReceivesFocus`；水印动画视觉未验证 |
| `ListPicker` / `ListPickerItem` | ✅ 已验证（行为） | `ListPicker_SelectionRoundTrips`、`…_PopupSelectsItemAndCloses`、`…_OpenFailsWithoutItems`；弹出层样式未与 WP 对比 |
| `ExpanderView` | 🟡 已实现、未验证 | `ExpanderView_TogglesAndRaisesEvents` 覆盖行为；高度动画与标题竖线间距属于纯视觉 |
| `HubTile` / `HubTileService` | 🟡 已实现、未验证 | `HubTile_PropertiesAndGroupFreeze`；翻转时序/视觉未验证 |
| `CustomMessageBox` | ✅ 已验证 | `CustomMessageBox_ShowDismissRaisesLifecycleEvents` |
| `ContextMenu` / `ContextMenuService` | ✅ 已验证 | `ContextMenuService_AttachesMenu`、`ContextMenu_OpensAndCloses` |
| `DatePicker` / `TimePicker` / `RecurringDaysPicker` | ✅ 已验证（行为） | `DateTimePickers_HaveExpectedDefaultsAndFormatting`、`DateTimePickers_FormatValuesAndRaiseValueChanged`、`RecurringDaysPicker_SummarizesWeekdays`；picker **页面**与 `LoopingSelector` 未移植（改用 WPF 对话框） |
| `PerformanceProgressBar` | 🟡 已实现、未验证 | 在 `Generic.xaml` 中有样式、示例中有展示；无专门测试，相对动画重写未移植 |
| `RelativeAnimatingContentControl` | 🟡 已实现、未验证 | 有样式、示例中有展示；无专门测试 |
| `ClipToBounds` | 🟡 已实现、未验证 | 位于 `Controls/ClipToBounds.cs`；无专门测试 |
| 转换器（`OffOnConverter`、tile-size、`RelativeTimeConverter`） | ✅ 已验证 | `OffOnConverterUsesLocalizedStrings`、`TileSizeConvertersReturnExpectedDimensions`、`RelativeTimeConverterFormatsMinutesAndHours` |
| 辅助类（`CultureInfoExtensions`、`RoutedPropertyChangingEventArgs`、`WeakReferenceHelper`） | 🟡 已实现、未验证 | 被已移植控件使用；无专门测试 |
| `AutoCompleteBox` | ⛔ 未实现 | 源码中不存在 |
| `LongListSelector`（WP7） | ⛔ 未实现 | 源码中不存在 |
| `LongListMultiSelector`（WP8） | ⛔ 未实现 | 源码中不存在 |
| `MultiselectList` | ⛔ 未实现 | 源码中不存在 |
| 地图（`MapChild`、`MapItemsControl`、`Pushpin` 等） | ⛔ 未实现 | 需要地图提供方 |
| `SpeechTextBox` | ⛔ 未实现 | 需要 WP 语音 API |
| `TransferControl` | ⛔ 未实现 | 需要 WP 后台传输服务 |
| `GestureListener` / `GestureService` | ⛔ 未实现 | 改用 WPF manipulation 事件 |
| `LockablePivot` / `Pivot` | ⛔ 未实现 | — |

## 6. 主题、本地化、打包

| 项目 | 状态 | 证据 / 说明 |
|---|---|---|
| Metro 主题 + WPF 基础控件 WP 样式 | ✅ 存在性已验证 / 🟡（保真度） | `Theme_ContainsMetroResources`；`Generic.xaml` 覆盖 `Button`、`TextBox`、`ComboBox`、`CheckBox`、`RadioButton`、`ListBox`、`ScrollBar`、`ContextMenu`、`ProgressBar`、`ToolTip` 及工具包控件。从未与 WP 做并排像素对比 |
| 本地化资源 | ✅ 已验证 | 50 种文化 + 中性资源，包内包含卫星程序集；`OffOnConverterUsesLocalizedStrings` |
| NuGet 包 | ✅ 已验证 | `build\verify-package.ps1`：`Community.Phone.Controls.Toolkit.WPF.0.1.0-preview.1.nupkg` + `.snupkg`，全新消费工程可构建运行 |
| 示例应用 | ✅ 已验证（可运行） | UI 自动化冒烟：turnstile 前进/返回与羽化导航正常，Accent 按钮稳定在 644×66 |

---

## 7. 重新验证命令

```powershell
dotnet build Community.Phone.Controls.Toolkit.slnx -c Release
dotnet test  Community.Phone.Controls.Toolkit.slnx -c Release
powershell -NoProfile -ExecutionPolicy Bypass -File build\verify-package.ps1
# 示例（重建前先结束进程）
.\src\Community.Phone.Controls.Toolkit.WPF.Sample\bin\Debug\net10.0-windows\Community.Phone.Controls.Toolkit.WPF.Sample.exe
```

## 8. 维护规则

行为变化或证据增减时，必须在同一次改动中更新本文件。某个 🟡 项获得自动化或实测证据后改为 ✅ 并引用测试/测量；出现回归则改回。**不得**基于“打算做”把某行升级。
