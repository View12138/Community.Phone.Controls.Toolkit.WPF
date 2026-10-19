# Known limitations

This port targets a truthful subset of the archived Windows Phone Toolkit. The list below is explicit so that no unsupported feature is mistaken for a finished one. If a feature is not listed as implemented in [api-compatibility.md](api-compatibility.md), assume it is not supported.

## 1. Unsupported controls and modules

| Original module | Status | Why / alternative |
|---|---|---|
| `AutoCompleteBox` | Not implemented | Silverlight popup, selection adapter, filtering and keyboard infrastructure; no WPF replacement in this package. Use an application-specific WPF autocomplete control. |
| `LongListSelector` (WP7) | Not implemented | Large virtualizing list control with grouping, jump list and item animation. Use `ListBox`/`ListView` with `CollectionViewSource` grouping. |
| `LongListMultiSelector` (WP8) | Not implemented | Depends on WP8 list infrastructure and selection visuals. Use `ListView` with `SelectionMode="Multiple"`. |
| `MultiselectList` | Not implemented | Not ported. |
| `Maps` / `MapExtensions` | Not implemented | No WPF map provider; depends on `Microsoft.Phone.Maps`. |
| `TransferControl` | Not implemented | Depends on `Microsoft.Phone.BackgroundTransfer`. |
| `SpeechTextBox` | Not implemented | Depends on `Windows.Phone.Speech.Recognition`. |
| `LockablePivot` / Pivot integration | Not implemented | Depends on the WP `Pivot` control. |
| `SlideInEffect` | Not implemented | Depends on `Pivot` and its selection events. |
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
7. **Journal** — WPF `Frame` journal behavior is retained. Silverlight page-instance caching and navigation modes are not identical.
8. **Navigation failures** — `NavigationFailed` is raised as an event on the port’s frame; a navigation failure does not automatically terminate the app as on WP.

## 3. Shell chrome

1. **System tray** — `SystemTray.IsVisible`, `SystemTray.ProgressIndicator`, `SystemTray.Opacity`, `SystemTray.BackgroundColor`, `SystemTray.ForegroundColor`, and state-change events are implemented as an API model. The default frame template does **not** draw a complete Windows Phone status bar (signal, battery, clock, full-screen behavior). Applications that need the visual chrome must render it themselves.
2. **Application bar** — `ApplicationBar`, `ApplicationBarIconButton`, `ApplicationBarMenuItem`, `IsVisible`, `IsMenuEnabled`, `BackgroundColor`, `ForegroundColor`, `Opacity`, and command/click events are modeled. `Mode`/`StateChanged` are not present in this preview. The default visual presenter is **not** included in the frame template. The sample uses ordinary WPF buttons to demonstrate command wiring.
3. **Theme colors** — Metro colors and text styles are supplied as resources, but automatic light/dark theme switching and WP accent-color integration are not implemented. Applications can override the resource keys.
4. **Screen metrics** — WP layout constants (800×480, 480×800, 768×1280, etc.) are not imposed. The sample uses desktop-sized windows.

## 4. Projection and 3D

1. **XAML surface** — WPF `UIElement` has no `Projection` property, and WPF does not allow adding a property element to a sealed framework type. The port provides `ProjectionHost` and `ProjectionService` instead; original `<Grid.Projection>` XAML must be rewritten.
2. **Projection perspective** — the camera reproduces XAML’s fixed 999 DIP perspective depth together with the XAML rotation/offset signs, so projected scale and foreshortening match WP. The remaining difference is `TurnstileFeatherEffect`: the original moves every feathered item’s perspective origin to the vertical center of the page (`GlobalOffsetY` plus a compensating render transform). A `ProjectionHost` is sized to the element it wraps and clips its 3D content, so this port feathers each item about its own vertical center. The rotation angles, stagger delays, and opacity timings are identical to the original.
3. **`Matrix3DProjection`** — the matrix is interpreted in normalized (0..1) coordinates and converted to pixel space. Complex matrices should be validated visually; not every Silverlight matrix convention is covered by tests.
4. **Wrapping limitations** — `ProjectionService` can wrap direct children of a `Panel` and some content controls. It intentionally does not wrap `ListBoxItem` or `ContentPresenter` because replacing those elements would break item generation and templating. Effects that need per-item projection skip such targets. An element effect always gets a host that directly contains the element: reusing an ancestor host would project the whole page, because pages are hosted by the transition frame's presenters.
5. **Layout** — the host measures and arranges both the 2D child and the hidden 3D viewport. Its desired size is the child’s desired size; explicit `Width`/`Height` on the host are recommended when projecting fixed-size content.
6. **Hit testing** — tested for the host and its content. Nested hosts and transformed content can change hit-test coordinates; verify application-specific interaction.
7. **Text clarity** — `Viewport2DVisual3D` renders the content as a texture, so ClearType subpixel rendering is replaced by grayscale anti-aliasing while an element is projected. Geometry stays 1:1 (an identity projection matches the 2D layout to within 0.01 px), and transitions set `ProjectionHost.Prefer3D` during preparation so the switch happens before the first animated frame instead of on it. No bitmap cache is used: WPF's 3D pipeline already rasterizes the content, and an extra `BitmapCache` softened it.
8. **Software rendering** — 3D rendering falls back to software when hardware acceleration is unavailable. Performance and visual quality may degrade; this is a WPF/platform limitation, not a toolkit feature.

## 5. Transitions and effects

1. **Storyboard state** — WPF `Storyboard.GetCurrentState()` throws when the storyboard has no containing clock. The port’s `Transition` tracks begun state and reports `Active`/`Stopped` itself. This preserves the `ITransition` contract for the implemented scenarios.
2. **Template parts** — `TransitionFrame` expects two `ProjectionHost` parts named `FirstContentPresenter` and `SecondContentPresenter`. A custom frame template that uses ordinary `ContentPresenter` parts will not run the transitions.
3. **Transition timing** — durations, key times, angles and easing were copied from the original storyboards, but WPF and Silverlight easing implementations may differ slightly. Visual parity is close, not bit-exact. **Turnstile deviation:** to match the observed WP 8.1 system animation, `Transitions.cs` pivots the page-level turnstile slightly outside the left edge (`TurnstileCenterOfRotationX = -0.05`, original toolkit: `0`) and scales both phases by `TurnstileDurationScale = 1.2` (toolkit: 0.25 s exit / 0.35 s entry). Both are single constants; set them to `0` and `1` to reproduce the original toolkit geometry and timing exactly.
4. **TurnstileFeatherEffect** — uses per-element wrappers and staggered animations; targets are collected from the page’s visual tree when the storyboard is composed (the attached-property subscriptions only prepare the wrappers, on the dispatcher, so the visual tree is never modified inside a layout pass). Targets that cannot be wrapped are skipped. Cleanup restores the original visual tree, but the effect is not designed for controls that regenerate their visual tree while the effect is active. A feathered transition on a page without feathered elements has nothing to animate and completes immediately instead of blocking the navigation.
5. **TiltEffect** — wraps plain controls in a `ProjectionHost` when they load, so only that control is tilted and the visual tree is never modified while a pointer is pressed; the wrapper is removed again when tilt is disabled. Mouse input is supported through tunneling preview events; list item containers are skipped. Tilt is applied to the whole control, not to individual item containers, and the maximum tilt/spring behavior is approximate. Mouse tilt ends on a mouse-up routed through the enabled container; touch manipulation is the more complete input path.
6. **SlideInEffect** — not implemented (requires Pivot).
7. **FeatheredTransition** — implemented, but the original `TurnstileFeatherTransition` was designed for page content inside a Pivot; the port applies it to the transition frame’s page hosts.

## 6. Controls

1. **Visual fidelity** — control templates approximate the Metro look using WPF primitives. The theme also supplies implicit WP styles for common WPF controls (`Button`, `TextBox`, `ListBox`/`ListBoxItem`, `ComboBox`/`ComboBoxItem`, `CheckBox`, `RadioButton`, `ScrollBar`, `ContextMenu`/`MenuItem`, `ProgressBar`, and `ToolTip`). Some specialized WPF chrome (for example the native calendar inside WPF `DatePicker`) and WP-specific visual states still differ. Functional API surfaces are the priority.
2. **`CustomMessageBox`** — hosted in a WPF `Window`, so it is a separate top-level window rather than an in-page overlay. `Dismiss()` and the result/event contract are preserved. The content is hosted in a `ProjectionHost` and swivels in (`BackwardIn`) and out (`BackwardOut`) like the original; because the dialog is its own window, the page behind it is not visible during the swivel.
3. **`ListPicker`** — normal mode uses a WPF `Popup` with the WP list-item hover/selection styling; full mode uses a modal `Window`. It does not navigate to `ListPickerPage`, so `PickerPageUri`, full-mode page lifecycle, and WP transition behavior are not reproduced. `SelectedItems` and `SelectionMode` are supported.
4. **`DatePicker`/`TimePicker`** — use a WPF `DatePicker`/hour-minute selectors in a modal window. WP’s looping selectors, picker page transitions, and date/time picker application bar are not reproduced. `ValueStringFormat`, `ValueString`, `ValueChanged`, and `Header` are supported.
5. **`ExpanderView`** — height animation is simplified; layout of arbitrary nested content may differ from WP. The original `ItemsCanvas`/`ExpanderPanel` part names are preserved. The expander gutter uses an 18-pixel content inset so the vertical rule does not touch the text.
6. **`HubTile`** — the template follows the WP8 original: a surface twice the tile height slides (`TranslateTransform`) between the title band, the image and the back panel, and the flip is a real `PlaneProjection.RotationX` animation (`0→180` and `180→360`) with the front/back panels swapped while the surface is edge-on. Two WPF-specific details: the back panel is pre-mirrored with `ScaleTransform ScaleY=-1` instead of the original’s `PlaneProjection RotationX=180` (visually identical, cheaper), and the host is switched to `Prefer3D` for the duration of the flip. Pixel fidelity is unverified. `HubTileService` group freeze/unfreeze is supported.
7. **`PhoneTextBox`** — hint, action icon, and length indicator behavior are implemented. The hint starts at the same offset as the caret and is hidden while the editor has keyboard focus or while an IME is composing (the composed characters are not in `Text` yet), so Pinyin input no longer leaves the placeholder visible. The inner editor is visible and receives focus from the templated control; WP’s built-in text selection and input-scope edge cases may still differ in WPF.
8. **`Rating`** — items are generated in code; half-item and zero-selection behavior are implemented. Mouse click/drag selection is supported in addition to touch manipulation. **Deviation:** with `AllowHalfItemIncrement` the toolkit stored the doubled position count in `Value`, so the filled area covered twice as many items as the pointer indicated; the port stores the value in item units (`ceil(position / width * 2N) / 2`). Template part names are preserved, but custom templates must provide the expected parts.
9. **`ToggleSwitch`** — header, switch foreground, checked/unchecked/indeterminate and click are supported. The thumb and background use animated transitions when the state changes. System accent integration remains approximate.
10. **`ContextMenu`** — derives from WPF `ContextMenu`; `ContextMenuService.ContextMenu` is the attached property. `IsZoomEnabled`, `IsFadeEnabled`, `RegionOfInterest`, and `Owner` are retained but some WP zoom/fade visuals are not reproduced.
11. **`PerformanceProgressBar`** — indeterminate mode is implemented with a simple fade; the original WP performance workarounds are not meaningful in WPF.
12. **`RelativeAnimatingContentControl`** — the control exists, but the original magic-number relative animation rewriting is not ported.
13. **`WrapPanel`** — original layout algorithm ported and tested. WPF’s own `WrapPanel` is a separate type.
14. **`RecurringDaysPicker`** — built on the ported `ListPicker`; localized day names are supplied, but WP’s visual chrome is not.

## 7. Localization

1. 51 of the original `.resx` files are embedded and `ControlResources` is generated from the original designer. 50 real cultures are available, plus the neutral resource. The `qps-ploc` pseudo-locale is excluded because NuGet normalizes its culture casing and emits a packaging warning; it is not a user-facing culture.
2. `Thread.CurrentThread.CurrentUICulture` (and WPF’s culture flow) determines the resource culture. There is no language-restart or WP culture-change event.
3. Some strings that WP supplied from the platform (e.g., application bar menu labels, system tray text) are not part of the toolkit resources and are not localized by the port.

## 8. Packaging and platform

1. The package targets `net10.0-windows` with `UseWPF`. It cannot be consumed by a WP/Silverlight project.
2. The package includes the compiled assembly, theme resources, localization resources, license, README, third-party notices, XML documentation, and the documentation set. It does not include the sample app or tests.
3. There is no WP SDK, `Microsoft.Phone`, Silverlight, or `System.Windows.Interactivity` dependency.
4. The package ID is `Community.Phone.Controls.Toolkit.WPF`; the assembly and root namespace are `Community.Phone.Controls.Toolkit.WPF` and `Microsoft.Phone.Controls`, respectively.
5. The package is preview-quality. It is not a drop-in binary replacement for the original `Microsoft.Phone.Controls.Toolkit.dll`.

## 9. Test and verification gaps

The following are not fully automated:

1. Visual comparison against the original WP controls (no WP emulator/device in this environment).
2. Full transition frame visual verification (navigation tests verify projection state changes, not rendered frames).
3. Application bar and system tray visuals (API tests only; visual presenter pending).
4. Real pointer input is covered for Rating (desktop click) and ToggleSwitch/PhoneTextBox interaction; TiltEffect uses synthetic routed events and no physical touch device is available in this environment.
5. Date/time picker modal window interaction (API/value tests; no UI automation).
6. All 50 packaged cultures (resource presence is verified; localized formatting is covered by converter tests for representative cultures).
7. High-DPI, multi-monitor, and software-rendering 3D behavior.
8. Physical touch hardware (synthetic mouse paths and the manipulation code are covered; no touch device is available in this environment).

## 10. Manual verification steps

Use these steps to check the areas that are not fully automated.

### Sample application

```powershell
dotnet run --project src\Community.Phone.Controls.Toolkit.WPF.Sample
```

1. Confirm the window opens and shows the Home page.
2. Click **Open detail (turnstile)** and confirm the turnstile transition runs on entry; click **Go back** and confirm the matching exit/entry transition runs.
3. Click **Animate projection**; the status line should show `RotationX` changing while the panel tilts in real 3D. The panel also rotates continuously on `RotationY`.
4. Toggle the switch and verify the thumb slides; click and drag across the Rating stars; type in the hint text box; open the ListPicker and hover/select items; expand the ExpanderView; open the ContextMenu; show the CustomMessageBox; inspect DatePicker, TimePicker, RecurringDaysPicker, PerformanceProgressBar, WrapPanel, HubTile, and the WPF base controls.
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
dotnet pack src\Community.Phone.Controls.Toolkit.WPF -c Release -o artifacts\packages
dotnet new wpf -n Consumer -o artifacts\consumer
dotnet add artifacts\consumer package Community.Phone.Controls.Toolkit.WPF --source artifacts\packages
dotnet build artifacts\consumer
```

Then add a `ProjectionHost` and a ported control to `MainWindow.xaml` and run the consumer. This verifies the package, theme resources, and namespace resolution outside the repository.

## 11. Roadmap candidates

In rough priority order, if further work is funded:

1. `LongListSelector`/`LongListMultiSelector` on top of WPF virtualization.
2. `AutoCompleteBox` using WPF popup/filtering.
3. Application bar and system tray visual presenters in the frame template.
4. More exact WP projection calibration and visual regression tests.
5. `GestureService` using WPF touch/manipulation.
6. `SlideInEffect` using a WPF `Pivot`-like control.
7. Visual-state and theme parity for all control templates.
8. Automated UI tests for modal pickers, message box, and transitions.
