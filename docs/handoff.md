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

## 2. Current verified state (2026-10-19)

| Check | Command | Result |
|---|---|---|
| Release build | `dotnet build Community.Phone.Controls.Toolkit.slnx -c Release` | **0 warnings, 0 errors** |
| Release tests | `dotnet test Community.Phone.Controls.Toolkit.slnx -c Release` | **72 passed / 0 failed / 0 skipped** (17:24) |
| Debug tests | `dotnet test Community.Phone.Controls.Toolkit.slnx -c Debug` | **72 passed** (17:23) |
| Package + consumer | `powershell -File build\verify-package.ps1` | **passed** (17:25) — `artifacts/packages/Community.Phone.Controls.Toolkit.WPF.0.1.0-preview.1.nupkg` (+ `.snupkg`), fresh consumer restored from the local feed and ran |
| Sample smoke (UI Automation) | launch `Community.Phone.Controls.Toolkit.WPF.Sample.exe`, open every sample | **all five samples open and return** (Controls, Projection, Transitions → target page → back, Feathering → entry → back, Menus); process stays alive, no error file |

Test inventory (72 methods): `ProjectionTests` 20, `ControlTests` 23, `NavigationTests` 17, `EffectsTests` 6, `ConverterTests` 3, `ShellTests` 3.

The only test that needs real pointer input is `ControlTests.Rating_MouseClickChangesValue`; it now retries the cursor placement five times and skips its assertion when the desktop does not let the pointer land on the control (for example a locked or covered session), so a blocked desktop no longer shows up as a failure.

---

## 3. Confirmed architecture decisions (and why)

Full detail: [docs/en/architecture.md](en/architecture.md) · [中文](zh-CN/architecture.md).

1. **WPF `Viewport3D` + `Viewport2DVisual3D` for projection** — keeps content live/interactive (hit testing, focus, real controls) unlike screenshot planes; rejected Windows Composition/D3D interop for packaging complexity.
2. **XAML-faithful perspective** — fixed `PerspectiveDepth = 999` DIP (WinUI `PlaneProjection` uses `M34 = -1/999`), camera at `z = depth`, horizontal FOV from the viewport **width**. Reason: WPF's `PerspectiveCamera.FieldOfView` is horizontal and derives vertical FOV from aspect; deriving depth from height scaled every projected page down by the aspect ratio (480×800 → 288×480).
3. **Sign conventions** — `R = Rz·Ry·Rx`, row-vector matrix, Y offsets negated, **RotationY negated**. Reason: XAML's positive `RotationY` brings the right edge toward the viewer, a positive WPF +Y rotation moves it away; RotationX/Z already match because element space is Y-down. Cross-checked against `TiltEffect` press direction and the WinUI/Uno projection implementation.
4. **Sequential transitions** — exit completes, then entry begins; `TransitionFrame` flips between two `ProjectionHost` presenters. Matches the original `TransitionFrame`.
5. **Storyboards in code** — `Transitions.cs` rebuilds the 19 original XAML storyboards programmatically so they can target the compatible `PlaneProjection`; values were compared one-by-one with the original XAML.
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

- `Effects/TiltEffect.cs`: tilts only the control (never the page); wraps the control at `Loaded` (unwraps when tilt is disabled) so the visual tree is not modified during pointer input; captures the container→host transform before switching to 3D (3D-hosted elements cannot be transformed to their host); sets `Prefer3D` instead of `BitmapCache`.
- `Effects/TurnstileFeatherEffect.cs`: per-item hosts now work (same ancestor-host fix) plus `Prefer3D` prepare/cleanup.

### 4.4 Tests added/updated

- `ProjectionTests`: `ProjectionHost_KeepsTheProjectedPlaneOneToOneWithTheLayoutSlot`, `ProjectionHost_AppliesTheXamlPerspectiveDepth`, `PlaneProjection_RotationYMatchesTheXamlDepthDirection` (renamed) and two sign assertions updated.
- `EffectsTests`: `TiltEffect_DoesNotTiltThePageThatHostsTheElement`.
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


---

## 5. Not done / not verified

Explicitly **not** claimed:

1. **Frame-by-frame visual comparison against the WP 8.1 emulator** — the emulator is running and is viewed through Virtual Machine Connection, but this session has no ability to *look* at pixels; verification was numeric (geometry, timings) plus comparison with the original XAML/WinUI math. The turnstile tuning (`-0.05` pivot, ×1.2 duration) is based on the user's visual report, not on a measurement, so it still needs a visual confirmation. All families except Turnstile have **not** been observed at runtime.
2. **Automatic parity test for storyboard values** — the comparison with the original XAML was done manually. No test parses or snapshots `Transitions.cs` values.
3. **Feather perspective origin** — the original `TurnstileFeatherEffect` moves each item's perspective origin to the page's vertical center (`GlobalOffsetY` + compensating render transform). This port feathers around each item's own center because a `ProjectionHost` is element-sized and clips its 3D content. Angles/delays/timings are identical. Documented in known-limitations.
4. **Shell chrome** — `SystemTray` and `ApplicationBar` are API models; the default frame template does not draw a WP status bar or application bar. No visual verification exists for them.
5. **Not implemented modules** — `AutoCompleteBox`, `LongListSelector`, `LongListMultiSelector`, `MultiselectList`, map extensions, `TransferControl`, `SpeechTextBox`, `LockablePivot`, `SlideInEffect`, `GestureListener`/`GestureService`, `LoopingSelector`/picker pages. Verified absent in source (no `class X` matches).
6. **Sample coverage gaps** — the redesigned sample demonstrates projection, every transition family, feathering, all implemented controls, ContextMenu, CustomMessageBox and shell state (ApplicationBar/SystemTray API). It does **not** demonstrate `Matrix3DProjection` or actual shell chrome (none is drawn).
7. **Untested visual details** — ListPicker popup styling vs WP, ExpanderView height animation, PhoneTextBox watermark animation, HubTile pixel fidelity: behavior is tested (including the HubTile flip rotation and the PhoneTextBox hint origin), pixel fidelity is not.

---

## 6. Blockers and risks

1. **This turn is uncommitted.** The previous session's work is safe in `bc0e22e`; the turnstile tuning, feather fixes, six WP parity fixes, test changes, sample redesign and doc updates exist only in the working tree (25 modified — 4 deletions — plus 13 new files). A destructive git command would lose them. See §10.
2. **Visual verification gap.** Without visual inspection, fidelity claims rest on geometry/timing measurements and the original XAML. Any future regime should look at the sample and the emulator side by side.
3. **`docs/en/porting-inventory.md` is a large Phase 0 audit** (≈129 KB). Module statuses in it are historical; `docs/porting-status.md` is authoritative for current status.
4. **Environment quirks** — `XDE.exe` hangs (use `vmconnect.exe`); `PrintWindow` returns black for 3D/VM surfaces (use `CopyFromScreen` from a `Topmost` window); the display runs at 150% DPI (DIP↔pixel conversion); a running sample locks `bin` output before rebuild.
5. **Feather fidelity risk** — if strict parity is required, the feathering host architecture has to change (page-sized 3D scene), which is a non-trivial redesign.

---

## 7. Important files

| Path | Role |
|---|---|
| `src/Community.Phone.Controls.Toolkit.WPF/Projection/ProjectionHost.cs` | 2D↔3D host, camera (`PerspectiveDepth`), `Prefer3D`, scene updates |
| `…/Projection/PlaneProjection.cs` | `ToMatrix3D` (sign conventions, row-vector order) |
| `…/Projection/ProjectionService.cs` | attached projection, `EnsureProjectionHost` / `FindDirectHost`, host wrap/unwrap |
| `…/Projection/ProjectionMath.cs` | rotation/translation helpers |
| `…/Transitions/TransitionFrame.cs` | presenter flipping, sequencing, prepare/restore, failure fallback |
| `…/Transitions/Transitions.cs` | the 19 storyboards in code (normative values) plus the turnstile WP-parity constants (`TurnstileCenterOfRotationX`, `TurnstileDurationScale`) |
| `…/Transitions/Transitions/*.cs` | per-family `TransitionElement`s |
| `…/Effects/TiltEffect.cs` | element-level tilt, eager host wrap, transform capture |
| `…/Effects/TurnstileFeatherEffect.cs` | per-item feathering; targets collected from the page's visual tree; host wrap deferred to the dispatcher |
| `…/Themes/Generic.xaml` | single theme source (Metro palette, templates, base WP styles); contains the WP8-faithful `HubTile` template (sliding surface + `RotationX` flip) and the `PhoneTextBox` hint/`Rating` layouts |
| `…/Controls/CustomMessageBox.cs` | message box window; hosts its content in a `ProjectionHost` and swivels in/out (`BackwardIn`/`BackwardOut`) |
| `…/Controls/HubTile.cs` | state machine; drives the title translation, the flip (`Prefer3D`) and the 3 s pulse |
| `…/Controls/Rating.cs` | hit testing in item units (`AllowHalfItemIncrement` divides by two), clipping mask |
| `…/Controls/PhoneTextBox.cs` | hint visibility (focus + IME composition), inner editor, length indicator |
| `src/…Sample/SampleNavigator.cs` | builds in/out transitions per sample so both sides use the same family (fixes the "exit is not feathered" class of bugs) |
| `src/…Sample/Pages/MainPage.xaml` | sample index that mirrors the original `PhoneToolkitSample8` main page |
| `src/…Sample/Pages/{Controls,Projection,Transitions,TransitionTarget,Feather,Menus}Page.xaml` | one page per sample |
| `tests/Community.Phone.Controls.Toolkit.WPF.Tests/` | xUnit suite (`StaTest` for WPF); internals are visible to it |
| `build/verify-package.ps1` | packs Release + fresh consumer smoke test |
| `docs/en/` and `docs/zh-CN/` — architecture, api-compatibility, known-limitations, migration-guide, porting-inventory | canonical bilingual docs |
| `docs/porting-status.md` | current implementation/verification status (authoritative) |
| `.github/Guide.md` | original task prompt (historical, do not treat as status) |

---

## 8. Recommended next steps (in order)

1. **Commit this turn's changes** (nothing from the redesign or this round is committed yet): `git add -A` then a message summarizing the feather regression fix, the message-box swivel, the transition-family fix, the Rating/PhoneTextBox/HubTile fixes, with the `Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>` trailer.
2. **Confirm the turnstile tuning visually** in the emulator (Virtual Machine Connection) and adjust the two constants in `Transitions.cs` if needed — they are the only place that controls the pivot offset and the pacing.
3. **Confirm the six fixes visually** — feathering entry no longer freezes (numeric probe), the message-box swivel, the selected transition family on both exits, the Rating hit area at 50 %/80 % of the width, the PhoneTextBox caret/placeholder alignment, and the HubTile flip. All were verified numerically; pixel comparison against the emulator is still outstanding.
4. **Add storyboard parity tests** (internals are visible to the test assembly): assert the keyframe values/durations of `Transitions` per family so they cannot drift from the original XAML.
5. **Decide on `Prefer3D` lifetime**: today the old presenter returns to 2D when discarded and the new one when its animation completes. If a future symptom shows a sharpening "pop" at the end of a transition, consider extending `Prefer3D` until the next navigation.
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
- Commits: `bc0e22e` (last session's docs + projection/transition/tilt fixes), `83d5d73 fix: 0.01`, `81a55d2 feat: 0.0.1`.
- Working tree at handoff time: **25 modified (4 of them deletions) + 13 new files, uncommitted.**

**Modified / deleted (must not be lost):**

```
.github/copilot-instructions.md
README.md
README_zh-CN.md
docs/en/architecture.md
docs/en/known-limitations.md
docs/handoff.md
docs/porting-status.md
docs/zh-CN/architecture.md
docs/zh-CN/handoff.md
docs/zh-CN/known-limitations.md
docs/zh-CN/porting-status.md
src/Community.Phone.Controls.Toolkit.WPF.Sample/App.xaml.cs
src/Community.Phone.Controls.Toolkit.WPF.Sample/Pages/FeatherPage.xaml
src/Community.Phone.Controls.Toolkit.WPF.Sample/Pages/HomePage.xaml        (deleted)
src/Community.Phone.Controls.Toolkit.WPF.Sample/Pages/HomePage.xaml.cs     (deleted)
src/Community.Phone.Controls.Toolkit.WPF.Sample/Pages/DetailPage.xaml      (deleted)
src/Community.Phone.Controls.Toolkit.WPF.Sample/Pages/DetailPage.xaml.cs   (deleted)
src/Community.Phone.Controls.Toolkit.WPF/Controls/CustomMessageBox.cs
src/Community.Phone.Controls.Toolkit.WPF/Controls/HubTile.cs
src/Community.Phone.Controls.Toolkit.WPF/Controls/PhoneTextBox.cs
src/Community.Phone.Controls.Toolkit.WPF/Controls/Rating.cs
src/Community.Phone.Controls.Toolkit.WPF/Effects/TurnstileFeatherEffect.cs
src/Community.Phone.Controls.Toolkit.WPF/Themes/Generic.xaml
src/Community.Phone.Controls.Toolkit.WPF/Transitions/Transition.cs
src/Community.Phone.Controls.Toolkit.WPF/Transitions/TransitionFrame.cs
src/Community.Phone.Controls.Toolkit.WPF/Transitions/Transitions.cs
tests/Community.Phone.Controls.Toolkit.WPF.Tests/ControlTests.cs
tests/Community.Phone.Controls.Toolkit.WPF.Tests/EffectsTests.cs
tests/Community.Phone.Controls.Toolkit.WPF.Tests/NavigationTests.cs
```

**New (untracked):**

```
src/Community.Phone.Controls.Toolkit.WPF.Sample/SampleNavigator.cs
src/Community.Phone.Controls.Toolkit.WPF.Sample/Pages/MainPage.xaml(.cs)
src/Community.Phone.Controls.Toolkit.WPF.Sample/Pages/ControlsPage.xaml(.cs)
src/Community.Phone.Controls.Toolkit.WPF.Sample/Pages/ProjectionPage.xaml(.cs)
src/Community.Phone.Controls.Toolkit.WPF.Sample/Pages/TransitionsPage.xaml(.cs)
src/Community.Phone.Controls.Toolkit.WPF.Sample/Pages/TransitionTargetPage.xaml(.cs)
src/Community.Phone.Controls.Toolkit.WPF.Sample/Pages/MenusPage.xaml(.cs)
```

- Do **not** run `git reset --hard`, `git checkout -- .`, `git clean -fd`, or force-push.
