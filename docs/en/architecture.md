# Architecture

## 1. Goals and constraints

The project ports the archived Windows Phone Toolkit to a modern WPF package while preserving the original public API wherever the platform allows it. The hard constraints that shaped the design were:

- WPF has no `UIElement.Projection` property and no way to add a CLR/XAML property to an existing WPF type such as `Grid`.
- WPF has its own `Frame`, `Page`, `ContextMenu`, `WrapPanel`, `DatePicker`, `HeaderedItemsControl`, and `Application` types. Defining types with the same names in `System.Windows.*` would create ambiguity and would violate the “do not redefine WPF types” rule.
- Windows Phone services such as `PhoneApplicationFrame`, `PhoneApplicationService`, `SystemTray`, maps, background transfer, speech, and Pivot are not present in WPF.
- The original XAML uses Silverlight-specific resource keys and animation paths such as `(UIElement.Projection).(PlaneProjection.RotationY)`.

The port therefore uses **compatibility types in the original `Microsoft.Phone.Controls` / `Microsoft.Phone.Shell` namespaces**, explicit `ProjectionHost` wrappers for true 3D, and WPF-native base types for framework-adjacent controls.

## 2. Repository and projects

```text
Community.Phone.Controls.Toolkit.slnx
├─ src/Community.Phone.Controls.Toolkit.WPF/          (single distributable assembly)
│  ├─ Projection/    Projection, PlaneProjection, Matrix3DProjection, ProjectionHost, ProjectionService
│  ├─ Navigation/    PhoneApplicationFrame, PhoneApplicationPage, orientation types
│  ├─ Shell/         PhoneApplicationService, SystemTray, ApplicationBar
│  ├─ Transitions/   ITransition/Transition, TransitionFrame, TransitionService, transition families
│  ├─ Effects/       TiltEffect, TurnstileFeatherEffect
│  ├─ Controls/      WrapPanel, ToggleSwitch, Rating, PhoneTextBox, ExpanderView, HubTile,
│  │                 ListPicker, ContextMenu, CustomMessageBox, DatePicker/TimePicker,
│  │                 PerformanceProgressBar, converters, helpers
│  ├─ LocalizedResources/  51 .resx resources copied from the original localization project
│  └─ Themes/Generic.xaml  Metro palette, fonts, frame templates, control templates
├─ src/Community.Phone.Controls.Toolkit.WPF.Sample/   runnable WPF sample
└─ tests/Community.Phone.Controls.Toolkit.WPF.Tests/  xUnit tests (STA helper for WPF)
```

The single package ID is `Community.Phone.Controls.Toolkit.WPF`. The package contains the main assembly, its satellite resources, theme dictionary, XML documentation, README, and license. There are no internal packages that a consumer must install separately.

## 3. Namespace and API strategy

| Original namespace | Port namespace | Reason |
|---|---|---|
| `Microsoft.Phone.Controls` | `Microsoft.Phone.Controls` | Preserves toolkit API names. |
| `Microsoft.Phone.Controls.Primitives` | `Microsoft.Phone.Controls.Primitives` | Preserves primitive API names. |
| `Microsoft.Phone.Controls.LocalizedResources` | same | Preserves resource class. |
| `Microsoft.Phone.Shell` | `Microsoft.Phone.Shell` | Preserves system tray/application service API. |
| `Microsoft.Phone.Maps.Toolkit` | not implemented | No WPF map provider. |
| `System.Windows.Controls` (toolkit’s own `WrapPanel`, `LengthConverter`, `TimeTypeConverter`) | `Microsoft.Phone.Controls` or WPF native | Avoid redefining WPF types. `WrapPanel` is in `Microsoft.Phone.Controls`; WPF’s own `WrapPanel` remains untouched. |
| `System.Windows` (`HierarchicalDataTemplate` in the toolkit) | WPF native | Use `System.Windows.HierarchicalDataTemplate`. |

WPF has a `System.Windows.Controls.ContextMenu`; the port’s `Microsoft.Phone.Controls.ContextMenu` derives from it. XAML users must choose the `phone:` prefix. This is a **requires migration** case, not a claim of identical XAML.

## 4. Projection subsystem

### 4.1 Public model

- `Projection : Animatable` — base class with `ProjectionChanged`, `IsIdentity`, `ToMatrix3D(Size)`.
- `PlaneProjection : Projection` — dependency properties:
  `CenterOfRotationX/Y/Z`, `GlobalOffsetX/Y/Z`, `LocalOffsetX/Y/Z`, `RotationX/Y/Z`.
- `Matrix3DProjection : Projection` — `ProjectionMatrix` in normalized 0..1 space.
- `ProjectionHost : FrameworkElement` — hosts one `Child` and renders it with a `Viewport3D`/`Viewport2DVisual3D` when the projection is non-identity; identity rendering uses the normal 2D tree.
- `ProjectionService` — attached `Projection` property and `EnsureProjectionHost/ReleaseProjectionHost` helpers. Setting the attached property on an element applies it to the nearest `ProjectionHost`; setting it on a host applies it directly.

### 4.2 Rendering choice

Three routes were evaluated:

1. **WPF `Viewport3D` + `Viewport2DVisual3D`** — chosen. It renders a live 2D visual on a 3D plane, preserves WPF hit testing through `SetIsVisualHostMaterial`, supports arbitrary WPF controls (text, input, popups), and is GPU-composited.
2. **Screenshot/VisualBrush plane** — rejected as the default. It loses interactivity and is a static snapshot; only suitable for short, non-interactive effects and not used for the projection implementation.
3. **Windows Composition/D3D interop** — rejected for portability and packaging complexity. It would require a Windows App SDK/Composition dependency and a separate hosting strategy; WPF 3D already satisfies the required interaction and perspective behavior.

### 4.3 Geometry and perspective

The host creates a rectangular mesh centered at the element origin:

```text
positions (Y up):  (-w/2, +h/2, 0), (+w/2, +h/2, 0), (+w/2, -h/2, 0), (-w/2, -h/2, 0)
texture coords:    (0,0), (1,0), (1,1), (0,1)
triangles:         0,2,1 and 0,3,2
```

A `PerspectiveCamera` is placed at `(0, 0, depth)` with `depth = PerspectiveDepth`, which defaults to `999` DIP. The field of view is derived from the viewport **width**: `fov = 2 * atan(viewportWidth / (2 * depth))`. WPF's `PerspectiveCamera.FieldOfView` is horizontal and derives the vertical field of view from the viewport aspect ratio, so a width-based field of view keeps an identity projection exactly 1:1 with the layout slot. Deriving the field of view from the height instead shrinks every projected element by the aspect ratio (a 480x800 page rendered at 288x480 during every 3D transition and snapped back to full size when the transition finished).

The 999 DIP depth is XAML's fixed `PlaneProjection` perspective (WinUI's `PlaneProjection` uses `M34 = -1/999` about the element center), so the effective scale of a point at depth `z` is `999 / (999 - z)` independent of element size. This matches WP's perspective instead of a size-proportional camera approximation.

`PlaneProjection.ToMatrix3D` uses the row-vector convention:

```text
p' = (((p - center) + localOffset) * R) + center + globalOffset
R = Rz * Ry * Rx    (rotations in degrees)
center = ((CenterOfRotationX - 0.5) * width, (0.5 - CenterOfRotationY) * height, CenterOfRotationZ)
Y offsets are negated because WPF 3D uses +Y up and screen coordinates use +Y down.
RotationY is negated because XAML's positive RotationY brings the right edge towards
the viewer, while a positive rotation around WPF's +Y axis moves it away.
```

The matrix is assigned to `Viewport2DVisual3D.Transform`. Camera clipping, back-face behavior, and hit testing are handled by WPF’s 3D pipeline. The host recomputes the geometry, the camera field of view, and the transform when the size or the projection changes.

### 4.4 XAML compatibility boundary

WPF cannot make `<Grid.Projection>` compile because `Grid` has no such property. The supported forms are:

```xml
<!-- Explicit host; preserves live interaction and true projection. -->
<phone:ProjectionHost>
    <Grid>...</Grid>
    <phone:ProjectionHost.Projection>
        <phone:PlaneProjection RotationY="-35" />
    </phone:ProjectionHost.Projection>
</phone:ProjectionHost>

<!-- Attached property on a ProjectionHost; enables storyboard property paths. -->
<phone:ProjectionHost
    phone:ProjectionService.Projection="{StaticResource MyProjection}">
    <Grid>...</Grid>
</phone:ProjectionHost>
```

The original `<Grid.Projection>` form is **not** source-compatible; migration requires the wrapper. This is recorded in the compatibility matrix and known limitations.

### 4.5 Effects integration

`ProjectionService.EnsureProjectionHost` can wrap a direct child of a `Panel` in a `ProjectionHost`, copying layout properties (size, margin, alignment, grid row/column, z-index, dock, canvas position) to the host. Only a host that **directly contains** the element is reused: pages are hosted by the transition frame’s presenters, so reusing an ancestor host would project the whole page instead of the element. `TiltEffect` wraps a tiltable control when it loads (and unwraps it when tilt is disabled) so the visual tree is never modified while a pointer is pressed; `TurnstileFeatherEffect` wraps a feathered element when it subscribes (and unwraps it when the feathering index is cleared), so a transition never re-parents content while it is animating. `ListBoxItem` and `ContentPresenter` targets are deliberately skipped because replacing item containers breaks item generation/virtualization.

Because the outgoing transition is read from the page being left, a page that wants a specific exit must declare it (or set it before navigating). The sample does the latter in `SampleNavigator` so that entering *and* leaving the feathering sample uses the feather family.

### 4.6 Render path switching

A host renders through the 2D path while its projection is the identity and through the `Viewport3D` path otherwise. Both paths are geometrically identical — an identity projection matches the 2D layout to within 0.01 px — but they rasterize text differently (ClearType versus grayscale anti-aliasing of the 3D texture). `ProjectionHost.Prefer3D` keeps the 3D path active while the projection is still the identity, so `TransitionFrame`, `TiltEffect`, and `TurnstileFeatherEffect` switch the render path while they prepare the animation instead of on its first frame. Transition presenters do not apply a `BitmapCache`: the 3D pipeline already rasterizes the content, and an extra 1x cache only softened it.

Measured frame pacing on the development machine (`CompositionTarget.Rendering`, 480×800 content): 2D baseline ~150 fps, a projected page ~92–96 fps, six independently projected feather items ~64–68 fps, and the 2D→3D switch costs a single ~6 ms frame. Wrapping a feather target up front (at subscribe time) keeps that cost out of the transition, because re-parenting elements while the animation runs forces a full layout pass per target.

## 5. Navigation subsystem

### 5.1 `PhoneApplicationFrame`

Derives from WPF `Frame`. WPF’s navigation journal, `NavigationService`, `CanGoBack`, and URI navigation are retained. The port adds:

- `Orientation` / `SupportedOrientations` DPs.
- `OrientationChanged` event; orientation is inferred from the frame size (width > height = landscape).
- Page lifecycle dispatch: `Navigating` calls the current page’s `OnNavigatingFrom`; `Navigated` calls the previous page’s `OnNavigatedFrom` and the new page’s `OnNavigatedTo`.
- `GoBack()` that first raises `PhoneApplicationPage.BackKeyPress`.
- `Navigate(PhoneApplicationPage, IDictionary<string,string>)` for navigation parameters, merged with any URI query string into a `NavigationContext` that is assigned to the page before `OnNavigatedTo`.
- Page state is stored by the frame per page instance, so it survives a back navigation to the same page.

WPF `Page` does not expose the WP `OnNavigatedTo/OnNavigatedFrom/OnNavigatingFrom` protected virtual methods, so the port declares them as virtual methods on `PhoneApplicationPage` and raises them from the frame. This preserves the API and gives deterministic ordering, but the methods are not called by WPF’s own `NavigationService` if a page is hosted outside a phone frame.

### 5.2 `PhoneApplicationPage`

Derives from WPF `Page` and adds `State`, `NavigationContext`, `Orientation`, `SupportedOrientations`, `ApplicationBar`, `NavigatedTo`, `NavigatingFrom`, `NavigatedFrom`, `BackKeyPress`, and `OrientationChanged`. `NavigationContext` exposes `Uri`, `ExtraData`, and an ordinal-ignore-case `QueryString` dictionary.

### 5.3 `TransitionFrame`

Derives from `PhoneApplicationFrame`. The template has two `ProjectionHost` parts named `FirstContentPresenter` and `SecondContentPresenter`. The original code and presenter-flipping logic are preserved; the part type changed from `ContentPresenter` to `ProjectionHost` so transitions can project pages.

Navigation transition sequence:

1. `Navigating`: capture the old page, choose `NavigationOutTransition.Forward/Backward`, get an `ITransition`, prepare the old host, and start the exit transition.
2. `OnContentChanged`: place the new page in the inactive host, get the `NavigationInTransition`, and either start the entrance transition or store it until the exit finishes.
3. Exit completion: stop/restore the old transition, hide the old host, then start the entrance transition.
4. Entrance completion: stop/restore, restore interactivity, raise `EndTransition`.
5. `NavigationStopped`: restore the old content and cancel stored transitions.

`Transition` saves/restores `CacheMode` and `IsHitTestVisible`; `ProjectionTransition` also saves/restores the attached projection. Because WPF’s `Storyboard.GetCurrentState()` requires a containing object for controllable clocks, `Transition` tracks its own begun/completed state and returns `ClockState.Active` while the storyboard is running. `Completed` is raised at most once per run, and `Stop()` cancels any pending completion so `EndTransition` cannot fire twice. The animation itself is a normal WPF `Storyboard` with explicit targets, which was verified in tests by observing the animated `PlaneProjection.RotationY`.

`TransitionFrame` also guards against duplicate `Navigating`/content-change notifications, clears stored transitions on rapid navigation, and raises `TransitionFailed` (with the exception, element, and exit/entry direction) if a transition factory or `Begin` fails. In that case the frame falls back to an untransitioned navigation and restores hit testing/cache state instead of leaving the page hidden or non-interactive.

### 5.4 Transition families

The original 19 XAML storyboards were re-expressed in `Transitions.cs` using `DoubleAnimation`/`DoubleAnimationUsingKeyFrames` with the same timings and easing functions. Storyboards target the `PlaneProjection` object directly, avoiding Silverlight property paths.

Families: Roll, Rotate (8 modes), Slide (8 modes), Swivel (6 modes), Turnstile (4 modes), TurnstileFeather (4 modes). `TransitionService` and `NavigationTransition`/`NavigationInTransition`/`NavigationOutTransition` preserve the original attached-property shape.

## 6. Effects

### `TiltEffect`

Preserves `IsTiltEnabled`, `SuppressTilt`, `TiltableItems`, `UseLogarithmicEase`. On a press it finds the nearest tiltable ancestor, ensures a `ProjectionHost`, creates a `PlaneProjection`, and applies the original normalized point → RotationX/RotationY/GlobalOffsetZ math. Mouse support uses tunneling `PreviewMouseDown/Move/Up` so a container can tilt a child button without marking the button’s click event handled; touch manipulation is also supported. The return storyboard targets the projection directly, tracks whether it has been applied (avoiding WPF `Storyboard` state exceptions), and clears its per-interaction state so it is safe across UI threads. Plain WPF controls are wrapped on demand; list item containers are not wrapped.

### `TurnstileFeatherEffect`

Preserves `FeatheringIndex`, `NonPermittedTypes`, and the staggered timing constants. Targets are collected from the page's **visual tree** when the storyboard is composed (`CollectTargets`, stable order by index), because an incoming page is composed before it is connected to the visual tree and the attached-property notifications have not run at that point; `TransitionFrame` forces a layout pass on the incoming presenter before composing so the targets have real geometry. The attached property subscriptions only prepare the wrappers, on the dispatcher after `Loaded`/`SizeChanged` (wrapping inside a layout pass can start a layout cycle), and a wrapper-induced `Unloaded` does not cancel the subscription. When a `TurnstileFeatherTransition` begins, the effect ensures a `ProjectionHost` for each on-screen target, assigns a `PlaneProjection` with `CenterOfRotationX = -0.2`, and composes RotationY/opacity animations with the original 40–50 ms stagger. Targets that cannot be wrapped (item containers, content presenters) are skipped. Cleanup restores opacity, removes the projection, and releases the wrapper.

A transition whose storyboard ends up empty (for example feathering a page that has no feathered elements) completes on the next dispatcher pass instead of blocking the frame — otherwise the incoming page would stay non-interactive forever.

`SlideInEffect` is not implemented because it depends on the WP `Pivot`/`PivotItem` controls and their selection/manipulation lifecycle. It is documented as unsupported.

## 7. Controls and theming

Implemented controls use WPF dependency properties, templates, and `VisualStateManager`. `DefaultStyleKeyProperty.OverrideMetadata` is called in **static constructors** to guarantee one metadata registration per type. `Themes/Generic.xaml` provides implicit styles keyed by type; the sample and tests also merge the dictionary explicitly so named Metro resources (`PhoneTextTitle1Style`, etc.) are available to application XAML.

Design decisions per control:

- **WrapPanel**: original layout algorithm and `ItemHeight/ItemWidth/Orientation` semantics.
- **ToggleSwitch / ToggleSwitchButton**: original visual-state contract and switch template; manipulation + template parts preserved.
- **Rating / RatingItem**: original star item generation, clipping mask, drag helper, orientation, half/zero selection.
- **PhoneTextBox**: inner TextBox with a two-way `Text` binding (WPF `TemplateBinding` is one-way), hint/action/length indicator.
- **ExpanderView**: WPF `HeaderedItemsControl`; parts `Presenter`, `ExpanderPanel`, `ItemsCanvas`; height animation performed in code because WPF cannot retrieve Silverlight keyframe template parts.
- **HubTile**: timer-driven state cycle with `Expanded → Semiexpanded → Collapsed → Expanded → Flipped`; `HubTileService` freeze groups.
- **ListPicker**: original DP surface; normal popup and full-mode modal Window; `PickerPageUri` retained but not navigated.
- **ContextMenu/MenuItem/Separator/ContextMenuService**: derives from WPF menu types; right-click opens the menu.
- **CustomMessageBox**: `ContentControl` API with `Show()`/`Dismiss()` hosted in a WPF Window.
- **DatePicker/TimePicker**: original Value/ValueString/Header/PickerPageUri API, implemented with WPF date/time UI in modal Windows.
- **PerformanceProgressBar**: embedded WPF `ProgressBar` and `ActualIsIndeterminate`; simple opacity fade.
- **Converters/localization**: original formatting rules and 51 resx files (the `qps-ploc` pseudo-locale is excluded from packaging).

## 8. Packaging

Single package project. `GenerateDocumentationFile`, package metadata (`PackageId`, description, tags, MS-PL license expression), README/LICENSE packing, symbols, and a local `artifacts/packages` output. The package contains `Community.Phone.Controls.Toolkit.WPF.dll`, satellite resource assemblies, `Themes/Generic.xaml` (compiled BAML), XML docs, README, and LICENSE.

A fresh WPF consumer project is used as an installation test: add the package from `artifacts/packages`, build, load theme resources, instantiate `ProjectionHost`, and run the sample control set. This is part of the test/verification procedure.

## 9. Testing strategy

- **Projection math**: defaults, DP round-trip, identity matrix, all three rotation axes, center-of-rotation pivots, local vs global offsets, element-size scaling, normalized `Matrix3DProjection`, and animation-driven transform changes.
- **Projection rendering**: `RenderTargetBitmap` checks that 2D and 3D modes produce visible pixels, top/bottom texture orientation is preserved, multiple hosts render independently, and projected content retains keyboard focus.
- **Hit testing**: `VisualTreeHelper.HitTest` reaches the projected content/3D visual.
- **Controls**: theme resource load, template application, DP defaults/round-trip, item generation, layout, selection, two-way text binding, action-icon events, length indicator, popup selection/close, message-box lifecycle, hint/length behavior, expander events, date/time formatting, hub-tile group freeze, recurring-day summarization.
- **Effects**: TiltEffect starts a real projection from a child button, honors `SuppressTilt`, and restores state; TurnstileFeatherEffect feathers on-screen elements, raises/cleans projections, and rejects page targets.
- **Navigation/transitions**: forward/back navigation, navigation parameters, state retention, back-key cancellation, rapid navigation, exit/entry choreography with begin/end events, transition failure fallback, orientation detection, turnstile transition state and actual `RotationY` animation, system tray/application service state.
- **Sample run**: the sample is started as a real WPF process; startup errors are written to `%TEMP%\community-phone-toolkit-sample-error.txt` and the process is checked for a stable window.
- **Package consumption**: `build/verify-package.ps1` packs, restores a fresh consumer from an isolated local feed/cache, builds it with zero warnings, and runs it; the consumer writes a success marker only after loading the theme and controls.

Tests that require a window or WPF dispatcher run on a dedicated STA thread via `StaTest.Run`; animations are advanced with a dispatcher frame pump.

## 10. Known architectural trade-offs

- `ProjectionHost` wrapping is required rather than transparent in-place projection. This is the only way to preserve true 3D and interaction without redefining WPF types.
- Page lifecycle is frame-driven; WPF’s own `Page` navigation does not call the WP-style methods if the page is hosted outside the port’s frames.
- System tray and application bar are API/state compatible but do not render full Windows Phone system chrome in this preview.
- Map, transfer, speech, Pivot, AutoCompleteBox, LongListSelector, and LongListMultiSelector remain architectural work items, not hidden shims.
