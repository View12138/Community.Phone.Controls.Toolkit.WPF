# Known limitations

This port targets a truthful subset of the archived Windows Phone Toolkit. The list below is explicit so that no unsupported feature is mistaken for a finished one. If a feature is not listed as implemented in [api-compatibility.md](api-compatibility.md), assume it is not supported.

## 1. Unsupported controls and modules

| Original module | Status | Why / alternative |
|---|---|---|
| `AutoCompleteBox` | Not implemented | Silverlight popup, selection adapter, filtering and keyboard infrastructure; no WPF replacement in this package. Use an application-specific WPF autocomplete control. |
| `LongListMultiSelector` (WP8) | Not implemented | Depends on WP8 list infrastructure and selection visuals. Use `ListView` with `SelectionMode="Multiple"`. |
| `MultiselectList` | Not implemented | Not ported. |
| `Maps` / `MapExtensions` | Not implemented | No WPF map provider; depends on `Microsoft.Phone.Maps`. |
| `TransferControl` | Not implemented | Depends on `Microsoft.Phone.BackgroundTransfer`. |
| `SpeechTextBox` | Not implemented | Depends on `Windows.Phone.Speech.Recognition`. |
| `LockablePivot` | Not implemented | The WP7 helper is superseded by `Pivot.IsLocked`, which the port implements on `Pivot` itself. |
| `SlideInEffect` | Not implemented | Animates pivot items when the selection changes; `Pivot` is ported, the effect itself is not. |
| `GestureListener` / `GestureService` | Not implemented | WP touch gesture service with flick/vertical/horizontal detection. Use WPF `Touch`/`Manipulation` events or a dedicated gesture library. |
| `LoopingSelector` | Not implemented | WP picker internals; the ported date/time pickers use WPF modal windows instead. |
| Picker pages (`DatePickerPage`, `TimePickerPage`) | Not implemented | The pickers host their own modal window with equivalent value editing. `PickerPageUri` is retained but not navigated. |
| `LengthConverter`, `TimeTypeConverter` | Not redefined | Use WPF type conversion or parse in application code. |

## 2. Navigation and page model

1. **WPF base types** — `PhoneApplicationPage` derives from WPF `Page`, and `PhoneApplicationFrame`/`TransitionFrame` derive from WPF `Frame`. Silverlight navigation APIs that do not exist in WPF are not emulated.
2. **Lifecycle** — `OnNavigatedTo`, `OnNavigatingFrom`, `OnNavigatedFrom`, `OnOrientationChanged` are declared as protected virtual methods, but WPF `Page` does not call them. `PhoneApplicationFrame` raises them from WPF navigation events. If a page is hosted outside the port’s frame, the lifecycle methods are not called.
3. **Orientation** — `PageOrientation` and `SupportedPageOrientation` are retained, and the frame infers landscape/portrait from its size. The desktop window is not forcibly rotated, and WP’s automatic orientation policy is not reproduced.
4. **Back key** — `BackKeyPress` and `NavigationService.GoBack()` are available, but there is no hardware back key. Applications must bind a keyboard shortcut or command if they need one.
5. **`PhoneApplicationService`** — `Launching`, `Activated`, `Deactivated`, `Closing`, `State`, `Current`, and idle detection are mapped to WPF application/window events where possible. WP’s application lifecycle and tombstoning semantics are not reproduced; WPF process lifetime and window lifetime are different.
6. **State persistence** — `PhoneApplicationPage.State` is an in-memory dictionary. There is no automatic tombstone serialization.
7. **Journal** — WPF `Frame` journal behavior is retained. `PhoneApplicationPage` sets `KeepAlive = true`, so a back navigation returns the page instance that was left instead of recreating it from its URI; this deliberately differs from WP's `NavigationCacheMode.Disabled` default. Set `KeepAlive = false` in the page constructor when a fresh instance is required. `NavigationCacheMode` itself is not ported.
8. **Navigation failures** — `NavigationFailed` is raised as an event on the port’s frame; a navigation failure does not automatically terminate the app as on WP.

## 3. Shell chrome

1. **System tray** — `SystemTray.IsVisible`, `SystemTray.ProgressIndicator`, `SystemTray.Opacity`, `SystemTray.BackgroundColor`, `SystemTray.ForegroundColor`, and state-change events are implemented as an API model. The default frame template does **not** draw a complete Windows Phone status bar (signal, battery, clock, full-screen behavior). Applications that need the visual chrome must render it themselves.
2. **Application bar** — the original `Microsoft.Phone.Shell` API is ported: `IApplicationBar`, `ApplicationBar` (max 4 icon buttons, max 50 menu items, text/icon validation, `Opacity` clamping, `Mode`, `StateChanged`), `ApplicationBarIconButton`, `ApplicationBarMenuItem`, `ApplicationBarMode` and `ApplicationBarStateChangedEventArgs`. The frame draws the bar through `ApplicationBarPresenter` (a port-only type that replaces the native compositor): circular icon buttons (a 32 DIP glyph inside a 48 DIP, 2 DIP white ring) with labels, the menu button, the menu overlay, minimized mode, colors/opacity, and the page space the bar reserves (an invisible or translucent bar floats over the page, like the original). Differences: the app bar is chrome of the frame, so it does not animate in with the page transition; the default colors are the Metro chrome colors because the native defaults are not observable from managed code; the menu opens as a WPF `Popup`; icon URIs are resolved as application resources and a missing icon renders the button without an image instead of failing.
3. **Theme colors** — Metro colors and text styles are supplied as resources, and `Microsoft.Phone.Shell.PhoneThemeManager` switches the light/dark theme and the accent palette at run time (a palette value or any custom `Color`); `ApplicationTheme.Auto` follows the Windows app theme. The supplied templates use `DynamicResource`, so they update in place. Application XAML that resolves a theme brush with `StaticResource` keeps the value it captured when that XAML was parsed, and colors captured in animation keyframes (for example the `PhoneTextBox` focus background) are refreshed only when the template is re-applied; use `DynamicResource` for theme brushes in application XAML.
4. **Screen metrics** — WP layout constants (800×480, 480×800, 768×1280, etc.) are not imposed. The sample uses desktop-sized windows.

## 4. Projection and 3D

1. **XAML surface** — WPF `UIElement` has no `Projection` property, and WPF does not allow adding a property element to a sealed framework type. The port provides `ProjectionHost` and `ProjectionService` instead; original `<Grid.Projection>` XAML is rewritten as `phone:ProjectionService.Projection` on the same element (wrapped on demand) or as an explicit `ProjectionHost`.
2. **Projection perspective** — the camera reproduces XAML’s fixed 999 DIP perspective depth together with the XAML rotation/offset signs, so projected scale and foreshortening match WP. The remaining difference is `TurnstileFeatherEffect`: the original moves every feathered item’s perspective origin to the vertical center of the page (`GlobalOffsetY` plus a compensating render transform). A `ProjectionHost` is sized to the element it wraps and clips its 3D content, so this port feathers each item about its own vertical center. The rotation angles, stagger delays, and opacity timings are identical to the original.
3. **`Matrix3DProjection`** — the matrix is interpreted in normalized (0..1) coordinates and converted to pixel space. Complex matrices should be validated visually; not every Silverlight matrix convention is covered by tests.
4. **Wrapping limitations** — `ProjectionService` can wrap a direct child of a `Panel` that is not an `ItemsControl` items panel. Assigning a projection uses the same rule: the element is wrapped on demand and the wrapper is released when the projection is cleared. Item containers (such as `ListBoxItem`, `ComboBoxItem` or a custom container) are skipped because WPF owns the children of an items panel and throws `InvalidOperationException` when they are modified; effects that need per-item projection skip those targets. `ContentPresenter` is wrapped like any other panel child: WPF keeps its `TemplatedParent`, template bindings and named template parts working after the re-parent. An element effect always gets a host that directly contains the element: reusing an ancestor host would project the whole page, because pages are hosted by the transition frame's presenters.
5. **Layout** — the host measures and arranges both the 2D child and the hidden 3D viewport. Its desired size is the child’s desired size; explicit `Width`/`Height` on the host are recommended when projecting fixed-size content.
6. **Hit testing** — tested for the host and its content. Nested hosts and transformed content can change hit-test coordinates; verify application-specific interaction.
7. **Text clarity** — `Viewport2DVisual3D` renders the content as a texture, so ClearType subpixel rendering is replaced by grayscale anti-aliasing while an element is projected. Geometry stays 1:1 (an identity projection matches the 2D layout to within 0.01 px), and transitions set `ProjectionHost.Prefer3D` during preparation so the switch happens before the first animated frame instead of on it. No bitmap cache is used: WPF's 3D pipeline already rasterizes the content, and an extra `BitmapCache` softened it.
8. **Software rendering** — 3D rendering falls back to software when hardware acceleration is unavailable. Performance and visual quality may degrade; this is a WPF/platform limitation, not a toolkit feature.

## 5. Transitions and effects

1. **Storyboard state** — WPF throws (`Cannot perform this operation because the specified Storyboard was not applied to this object for interactive control`) when the parameterless storyboard control methods are used on a clock that does not exist yet, and it needs the containing object for `Pause`/`Resume`/`Seek`/`SkipToFill` on a controllable clock. The port's `Transition` therefore starts the storyboard controllably with the element as containing object, tracks begun/completed state itself and reports `Active`/`Stopped` instead of querying a clock that may not exist; it also raises `Completed` at most once per run (WPF re-raises it when a controllable clock is stopped or removed), which is what keeps `EndTransition` from firing twice. `TiltEffect` keeps the same begun flag instead of querying its storyboard. A `CacheMode` cached during a transition is created per transition, never shared: a `Freezable` belongs to its dispatcher.
2. **Template parts** — `TransitionFrame` expects two `ProjectionHost` parts named `FirstContentPresenter` and `SecondContentPresenter`. A custom frame template that uses ordinary `ContentPresenter` parts will not run the transitions.
3. **Transition timing** — the port plays the original storyboard XAML directly (`Transitions/Storyboards/*.xaml`, loaded from the assembly resources at runtime), so durations, key times, angles and easing are the toolkit values; the only edit inside those files is the projection property path. WPF and Silverlight may still evaluate the same easing function slightly differently, so visual parity is close, not bit-exact. **Turnstile deviation:** to match the observed WP 8.1 system animation, `Transitions.cs` pivots the page-level turnstile slightly outside the left edge (`TurnstileCenterOfRotationX = -0.05`, original toolkit: `0`) and scales both phases by `TurnstileDurationScale = 1.2` (toolkit: 0.25 s exit / 0.35 s entry) through `Storyboard.SpeedRatio`. Both are single constants; set them to `0` and `1` to reproduce the original toolkit geometry and timing exactly.
4. **Navigation differences** — the original `TransitionFrame` ignores navigations the application did not initiate (the shell animating the frame out on deactivation). WPF reports `IsNavigationInitiator == false` for every `Frame.Navigate` call and has no shell-driven navigation, so the port drops that guard. The original has no exception handling around transition factories and the port matches it: a throwing `TransitionElement` fails the navigation, there is no fallback event. `Transitions.ProjectionTransition` clears the projection when a projected transition completes or is stopped, so an idle page returns to the plain 2D path.
5. **TurnstileFeatherEffect** — uses per-element wrappers and staggered animations; targets are collected from the page’s visual tree when the storyboard is composed (the attached-property subscriptions only prepare the wrappers, on the dispatcher, so the visual tree is never modified inside a layout pass). Targets that cannot be wrapped are skipped. Cleanup restores opacity, projection, render transform and the 2D render path, and invalidates the element and its wrapper: a wrapper that rendered in 3D is composited on its own surface and would otherwise keep its last 3D frame. The wrapper itself stays attached until the element unloads, so the element is not re-parented twice. The effect is not designed for controls that regenerate their visual tree while the effect is active. A feathered transition on a page without feathered elements has nothing to animate and completes immediately instead of blocking the navigation.
6. **TiltEffect** — assigns the tilt projection through `ProjectionService`, which wraps plain controls in a `ProjectionHost` on demand and releases the wrapper again when the tilt projection is cleared, so only that control is tilted and the visual tree is never modified while a pointer is pressed. The rotation is centred on the control’s first visual child (the template root), matching the original toolkit’s `GetChild(control, 0)`, and `CenterOfRotation` is expressed relative to the layout slot. Mouse input is supported through tunneling preview events, so a container can tilt a child button without consuming its click; list item containers are skipped. The maximum tilt and spring behaviour are approximate, and touch manipulation is the more complete input path.
7. **SlideInEffect** — not implemented (requires Pivot).
8. **FeatheredTransition** — implemented, but the original `TurnstileFeatherTransition` was designed for page content inside a Pivot; the port applies it to the transition frame’s page hosts.

## 6. Controls

1. **Visual fidelity** — control templates approximate the Metro look using WPF primitives. The theme also supplies implicit WP styles for common WPF controls (`Button`, `TextBox`, `ListBox`/`ListBoxItem`, `ComboBox`/`ComboBoxItem`, `CheckBox`, `RadioButton`, `ScrollBar`, `ContextMenu`/`MenuItem`, `ProgressBar`, and `ToolTip`). Some specialized WPF chrome (for example the native calendar inside WPF `DatePicker`) and WP-specific visual states still differ. Functional API surfaces are the priority.
2. **`CustomMessageBox`** — hosted in a WPF `Window`, so it is a separate top-level window rather than an in-page overlay. `Dismiss()` and the result/event contract are preserved. The content is hosted in a `ProjectionHost` and swivels in (`BackwardIn`) and out (`BackwardOut`) like the original; because the dialog is its own window, the page behind it is not visible during the swivel.
3. **`ListPicker`** — the original WP8 sources are ported, including the normal-mode canvas height/translation animation and the full-mode `ListPickerPage` navigation: full mode pushes the page through the host `PhoneApplicationFrame` (the pack URI in `PickerPageUri`) and commits the selection when `Done` is tapped or the page is navigated away from. Differences: the page’s actions are the original `ApplicationBar` (circular check/cancel icon buttons with the localized `done`/`cancel` text, shown for multiple selection and hidden for single selection, icons from the toolkit’s own `Content/ApplicationBar.*.png` resources), the page transitions use the ported turnstile projection, and the page carries the phone colors itself (`Themes/Theme/Dark.xaml` is merged into its own `Resources`) because it is created by the control before it is attached to a tree, where it cannot see `Application.Resources`; template keyframe values therefore use `StaticResource` (a `DynamicResource` inside a template storyboard is not applied under those conditions). The full-mode items flip in one after another: since item containers cannot be wrapped in a `ProjectionHost`, the page’s item templates host their content in one and the animations target that host. The normal-mode canvas height and translation are applied with direct `BeginAnimation` calls (a storyboard cannot animate the template’s `TranslateTransform`), so navigating back from full mode shows the newly selected item, not the first one; the height animation starts only after the selected item has been measured (WPF throws when animating `Height` from an unset origin, unlike Silverlight). Long lists work with the default virtualization: only the containers in the view port are collected for the animation (unrealized containers are skipped), and the page repeats `ScrollIntoView` on dispatcher turns until the selected item’s container exists and is on screen, because the request issued during `Loaded` is dropped by WPF. `SelectedItems`, `SelectionMode` and the multiple-selection summary are supported.
4. **`DatePicker`/`TimePicker`** — use a WPF `DatePicker`/hour-minute selectors in a modal window. WP’s looping selectors, picker page transitions, and date/time picker application bar are not reproduced. `ValueStringFormat`, `ValueString`, `ValueChanged`, and `Header` are supported.
5. **`ExpanderView`** — height animation is simplified; layout of arbitrary nested content may differ from WP. The original `ItemsCanvas`/`ExpanderPanel` part names are preserved. The expander gutter uses an 18-pixel content inset so the vertical rule does not touch the text.
6. **`HubTile`** — the template follows the WP8 original: a surface twice the tile height slides (`TranslateTransform`) between the title band, the image and the back panel, and the flip is a real `PlaneProjection.RotationX` animation (`0→180` and `180→360`) with the front/back panels swapped while the surface is edge-on. The template lives only in `Themes/Controls/HubTile.xaml` (an earlier inline copy in `Themes/Generic.xaml` shadowed it and never slid). Four WPF-specific details: the sliding offset is applied from code because WPF does not apply a storyboard value that binds to the templated parent; the `TranslateTransform` is created in code because a `Freezable` declared in a template is frozen and cannot be animated; the viewport sits in a plain panel because `ProjectionService` only wraps an element whose parent is a panel; and `CenterOfRotationY` is `0.5`, which is the original’s `0.25` measured against the two-tile-high surface. Clipping follows the platform’s projection: `ProjectionHost` clips the *content* to the layout slot but not the projected result, so the band that has slid out of the viewport stays hidden while the magnified near half of the flip draws outside the tile instead of being cut at its edge. `HubTile_ShowsTheSourceImageAndFlipsToTheBackPanel` checks the offsets and the rendered pixels, `HubTile_FlipDrawsOutsideTheTileBounds` checks that the flip spills over the tile bounds, and projections keep rendering past 90 degrees because `ProjectionHost` carries a double-sided quad. On-screen pixel fidelity is not measured. `HubTileService` group freeze/unfreeze is supported.
7. **`PhoneTextBox`** — hint, action icon, and length indicator behavior are implemented. The hint starts at the same offset as the caret and is hidden while the editor has keyboard focus or while an IME is composing (the composed characters are not in `Text` yet), so Pinyin input no longer leaves the placeholder visible. The inner editor is visible and receives focus from the templated control; WP’s built-in text selection and input-scope edge cases may still differ in WPF.
8. **`Rating`** — items are generated in code; half-item and zero-selection behavior are implemented. Mouse click/drag selection is supported in addition to touch manipulation. **Deviation:** with `AllowHalfItemIncrement` the toolkit stored the doubled position count in `Value`, so the filled area covered twice as many items as the pointer indicated; the port stores the value in item units (`ceil(position / width * 2N) / 2`). Template part names are preserved, but custom templates must provide the expected parts.
9. **`ToggleSwitch`** — header, switch foreground, checked/unchecked/indeterminate and click are supported. The thumb and background use animated transitions when the state changes. System accent integration remains approximate.
10. **`ContextMenu`** — derives from WPF `ContextMenu`; `ContextMenuService.ContextMenu` is the attached property. `IsZoomEnabled`, `IsFadeEnabled`, `RegionOfInterest`, and `Owner` are retained but some WP zoom/fade visuals are not reproduced.
11. **`PerformanceProgressBar`** — indeterminate mode is implemented with a simple fade; the original WP performance workarounds are not meaningful in WPF.
12. **`RelativeAnimatingContentControl`** — the control exists, but the original magic-number relative animation rewriting is not ported.
13. **`WrapPanel`** — original layout algorithm ported and tested. WPF’s own `WrapPanel` is a separate type.
14. **`RecurringDaysPicker`** — built on the ported `ListPicker`; localized day names are supplied, but WP’s visual chrome is not.
15. **`Pivot`** — the phone control is driven by DirectManipulation and the compositor; the port produces the same motion with WPF animations (0.25 s content slide with a `QuarticEase`, 0.5 s header scroll, a drag that follows the pointer at the original 600 px/s). Differences: (a) input is a mouse drag plus Left/Right/PageUp/PageDown/Home/End, not touch manipulation — enabling `IsManipulationEnabled` on the pivot would stop touch input from reaching child controls such as buttons; (b) there is no flick velocity detection, a drag changes the item once it passes half the viewport; (c) the header run is circular like the original — the first header follows the last one — but it is rendered once, so a list whose headers are narrower than the viewport shows empty space past the last item; (d) `IsLocked` is a dependency property (the platform exposed a plain property) and fades the headers with a 200 ms animation; (e) there is no custom automation peer, so assistive technology sees WPF’s default `ItemsControl` peer; (f) only the selected item is visible, the outgoing item is collapsed when its slide completes; (g) setting `SelectedIndex`/`SelectedItem` outside the range still throws, as on the phone.
16. **`LongListSelector`** — the public surface of the phone 8.1 control is preserved, but the internals are WPF: a recycling `VirtualizingStackPanel` for list mode, a fixed-cell panel for grid mode, and a `Popup` for the jump list. Differences: (a) grid mode realizes every item (list mode virtualizes); (b) the jump list opens without the phone’s `RotationX` flip animation and uses a plain `Popup`; (c) groups must implement `IList` (the phone code casts groups to `IList` too); (d) the template part named `ViewportControl` hosts the items and is a `ContentControl`, not the phone’s `ViewportControl` scroller, and `VerticalScrollBar` is a real WPF `ScrollBar`; (e) `SelectedItem` is a dependency property; (f) selection has no visual chrome, as on the phone. Group headers, list headers/footers, grid cell sizes, `ScrollTo`, keyboard navigation, `ItemRealized`/`ItemUnrealized` and collection-change refreshes are covered by `LongListSelectorTests`; pixel fidelity and touch manipulation are unverified.
17. **`HyperlinkButton`** — relative and `pack:` URIs navigate through the nearest `NavigationService`/`Frame`; any other absolute URI opens the default shell handler (browser for http/https, mail client for mailto, …). WPF has no named navigation scopes, so `TargetName` is retained but not applied, and absolute external links open regardless of it. The default theme renders string `Content` as underlined text (the phone template); non-string content needs a template such as the sample’s `HyperlinkEmptyStyle`. Click, Enter/Space activation, `CommonStates` and the `IInvokeProvider`/hyperlink automation peer are covered by `HyperlinkButtonTests`; launching a real browser/mail client is replaced by an overridable `OpenExternalUri` hook in tests.

## 7. Localization

1. 51 of the original `.resx` files are embedded and `ControlResources` is generated from the original designer. 50 real cultures are available, plus the neutral resource. The `qps-ploc` pseudo-locale is excluded because NuGet normalizes its culture casing and emits a packaging warning; it is not a user-facing culture.
2. `Thread.CurrentThread.CurrentUICulture` (and WPF’s culture flow) determines the resource culture. There is no language-restart or WP culture-change event.
3. Some strings that WP supplied from the platform (e.g., application bar menu labels, system tray text) are not part of the toolkit resources and are not localized by the port.

## 8. Packaging and platform

1. The package targets `net10.0-windows` with `UseWPF`. It cannot be consumed by a WP/Silverlight project.
2. The package includes both compiled assemblies, theme resources, localization resources, license, README, third-party notices, XML documentation, and the documentation set. It does not include the sample app or tests.
3. There is no WP SDK, `Microsoft.Phone`, Silverlight, or `System.Windows.Interactivity` dependency.
4. The package ID is `Community.Phone.Controls.Toolkit`; it contains the toolkit assembly of the same name and the `Community.Phone` compatibility layer. The root namespaces stay `Microsoft.Phone.Controls` / `Microsoft.Phone.Shell`.
5. The package is preview-quality. It is not a drop-in binary replacement for the original `Microsoft.Phone.Controls.Toolkit.dll`.

## 9. Test and verification gaps

The following are not fully automated:

1. Visual comparison against the original WP controls (no WP emulator/device in this environment).
2. Pixel comparison of transitions against the WP 8.1 emulator (navigation tests verify projection state changes and that a mid-transition frame renders as a perspective trapezoid; timing was measured against the original key times).
3. Application bar and system tray visuals: the bar's presenter now renders (bar height, slot metrics, the circular icon chrome, colors, menu popup and the reserved-space rule are covered by `ShellTests`, and the ListPicker full-mode page's bar by dedicated ListPicker tests). The ListPicker full-mode page (item chrome, single/multiple selection styles, staggered flip) and its bar were compared with WP 8.1 screenshots; the bar on other pages was not compared pixel by pixel, and `SystemTray` is still an API model with nothing painted.
4. Input is driven through routed events rather than the OS: no test needs a real desktop pointer (the `Rating` test calls the control's internal hit-test path directly) and no physical touch device is available, so TiltEffect, manipulation and drag behaviour are covered by synthetic events only.
5. Date/time picker modal window interaction (API/value tests; no UI automation).
6. All 50 packaged cultures (resource presence is verified; localized formatting is covered by converter tests for representative cultures).
7. High-DPI, multi-monitor, and software-rendering 3D behavior.
8. Physical touch hardware (synthetic mouse paths and the manipulation code are covered; no touch device is available in this environment).

## 10. Manual verification steps

Use these steps to check the areas that are not fully automated.

### Sample application

```powershell
dotnet run --project src\Community.Phone.Controls.Toolkit.Sample
```

1. Confirm the window opens and shows the Home page.
2. Click **Open detail (turnstile)** and confirm the turnstile transition runs on entry; click **Go back** and confirm the matching exit/entry transition runs.
3. Click **Animate projection**; the status line should show `RotationX` changing while the panel tilts in real 3D. The panel also rotates continuously on `RotationY`. The second panel has no `ProjectionHost` (its projection is assigned directly and wrapped on demand) and rotates on its own.
4. Toggle the switch and verify the thumb slides; click and drag across the Rating stars; type in the hint text box; open the ListPicker and hover/select items; expand the ExpanderView; open the ContextMenu; show the CustomMessageBox; inspect DatePicker, TimePicker, RecurringDaysPicker, PerformanceProgressBar, WrapPanel, HubTile, and the WPF base controls; in the Pivot sample, drag the content, click a header and tick the lock box. Open the `longlistselector` sample: the grouped list shows list header, group headers and items; clicking a group header opens the jump-list grid and choosing a group scrolls to it; **Toggle grid** switches the items to fixed cells; **Go back** returns to the index.
5. Check `%TEMP%\community-phone-toolkit-sample-error.txt` after closing; it should not exist or should be empty.

### TiltEffect

1. Add `phone:TiltEffect.IsTiltEnabled="True"` to a container with a button.
2. Press and hold the button; it should tilt in the press direction and spring back on release.
3. Verify the button still receives clicks.

### TurnstileFeatherEffect

1. Add `phone:TurnstileFeatherEffect.FeatheringIndex` to several `TextBlock`s.
2. Navigate to the page; items should feather in with staggered timing.
3. Navigate away; wrappers should be removed and the visual tree restored.

### Package consumption

```powershell
dotnet pack src\Community.Phone.Controls.Toolkit -c Release -o artifacts\packages
dotnet new wpf -n Consumer -o artifacts\consumer
dotnet add artifacts\consumer package Community.Phone.Controls.Toolkit --source artifacts\packages
dotnet build artifacts\consumer
```

Then add a `ProjectionHost` and a ported control to `MainWindow.xaml` and run the consumer. This verifies the package, theme resources, and namespace resolution outside the repository.

## 11. Roadmap candidates

In rough priority order, if further work is funded:

1. `LongListMultiSelector` on top of WPF virtualization.
2. `AutoCompleteBox` using WPF popup/filtering.
3. Application bar and system tray visual presenters in the frame template.
4. More exact WP projection calibration and visual regression tests.
5. `GestureService` using WPF touch/manipulation.
6. `SlideInEffect` on top of the ported `Pivot`.
7. Visual-state and theme parity for all control templates.
8. Automated UI tests for modal pickers, message box, and transitions.
