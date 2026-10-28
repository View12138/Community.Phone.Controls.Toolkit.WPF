# Porting status — Community.Phone.Controls.Toolkit.WPF

English · [中文](zh-CN/porting-status.md)

**What this document is.** The authoritative, current implementation/verification status per module. It is derived from the source tree, the test suite, and measured runs — not from the original plan. The Phase 0 audit (`docs/*/porting-inventory.md`) records what the original toolkit contains; it does not describe what this port currently does.

**Status vocabulary**

| Status | Meaning |
|---|---|
| ✅ **Verified** | Implemented and covered by an automated test or a measured run (numbers are cited). |
| 🟡 **Implemented / unverified** | Implemented and reachable from the sample or tests, but at least one aspect (usually pixel fidelity or timing) has no evidence. |
| ⛔ **Not implemented** | No compatible implementation. Never claim otherwise. |

**Evidence snapshot (2026-10-27):** Release build 0 errors (22 pre-existing warnings, all in `LongListSelector`, `ViewportControl` and `WindowHelper`) · **124/124 tests pass** in Release (about 44 s; the waits are condition-based instead of fixed sleeps and no test needs a real desktop pointer) · `build/verify-package.ps1` passes (the fresh consumer window contains a `Pivot`) · sample smoke run: the main page lists the `Pivot` sample, opening it renders the pivot item, the header strip and the `SelectedIndex 0 of 3` status, and no new entry is written to the sample error log. `WINDOWS_PHONE;WP8` is defined in every configuration, which is what makes the Release compilation of the ported sources work at all.

---

## 1. Projection

| Item | Status | Evidence / notes |
|---|---|---|
| `Projection` base type | ✅ Verified | `PlaneProjection_HasSilverlightDefaults`, `…_DependencyPropertiesRoundTrip`; `Animatable`, `IsIdentity`, `ProjectionChanged` |
| `PlaneProjection` matrix | ✅ Verified | `…_IdentityMatrixLeavesCenterAndCornersUnchanged`, `…_RotationXAndZRespectTheCoordinateSystem`, `…_CenterOfRotationMovesThePivot`, `…_LocalOffsetIsRotatedButGlobalOffsetIsNot`, `…_UsesElementSizeForCentersAndScales`, `…_RotationYMatchesTheXamlDepthDirection` |
| Perspective model (999 DIP depth, width-based FOV) | ✅ Verified | `ProjectionHost_AppliesTheXamlPerspectiveDepth` (hinge 800 px, free edge x≈134 / 543 px at −80°), `ProjectionHost_KeepsTheProjectedPlaneOneToOneWithTheLayoutSlot` (200×400 slot renders 199–200 px wide, not 60%) |
| 2D↔3D switching, `Prefer3D` | ✅ Verified | `ProjectionHost_RendersContentInTwoAndThreeDimensions`, `ProjectionHost_Enters3DWhenIdentityProjectionIsAnimated`, `TransitionFrame_PlaysTurnstileTransition` (asserts `Prefer3D`/`IsRenderingIn3D` on the first frame and after completion) |
| Hit testing, focus, texture orientation | ✅ Verified | `ProjectionHost_ForwardsHitTestingToProjectedContent`, `…_KeepsProjectedContentFocusable`, `…_PreservesTextureOrientation` |
| Multiple hosts / high DPI | ✅ Verified | `…_RendersIndependentProjectionsForMultipleHosts`, `…_RendersAtHighDpiWithoutChangingLayoutUnits` |
| Storyboard targeting (`(phone:ProjectionService.Projection).(phone:PlaneProjection.X)` path) | ✅ Verified | `Storyboard_AnimatesProjectionTarget`, `ProjectionHost_UpdatesTheSceneWhenTheProjectionIsAnimated`, and the `TransitionStoryboardTests` families, which load the prefixed paths from the original storyboard resources |
| `ProjectionService` on-demand wrapping | ✅ Verified | `ProjectionService_WrapsAnElementThatReceivesAProjection`, `ProjectionService_WrapsAContentPresenterDeclaredInAPanel`, `ProjectionService_KeepsTemplateBindingsWhenWrappingATemplatedContentPresenter`, `ProjectionService_WrapsAnItemContainerOutsideAnItemsControl`; item containers inside an items panel are refused without throwing (`ProjectionService_DoesNotWrapItemContainersInAnItemsPanel`) because WPF owns that panel's children |
| `Matrix3DProjection` | 🟡 Implemented / unverified | `Matrix3DProjection_TranslatesInNormalizedCoordinates` covers translation only; other matrix conventions and visual parity are untested; the sample has no demo |
| Pixel-exact identity mapping | ✅ Verified | Measured 2026-10-19: 1 px feature lines at 100.00 (2D) vs 100.01 (3D) px; no shift, no scale change. Residual difference is ClearType→grayscale AA |

## 2. Navigation

| Item | Status | Evidence / notes |
|---|---|---|
| `PhoneApplicationFrame` (WPF `Frame` based), journal, `Navigate(page, parameters)` | ✅ Verified | `Frame_NavigatesAndRaisesPageLifecycle`, `Frame_NavigationContextCarriesParameters`, `Frame_RapidNavigationKeepsTheLatestPage`; sample navigation |
| `PhoneApplicationPage`, lifecycle (`OnNavigatedTo/From`, `OnBackKeyPress`), `State` | ✅ Verified | `Frame_NavigatesAndRaisesPageLifecycle`, `Frame_StateSurvivesBackNavigation`, `Frame_BackKeyPressCanCancelBackNavigation` |
| Orientation detection | ✅ Verified | `Frame_DetectsOrientationFromSize` |
| Tombstoning / real WP lifecycle | ⛔ Not implemented | `State` is an in-memory dictionary; see `known-limitations.md` §2 |
| `PhoneApplicationService` state API | ✅ Verified | `SystemTrayAndApplicationServiceExposeState`; idle/deactivation policy is not implemented |
| `SystemTray` (state API) | ✅ Verified | `SystemTrayStatePublishesPropertyChanges`; **visual chrome is not drawn** |
| `ApplicationBar` (API, chrome and menu) | ✅ Verified | `ApplicationBarItemsRaiseClicksThroughTheirViews`, `…DisabledItemsDoNotRaiseEvents`, `…EnforcesItemLimitsAndValidation`, `…MenuAcceptsTheMaximumNumberOfItems`, `…ReservedHeightFollowsTheOriginalRule`, `ApplicationBarPresenterOpensAndClosesTheMenu`, `ApplicationBarIconButtonDrawsThePhoneCircle`, `PhoneApplicationFrameHostsThePageApplicationBar`, `…HidesTheBarWhenThePageHasNone`, `…RejectsForeignApplicationBarImplementations`; the bar is drawn by `ApplicationBarPresenter` in the frame template (96×72 slots, 48 DIP circle with a 32 DIP glyph, 12 px label) and the sample uses it (`MenusPage`) |
| `NavigationContext` query/extra data | ✅ Verified | `Frame_NavigationContextCarriesParameters` |

## 3. Transitions

| Family | Status | Evidence / notes |
|---|---|---|
| `TransitionFrame` (sequential exit→entry, presenter flip, interactivity restore) | ✅ Verified | `TransitionFrame_PlaysExitAndEntryTransitionsAndRestoresInteractivity`, `…_HostsInitialContentInTheOutgoingPresenter`, `…_RapidNavigationDoesNotLeaveTransitionsActive`, `…_PlaysTurnstileTransition`, `…_UsesAnOutTransitionSetJustBeforeNavigating` (the sample sets the family at navigation time), `…_CompletesAFeatheredExitWithoutTargets` (an empty transition must not freeze the incoming page). The presenters get `Prefer3D`/hit-test preparation but no `BitmapCache` (the 3D path already rasterizes the page). WPF reports `IsNavigationInitiator == false` for every `Frame.Navigate`, so the original guard against shell-initiated navigation is not ported. |
| `TurnstileTransition` (4 modes) | ✅ Verified | Original `Turnstile*` XAML resources; `TurnstileTransition_AnimatesRotationYOverTheOriginalRange` + `TransitionFrame_PlaysTurnstileTransition`; entry `−80°→0°` measured at 420 ms and exit `0→50°` at ≈300 ms (0.35 s / 0.25 s scaled by `TurnstileDurationScale = 1.2`), and a mid-transition screen capture shows the page as a perspective trapezoid. Geometry measured against the analytic 999-DIP model. WP-parity tuning: `TurnstileCenterOfRotationX = -0.05` and `TurnstileDurationScale = 1.2` (single constants in `Transitions.cs`; `0` / `1` restore the original toolkit values). |
| `TurnstileFeatherTransition` | 🟡 Implemented / unverified | `TurnstileFeatherEffect_AnimatesFeatheredElements`, `…_FeathersElementsInOrderAndCleansUp`, `…_RejectsPageAsATarget`, `TransitionFrame_AnimatesFeatheredEntryOnFirstLayout` cover animation, target collection on first layout and cleanup; a transition without animations (for example feathering on a page without feathered elements) completes instead of freezing the frame (`TransitionFrame_CompletesAFeatheredExitWithoutTargets`). The original's page-centered perspective origin is **not** reproduced (documented deviation) and no visual comparison exists. Frame pacing measured (6 items ≈ 64–68 fps). Note: the outgoing transition belongs to the page being left — a feathered exit requires the source page to declare or set it (the sample does this in `SampleNavigator`, and falls back to a turnstile exit when the page has no feathered elements). |
| `SlideTransition` (8 modes) | ✅ Verified | Original `Slide*Fade*` XAML resources; `SlideTransition_AnimatesTheRenderTransformOverTheOriginalRange` observes the animated `TranslateTransform.X` (200 → 0) and opacity at runtime |
| `RotateTransition` (8 modes) | ✅ Verified | Original `Rotate*` XAML resources; `RotateTransition_AnimatesRotationZOverTheOriginalRange` observes the animated `RotationZ` (90 → 0) at runtime; flow-direction adjustment covered by the factory |
| `SwivelTransition` (6 modes) | ✅ Verified | Original `Swivel*` XAML resources; `SwivelTransition_AnimatesRotationXOverTheOriginalRange` observes the animated `RotationX` (−45 → 0) at runtime |
| `RollTransition` (1) | ✅ Verified | Original `Roll` XAML resource; `RollTransition_AnimatesRotationZOverTheOriginalRange` observes the animated `RotationZ` (0 → 45 → 90) at runtime |
| `Storyboard` value parity | ✅ Verified | `TransitionStoryboardTests` assert the animated property and range of every family at runtime, so an edit to a storyboard resource (or to a factory) fails the suite |

## 4. Effects

| Item | Status | Evidence / notes |
|---|---|---|
| `TiltEffect` | ✅ Verified | `TiltEffect_WrapsAChildButtonAndAnimatesARealProjection`, `TiltEffect_DoesNotTiltThePageThatHostsTheElement`, `TiltEffect_DoesNotWrapWhenSuppressed`; press direction cross-checked analytically against XAML conventions |
| `TurnstileFeatherEffect` | 🟡 Implemented / unverified | Same tests as the feather transition (`…_AnimatesFeatheredElements`, `…_FeathersElementsInOrderAndCleansUp`, `…_RejectsPageAsATarget`) plus `TransitionFrame_AnimatesFeatheredEntryOnFirstLayout`; targets are collected from the page's visual tree and the host wrap is deferred to the dispatcher. Per-item hosts and page-level projection work; perspective origin differs from the original |
| `SlideInEffect` | ⛔ Not implemented | The effect animates pivot items when the selection changes; `Pivot` now exists, the effect itself is still not ported |

## 5. Controls

| Control | Status | Evidence / notes |
|---|---|---|
| `WrapPanel` | ✅ Verified | `WrapPanel_WrapsChildrenAndHonorsItemSize` |
| `ToggleSwitch` + `ToggleSwitchButton` | ✅ Verified | `ToggleSwitch_HasTemplateAndDependencyProperties`, `…_AnimatesThumbTranslation`, `…_SwitchButtonDrivesEvents` |
| `Rating` | ✅ Verified | `Rating_CreatesConfiguredNumberOfItems`, `…_MouseClickChangesValue`, `…_ValueChangesRaiseEventAndUpdateItems`, `…_HalfItemIncrementKeepsValueInItemUnits`. Half increments now store the value in item units (documented deviation: the toolkit filled twice as many items as the pointer indicated) |
| `PhoneTextBox` | ✅ Verified (behaviour) | `PhoneTextBox_ShowsHintWhenEmpty`, `…_TemplatePartsAndTwoWayText`, `…_InnerEditorIsVisibleAndReceivesFocus`. The hint now starts where the caret does (measured: 12 px) and is hidden while focused or during an IME composition; hint animation visuals unverified |
| `ListPicker` / `ListPickerItem` | ✅ Verified (behaviour) | `ListPicker_SelectionRoundTrips`, `…_ExpandedModeSelectsItemAndCollapses`, `…_OpenWithoutItemsExpands`, `…_FullModeWithoutFrameStaysUsable`, `…_FullModeNavigatesToPickerPageAndCommitsSelection`, `…_PageWithPickersLoadsWithoutAnimationErrors`, `…_HighlightedVisualStateUsesBrushResources`, `…_FullModePageUsesTheApplicationBarForSingleSelection`, `…_FullModePageCommitsFromTheApplicationBarDoneButton` (the page's original app bar, its localized texts, its library icons and the done→commit flow), `…_FullModePageAnimatesEveryItemInView` (every item has a `ProjectionHost`, starts edge-on and ends flat), `…_FullModeSingleSelectionUsesThePhoneItemStyle` (no desktop selection fill, accent foreground, 98 px rows instead of the desktop 118), `…_MultipleSelectionCheckBoxUsesThePhoneStyle` (transparent fill, white border), `…_ReturnsFromFullModeShowsTheSelectedItem` (the presenter translate offset moves to the chosen row), `…_FullModePageWithManyItemsScrollsToTheSelectionAndAnimatesIt` (120 virtualized items: no `NullReferenceException`, the selection is scrolled into view and animated, no container left edge-on). The full-mode page's chrome and spacing were compared with WP 8.1 screenshots |
| `ExpanderView` | 🟡 Implemented / unverified | `ExpanderView_TogglesAndRaisesEvents` covers behaviour; height animation and the header rule's spacing are visual-only |
| `HubTile` / `HubTileService` | ✅ Verified | `HubTile_PropertiesAndGroupFreeze`, `HubTile_ShowsTheSourceImageAndFlipsToTheBackPanel`; the template is the WP8 original (sliding surface + `RotationX` flip): Expanded/Semiexpanded slide the surface to `-H`/`-0.4566H`, the flip rotates `0→180` and renders the back panel because the projected quad is double-sided, and the `Source` image is verified from rendered pixels at 96 DPI |
| `CustomMessageBox` | ✅ Verified | `CustomMessageBox_ShowDismissRaisesLifecycleEvents`; the dialog swivels in (`BackwardIn`) and out (`BackwardOut`) with the real projection pipeline (probe: `RotationX −45°→0°` on show, `0°→42°` on dismiss before the window closes). `Show()` also works in a host without an `Application` (it falls back to the theme colours and a default size), which is how the test drives it |
| `ContextMenu` / `ContextMenuService` | ✅ Verified | `ContextMenuService_AttachesMenu`, `ContextMenu_OpensAndCloses` |
| `DatePicker` / `TimePicker` / `RecurringDaysPicker` | ✅ Verified (behaviour) | `DateTimePickers_HaveExpectedDefaultsAndFormatting`, `DateTimePickers_FormatValuesAndRaiseValueChanged`, `RecurringDaysPicker_SummarizesWeekdays`. Picker *pages* and `LoopingSelector` are not ported (WPF dialogs are used) |
| `PerformanceProgressBar` | 🟡 Implemented / unverified | Styled in `Generic.xaml`, shown in the sample; no dedicated test, relative animation rewriting not ported |
| `RelativeAnimatingContentControl` | 🟡 Implemented / unverified | Styled and shown in the sample; no dedicated test |
| `ClipToBounds` | 🟡 Implemented / unverified | Exists in `Controls/ClipToBounds.cs`; no dedicated test |
| Converters (`OffOnConverter`, tile-size converters, `RelativeTimeConverter`) | ✅ Verified | `OffOnConverterUsesLocalizedStrings`, `TileSizeConvertersReturnExpectedDimensions`, `RelativeTimeConverterFormatsMinutesAndHours` |
| Helpers (`CultureInfoExtensions`, `RoutedPropertyChangingEventArgs`, `WeakReferenceHelper`) | 🟡 Implemented / unverified | Used by the ported controls; no dedicated tests |
| `AutoCompleteBox` | ⛔ Not implemented | No source file exists |
| `LongListSelector` (WP7) | ⛔ Not implemented | No source file exists |
| `LongListMultiSelector` (WP8) | ⛔ Not implemented | No source file exists |
| `MultiselectList` | ⛔ Not implemented | No source file exists |
| Maps (`MapChild`, `MapItemsControl`, `Pushpin`, …) | ⛔ Not implemented | Requires a map provider |
| `SpeechTextBox` | ⛔ Not implemented | Requires WP speech APIs |
| `TransferControl` | ⛔ Not implemented | Requires the WP background-transfer service |
| `GestureListener` / `GestureService` | ⛔ Not implemented | WPF manipulation events are used instead |
| `Pivot` / `PivotItem` / `PivotHeadersControl` / `PivotHeaderItem` | ✅ Verified (behaviour) / 🟡 (pointer drag) | `Pivot_ShowsTheFirstItemAndBuildsTheHeaderStrip`, `…_SelectedIndexSlidesTheIncomingItemAndCompletes`, `…_BackwardSelectionEntersFromTheLeft`, `…_ClickingAHeaderSelectsItsItem`, `…_HeaderRunWrapsAroundAfterTheLastHeader`, `…_KeyboardNavigatesAndWrapsAround`, `…_IsLockedRefusesProgrammaticNavigationAndFadesHeaders`, `…_RaisesTheItemLifecycleEventsInOrder`, `…_ItemsAddedAfterTheTemplateAppears`, `…_OutOfRangeSelectionIsRejected`, `…_RendersTheTitleHeadersAndContent` (96 DPI render: title, header and content pixels in the expected bands). The pointer-drag path (capture, 600 px/s tracking, half-viewport threshold) has no automated test and could not be exercised interactively here (no pointer reaches the window); it is implemented but unverified. Input is a mouse drag plus arrow keys instead of DirectManipulation; the header run is circular like the original but drawn once (`docs/*/known-limitations.md`) |
| `LockablePivot` | ⛔ Not implemented | The toolkit's WP7 helper is superseded by `Pivot.IsLocked`, which the port implements on `Pivot` itself |

## 6. Theme, localization, packaging

| Item | Status | Evidence / notes |
|---|---|---|
| Metro theme + WP base-control styles | ✅ Verified (presence) / 🟡 (fidelity) | `Theme_ContainsMetroResources`; `Generic.xaml` styles `Button`, `TextBox`, `ComboBox`, `CheckBox`, `RadioButton`, `ListBox`, `ScrollBar`, `ContextMenu`, `ProgressBar`, `ToolTip` and the toolkit controls. Pixel parity with WP was never compared side by side |
| Localization resources | ✅ Verified | 50 cultures + neutral, satellites in the package; `OffOnConverterUsesLocalizedStrings` |
| NuGet package | ✅ Verified | `build\verify-package.ps1`: `Community.Phone.Controls.Toolkit.WPF.0.1.0-preview.1.nupkg` + `.snupkg`, fresh consumer builds and runs |
| Sample application | ✅ Verified (runs) | Mirrors the original `PhoneToolkitSample8` layout: `MainPage` lists samples and each sample is its own page (`ControlsPage`, `ProjectionPage`, `TransitionsPage`, `TransitionTargetPage`, `FeatherPage`, `MenusPage`, `ListPickerSample`, `CustomMessageBoxSample`, `PivotSample`, `HubTileSample`). UI-automation smoke 2026-10-27: the Pivot sample opens (active `PivotItem`, headers, status text), `IsLocked` toggles and the back button returns to the index; `HubTileSample` (a three-item `Pivot` with the original HubTiles and images) renders its text and images. Both configurations were rebuilt first: adding `Resource` images to the sample project needs a rebuild, otherwise the `.g.resources` keeps the previous file list and the pack URIs fail |

---

## 7. Re-verification commands

```powershell
dotnet build PhoneToolkit.WPF.slnx -c Release
dotnet test  PhoneToolkit.WPF.slnx -c Release
powershell -NoProfile -ExecutionPolicy Bypass -File build\verify-package.ps1
# sample (stop it before rebuilding)
.\src\Community.Phone.Controls.Toolkit.WPF.Sample\bin\Debug\net10.0-windows\Community.Phone.Controls.Toolkit.WPF.Sample.exe
```

## 8. Maintenance rule

Update this file in the same change that alters behaviour or adds/removes evidence. When a 🟡 item gets automated or measured evidence, move it to ✅ and cite the test/measurement; when something regresses, move it back. Never upgrade a row based on intent.
