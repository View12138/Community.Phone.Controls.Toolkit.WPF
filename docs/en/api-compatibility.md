# API compatibility matrix

This matrix is based on the source audit in [porting-inventory.md](porting-inventory.md). “Fully compatible” means the public API and observed behavior are implemented and covered by tests. It does **not** mean arbitrary Windows Phone XAML compiles unchanged: WPF still requires namespace changes and, for projection, a wrapper element.

## Compatibility levels

| Level | Meaning |
|---|---|
| **A. Fully compatible** | API and behavior match the original for the supported scenario; tests exist. |
| **B. API compatible, behavior differs** | Names, DPs, events and general behavior exist, but the WPF implementation uses a different host or platform service. |
| **C. Requires migration** | XAML or C# must change (namespace, wrapper, different base type). |
| **D. Alternative/wrapper** | A new WPF-specific type is required to express the feature. |
| **E. Not supported** | No compatible implementation; use an alternative or keep the WP application. |

## Namespace and type mapping

| WP original | WPF port | Level | Notes |
|---|---|---|---|
| `Microsoft.Phone.Controls.PlaneProjection` (platform `System.Windows.Media.PlaneProjection`) | `Microsoft.Phone.Controls.PlaneProjection` | C/D | Use the `phone:` prefix; assign it to `ProjectionService.Projection` on an element (wrapped on demand) or to a `ProjectionHost`. |
| `Microsoft.Phone.Controls.Projection` (platform) | `Microsoft.Phone.Controls.Projection` | D | New base type; `Animatable`. |
| `Microsoft.Phone.Controls.Matrix3DProjection` (platform) | `Microsoft.Phone.Controls.Matrix3DProjection` | C/D | Matrix is normalized 0..1. |
| `UIElement.Projection` | `ProjectionHost.Projection` / `ProjectionService.Projection` | C/D | No in-place property on WPF elements; assigning it wraps the element in a `ProjectionHost` on demand, and `(ProjectionService.Projection).(PlaneProjection.RotationY)` works as a storyboard path. |
| `Microsoft.Phone.Controls.PhoneApplicationFrame` | `Microsoft.Phone.Controls.PhoneApplicationFrame` | B/C | Derives from WPF `Frame`; adds page-parameter navigation. |
| `Microsoft.Phone.Controls.PhoneApplicationPage` | `Microsoft.Phone.Controls.PhoneApplicationPage` | B/C | Derives from WPF `Page`; lifecycle raised by the frame. |
| `System.Windows.Navigation.NavigationContext` | `Microsoft.Phone.Controls.NavigationContext` | B/C | WPF has no equivalent; exposes `Uri`, `QueryString`, `ExtraData`. |
| `Microsoft.Phone.Shell.PhoneApplicationService` | `Microsoft.Phone.Shell.PhoneApplicationService` | B | WPF app/window lifetime mapping. |
| `Microsoft.Phone.Shell.SystemTray` | `Microsoft.Phone.Shell.SystemTray` | B | API/state implemented; chrome rendering pending. |
| `Microsoft.Phone.Shell.ApplicationBar` (and `IApplicationBar`, `IApplicationBarIconButton`, `IApplicationBarMenuItem`, `ApplicationBarIconButton`, `ApplicationBarMenuItem`, `ApplicationBarMode`, `ApplicationBarStateChangedEventArgs`) | same names | A/B | Original API shape (max 4 icon buttons, max 50 menu items, text/icon validation, `StateChanged`); the bar is drawn by `ApplicationBarPresenter` hosted by the frame, which reserves the page space like the original. |
| `Microsoft.Phone.Controls.TransitionFrame` | `Microsoft.Phone.Controls.TransitionFrame` | B | Template parts are `ProjectionHost`, not `ContentPresenter`. |
| `Microsoft.Phone.Controls.ITransition` and transition types | same names | A/B | Same surface; WPF storyboards and state tracking. |
| `Microsoft.Phone.Controls.ContextMenu` | `Microsoft.Phone.Controls.ContextMenu` | B/C | Derives from WPF `ContextMenu`. |
| `Microsoft.Phone.Controls.WrapPanel` | `Microsoft.Phone.Controls.WrapPanel` | A | WPF’s own `System.Windows.Controls.WrapPanel` remains separate. |
| `System.Windows.Controls.HierarchicalDataTemplate` (toolkit) | `System.Windows.HierarchicalDataTemplate` | A | Use WPF native. |
| `System.Windows.Controls.HeaderedItemsControl` (toolkit) | `System.Windows.Controls.HeaderedItemsControl` | A | Use WPF native. |
| `System.Windows.Controls.LengthConverter` / `TimeTypeConverter` (toolkit) | not redefined | E | Use WPF type converters or parse values in app code. |
| `Microsoft.Phone.Maps.Toolkit.*` | not implemented | E | No WPF map provider. |
| `Microsoft.Phone.BackgroundTransfer` / `TransferControl` | not implemented | E | No WPF equivalent service. |
| `Windows.Phone.Speech.Recognition` / `SpeechTextBox` | not implemented | E | No WPF equivalent speech service. |

## Projection and effects

| Original type | Port | Level | XAML change | Behavior difference |
|---|---|---|---|---|
| `PlaneProjection` | `PlaneProjection` | C/D | `phone:` prefix; assign to an element or a `ProjectionHost` | Real WPF 3D via `Viewport2DVisual3D`; XAML's fixed 999 DIP perspective depth; rotation, center, and offset signs match XAML. An element that receives a projection is wrapped in a `ProjectionHost` on demand. |
| `Matrix3DProjection` | `Matrix3DProjection` | C/D | Same | Matrix normalized 0..1; converted to pixel space. |
| `TiltEffect` | `TiltEffect` | C/D | `phone:TiltEffect.IsTiltEnabled` works on containers | Plain controls are wrapped on demand; `PreviewMouseDown/Move/Up` support means a container can tilt a child button without consuming its click; list item containers are skipped. |
| `TurnstileFeatherEffect` | `TurnstileFeatherEffect` | C/D | `phone:TurnstileFeatherEffect.FeatheringIndex` | Requires wrappable targets; skipped item containers/content presenters. |
| `SlideInEffect` | not implemented | E | — | Depends on WP Pivot. |

## Navigation and transitions

| Original | Port | Level | Notes |
|---|---|---|---|
| `PhoneApplicationFrame` | same | B/C | WPF journal/`NavigationService`; orientation inferred from size. |
| `PhoneApplicationPage` | same | B/C | `State`, orientation, events; lifecycle methods are frame-driven. |
| `TransitionFrame` | same | B | Custom template with two `ProjectionHost` parts; automatic entry/exit transitions for direct page navigation. |
| All transition factories | same | A/B | Storyboards are the original XAML resources, parsed at runtime; angles, key times and easing come from the original files. A throwing factory propagates, exactly like the original (no fallback event). |
| `TransitionService.NavigationInTransition` | same | A | Attached property names preserved. |
| `TransitionService.NavigationOutTransition` | same | A | Attached property names preserved. |
| `NavigationInTransition` / `NavigationOutTransition` | same | A | Forward/Backward DPs. |
| `TurnstileTransition` | same | A | Mode values preserved. |
| `TurnstileTransitionMode` | same | A | ForwardIn/ForwardOut/BackwardIn/BackwardOut. |
| `SlideTransition` | same | A | Modes preserved. |
| `RotateTransition` | same | A | Modes preserved; flow-direction adjustment preserved. |
| `SwivelTransition` | same | A | Modes preserved. |
| `RollTransition` | same | A | Single mode. |
| `TurnstileFeatherTransition` | same | A/B | `Mode`, `BeginTime`; uses wrappers for targets. |
| `FeatheredTransition` | same | A/B | Public type preserved. |

The 27 original `Transitions/Storyboards/*.xaml` files ship as WPF resources and are loaded at runtime, so the durations, key times, angles and easing functions are the original ones. The only change inside them is the projection property path (`(phone:ProjectionService.Projection).(phone:PlaneProjection.X)`), because WPF has no `UIElement.Projection`.

## Controls

| Control | Port status | Level | Preserved | Differences |
|---|---|---|---|---|
| `AutoCompleteBox` | Not implemented | E | — | Large Silverlight popup/selection infrastructure not ported. |
| `ContextMenu` / `MenuItem` / `Separator` | Implemented | B/C | `ContextMenuService` attached property; `Owner`, `IsZoomEnabled`, `IsFadeEnabled`, `RegionOfInterest` | Derives from WPF menu types; opens on right-click. |
| `CustomMessageBox` | Implemented | B | `Show`, `Dismiss`, `Dismissing`, `Dismissed`, buttons, title/caption/message, result enum | Hosted in a WPF `Window`; visual layout rebuilt in code. |
| `DatePicker` | Implemented | B | `Value`, `ValueString`, `ValueStringFormat`, `ValueChanged`, `Header`, `PickerPageUri` | Uses a WPF `DatePicker` in a modal Window; picker page not ported. |
| `TimePicker` | Implemented | B | Same | Uses hour/minute ComboBoxes in a modal Window. |
| `RecurringDaysPicker` | Implemented | A/B | Weekday list, multiple selection, summary | Built on the WPF `ListPicker`; localized day names. |
| `ExpanderView` | Implemented | B | `Expander`, `ExpanderTemplate`, `NonExpandableHeader`, `HasItems`, `IsExpanded`, `Expanded`/`Collapsed` | WPF `HeaderedItemsControl`; simplified height animation; parts `Presenter`/`ExpanderPanel`/`ItemsCanvas`. |
| `HubTile` | Implemented | B | `Source`, `Title`, `Notification`, `Message`, `DisplayNotification`, `IsFrozen`, `GroupTag`, `Size`; `HubTileService` | Timer-driven cycle; animation timing approximate. |
| `ListPicker` | Implemented | A/B | `SelectedIndex`, `SelectedItem`, `SelectedItems`, `SelectionMode`, `Header`, `FullModeHeader`, `FullModeItemTemplate`, `ItemCountThreshold`, `ExpansionMode`, `PickerPageUri`, `Open()`, `SelectionChanged`, `ListPickerMode` | Full mode navigates to the original `ListPickerPage` (`PickerPageUri`) and commits on `Done`/back; the page uses the original `ApplicationBar` (check/cancel icon buttons) for its actions. Normal mode animates the template canvas like the original. |
| `LongListSelector` (WP7) | Not implemented | E | — | Virtualized Silverlight control; large remaining item. |
| `LongListMultiSelector` (WP8) | Not implemented | E | — | Depends on WP8 list infrastructure. |
| `Maps` extensions | Not implemented | E | — | No WPF map provider. |
| `MultiselectList` | Not implemented | E | — | Not ported. |
| `PhoneTextBox` | Implemented | A/B | `Hint`, `HintStyle`, `ActualHintVisibility`, `LengthIndicatorVisible`, `LengthIndicatorThreshold`, `DisplayedMaxLength`, `ActionIcon`, `HidesActionItemWhenEmpty`, `ActionIconTapped` | Inner TextBox uses a two-way Text binding for WPF. |
| `Rating` / `RatingItem` | Implemented | A/B | `FilledItemStyle`, `UnfilledItemStyle`, `RatingItemCount`, `Value`, `ReadOnly`, `AllowHalfItemIncrement`, `AllowSelectingZero`, `ShowSelectionHelper`, `Orientation`, `ValueChanged` | Star items generated in code; template parts preserved. |
| `ToggleSwitch` / `ToggleSwitchButton` | Implemented | A/B | `Header`, `HeaderTemplate`, `SwitchForeground`, `IsChecked`, `Checked`/`Unchecked`/`Indeterminate`/`Click`; switch template parts | WPF visual states and manipulation. |
| `TransferControl` | Not implemented | E | — | Requires WP background transfer service. |
| `SpeechTextBox` | Not implemented | E | — | Requires Windows Phone speech. |
| `PerformanceProgressBar` | Implemented | B | `IsIndeterminate`, `ActualIsIndeterminate` | Indeterminate template; simple fade. |
| `RelativeAnimatingContentControl` | Implemented | B | Public content control | Magic-number relative animation rewriting not ported. |
| `WrapPanel` | Implemented | A | `ItemHeight`, `ItemWidth`, `Orientation`, layout algorithm | None observed in tests. |
| `LockablePivot` | Not implemented | E | — | Requires WP Pivot. |
| `GestureListener` / `GestureService` | Not implemented | E | — | WPF touch/manipulation APIs differ. |
| `LoopingSelector` / `LoopingSelectorItem` | Not implemented | E | — | Used by WP date/time pages; not needed for the ported picker windows. |
| `ClipToBounds` | Implemented | A | `IsEnabled` attached property | Uses WPF `Clip`. |
| `LengthConverter` / `TimeTypeConverter` | Not redefined | E | — | Use WPF native converters. |

## Converters and localization

| Type | Status | Notes |
|---|---|---|
| `DailyDateTimeConverter` | Implemented | Original formatting/validation. |
| `HourlyDateTimeConverter` | Implemented | Original formatting/validation. |
| `FullViewDateTimeConverter` | Implemented | Original culture branches. |
| `ListViewDateTimeConverter` | Implemented | Original formatting. |
| `ThreadDateTimeConverter` | Implemented | Original formatting. |
| `RelativeTimeConverter` | Implemented | Original thresholds and pluralization; uses embedded `ControlResources`. |
| `OffOnConverter` | Implemented | Uses localized On/Off. |
| `ControlResources` | Implemented | 51 resx files embedded; 50 real cultures plus the neutral resource. |

## Dependency property defaults that matter

| Type | Property | Default |
|---|---|---|
| `PlaneProjection` | `CenterOfRotationX/Y` | `0.5` |
| `PlaneProjection` | `CenterOfRotationZ`, rotations, offsets | `0` |
| `Rating` | `RatingItemCount` | `5` |
| `Rating` | `Value` | `0` |
| `ToggleSwitch` | `IsChecked` | `false` |
| `ListPicker` | `SelectedIndex` | `-1` |
| `ListPicker` | `ItemCountThreshold` | `5` |
| `ListPicker` | `ExpansionMode` | `ExpansionAllowed` |
| `ExpanderView` | `IsExpanded` | `false` |
| `DatePicker` | `Value` | today (at construction) |
| `TimePicker` | `Value` | now (at construction) |
| `PerformanceProgressBar` | `IsIndeterminate` / `ActualIsIndeterminate` | `false` |

## XAML compatibility boundary

| XAML form | Result |
|---|---|
| `xmlns:phone="clr-namespace:Microsoft.Phone.Controls;assembly=Community.Phone.Controls.Toolkit.WPF"` | Required for all ported types. |
| `<phone:ToggleSwitch ... />`, `<phone:Rating ... />`, `<phone:WrapPanel ... />` | Works with migration of the namespace only (and WPF resource keys if the app uses Metro text styles). |
| `<toolkit:TransitionService.NavigationInTransition>...` with `phone:` prefix | Works. |
| `<Grid.Projection><PlaneProjection .../></Grid.Projection>` | Does not compile in WPF; use `ProjectionService.Projection` (wrapped on demand) or a `ProjectionHost`. |
| `<Grid phone:ProjectionService.Projection="...">` | Works: the element is wrapped in a `ProjectionHost` on demand (direct child of a panel) and unwrapped when the projection is cleared. |
| `<phone:ProjectionHost.Projection><phone:PlaneProjection .../></phone:ProjectionHost.Projection>` | Works. |
| Original `PhoneApplicationPage` XAML using `shell:SystemTray.IsVisible` | Parses with the port’s `shell` namespace; visual chrome is limited. |
| Original `shell:ApplicationBar` XAML | Works: the bar is rendered by the frame chrome (`ApplicationBarPresenter`), including its menu, minimized mode and the page space it reserves. |
| Original `phone:PhoneApplicationPage` `SupportedOrientations`/`Orientation` | Parses and is retained; desktop window rotation is not forced. |
| `phone:LongListSelector`, `phone:AutoCompleteBox`, map, speech, transfer XAML | Not supported in this preview. |

## Compatibility caveats by feature

1. **Projection and 3D** — real 3D rendering and animation are implemented. The original `<Grid.Projection>` form does not compile, but assigning `ProjectionService.Projection` (or `element.Projection`) to any element wraps it in a `ProjectionHost` on demand, so no manual wrapper is needed; explicit hosts remain the form for templates. The camera reproduces XAML's fixed 999 DIP perspective depth, so scale and foreshortening match WP; only effects that rely on the perspective origin moving outside the element bounds differ (see known limitations).
2. **Transitions** — timing, angles and modes match the original storyboards. Storyboard state is tracked by the port because WPF’s `GetCurrentState()` requires a controllable containing object.
3. **Navigation** — WPF `Frame`/`Page` behavior and journal are retained, but WP page lifecycle is raised by the port’s frame; `PhoneApplicationService` idle-detection policy has no WPF system equivalent.
4. **Effects** — TiltEffect and TurnstileFeatherEffect use the real projection pipeline. Targets that cannot be wrapped (item containers/content presenters) are skipped.
5. **Controls** — DP surfaces and core behavior are preserved; visual details and modal hosts differ where WPF has no WP picker/popup infrastructure.
6. **Localization** — all original resx resources are included; `CurrentUICulture` is used by `ControlResources`.
7. **Unsupported modules** — AutoCompleteBox, LongListSelector, LongListMultiSelector, MultiselectList, maps, transfer, speech, Pivot/LockablePivot, SlideInEffect, and gesture APIs are explicitly unsupported, not stubbed.

## Verification status

| Evidence | Location |
|---|---|
| Build (library, sample, tests) | `dotnet build -c Debug/Release` |
| Projection rendering/orientation/hit-test tests | `tests/.../ProjectionTests.cs` |
| Control/API tests | `tests/.../ControlTests.cs` |
| Navigation/transition tests | `tests/.../NavigationTests.cs` |
| Sample runtime smoke test | `src/Community.Phone.Controls.Toolkit.WPF.Sample` + `%TEMP%\community-phone-toolkit-sample-error.txt` |
| Package install test | `build/verify-package.ps1` + `tests/PackageConsumer` (fresh isolated cache, build and run) |

This matrix is updated when implementation status changes; it must not be used to claim a feature is complete unless the corresponding build/test or manual evidence exists.
