# 移植状态 — Community.Phone.Controls.Toolkit.WPF

[English](../porting-status.md) · 中文

**本文档是什么。** 按模块记录的**当前**实现/验证状态，是本项目的权威状态表。内容来自源码树、测试套件和实测运行，而不是原始计划。Phase 0 审计（`docs/*/porting-inventory.md`）记录的是原版工具包**有什么**，不代表本移植版**现在能做什么**。

**状态定义**

| 状态 | 含义 |
|---|---|
| ✅ **已验证** | 已实现，且有自动化测试或实测数据（会引用具体数字）。 |
| 🟡 **已实现、未验证** | 已实现且示例或测试可达，但至少有一个方面（通常是像素保真度或时序）没有证据。 |
| ⛔ **未实现** | 没有兼容实现，不得声称可用。 |

**证据快照（2026-10-27）：** Release 构建 0 错误（22 个既有警告，全部位于 `LongListSelector`、`ViewportControl`、`WindowHelper`）· Release 下 **124/124 测试通过**（约 44 秒；等待改为条件轮询而非固定 sleep，且没有任何测试依赖真实桌面指针）· `build\verify-package.ps1` 通过（全新消费工程窗口内含一个 `Pivot`）· 示例冒烟运行：主页面列出 `Pivot` 示例，打开后渲染出 pivot 项、页眉条和 `SelectedIndex 0 of 3` 状态文本，示例错误日志没有新增内容。`WINDOWS_PHONE;WP8` 在所有配置下都定义，这本身就是移植源码能在 Release 下编译的前提。

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
| storyboard 定位（`(phone:ProjectionService.Projection).(phone:PlaneProjection.X)` 路径） | ✅ 已验证 | `Storyboard_AnimatesProjectionTarget`、`ProjectionHost_UpdatesTheSceneWhenTheProjectionIsAnimated`，以及从原版 storyboard 资源加载带前缀路径的 `TransitionStoryboardTests` 各族 |
| `ProjectionService` 按需包装 | ✅ 已验证 | `ProjectionService_WrapsAnElementThatReceivesAProjection`、`ProjectionService_WrapsAContentPresenterDeclaredInAPanel`、`ProjectionService_KeepsTemplateBindingsWhenWrappingATemplatedContentPresenter`、`ProjectionService_WrapsAnItemContainerOutsideAnItemsControl`；items panel 内的 item 容器会被拒绝包装且不抛异常（`ProjectionService_DoesNotWrapItemContainersInAnItemsPanel`），因为该 Panel 的子元素由 WPF 管理 |
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
| `ApplicationBar`（API、chrome 与菜单） | ✅ 已验证 | `ApplicationBarItemsRaiseClicksThroughTheirViews`、`…DisabledItemsDoNotRaiseEvents`、`…EnforcesItemLimitsAndValidation`、`…MenuAcceptsTheMaximumNumberOfItems`、`…ReservedHeightFollowsTheOriginalRule`、`ApplicationBarPresenterOpensAndClosesTheMenu`、`ApplicationBarIconButtonDrawsThePhoneCircle`、`PhoneApplicationFrameHostsThePageApplicationBar`、`…HidesTheBarWhenThePageHasNone`、`…RejectsForeignApplicationBarImplementations`；应用栏由 Frame 模板中的 `ApplicationBarPresenter` 绘制（96×72 槽位、48 DIP 圆环内嵌 32 DIP 图标、12 px 标签），示例 `MenusPage` 已使用 |
| `NavigationContext` 查询参数/附加数据 | ✅ 已验证 | `Frame_NavigationContextCarriesParameters` |

## 3. 转场

| 族 | 状态 | 证据 / 说明 |
|---|---|---|
| `TransitionFrame`（退场→入场串行、presenter 翻转、交互恢复） | ✅ 已验证 | `TransitionFrame_PlaysExitAndEntryTransitionsAndRestoresInteractivity`、`…_HostsInitialContentInTheOutgoingPresenter`、`…_RapidNavigationDoesNotLeaveTransitionsActive`、`…_PlaysTurnstileTransition`、`…_UsesAnOutTransitionSetJustBeforeNavigating`（示例在导航前设置转场族）、`…_CompletesAFeatheredExitWithoutTargets`（空转场不得冻结新页面）。presenter 会做 `Prefer3D`/命中测试准备，但不加 `BitmapCache`（3D 路径本身已栅格化页面）。WPF 对每次 `Frame.Navigate` 都报告 `IsNavigationInitiator == false`，因此原版针对 shell 导航的保护未移植。 |
| `TurnstileTransition`（4 模式） | ✅ 已验证 | 使用原版 `Turnstile*` XAML 资源；`TurnstileTransition_AnimatesRotationYOverTheOriginalRange` + `TransitionFrame_PlaysTurnstileTransition`；实测入场 `−80°→0°` 为 420 ms、退场 `0→50°` 约 300 ms（0.35 s / 0.25 s 乘以 `TurnstileDurationScale = 1.2`），转场中间帧截图中页面呈透视梯形。几何按 999 DIP 解析模型实测。WP 对齐调优：`TurnstileCenterOfRotationX = -0.05`、`TurnstileDurationScale = 1.2`（`Transitions.cs` 中的单个常量；设为 `0` / `1` 即还原原工具包值）。 |
| `TurnstileFeatherTransition` | ✅ 已验证 | `TurnstileFeatherEffect_AnimatesFeatheredElements`、`…_FeathersElementsInOrderAndCleansUp`、`…_RejectsPageAsATarget`、`TransitionFrame_AnimatesFeatheredEntryOnFirstLayout` 覆盖动画、首次布局时的目标收集与清理；没有动画的转场（例如页面没有可羽化元素时的羽化）会正常完成而不冻结（`TransitionFrame_CompletesAFeatheredExitWithoutTargets`）。示例中实测：入场转场按序羽化 `FeatherPage`，转场结束后页面正常重绘（转场后截屏）。原版“页面中心透视原点”**未**复刻（已文档化）。已实测帧率（6 个元素约 64–68 fps）。注意：出站转场属于被离开的页面——想要羽化退场必须由源页声明或动态设置（示例在 `SampleNavigator` 中完成，源页没有可羽化元素时回退为 turnstile 退场）。 |
| `SlideTransition`（8 模式） | ✅ 已验证 | 原版 `Slide*Fade*` XAML 资源；`SlideTransition_AnimatesTheRenderTransformOverTheOriginalRange` 在运行时观察 `TranslateTransform.X`（200 → 0）和透明度 |
| `RotateTransition`（8 模式） | ✅ 已验证 | 原版 `Rotate*` XAML 资源；`RotateTransition_AnimatesRotationZOverTheOriginalRange` 在运行时观察 `RotationZ`（90 → 0）；流方向适配由工厂覆盖 |
| `SwivelTransition`（6 模式） | ✅ 已验证 | 原版 `Swivel*` XAML 资源；`SwivelTransition_AnimatesRotationXOverTheOriginalRange` 在运行时观察 `RotationX`（−45 → 0） |
| `RollTransition`（1 模式） | ✅ 已验证 | 原版 `Roll` XAML 资源；`RollTransition_AnimatesRotationZOverTheOriginalRange` 在运行时观察 `RotationZ`（0 → 45 → 90） |
| storyboard 数值一致性 | ✅ 已验证 | `TransitionStoryboardTests` 在运行时断言每个族的动画属性与取值范围，因此改动 storyboard 资源或工厂都会让测试失败 |

## 4. 效果

| 项目 | 状态 | 证据 / 说明 |
|---|---|---|
| `TiltEffect` | ✅ 已验证 | `TiltEffect_WrapsAChildButtonAndAnimatesARealProjection`、`TiltEffect_DoesNotTiltThePageThatHostsTheElement`、`TiltEffect_DoesNotWrapWhenSuppressed`；按压方向已按 XAML 约定做解析验证 |
| `TurnstileFeatherEffect` | ✅ 已验证 | 同羽化转场的测试，另加 `TransitionFrame_AnimatesFeatheredEntryOnFirstLayout`；目标从页面可视树收集，宿主包装延迟到 dispatcher，元素卸载时释放包装。逐元素宿主与页面级投影可用，示例 `FeatherPage` 按序羽化且转场后正确重绘，但透视原点与原版不同 |
| `SlideInEffect` | ⛔ 未实现 | 该效果在 Pivot 选中项变化时播放逐项动画；`Pivot` 已实现，但效果本身尚未移植 |

## 5. 控件

| 控件 | 状态 | 证据 / 说明 |
|---|---|---|
| `WrapPanel` | ✅ 已验证 | `WrapPanel_WrapsChildrenAndHonorsItemSize` |
| `ToggleSwitch` + `ToggleSwitchButton` | ✅ 已验证 | `ToggleSwitch_HasTemplateAndDependencyProperties`、`…_AnimatesThumbTranslation`、`…_SwitchButtonDrivesEvents` |
| `Rating` | ✅ 已验证 | `Rating_CreatesConfiguredNumberOfItems`、`…_MouseClickChangesValue`、`…_ValueChangesRaiseEventAndUpdateItems`、`…_HalfItemIncrementKeepsValueInItemUnits`；半星模式现在以“星”为单位存值（已文档化的偏离：原版会填出两倍于指针位置的数量） |
| `PhoneTextBox` | ✅ 已验证（行为） | `PhoneTextBox_ShowsHintWhenEmpty`、`…_TemplatePartsAndTwoWayText`、`…_InnerEditorIsVisibleAndReceivesFocus`；占位符起点现在与光标一致（实测 12 px），聚焦或输入法合成期间隐藏；水印动画视觉未验证 |
| `ListPicker` / `ListPickerItem` | ✅ 已验证（行为） | `ListPicker_SelectionRoundTrips`、`…_ExpandedModeSelectsItemAndCollapses`、`…_OpenWithoutItemsExpands`、`…_FullModeWithoutFrameStaysUsable`、`…_FullModeNavigatesToPickerPageAndCommitsSelection`、`…_PageWithPickersLoadsWithoutAnimationErrors`、`…_HighlightedVisualStateUsesBrushResources`、`…_FullModePageUsesTheApplicationBarForSingleSelection`、`…_FullModePageCommitsFromTheApplicationBarDoneButton`（页面的原版应用栏、本地化文本、库内图标以及"完成→提交"链路）、`…_FullModePageAnimatesEveryItemInView`（每个 item 都有 `ProjectionHost`，从侧立开始、最终归零）、`…_FullModeSingleSelectionUsesThePhoneItemStyle`（没有桌面选中底色、选中文字为 accent 色、行高 98 px 而非桌面的 118）、`…_MultipleSelectionCheckBoxUsesThePhoneStyle`（透明填充、白色边框）、`…_ReturnsFromFullModeShowsTheSelectedItem`（presenter 位移到选中行）、`…_FullModePageWithManyItemsScrollsToTheSelectionAndAnimatesIt`（120 个虚拟化项目：不再抛 `NullReferenceException`，选中项滚入视口并参与动画，没有容器停留在侧立状态）。完整模式页面的 chrome 与间距已与 WP 8.1 截图对比 |
| `ExpanderView` | 🟡 已实现、未验证 | `ExpanderView_TogglesAndRaisesEvents` 覆盖行为；高度动画与标题竖线间距属于纯视觉 |
| `HubTile` / `HubTileService` | ✅ 已验证 | `HubTile_PropertiesAndGroupFreeze`、`HubTile_ShowsTheSourceImageAndFlipsToTheBackPanel`；模板即 WP8 原版（滑动表面 + `RotationX` 翻转）：Expanded/Semiexpanded 把表面滑到 `-H`/`-0.4566H`，翻转从 `0` 转到 `180` 并渲染背面（投影四边形为双面），`Source` 图片则用 96 DPI 渲染像素验证 |
| `CustomMessageBox` | ✅ 已验证 | `CustomMessageBox_ShowDismissRaisesLifecycleEvents`；弹出/关闭分别播放 `BackwardIn`/`BackwardOut` Swivel（探针：弹出 `RotationX −45°→0°`，关闭 `0°→42°` 后窗口才关闭）。`Show()` 在没有 `Application` 的宿主中同样可用（主题色回退到固定值、窗口尺寸取默认值），测试正是这样驱动它的 |
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
| `Pivot` / `PivotItem` / `PivotHeadersControl` / `PivotHeaderItem` | ✅ 已验证（行为）/ 🟡（指针拖拽） | `Pivot_ShowsTheFirstItemAndBuildsTheHeaderStrip`、`…_SelectedIndexSlidesTheIncomingItemAndCompletes`、`…_BackwardSelectionEntersFromTheLeft`、`…_ClickingAHeaderSelectsItsItem`、`…_HeaderRunWrapsAroundAfterTheLastHeader`、`…_KeyboardNavigatesAndWrapsAround`、`…_IsLockedRefusesProgrammaticNavigationAndFadesHeaders`、`…_RaisesTheItemLifecycleEventsInOrder`、`…_ItemsAddedAfterTheTemplateAppears`、`…_OutOfRangeSelectionIsRejected`、`…_RendersTheTitleHeadersAndContent`（96 DPI 离屏渲染：标题、页眉与内容像素落在预期区域）。指针拖拽路径（捕获、600 px/s 跟踪、半视口阈值）没有自动化测试，且本环境无法用真实指针验证；功能已实现但未验证。输入使用鼠标拖拽加方向键，而不是 DirectManipulation；页眉条与原版一样循环排布，但只绘制一圈（见 `docs/*/known-limitations.md`） |
| `LockablePivot` | ⛔ 未实现 | toolkit 的 WP7 辅助控件已被 `Pivot.IsLocked` 取代，移植版直接在 `Pivot` 上实现 |

## 6. 主题、本地化、打包

| 项目 | 状态 | 证据 / 说明 |
|---|---|---|
| Metro 主题 + WPF 基础控件 WP 样式 | ✅ 存在性已验证 / 🟡（保真度） | `Theme_ContainsMetroResources`；`Generic.xaml` 覆盖 `Button`、`TextBox`、`ComboBox`、`CheckBox`、`RadioButton`、`ListBox`、`ScrollBar`、`ContextMenu`、`ProgressBar`、`ToolTip` 及工具包控件。从未与 WP 做并排像素对比 |
| 本地化资源 | ✅ 已验证 | 50 种文化 + 中性资源，包内包含卫星程序集；`OffOnConverterUsesLocalizedStrings` |
| NuGet 包 | ✅ 已验证 | `build\verify-package.ps1`：`Community.Phone.Controls.Toolkit.WPF.0.1.0-preview.1.nupkg` + `.snupkg`，全新消费工程可构建运行 |
| 示例应用 | ✅ 已验证（可运行） | 结构与原版 `PhoneToolkitSample8` 对齐：`MainPage` 列出示例，每个示例独立成页（`ControlsPage`、`ProjectionPage`、`TransitionsPage`、`TransitionTargetPage`、`FeatherPage`、`MenusPage`、`ListPickerSample`、`CustomMessageBoxSample`、`PivotSample`、`HubTileSample`）。2026-10-27 UI 自动化冒烟：Pivot 示例可打开（活动 `PivotItem`、页眉、状态文本），`IsLocked` 可切换，返回按钮回到索引页；`HubTileSample`（三项 `Pivot` + 原版 HubTile 与图片）能渲染文本和图片。两个配置都先做了重新构建：往示例工程里新增 `Resource` 图片后必须重建，否则 `.g.resources` 仍是旧文件列表，pack URI 会找不到资源 |

---

## 7. 重新验证命令

```powershell
dotnet build PhoneToolkit.WPF.slnx -c Release
dotnet test  PhoneToolkit.WPF.slnx -c Release
powershell -NoProfile -ExecutionPolicy Bypass -File build\verify-package.ps1
# 示例（重建前先结束进程）
.\src\Community.Phone.Controls.Toolkit.WPF.Sample\bin\Debug\net10.0-windows\Community.Phone.Controls.Toolkit.WPF.Sample.exe
```

## 8. 维护规则

行为变化或证据增减时，必须在同一次改动中更新本文件。某个 🟡 项获得自动化或实测证据后改为 ✅ 并引用测试/测量；出现回归则改回。**不得**基于“打算做”把某行升级。
