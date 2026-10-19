# Porting status — Community.Phone.Controls.Toolkit.WPF

English · [中文](zh-CN/porting-status.md)

**What this document is.** The authoritative, current implementation/verification status per module. It is derived from the source tree, the test suite, and measured runs — not from the original plan. The Phase 0 audit (`docs/*/porting-inventory.md`) records what the original toolkit contains; it does not describe what this port currently does.

**Status vocabulary**

| Status | Meaning |
|---|---|
| ✅ **Verified** | Implemented and covered by an automated test or a measured run (numbers are cited). |
| 🟡 **Implemented / unverified** | Implemented and reachable from the sample or tests, but at least one aspect (usually pixel fidelity or timing) has no evidence. |
| ⛔ **Not implemented** | No compatible implementation. Never claim otherwise. |

**Evidence snapshot (2026-10-19):** Release build 0 warnings / 0 errors · 68/68 tests pass (Debug and Release) · `build/verify-package.ps1` passes · sample UI-automation smoke passes.

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
| Storyboard targeting (`(UIElement.Projection).(PlaneProjection.X)` path) | ✅ Verified | `Storyboard_AnimatesProjectionTarget`, `ProjectionHost_UpdatesTheSceneWhenTheProjectionIsAnimated` |
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
| `ApplicationBar` (commands/clicks) | ✅ Verified | `ApplicationBarButtonsRaiseClicksAndExecuteCommands`, `ApplicationBarDisabledItemsDoNotRaiseEvents`; **no visual presenter** in the frame template |
| `NavigationContext` query/extra data | ✅ Verified | `Frame_NavigationContextCarriesParameters` |

## 3. Transitions

| Family | Status | Evidence / notes |
|---|---|---|
| `TransitionFrame` (sequential exit→entry, presenter flip, interactivity restore, failure fallback) | ✅ Verified | `TransitionFrame_PlaysExitAndEntryTransitionsAndRestoresInteractivity`, `…_HostsInitialContentInTheOutgoingPresenter`, `…_RapidNavigationDoesNotLeaveTransitionsActive`, `…_FallsBackWhenTransitionFactoryThrows`, `…_PlaysTurnstileTransition` |
| `TurnstileTransition` (4 modes) | ✅ Verified | Tests above + runtime trace 2026-10-19: exit 0→45° in 249 ms, entry −69°→0° in 265–592 ms; geometry measured against the analytic 999-DIP model |
| `TurnstileFeatherTransition` | 🟡 Implemented / unverified | `TurnstileFeatherEffect_AnimatesFeatheredElements`, `…_FeathersElementsInOrderAndCleansUp`, `…_RejectsPageAsATarget` cover animation and cleanup; the original's page-centered perspective origin is **not** reproduced (documented deviation) and no visual comparison exists |
| `SlideTransition` (8 modes) | 🟡 Implemented / unverified | Storyboard values compared manually with the original XAML (all 8); behaviour exercised only indirectly through `ITransition` tests; no runtime/visual measurement |
| `RotateTransition` (8 modes) | 🟡 Implemented / unverified | Same as Slide (values verified against the original XAML; not observed at runtime) |
| `SwivelTransition` (6 modes) | 🟡 Implemented / unverified | Same as Slide |
| `RollTransition` (1) | 🟡 Implemented / unverified | Same as Slide |
| `Storyboard` value parity test | ⛔ Missing | No automated snapshot of `Transitions.cs` values; the comparison with the original XAML was manual |

## 4. Effects

| Item | Status | Evidence / notes |
|---|---|---|
| `TiltEffect` | ✅ Verified | `TiltEffect_WrapsAChildButtonAndAnimatesARealProjection`, `TiltEffect_DoesNotTiltThePageThatHostsTheElement`, `TiltEffect_DoesNotWrapWhenSuppressed`; press direction cross-checked analytically against XAML conventions |
| `TurnstileFeatherEffect` | 🟡 Implemented / unverified | Same tests as feather transition; per-item hosts work, perspective origin differs from the original |
| `SlideInEffect` | ⛔ Not implemented | Requires Pivot |

## 5. Controls

| Control | Status | Evidence / notes |
|---|---|---|
| `WrapPanel` | ✅ Verified | `WrapPanel_WrapsChildrenAndHonorsItemSize` |
| `ToggleSwitch` + `ToggleSwitchButton` | ✅ Verified | `ToggleSwitch_HasTemplateAndDependencyProperties`, `…_AnimatesThumbTranslation`, `…_SwitchButtonDrivesEvents` |
| `Rating` | ✅ Verified | `Rating_CreatesConfiguredNumberOfItems`, `…_MouseClickChangesValue`, `…_ValueChangesRaiseEventAndUpdateItems` |
| `PhoneTextBox` | ✅ Verified (behaviour) | `PhoneTextBox_ShowsHintWhenEmpty`, `…_TemplatePartsAndTwoWayText`, `…_InnerEditorIsVisibleAndReceivesFocus`. Hint animation visuals unverified |
| `ListPicker` / `ListPickerItem` | ✅ Verified (behaviour) | `ListPicker_SelectionRoundTrips`, `…_PopupSelectsItemAndCloses`, `…_OpenFailsWithoutItems`. Popup styling vs WP not compared |
| `ExpanderView` | 🟡 Implemented / unverified | `ExpanderView_TogglesAndRaisesEvents` covers behaviour; height animation and the header rule's spacing are visual-only |
| `HubTile` / `HubTileService` | 🟡 Implemented / unverified | `HubTile_PropertiesAndGroupFreeze`; flip timing/visuals unverified |
| `CustomMessageBox` | ✅ Verified | `CustomMessageBox_ShowDismissRaisesLifecycleEvents` |
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
| `LockablePivot` / `Pivot` | ⛔ Not implemented | — |

## 6. Theme, localization, packaging

| Item | Status | Evidence / notes |
|---|---|---|
| Metro theme + WP base-control styles | ✅ Verified (presence) / 🟡 (fidelity) | `Theme_ContainsMetroResources`; `Generic.xaml` styles `Button`, `TextBox`, `ComboBox`, `CheckBox`, `RadioButton`, `ListBox`, `ScrollBar`, `ContextMenu`, `ProgressBar`, `ToolTip` and the toolkit controls. Pixel parity with WP was never compared side by side |
| Localization resources | ✅ Verified | 50 cultures + neutral, satellites in the package; `OffOnConverterUsesLocalizedStrings` |
| NuGet package | ✅ Verified | `build\verify-package.ps1`: `Community.Phone.Controls.Toolkit.WPF.0.1.0-preview.1.nupkg` + `.snupkg`, fresh consumer builds and runs |
| Sample application | ✅ Verified (runs) | UI-automation smoke: turnstile forward/back and feather navigation, accent button stable at 644×66 |

---

## 7. Re-verification commands

```powershell
dotnet build Community.Phone.Controls.Toolkit.slnx -c Release
dotnet test  Community.Phone.Controls.Toolkit.slnx -c Release
powershell -NoProfile -ExecutionPolicy Bypass -File build\verify-package.ps1
# sample (stop it before rebuilding)
.\src\Community.Phone.Controls.Toolkit.WPF.Sample\bin\Debug\net10.0-windows\Community.Phone.Controls.Toolkit.WPF.Sample.exe
```

## 8. Maintenance rule

Update this file in the same change that alters behaviour or adds/removes evidence. When a 🟡 item gets automated or measured evidence, move it to ✅ and cite the test/measurement; when something regresses, move it back. Never upgrade a row based on intent.
