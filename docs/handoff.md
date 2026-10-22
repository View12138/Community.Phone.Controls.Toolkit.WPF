# Handoff — Community.Phone.Controls.Toolkit.WPF

English · [中文](zh-CN/handoff.md)

**Purpose.** This is the entry point for a fresh session. It records what is actually true in the working tree: verified build/test/run results, confirmed design decisions, uncommitted work that must not be lost, and the concrete next steps. It does **not** restate the original task plan (`.github/Guide.md`) or the full API matrix.

**Last updated:** 2026-10-19 17:24 (local), branch `main`, working tree **dirty** (25 modified — 4 of them deletions — and 13 new files — see §10). The previous session's work is committed as `bc0e22e`.

---

## 1. Project goal and acceptance criteria

A WPF compatibility port of the archived Windows Phone Toolkit, shipped as one NuGet package (`Community.Phone.Controls.Toolkit.WPF`, root namespace `Microsoft.Phone.Controls`, `net10.0-windows`).

Acceptance criteria in use:

1. **API compatibility** — original `Microsoft.Phone.Controls` / `Microsoft.Phone.Shell` types exist with compatible members and defaults; every deviation is documented in `docs/*/api-compatibility.md` and `docs/*/known-limitations.md`.
2. **Real behavior, not stubs** — projection is real `Viewport3D`/`Viewport2DVisual3D`, transitions are real storyboards, controls are real WPF controls with WP Metro styling.
3. **Evidence** — solution builds with 0 warnings/0 errors, the full test suite passes, `build\verify-package.ps1` passes (package + fresh consumer), and the sample app runs and exercises the features.
4. **Honest status** — unimplemented modules are listed as unimplemented; nothing is claimed as working without a verification method.

---

## 2. Current verified state (2026-10-22)

| Check | Command | Result |
|---|---|---|
| Release build | `dotnet build Community.Phone.Controls.Toolkit.slnx -c Release --no-incremental` (or after deleting `obj`/`bin`) | **0 errors**; the only warnings come from the untracked in-progress files (`LongListSelector.cs`, `ViewportControl.cs`, `WindowHelper.cs` — unused events / unreachable code). Tracked code, including the transition and projection subsystems, compiles warning-free |
| Release tests | `dotnet test Community.Phone.Controls.Toolkit.slnx -c Release` | **103 passed / 0 failed / 0 skipped** in about 30 s — every transition/navigation/projection/effect/ListPicker/application-bar test passes, including the previously failing `ControlTests.CustomMessageBox_ShowDismissRaisesLifecycleEvents` |
| Debug tests | `dotnet test Community.Phone.Controls.Toolkit.slnx -c Debug` | **103 passed / 0 failed** |
| Package + consumer | `powershell -File build\verify-package.ps1` | **passed** — `artifacts/packages/Community.Phone.Controls.Toolkit.WPF.0.1.0-preview.1.nupkg` (+ `.snupkg`), fresh consumer restored from the local feed and ran |
| Sample smoke (UI Automation) | launch `Community.Phone.Controls.Toolkit.WPF.Sample.exe`, open every sample | **all five samples open and return** (Controls, Projection, Transitions → target page → back, Feathering → entry → back, Menus); the Controls page (both list pickers) loads without an animation exception, and the `RecurringDaysPicker` opens the full-mode `ListPickerPage` (header `REPEATS`, seven day rows, the original `ApplicationBar` with 完成/取消 icon buttons) and closes on 完成; process stays alive, no error file. **Not repeated in the newest turn** — the interactive desktop was unavailable (`CopyFromScreen` → "invalid handle", UIA clicks do not land); the page path there is covered by the three `ListPickerPage` tests instead |

Test inventory (103 cases, 99 methods): `ControlTests` 29, `ProjectionTests` 25, `NavigationTests` 16, `TransitionStoryboardTests` 9 methods / 13 cases, `EffectsTests` 7, `ShellTests` 10, `ConverterTests` 3, plus the `StaTest`/`WpfTestHelper` infrastructure.

`NavigationTests.TransitionFrame_FallsBackWhenTransitionFactoryThrows` was removed together with the port's `TransitionFailed` event: the port now behaves like the original, where a throwing `TransitionElement` factory propagates instead of being downgraded to an untransitioned navigation.

No test depends on the OS pointer any more. `ControlTests.Rating_MouseClickChangesValue` (which moved the physical cursor and skipped itself when the desktop refused) is now `Rating_PointerPositionUpdatesValue` and drives the control's `internal UpdateValueFromMouse` directly, so a locked, covered or virtualized desktop can no longer turn into a failure or a silent skip. The five `TransitionFrame_NavigatesWithEveryFamily` cases and the effect/navigation waits no longer sleep fixed 2000–3000 ms; they poll observable state through `WpfTestHelper.PumpUntil(condition, timeout)`, which is what brought the suite from ~45 s down to ~30 s.

---

## 3. Confirmed architecture decisions (and why)

Full detail: [docs/en/architecture.md](en/architecture.md) · [中文](zh-CN/architecture.md).

1. **WPF `Viewport3D` + `Viewport2DVisual3D` for projection** — keeps content live/interactive (hit testing, focus, real controls) unlike screenshot planes; rejected Windows Composition/D3D interop for packaging complexity.
2. **XAML-faithful perspective** — fixed `PerspectiveDepth = 999` DIP (WinUI `PlaneProjection` uses `M34 = -1/999`), camera at `z = depth`, horizontal FOV from the viewport **width**. Reason: WPF's `PerspectiveCamera.FieldOfView` is horizontal and derives vertical FOV from aspect; deriving depth from height scaled every projected page down by the aspect ratio (480×800 → 288×480).
3. **Sign conventions** — `R = Rz·Ry·Rx`, row-vector matrix, Y offsets negated, **RotationY negated**. Reason: XAML's positive `RotationY` brings the right edge toward the viewer, a positive WPF +Y rotation moves it away; RotationX/Z already match because element space is Y-down. Cross-checked against `TiltEffect` press direction and the WinUI/Uno projection implementation.
4. **Sequential transitions** — exit completes, then entry begins; `TransitionFrame` flips between two `ProjectionHost` presenters. Matches the original `TransitionFrame`.
5. **Original storyboard XAML, loaded at runtime** — the 27 original `Transitions/Storyboards/*.xaml` files ship as WPF resources and are parsed with `XamlReader` (the only edit is the projection path, which needs the `phone:` prefix). Reason: keeping the toolkit's normative key times/angles/easing byte-for-byte instead of re-expressing them in code, which had already drifted (the previous port animated `-69°`/`45°` instead of `-80°`/`50°`).
6. **Render-path consistency** — `ProjectionHost.Prefer3D` keeps the 3D path active while the projection is identity, so transitions/effects switch paths while preparing, not on the first animated frame. No `BitmapCache` on transition/effect hosts (the 3D pipeline already rasterizes; the extra 1× cache measurably softened content).
7. **Element effects get their own host** — `ProjectionService.EnsureProjectionHost` only reuses a host that directly contains the element; using an ancestor host projected the whole page (this was a real bug, now covered by a test).

---

## 4. Work completed in the most recent session

All of the following is **implemented and verified** unless marked otherwise.

### 4.1 Projection geometry (root-cause fixes for "transitions look nothing like WP")

- `Projection/ProjectionHost.cs`: replaced the FOV knob with `PerspectiveDepth` (999 DIP) and rewrote `UpdateCamera` to derive the horizontal FOV from the viewport **width**; added `Prefer3D`.
- `Projection/PlaneProjection.cs`: negated `RotationY` in `ToMatrix3D` (commented) to match XAML depth direction.
- Measured evidence (offscreen `RenderTargetBitmap`, 480×800): with `RotationY = −80`, `CenterOfRotationX = 0` the hinge edge is 800 px tall and the free edge lands at x ≈ 133.7 with height ≈ 543 px — exactly the analytic `999/(999+472.7) = 0.6788`; with `RotationY = +50` the free edge is magnified (×1.58) at x ≈ 348. On-screen `CopyFromScreen` measurement of a static −60° projection matched the analytic trapezoid (free edge at 166 vs analytic 160).
- Identity↔3D fidelity measured: feature positions 100.00 (2D) vs 100.01 (3D) px — no shift, no scale change; the residual difference is ClearType→grayscale AA plus the (now removed) bitmap cache.

### 4.2 Transitions

- `Transitions/TransitionFrame.cs`: removed `BitmapCache`, added `Prefer3D` prepare/restore, and aligned the non-cancelled exit path with the original (`CompleteTransition(..., null, ...)`).
- Runtime trace of a real turnstile navigation: exit `0° → 45°` over 249 ms, then entry `−69° → 0°` over 265→592 ms — sequential, matching the original storyboards; presenter hit-testing disabled during the animation and restored afterwards.
- All transition families' values/durations/easings were compared against `Repo-other\...\Transitions\Storyboards\*.xaml` (Roll, Rotate ×8, Slide ×8, Swivel ×6, Turnstile ×4): identical.

### 4.3 Effects

- `Effects/TiltEffect.cs`: tilts only the control (never the page); wraps the control at `Loaded` (unwraps when tilt is disabled) so the visual tree is not modified during pointer input; captures the container→host transform before switching to 3D (3D-hosted elements cannot be transformed to their host); sets `Prefer3D` instead of `BitmapCache`. The rotation pivots at the **first visual child's** bounds (the visible template content) and normalizes the press point against them, like the original toolkit's `GetChild(control, 0)`; `CenterOfRotation` is relative to the layout slot that is actually projected. The return animation tracks its begun state instead of `Storyboard.GetCurrentState()` (which throws without a containing clock) and resets all per-tilt state, so it is safe across UI threads. Since `ProjectionService` wraps on assignment, the tilt no longer manages hosts: it assigns the projection, keeps `Prefer3D` on the direct host, and clears the projection (which releases the on-demand wrapper).
- `Effects/TurnstileFeatherEffect.cs`: per-item hosts now work (same ancestor-host fix) plus `Prefer3D` prepare/cleanup.
- `Projection/ProjectionHost.cs`: the 3D scene now projects an internal slot-sized surface instead of the content itself. `Viewport2DVisual3D` maps the visual's **painted bounds** onto the quad, so content that paints less than its layout slot was stretched across the plane — visible as a horizontally stretched `CheckBox` (its template paints a left-aligned strip) and a distorted feathering label. The surface is pinned to the layout slot, and the plane geometry plus `CenterOfRotation`/offsets are computed from the slot (`UpdateScene`) instead of `Child.RenderSize`, so a child smaller than the host keeps its 2D size and position.
- `Projection/ProjectionService.cs`: assigning `Projection` (attached property, `SetProjection`, or the `element.Projection` extension member) applies to the host that **directly** contains the element and wraps the element on demand (`ProjectionHost` is no longer required up front). The on-demand wrapper is released when the projection is cleared, so the visual tree ends up unchanged; item containers are still skipped. `GetDirectProjectionHost` exposes the host that actually receives the projection, and `(ProjectionService.Projection).(PlaneProjection.RotationY)` remains the storyboard path.

### 4.4 Tests added/updated

- `ProjectionTests`: `ProjectionHost_KeepsTheProjectedPlaneOneToOneWithTheLayoutSlot`, `ProjectionHost_AppliesTheXamlPerspectiveDepth`, `PlaneProjection_RotationYMatchesTheXamlDepthDirection` (renamed) and two sign assertions updated; `ProjectionService_WrapsAnElementThatReceivesAProjection`, `ProjectionService_AppliesAProjectionDeclaredBeforeTheElementLoads`, `ProjectionService_DoesNotProjectThePageHostOfAContainedElement`, `ProjectionHost_DoesNotStretchContentThatPaintsLessThanItsSlot`, `ProjectionHost_ProjectsTheLayoutSlotNotTheChildSize`.
- `EffectsTests`: `TiltEffect_DoesNotTiltThePageThatHostsTheElement`; `TiltEffect_WrapsAChildButtonAndAnimatesARealProjection` now also checks the 3D render path, the return-animation delay, and the return to 2D; `TiltEffect_RotatesAboutTheVisibleContentNotTheLayoutSlot` asserts that the pivot is the template root's centre (a left-aligned `CheckBox` inside a wide slot).
- `NavigationTests`: `TransitionFrame_PlaysTurnstileTransition` now asserts `Prefer3D`/`IsRenderingIn3D` on the first frame and after completion.

### 4.5 Latest turn: WP parity tuning, feather smoothness, sample redesign

- **Turnstile WP parity** (`Transitions/Transitions.cs`): two documented constants — `TurnstileCenterOfRotationX = -0.05` (original toolkit: `0`, i.e. exactly on the left edge) and `TurnstileDurationScale = 1.2` (toolkit: 0.25 s exit / 0.35 s entry). Reason: the user compared against the WP 8.1 emulator and reported the rotation axis sitting slightly away from the left edge and the animation running faster than WP. Setting them to `0` / `1` reproduces the original toolkit exactly. The storyboard numbers themselves are untouched.
- **Feather smoothness** (`Effects/TurnstileFeatherEffect.cs`): feathered elements are wrapped in their `ProjectionHost` ahead of the transition so a transition does not re-parent content while animating (that forced a full layout pass per target and caused the visible stall). *Superseded in §4.6*: the wrap is now deferred to the dispatcher and the targets are collected from the visual tree, because the eager wrap in `SizeChanged` could run inside a layout pass (see §4.6 item 1).
- **Feather exit was not feathered**: root cause was in the sample, not the library — the outgoing transition is read from the page being *left*, and the home page only declared a plain turnstile exit. The redesigned sample sets both sides explicitly (`SampleNavigator`), so entering and leaving the feathering sample uses the feather family.
- **Sample redesigned to mirror `PhoneToolkitSample8`**: `MainPage` lists the samples; each sample is its own page (`ControlsPage`, `ProjectionPage`, `TransitionsPage` + `TransitionTargetPage`, `FeatherPage`, `MenusPage`); `HomePage`/`DetailPage` were removed; `App.xaml.cs` now starts at `MainPage`. `TransitionsPage` lets the user pick the family (Turnstile, Slide, Rotate, Swivel, Roll, feather) before navigating, which makes every family reachable for visual comparison.
- **Measured frame pacing** (`CompositionTarget.Rendering`, 480×800): 2D baseline ~150 fps, projected page ~92–96 fps, six feather item hosts ~64–68 fps, 2D→3D switch ≈ 6 ms.

### 4.6 Latest turn: six WP parity issues (feather regression, message box swivel, transition family, Rating, PhoneTextBox, HubTile)

1. **Feather entry animation missing + page frozen (regression)** — two root causes, both reproduced with an in-process probe (`TransitionFrame` + a page whose elements get their index before they are parented, i.e. XAML order):
   - A feathered transition on a page that has **no feathered elements** produced an *empty* storyboard; an empty storyboard never raises `Completed`, so `TransitionFrame` waited forever, the incoming presenter stayed `IsHitTestVisible = false` and the page looked frozen (the entry never began). Fix: `Transitions/Transition.cs` completes a transition without animations on the next dispatcher pass (`CompleteWithoutAnimation`), keeping the asynchronous contract.
   - `TurnstileFeatherEffect.ComposeStoryboard` used to read the targets from the attached-property subscription list, but an incoming page is composed **before** it is connected to the visual tree, so no notifications had run and the storyboard was empty even without the first bug. Fix: targets are collected from the page's visual tree (`CollectTargets`, stable order by index); `TransitionFrame` forces a layout pass (`_newContentPresenter.UpdateLayout()`) before composing; `Subscribe` now defers its wrap to the dispatcher (it runs from `SizeChanged`, and re-parenting inside a layout pass can start a layout cycle); a `Loaded` handler was added; targets left over from another dispatcher are dropped instead of touched.
   - Probe result after the fix: entry rotations `−80° → 0°` on all six elements with staggered delays, hit-testing restored after the animation, exit feathers `0 → −80°`.
   - Sample: `SampleNavigator` now falls back to the turnstile exit when the outgoing page has no feathered elements (`HasFeatheredElements`), so leaving the main page towards the feathering sample shows a page-level exit instead of a static page.
2. **`CustomMessageBox` swivel animation** — the dialog content is hosted in a `ProjectionHost` and uses the real transition pipeline: `SwivelTransitionMode.BackwardIn` on `ContentRendered`, `BackwardOut` on `Dismiss` (the window closes when the storyboard completes, with a 500 ms safety timer). Verified numerically: `RotationX −45° → 0°` on show, `0° → 42°` (towards 60°) on dismiss, window closed and `Dismissed` raised afterwards.
3. **Transitions sample exit family** — a library test now proves the frame honours an out-transition set immediately before `Navigate` (`TransitionFrame_UsesAnOutTransitionSetJustBeforeNavigating`). The remaining gap was the *back* navigation: `Frame.GoBack()` used the target page's XAML-declared (turnstile) entry and no exit at all. `SampleNavigator.GoBack(source, kind)` now applies the selected family to both sides (it keeps a stack of the pages it navigated from), and `TransitionsPage`/`TransitionTargetPage` use it.
4. **`Rating` click area vs. rendered items** — with `AllowHalfItemIncrement` the control doubled the position count but stored the result in `Value`, which is in item units; the clipping mask and the drag helper both scale `Value` by `RatingItemCount`, so a click at 50 % filled the whole control. `PerformValueCalculation` now divides by two (documented deviation from the toolkit, which has the same defect) and a 1e-9 epsilon keeps a click exactly on an item boundary on that item. Test: `Rating_HalfItemIncrementKeepsValueInItemUnits`.
5. **`PhoneTextBox` caret/placeholder and IME** — the hint started 7 px left of the caret (`Margin="3,0,3,0"` inside a 2 px border vs. `PhoneTextBoxInnerMargin` = 12). The hint now starts exactly where the caret does (`Margin="10,0,10,0"`, verified: hint origin 12 px, text margin 12 px). The hint is hidden while the editor has keyboard focus **and** during an IME composition (`TextCompositionManager.PreviewTextInputStart/Update`), so Pinyin composition no longer leaves the placeholder visible; focus/composition state is re-evaluated in `UpdateHintVisibility`.
6. **`HubTile` slide stutter and missing flip** — the template was rebuilt after the original WP8 template: one surface of twice the tile height whose title band, image and back panel slide with a `TranslateTransform` (translation does not re-rasterize the image, unlike the previous `ScaleTransform`), and a real flip driven by a `PlaneProjection.RotationX` animation on a `ProjectionHost` (`0 → 180` on `Expanded → Flipped`, `180 → 360` back), with the front image and the back panel swapped at 0.185 s (when the surface is edge-on). The back panel is pre-mirrored (`ScaleTransform ScaleY=-1`) so it reads upright at 180°, exactly like the original's `RotationX=180` plane. `HubTile` switches the host to `Prefer3D` before a flip and back to 2D afterwards. Probe: `RotationX 0 → 180` with the back panel visible and the image hidden, `180 → 0` on flip-out, title translation reaching `−H`, `−0.4566H` and `0` for Expanded/Semiexpanded/Collapsed.

Added tests in this turn: `NavigationTests.TransitionFrame_CompletesAFeatheredExitWithoutTargets`, `NavigationTests.TransitionFrame_AnimatesFeatheredEntryOnFirstLayout`, `NavigationTests.TransitionFrame_UsesAnOutTransitionSetJustBeforeNavigating`, `ControlTests.Rating_HalfItemIncrementKeepsValueInItemUnits`; `ControlTests.CustomMessageBox_ShowDismissRaisesLifecycleEvents` now waits for the swivel before asserting `Dismissed`.

### 4.7 Latest turn: the original XAML storyboard pipeline (and the transition bug it exposed)

`Transitions.cs` was re-ported to the original toolkit's shape: the 27 original `Transitions/Storyboards/*.xaml` files ship as WPF resources and are loaded the way the original loaded them (`Application.GetResourceStream` + cached text + `XamlReader.Load`, which needs an `XmlReader` in WPF instead of a string). `Storyboard.SetTarget(storyboard, element)` binds a page-wide target, the factories assign the projection (or the `RenderTransform` for Slide) and return a `Transition`. The only edits inside the storyboards are the property paths: `(phone:ProjectionService.Projection).(phone:PlaneProjection.X)` with `xmlns:phone` — WPF resolves the prefixed path (verified by observing the animated `PlaneProjection.RotationY`).

Two port-only pieces remain, both documented in the source:

1. `ProjectionTransition` clears the attached projection when a projected transition completes or is stopped, so an idle page returns to the plain 2D path (Silverlight could leave an identity projection assigned).
2. The two turnstile constants (`TurnstileCenterOfRotationX = -0.05`, `TurnstileDurationScale = 1.2`); the scale is applied as `Storyboard.SpeedRatio`, so the original key times stay untouched.

Three real bugs were found and fixed while validating the new pipeline:

1. **No transition played at all.** `TransitionFrame.OnNavigating` kept the original `if (!e.IsNavigationInitiator) return;` guard, but WPF reports `IsNavigationInitiator == false` for every `Frame.Navigate` call. `_isForwardNavigation` therefore stayed `false`, `NavigationInTransition.Backward` was picked (usually `null`), and *every* transition in the library and the sample was silently skipped — which is why six navigation tests were already failing at `cd606b4`. Fix: drop the guard (WPF has no shell-driven navigation) and document the deviation.
2. **Cross-thread `BitmapCache`.** `TransitionFrame` kept a shared `static readonly BitmapCache` and assigned it to the presenters. Each test runs on its own STA thread, so the second test threw `ArgumentException` from `MediaSystem.AssertSameContext`. Fix: stop applying a cache to the presenters at all — `Viewport2DVisual3D` already rasterizes the page and the architecture note already forbids a second cache.
3. **Stale test.** `TransitionFrame_FallsBackWhenTransitionFactoryThrows` asserted a counter that was no longer incremented (the `TransitionFailed` event was removed in the previous turn); the test and its `ThrowingTransitionElement` helper were deleted.

Verification added this turn (all automated, `dotnet test -c Release`):

- `TransitionStoryboardTests` (6): each family is started through its public `TransitionElement` and the animated property is read at runtime — Roll `RotationZ 0→45→90`, Rotate `90→0`, Swivel `RotationX −45→0`, Turnstile `RotationY −80→0` (with `IsRenderingIn3D`), Slide `TranslateTransform.X 200→0` + opacity, and `ProjectionTransition` clearing the projection on `Stop()`.
- Measured durations through the real frame: turnstile entry **420 ms**, exit ≈300 ms (0.35 s / 0.25 s × 1.2).
- Rendered-frame evidence: a `RenderTargetBitmap` of a `TransitionFrame` mid-turnstile shows the page as a perspective trapezoid (painted columns 0–325 at the top, 0–399 in the middle, 0–226 at the bottom) while the settled frame is a flat 0–466.
- Sample smoke: the sample launches, the UI Automation tree shows every sample entry, and opening the Transitions sample navigates to its page without an error file.

Docs updated for this turn (both languages): `architecture.md` §5.3/§5.4, `known-limitations.md` §5/§9, `api-compatibility.md`, `migration-guide.md`, `porting-inventory.md`, `porting-status.md` §3 (all families moved to ✅ with the new evidence) plus both READMEs, which no longer advertise `TransitionFailed`.

### 4.8 Latest turn: `InvalidOperationException` from `Transition.GetCurrentState()`

After the transition sources were replaced with the original toolkit files (`Transition` moved to `Transitions/Transitions/Transition.cs`), a real app crashed with:

```
System.InvalidOperationException: Cannot perform this operation because the
specified Storyboard was not applied to this object for interactive control
  at System.Windows.Media.Animation.Storyboard.GetCurrentState()
  at Microsoft.Phone.Controls.Transition.GetCurrentState()
  at Microsoft.Phone.Controls.TransitionFrame.EnsureStoppedTransition(...)
  at Microsoft.Phone.Controls.TransitionFrame.TransitionNewContent(...)
```

Root cause: Silverlight's parameterless `Storyboard` control methods tolerate a storyboard that has no clock yet; WPF throws. `TransitionFrame` calls `EnsureStoppedTransition(transition)` → `transition.GetCurrentState()` **before** `Begin()`, so every navigation crashed on the first transition.

`Transition` now keeps the original Silverlight shape and adds the WPF adaptations it needs:

1. `_isBegun` is tracked: `GetCurrentState()`/`GetCurrentTime()` return `Stopped`/`Zero` before the storyboard starts, and `Pause`/`Resume`/`Seek`/`SeekAlignedToLastTick`/`SkipToFill` are no-ops while it has no clock; `Stop()` on a never-started transition only restores the element.
2. `Begin()` starts the storyboard as a controllable clock on the element (`Storyboard.Begin(containingObject, true)`), because WPF requires the containing object for the interactive operations; every control call passes it.
3. `_isCompleted` makes `Completed` fire at most once per run. WPF re-raises `Timeline.Completed` when a controllable clock is stopped/removed, and the frame completes the navigation from that event — without the guard `NavigationTransition.EndTransition` fired twice (`TransitionFrame_PlaysExitAndEntryTransitionsAndRestoresInteractivity` asserted 1, got 2). `Stop()` also sets the flag so a pending completion cannot fire after the frame already completed.
4. `Stop()` now also `Storyboard.Remove`s the clock, so the animated properties return to their base values.
5. `Save()` creates a `BitmapCache` per transition instead of using the shared `TransitionFrame.BitmapCacheMode` static: a `Freezable` belongs to the dispatcher that created it, and the shared instance threw `ArgumentException` (`MediaSystem.AssertSameContext`, and `Freezable.RemoveContextInformation` in `Restore()`) on every second test thread. The now-unused static was removed from `TransitionFrame`.

Verification:

- Three regression tests were added to `TransitionStoryboardTests`: `Transition_ReportsStateAndTimeBeforeItBegins` (probes state/time and every control method before `Begin()` — this is the crash), `Transition_RaisesCompletedOncePerRun` (asserts a single `Completed` even after repeated `Stop()`), and `TransitionFrame_NavigatesWithEveryFamily` (a `[Theory]` over Turnstile/Slide/Rotate/Swivel/Roll that drives exit+entry through the frame and asserts one `BeginTransition`/`EndTransition` per side, an interactive page and a cleared projection).
- An in-process probe of the real frame (turnstile out + in) reports `outBegun=1 outEnded=1 inBegun=1 inEnded=1`, clears both projections and restores hit testing — no exception.
- A standalone transition probe: `Stopped`→`Active`, time advances, `Pause` freezes (≈7 ms overshoot from tick granularity), `Resume` continues, `Seek(50 ms)` defers to the next tick, `SeekAlignedToLastTick(10 ms)` lands at exactly 10 ms, `SkipToFill` completes once, `Stop` clears the projection.
- `dotnet test -c Release`: **89 passed / 1 failed / 0 skipped** (90 methods); the failure is the pre-existing `CustomMessageBox_ShowDismissRaisesLifecycleEvents` (`Application.Current` is null in the test host). Two `EffectsTests` tilt tests were flaky in one run as well — they leak `TiltEffect` static state across STA threads when a window closes mid-tilt; re-running the suite passes them.

### 4.9 Latest turn: original WP8 `ListPicker` port

The earlier simplified `ListPicker` (a WPF `Popup` plus a modal `Window` for full mode) was replaced with the **original WP8 sources** (`Controls/ListPicker/{ListPicker,ListPickerItem,ListPickerMode,ExpansionMode}.cs`, `ListPickerPage.xaml(.cs)`) and the original `Themes/Controls/ListPicker.xaml` style. `Themes/Generic.xaml` no longer carries a ListPicker style of its own; it merges `Controls/ListPicker.xaml`.

WPF adaptations (each is a WPF requirement, not a behavior change):

1. `ProjectionService`/`PlaneProjection` replace `UIElement.Projection` for the page's item/header entry animations (the storyboard animates the `PlaneProjection` object, the element keeps its host).
2. `Tap`/`GestureEventArgs` → `OnMouseLeftButtonUp` plus mouse enter/leave highlight; the item container's handler sets `e.Handled` so the control's own handler still opens the picker in normal mode.
3. `Application.Current.RootVisual` → `Window.GetWindow(this)` with a visual-tree fallback, so the frame is found in the app *and* in tests.
4. `OnBackKeyPress` override → the shim's `BackKeyPress` event (`e.Cancel = true`), and the picker page's Done/Cancel close path goes through the close storyboard → `NavigationService.GoBack()`.
5. `Extensions.RegisterNotification` is not used: it registers an attached DP on every call and throws on the second `ListPicker` instance; `IsEnabledChanged` covers the same notification.
6. `SelectionChangedEventArgs` needs a `RoutedEvent` (`Selector.SelectionChangedEvent`), `ManipulationContainer` must be cast to `UIElement` for `TransformToVisual`, and `Common/ItemsControlExtensions.GetItemsInViewPort` was added for the page.
7. Theme file: `Dark.xaml` is merged locally (a `StaticResource` from a split theme dictionary is not visible to it) and the page template's root attributes use `DynamicResource` (forward references inside the same dictionary).
8. `PhoneTextBoxEditBackgroundColor` is a `Color`; WPF rejects it for the `Border.Background` keyframe animation, so the Highlighted state uses `PhoneTextBoxEditBackgroundBrush`.
9. Full mode creates a `ListPickerPage` and passes the instance to `Frame.Navigate(object)` (robust across the frame's deferred/transitioned navigation); `PickerPageUri` is still set to the page's pack URI. The host page's transitions are saved and cleared for the "popup" navigation, exactly like the original.

Two runtime bugs were found only by running the sample and are now covered by regression tests:

- **`AnimationException` on the first render tick** — `SizeForAppropriateView` began the canvas `Height` animation before the selected item had been measured, so the `DoubleAnimation` had no origin and the canvas height was `Auto` (`NaN`). Silverlight tolerated that; WPF throws. The storyboard now starts only once the height animation has real from/to values (`ListPicker_PageWithPickersLoadsWithoutAnimationErrors` fails with the exact exception before the fix).
- **`AnimationException` when hovering a picker** — the Highlighted state animated `Background` to a `Color` resource (`ListPicker_HighlightedVisualStateUsesBrushResources` fails before the fix).

Tests: `ListPicker_SelectionRoundTrips`, `…_ExpandedModeSelectsItemAndCollapses`, `…_OpenWithoutItemsExpands`, `…_FullModeWithoutFrameStaysUsable`, `…_FullModeNavigatesToPickerPageAndCommitsSelection` (now also asserts the page stays open until the user acts), `…_PageWithPickersLoadsWithoutAnimationErrors`, `…_HighlightedVisualStateUsesBrushResources`.

Manual evidence (sample, UI Automation): the Controls page loads both pickers without an exception; clicking a picker expands it and a second click commits the selection (the host page's bound text changes); the `RecurringDaysPicker` opens the full-mode page (`REPEATS` header, seven day rows with check boxes, a 完成/取消 command bar — since §4.11 the original `ApplicationBar`) and 完成 returns to the Controls page with no error file.

Docs updated for this turn (both languages): `api-compatibility.md` (control row), `known-limitations.md` §6.3, `architecture.md` §5, `migration-guide.md` §6, and `porting-status.md` (ListPicker test names + evidence snapshot).

### 4.10 Latest turn: original `Microsoft.Phone.Shell.ApplicationBar` port

The previous `ApplicationBar` was a `DependencyObject` API model with no presenter. It is replaced by the original WP8 shape (decompiled sources under `D:\repos\repo-other\WindowsPhone\Microsoft.Phone\Microsoft.Phone.Shell`), adapted to WPF, and it now renders in the frame:

- **Public surface** (`Shell/`): `IApplicationBar`, `IApplicationBarIconButton`, `IApplicationBarMenuItem`, `ApplicationBar` (`[ContentProperty("Buttons")]`, `IsVisible` default `true`, `Opacity` clamped to 0..1, `IsMenuEnabled`, `BackgroundColor`, `ForegroundColor`, `Mode`, `DefaultSize` 72, `MiniSize` 30, `Buttons`, `MenuItems`, `StateChanged`), `ApplicationBarIconButton` (`IconUri`, `Text`, `IsEnabled`, `Click`), `ApplicationBarMenuItem` (`Text`, `IsEnabled`, `Click`), `ApplicationBarMode`, `ApplicationBarStateChangedEventArgs`. The port-only `ApplicationBarItem` base type and the `Command`/`CommandParameter`/`RaiseClick` members are gone: the original has neither, so commands are wired through `Click`.
- **Infrastructure**: `ApplicationBarItemList<T>` (the original `IList` implementation with 4/50 limits and the original exception messages), `ApplicationBarItemContainer` + icon/menu containers + factory (validate text — non-empty, single line, trimmed, ≤255 chars — and require an icon while attached; dispatch clicks on the UI thread), and the original "reserves space" rule (visible, opacity 1, background alpha 255 or 0).
- **WPF chrome**: `ApplicationBarPresenter` (additive, documented) replaces the native compositor. It draws a 72 DIP bar (30 when minimized) with 96×72 icon slots (48×48 icon above a 12 px label), a three-dot menu button at the top right, and a `Popup` menu with 48 px rows; colors/opacity follow the bar, disabled items are dimmed, and keyboard activation works. Icon URIs resolve as application resources and a missing icon renders the button without an image.
- **Frame plumbing**: both frame templates gained `ApplicationBarHost` (a row that reserves `ReservedHeight`) and `ApplicationBar` (`ApplicationBarPresenter`). `PhoneApplicationFrame.OnApplyTemplate` wires the parts, `Navigated` pushes the new page's bar, and `PhoneApplicationPage.ApplicationBar` (now typed `IApplicationBar`, rejecting foreign implementations like the original) forwards changes while the page is active. A translucent or hidden bar floats over the page instead of shrinking it.
- **Sample**: `MenusPage` declares its bar in XAML with three icon buttons and three menu items; `Images/appbar.*.rest.png` are small generated glyphs shipped as resources.

Verification: `ShellTests` (10 tests) covers clicks, disabled items, limits/validation, the reserved-height rule, menu open/close through `StateChanged`, the frame hosting (72 DIP host + presenter, page laid out above it), the no-bar case and the foreign-implementation rejection; the full suite is green (see §4.11). Pixel evidence from the running sample: the bar occupies 72 DIP at the frame bottom with the `#1F1F1F` chrome colour, white icons and labels in the three slots, and the three dots at the top right; opening the menu and activating `minimize` collapsed the bar to the 30 DIP minimized mode and updated the page status text.

Docs updated for this turn (both languages): `api-compatibility.md` (shell mapping + XAML row), `known-limitations.md` §3.2, `architecture.md` §5.1/§5.2.1, `migration-guide.md` §6, `porting-status.md` (ApplicationBar row + evidence snapshot).

### 4.11 Latest turn: `ListPickerPage` back on the original `ApplicationBar`, and a usable test loop

- **`ListPickerPage` uses the ported `ApplicationBar` again.** The temporary `CommandArea` `StackPanel` (a hand-built done/cancel strip) is gone. `ListPickerPage.xaml` now declares `<phone:PhoneApplicationPage.ApplicationBar><shell:ApplicationBar IsVisible="False">` with two `ApplicationBarIconButton`s pointing at `Content/ApplicationBar.Check.png` and `Content/ApplicationBar.Cancel.png` (library resources, pack URI `/Community.Phone.Controls.Toolkit.WPF;component/Content/...`), and the code-behind is the WP8 original: `OnLoaded` walks `ApplicationBar.Buttons`, matches the case-insensitive `DONE`/`CANCEL` texts, applies the localized `appbar_done`/`appbar_cancel` strings and hooks `Click` to `Done_Click`/`Cancel_Click`; `OnNavigatedTo` sets `ApplicationBar.IsVisible = SelectionMode != SelectionMode.Single`. In single-selection mode the frame therefore reserves no bar height at all.
- **`CustomMessageBox.Show()` no longer needs an `Application`.** It resolves the theme background through a resource lookup with a `Colors.Black` fallback, tolerates a null `MainWindow` and guards `OnWindowContentRendered` against a null owner, so showing it in a plain window (or a test host) works. The test also had to dismiss the dialog from *inside* the nested dispatcher loop `ShowDialog()` runs, which is why it schedules the dismiss with a `DispatcherTimer`.
- **Test-loop speed and determinism.** `WpfTestHelper.PumpUntil(predicate, timeout)` / `PumpFor` were added, and the fixed waits that dominated the runtime were converted: the five `TransitionFrame_NavigatesWithEveryFamily` cases (5 × 3000 ms), the 2000 ms tilt test and eight `NavigationTests` waits now poll observable state. The suite went from ~45 s to ~30 s. The `Rating` test stopped moving the physical cursor (`Rating.UpdateValueFromMouse` is `internal`; `Rating_PointerPositionUpdatesValue` calls it), so no test depends on the desktop letting the pointer land somewhere.
- **Parallelism stays off, deliberately.** Removing `[assembly: CollectionBehavior(DisableTestParallelization = true)]` hung the run (>600 s) because several tests drive shared WPF singletons (the theme dictionary, `Popup`s and the storyboard XAML cache); the attribute is kept with a comment. The two caches that were genuinely racy (`Transitions._storyboardXamlCache`, `ApplicationBarIconLoader.Cache`) got locks anyway.

Verification: two new tests — `ListPicker_FullModePageUsesTheApplicationBarForSingleSelection` (bar hidden, two buttons, presenter collapsed, host row height 0) and `ListPicker_FullModePageCommitsFromTheApplicationBarDoneButton` (bar visible, presenter 72 DIP, localized texts, both icons resolved from pack URIs, clicking Done commits the selection and returns to the host page). Full suite **103/103** in Release and Debug. Sample evidence (UI Automation + pixels): the `RecurringDaysPicker` page shows `REPEATS`, seven day rows, and the bar at the frame bottom with the `#1F1F1F` chrome, the `完成`/`取消` labels (y≈1258 in that run's screen pixels) each with a white icon above it (centres x≈683 and x≈827); clicking the `完成` slot returns to the Controls page.

Docs updated for this turn (both languages): `known-limitations.md` (ListPicker page chrome now uses the real `ApplicationBar`), `api-compatibility.md` (ListPicker row), `architecture.md` §9 (condition waits), `porting-status.md` (evidence snapshot + ListPicker/CustomMessageBox rows).

### 4.12 Latest turn: clean-build break and the `ListPickerPage` regressions it hid

The committed tree did not build from scratch, and the three `ListPickerPage` tests failed once it did. Both are fixed; no test was changed in the end.

- **Cold build failed with `MC3074`** ("`PhoneApplicationPage` does not exist in `clr-namespace:Microsoft.Phone.Controls;assembly=Community.Phone.Controls.Toolkit.WPF`"). `MarkupCompilePass1` only sees the project's own types through `@(Compile)`; when a page of the library refers to the library's own assembly *by name*, the pass can't resolve it on a clean `obj` (an incremental build with leftover generated files hid this, which is why the failure appeared only after the tree was rebuilt from scratch). Fix: `ListPickerPage.xaml` declares `xmlns:phone`/`xmlns:shell`/`xmlns:toolkit` **without** `assembly=`, exactly like the library theme XAML. Verified with the library's `obj`/`bin` deleted.
- **The picker page has to bring its own theme.** `ListPicker` creates `ListPickerPage` itself, so the page is parsed while it is attached to nothing, and it is afterwards hosted inside the frame's `ProjectionHost`, where inherited resources are not reachable (the same WPF limitation that stops `DataContext` inheritance through `Viewport2DVisual3D`). Consequences handled in the page: its `Resources` now wraps the styles in an explicit `ResourceDictionary` that merges `Themes/Generic.xaml` (so every `StaticResource` inside the page's styles/templates resolves without an application-level theme), the properties set on the root element and header (`FontFamily`, `FontSize`, `Foreground`, `MainGrid.Background`) use `DynamicResource` because they are evaluated while the page is still detached, and the `TransparentBrush` used as an `ObjectAnimationUsingKeyFrames` value is now an inline `SolidColorBrush` (a keyframe value cannot use `DynamicResource`, and a `StaticResource` there is resolved in the template's own dictionary). The same inline brush was applied to `Themes/Controls/ListPicker.xaml`, whose template had the identical problem when the theme is used as the WPF *theme* dictionary rather than as an application dictionary.
- **Wrong storyboard target in `AnimationForElement`** — the open/close animation targeted the `PlaneProjection` while its property path starts at the element's attached `ProjectionService.Projection`, so `Storyboard.Begin()` threw `InvalidOperationException` ("the Projection property does not point to a DependencyObject"). Now the target is the element, which is what the original WP code did (`(UIElement.Projection).(PlaneProjection.RotationX)`).
- **`Extensions.IsExternalNavigation` dereferenced a null `Uri`.** The port navigates with page instances, so `NavigationEventArgs.Uri` is `null`; `ListPickerPage.OnNavigatedFrom` then threw `NullReferenceException` on the way back. The extension is null-safe now.

Verification: library `obj`/`bin` deleted → Release build **0 errors, 22 warnings** (all pre-existing, in `LongListSelector`/`ViewportControl`/`WindowHelper`); `dotnet test` **103/103 in Release and Debug** (~28 s); `build\verify-package.ps1` passes. The on-screen sample check of the `RecurringDaysPicker` page could not be repeated this turn because the interactive desktop was unavailable (`CopyFromScreen` fails with "invalid handle", UI Automation clicks never reach the window); the same path is covered by the three `ListPickerPage` tests, which drive a real `TransitionFrame`.

Rejected experiment (worth remembering): giving the test process an `Application` (even on one long-lived STA thread) makes `Application.Current.MainWindow` point at a window that a previous test already closed, which changes `ListPicker`'s frame discovery and left four tests failing — the tests keep one STA thread per test and no `Application`.

Docs updated for this turn (both languages): this section, `porting-status.md` (evidence snapshot, ListPicker row), `known-limitations.md` (page-provided theme, `DynamicResource` on load-time properties), `architecture.md` (theme-dictionary rules).

---

## 5. Not done / not verified

Explicitly **not** claimed:

1. **Frame-by-frame visual comparison against the WP 8.1 emulator** — the emulator is running and is viewed through Virtual Machine Connection, but this session has no ability to *look* at pixels; verification was numeric (geometry, timings, rendered-frame geometry) plus comparison with the original XAML/WinUI math. The turnstile tuning (`-0.05` pivot, ×1.2 duration) is based on the user's visual report, not on a measurement, so it still needs a visual confirmation. Every family now has a runtime test of the animated property (`TransitionStoryboardTests`), but only Turnstile has rendered-frame evidence.
2. **Easing/pixel parity per family** — the storyboards *are* the original XAML, so values are exact by construction; whether WPF and Silverlight evaluate the same `ExponentialEase`/`SineEase` identically at every frame is not measured.
3. **Feather perspective origin** — the original `TurnstileFeatherEffect` moves each item's perspective origin to the page's vertical center (`GlobalOffsetY` + compensating render transform). This port feathers around each item's own center because a `ProjectionHost` is element-sized and clips its 3D content. Angles/delays/timings are identical. Documented in known-limitations.
4. **Shell chrome** — `ApplicationBar` is now drawn by the frame (`ApplicationBarPresenter`), but `SystemTray` is still only an API model: no WP status bar is painted. The app bar does not animate with the page transition, the menu is a WPF `Popup` rather than a native surface, and the exact WP8 metrics (menu slide, ellipsis position) are approximated rather than pixel-compared.
5. **Not implemented modules** — `AutoCompleteBox`, `LongListSelector`, `LongListMultiSelector`, `MultiselectList`, map extensions, `TransferControl`, `SpeechTextBox`, `LockablePivot`, `SlideInEffect`, `GestureListener`/`GestureService`, `LoopingSelector`/picker pages. Verified absent in source (no `class X` matches).
6. **Sample coverage gaps** — the redesigned sample demonstrates projection, every transition family, feathering, all implemented controls, ContextMenu, CustomMessageBox and shell state (ApplicationBar/SystemTray API). It does **not** demonstrate `Matrix3DProjection` or actual shell chrome (none is drawn).
7. **Untested visual details** — ListPicker expanded-mode styling and the full-mode page's `ApplicationBar` (it is the real ported bar now, but its exact WP8 icon/text metrics were not pixel-compared), ExpanderView height animation, PhoneTextBox watermark animation, HubTile pixel fidelity: behavior is tested (including the HubTile flip rotation and the PhoneTextBox hint origin), pixel fidelity is not.

---

## 6. Blockers and risks

1. **This turn is uncommitted.** HEAD is `4c62a43`; the WP8 `ListPicker` port (`Controls/ListPicker/`, `Themes/Controls/ListPicker.xaml`, `Common/ItemsControlExtensions.cs`), the `ApplicationBar` port (`Shell/ApplicationBar*`), the frame/page/template wiring, the test and sample changes and the doc updates exist only in the working tree. A destructive git command would lose them. See §10.
2. **Visual verification gap.** Without visual inspection, fidelity claims rest on geometry/timing measurements and the original XAML. Any future regime should look at the sample and the emulator side by side.
3. **`docs/en/porting-inventory.md` is a large Phase 0 audit** (≈129 KB). Module statuses in it are historical; `docs/porting-status.md` is authoritative for current status.
4. **Environment quirks** — `XDE.exe` hangs (use `vmconnect.exe`); `PrintWindow` returns black for 3D/VM surfaces (use `CopyFromScreen` from a `Topmost` window); the display runs at 150% DPI (DIP↔pixel conversion); a running sample locks `bin` output before rebuild.
5. **Feather fidelity risk** — if strict parity is required, the feathering host architecture has to change (page-sized 3D scene), which is a non-trivial redesign.

---

## 7. Important files

| Path | Role |
|---|---|
| `src/Community.Phone.Controls.Toolkit.WPF/Projection/ProjectionHost.cs` | 2D↔3D host, camera (`PerspectiveDepth`), `Prefer3D`, slot-sized projected surface, scene updates |
| `…/Projection/PlaneProjection.cs` | `ToMatrix3D` (sign conventions, row-vector order) |
| `…/Projection/ProjectionService.cs` | attached projection (wraps on demand, releases on clear), `EnsureProjectionHost` / `FindDirectHost`, host wrap/unwrap |
| `…/Projection/ProjectionMath.cs` | rotation/translation helpers |
| `…/Transitions/TransitionFrame.cs` | presenter flipping, sequencing, prepare/restore (3D + hit testing, no bitmap cache) |
| `…/Transitions/Transitions.cs` | loads the original storyboard XAML resources (`GetResourceStream` + `XamlReader`), attaches the page target, `ProjectionTransition` cleanup, turnstile WP-parity constants (`TurnstileCenterOfRotationX`, `TurnstileDurationScale`) |
| `…/Transitions/Storyboards/*.xaml` | the 27 original storyboards (normative angles/key times/easing); only the projection path is rewritten with the `phone:` prefix |
| `…/Transitions/Transitions/*.cs` | per-family `TransitionElement`s |
| `…/Effects/TiltEffect.cs` | element-level tilt through `ProjectionService` (no host management), pivot on the first visual child, container→control transform capture |
| `…/Effects/TurnstileFeatherEffect.cs` | per-item feathering; targets collected from the page's visual tree; host wrap deferred to the dispatcher |
| `…/Themes/Generic.xaml` | single theme source (Metro palette, templates, base WP styles); contains the WP8-faithful `HubTile` template (sliding surface + `RotationX` flip) and the `PhoneTextBox` hint/`Rating` layouts |
| `…/Controls/CustomMessageBox.cs` | message box window; hosts its content in a `ProjectionHost` and swivels in/out (`BackwardIn`/`BackwardOut`) |
| `…/Controls/HubTile.cs` | state machine; drives the title translation, the flip (`Prefer3D`) and the 3 s pulse |
| `…/Controls/Rating.cs` | hit testing in item units (`AllowHalfItemIncrement` divides by two), clipping mask |
| `…/Controls/PhoneTextBox.cs` | hint visibility (focus + IME composition), inner editor, length indicator |
| `…/Shell/ApplicationBar.cs` (+ `IApplicationBar*`, `ApplicationBarIconButton`, `ApplicationBarMenuItem`, `ApplicationBarMode`, `ApplicationBarStateChangedEventArgs`) | original `Microsoft.Phone.Shell` API surface and state |
| `…/Shell/ApplicationBarItemList.cs`, `ApplicationBarItemContainer*.cs`, `BoolEventArgs.cs` | item list limits/exceptions, per-item validation and click dispatch |
| `…/Shell/ApplicationBarPresenter.cs`, `ApplicationBarItemView.cs`, `ApplicationBarIconButtonView.cs`, `ApplicationBarMenuItemView.cs` | WPF chrome: bar, icon slots, menu button, menu popup, icon loading || `…/Controls/ListPicker/ListPicker.cs` | original WP8 control: normal/expanded canvas height animation, full-mode page navigation, frame discovery, WPF mouse/back-key/selection adaptations |
| `…/Controls/ListPicker/ListPickerPage.xaml(.cs)` | original full-mode page: item/header entry animations through `ProjectionService`, original `ApplicationBar` with check/cancel icon buttons (`Content/ApplicationBar.*.png`) |
| `…/Themes/Controls/ListPicker.xaml` | original `ListPicker`/`ListPickerItem` styles (merged from `Generic.xaml`, `Dark.xaml` merged in, `PickerPageUri` set) |
| `src/…Sample/SampleNavigator.cs` | builds in/out transitions per sample so both sides use the same family (fixes the "exit is not feathered" class of bugs) |
| `src/…Sample/Pages/MainPage.xaml` | sample index that mirrors the original `PhoneToolkitSample8` main page |
| `src/…Sample/Pages/{Controls,Projection,Transitions,TransitionTarget,Feather,Menus}Page.xaml` | one page per sample |
| `tests/Community.Phone.Controls.Toolkit.WPF.Tests/` | xUnit suite (`StaTest` for WPF); internals are visible to it; `WpfTestHelper.PumpUntil`/`PumpFor` are the way to wait on observable state (no fixed sleeps) |
| `build/verify-package.ps1` | packs Release + fresh consumer smoke test |
| `docs/en/` and `docs/zh-CN/` — architecture, api-compatibility, known-limitations, migration-guide, porting-inventory | canonical bilingual docs |
| `docs/porting-status.md` | current implementation/verification status (authoritative) |
| `.github/Guide.md` | original task prompt (historical, do not treat as status) |

---

## 8. Recommended next steps (in order)

1. **Commit this turn's changes** (the WP8 `ListPicker` port and its two animation fixes are not committed yet): `git add -A` then a message summarizing the original-source port, the WPF adaptations, and the height-animation/highlight-brush fixes, with the `Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>` trailer.
2. **Confirm the turnstile tuning visually** in the emulator (Virtual Machine Connection) and adjust the two constants in `Transitions.cs` if needed — they are the only place that controls the pivot offset and the pacing.
3. **Confirm the remaining fixes visually** — feathering entry no longer freezes (numeric probe), the message-box swivel, the selected transition family on both exits, the Rating hit area at 50 %/80 % of the width, the PhoneTextBox caret/placeholder alignment, and the HubTile flip. All were verified numerically; pixel comparison against the emulator is still outstanding.
4. **Decide on `Prefer3D` lifetime**: today the old presenter returns to 2D when discarded and the new one when its animation completes. If a future symptom shows a sharpening "pop" at the end of a transition, consider extending `Prefer3D` until the next navigation.
5. **Revisit test parallelization only with isolated state** — parallel runs deadlock because several tests share the theme dictionary, `Popup`s and the storyboard cache. Making those per-test is the prerequisite, not simply removing the collection attribute.
6. **Keep docs in sync** — update `docs/porting-status.md` and `docs/handoff.md` (§2 results, §5 state) whenever behavior or verification changes.

---

## 9. Information precedence

1. **Source code + test/build results + git state** — what is true now.
2. **`docs/handoff.md`** (this file) — latest handoff snapshot.
3. **`docs/*/architecture.md`, `docs/*/api-compatibility.md`, `docs/porting-status.md`** — confirmed decisions and status.
4. **`docs/*/porting-inventory.md`** — Phase 0 audit, historical.
5. **`.github/Guide.md`** — original task prompt, historical.

---

## 10. Git state — protect this

- Branch: **`main`**, no remote tracking configured in this workspace.
- HEAD: **`83c24f6` "fix"** — it contains everything from §4.9–§4.11 (the WP8 `ListPicker` port, the `ApplicationBar` port, the `ListPickerPage` app bar and the test-loop changes) plus this workspace's other in-progress work.
- Working tree at handoff time: **uncommitted and small** — exactly the four files from §4.12: `Common/Extensions.cs`, `Controls/ListPicker/ListPickerPage.xaml`, `Controls/ListPicker/ListPickerPage.xaml.cs`, `Themes/Controls/ListPicker.xaml`.

**Modified (this work):**

```
docs/en/api-compatibility.md, docs/en/architecture.md, docs/en/known-limitations.md,
docs/en/migration-guide.md, docs/handoff.md, docs/porting-status.md,
docs/zh-CN/{api-compatibility,architecture,handoff,known-limitations,migration-guide,porting-status}.md
src/Community.Phone.Controls.Toolkit.WPF/Navigation/PhoneApplicationFrame.cs   (app bar chrome wiring)
src/Community.Phone.Controls.Toolkit.WPF/Navigation/PhoneApplicationPage.cs    (ApplicationBar is IApplicationBar)
src/Community.Phone.Controls.Toolkit.WPF/Shell/ApplicationBar.cs               (rewritten: original API)
src/Community.Phone.Controls.Toolkit.WPF/Themes/Generic.xaml                   (frame templates host the bar; ListPicker styles moved out)
src/Community.Phone.Controls.Toolkit.WPF.Sample/Pages/MenusPage.xaml(.cs)      (uses the ported bar)
src/Community.Phone.Controls.Toolkit.WPF.Sample/Community.Phone.Controls.Toolkit.WPF.Sample.csproj  (Images are resources)
src/Community.Phone.Controls.Toolkit.WPF/Community.Phone.Controls.Toolkit.WPF.csproj  (Content/*.png are resources)
src/Community.Phone.Controls.Toolkit.WPF/Controls/ListPicker/ListPickerPage.xaml(.cs)  (original ApplicationBar instead of the command area)
src/Community.Phone.Controls.Toolkit.WPF/Controls/CustomMessageBox/CustomMessageBox.cs (no Application required)
src/Community.Phone.Controls.Toolkit.WPF/Controls/Rating.cs                     (UpdateValueFromMouse is internal)
src/Community.Phone.Controls.Toolkit.WPF/Transitions/Transitions.cs            (storyboard cache lock)
src/Community.Phone.Controls.Toolkit.WPF/Shell/ApplicationBarPresenter.cs      (icon cache lock)
tests/Community.Phone.Controls.Toolkit.WPF.Tests/ControlTests.cs               (ListPicker tests replaced/added; Rating, CustomMessageBox, ListPickerPage)
tests/Community.Phone.Controls.Toolkit.WPF.Tests/ShellTests.cs                 (ApplicationBar tests rewritten)
tests/Community.Phone.Controls.Toolkit.WPF.Tests/NavigationTests.cs            (condition-based waits)
tests/Community.Phone.Controls.Toolkit.WPF.Tests/EffectsTests.cs               (condition-based waits)
tests/Community.Phone.Controls.Toolkit.WPF.Tests/TransitionStoryboardTests.cs  (condition-based waits)
tests/Community.Phone.Controls.Toolkit.WPF.Tests/WpfTestHelper.cs              (PumpUntil / PumpFor)
tests/Community.Phone.Controls.Toolkit.WPF.Tests/StaTest.cs                    (parallelization stays disabled, with a comment)
```

**Deleted:**

```
src/Community.Phone.Controls.Toolkit.WPF/Controls/ListPicker.cs        (replaced by Controls/ListPicker/)
src/Community.Phone.Controls.Toolkit.WPF/Controls/ListPickerItem.cs
src/Community.Phone.Controls.Toolkit.WPF/Controls/ListPickerMode.cs
src/Community.Phone.Controls.Toolkit.WPF/Controls/ExpansionMode.cs
```

**New (untracked, this work):**

```
src/Community.Phone.Controls.Toolkit.WPF/Common/ItemsControlExtensions.cs
src/Community.Phone.Controls.Toolkit.WPF/Controls/ListPicker/          (ListPicker.cs, ListPickerItem.cs, ListPickerMode.cs, ExpansionMode.cs, ListPickerPage.xaml(.cs))
src/Community.Phone.Controls.Toolkit.WPF/Themes/Controls/ListPicker.xaml
src/Community.Phone.Controls.Toolkit.WPF/Shell/IApplicationBar*.cs, ApplicationBarIconButton.cs,
    ApplicationBarMenuItem.cs, ApplicationBarMode.cs, ApplicationBarStateChangedEventArgs.cs,
    ApplicationBarItemList.cs, ApplicationBarItemContainer*.cs, ApplicationBarItemContainerFactory.cs,
    BoolEventArgs.cs, ApplicationBarPresenter.cs, ApplicationBarItemView.cs,
    ApplicationBarIconButtonView.cs, ApplicationBarMenuItemView.cs
src/Community.Phone.Controls.Toolkit.WPF.Sample/Images/appbar.*.rest.png
src/Community.Phone.Controls.Toolkit.WPF/Content/ApplicationBar.Check.png, ApplicationBar.Cancel.png
```

**Also in the working tree but not part of this work** (someone else’s in-progress sample page): `Sample/Data/`, `Sample/Samples/ListPickerSample.xaml(.cs)`, the `ListPicker` entry added to `MainPage.xaml(.cs)`, a duplicated `<RootNamespace>` line in the library csproj, and `Community.Phone.Controls.Toolkit.WPF.Sample.csproj.user`. Leave them alone when committing.

- Do **not** run `git reset --hard`, `git checkout -- .`, `git clean -fd`, or force-push: the untracked `Controls/ListPicker/`, `Shell/ApplicationBar*` and sample asset files would be lost.
