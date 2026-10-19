# Handoff — Community.Phone.Controls.Toolkit.WPF

English · [中文](zh-CN/handoff.md)

**Purpose.** This is the entry point for a fresh session. It records what is actually true in the working tree: verified build/test/run results, confirmed design decisions, uncommitted work that must not be lost, and the concrete next steps. It does **not** restate the original task plan (`.github/Guide.md`) or the full API matrix.

**Last updated:** 2026-10-19 16:00 (local), branch `main`, working tree **dirty** (21 modified + 8 new files — see §10).

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
| Release build | `dotnet build Community.Phone.Controls.Toolkit.slnx -c Release` | **0 warnings, 0 errors** (15:55:28) |
| Release tests | `dotnet test Community.Phone.Controls.Toolkit.slnx -c Release --no-build` | **68 passed / 0 failed / 0 skipped** (23 s) |
| Debug tests | `dotnet test Community.Phone.Controls.Toolkit.slnx -c Debug` | **68 passed** |
| Package + consumer | `powershell -File build\verify-package.ps1` | **passed** (16:00) — `artifacts/packages/Community.Phone.Controls.Toolkit.WPF.0.1.0-preview.1.nupkg` (+ `.snupkg`), fresh consumer restored from the local feed and ran |
| Sample smoke (UI Automation) | launch `Community.Phone.Controls.Toolkit.WPF.Sample.exe`, invoke buttons | Home → Detail (turnstile) → Go back → Home → Feather page → Go back all succeed; `Accent button` stays `644×66` before/after clicks and after both round-trips; process stays alive |

Test inventory (68 methods): `ProjectionTests` 20, `ControlTests` 22, `NavigationTests` 14, `EffectsTests` 6, `ConverterTests` 3, `ShellTests` 3.

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

---

## 5. Not done / not verified

Explicitly **not** claimed:

1. **Frame-by-frame visual comparison against the WP 8.1 emulator** — the emulator is running and is viewed through Virtual Machine Connection, but this session has no ability to *look* at pixels; verification was numeric (geometry, timings) plus comparison with the original XAML/WinUI math. All families except Turnstile have **not** been observed at runtime.
2. **Automatic parity test for storyboard values** — the comparison with the original XAML was done manually. No test parses or snapshots `Transitions.cs` values.
3. **Feather perspective origin** — the original `TurnstileFeatherEffect` moves each item's perspective origin to the page's vertical center (`GlobalOffsetY` + compensating render transform). This port feathers around each item's own center because a `ProjectionHost` is element-sized and clips its 3D content. Angles/delays/timings are identical. Documented in known-limitations.
4. **Shell chrome** — `SystemTray` and `ApplicationBar` are API models; the default frame template does not draw a WP status bar or application bar. No visual verification exists for them.
5. **Not implemented modules** — `AutoCompleteBox`, `LongListSelector`, `LongListMultiSelector`, `MultiselectList`, map extensions, `TransferControl`, `SpeechTextBox`, `LockablePivot`, `SlideInEffect`, `GestureListener`/`GestureService`, `LoopingSelector`/picker pages. Verified absent in source (no `class X` matches).
6. **Sample coverage gaps** — the sample demonstrates projection, turnstile/feather navigation, TiltEffect, WrapPanel, ToggleSwitch, Rating, PhoneTextBox, ListPicker, RecurringDaysPicker, ExpanderView, DatePicker, TimePicker, PerformanceProgressBar, RelativeAnimatingContentControl, HubTile, ContextMenu, CustomMessageBox. It does **not** demonstrate `Matrix3DProjection`, `SystemTray` visuals or `ApplicationBar` visuals.
7. **Untested visual details** — ListPicker popup styling vs WP, ExpanderView height animation, HubTile timing, PhoneTextBox watermark animation: behavior is tested, pixel fidelity is not.

---

## 6. Blockers and risks

1. **No commit yet.** All recent fixes exist only in the working tree (branch `main`, 21 modified + 8 new files). A destructive git command would lose them. See §10.
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
| `…/Transitions/Transitions.cs` | the 19 storyboards in code (normative values) |
| `…/Transitions/Transitions/*.cs` | per-family `TransitionElement`s |
| `…/Effects/TiltEffect.cs` | element-level tilt, eager host wrap, transform capture |
| `…/Effects/TurnstileFeatherEffect.cs` | per-item feathering |
| `…/Themes/Generic.xaml` | single theme source (Metro palette, templates, base WP styles) |
| `src/…Sample/` | runnable sample and manual verification surface |
| `tests/Community.Phone.Controls.Toolkit.WPF.Tests/` | xUnit suite (`StaTest` for WPF); internals are visible to it |
| `build/verify-package.ps1` | packs Release + fresh consumer smoke test |
| `docs/en/` and `docs/zh-CN/` — architecture, api-compatibility, known-limitations, migration-guide, porting-inventory | canonical bilingual docs |
| `docs/porting-status.md` | current implementation/verification status (authoritative) |
| `.github/Guide.md` | original task prompt (historical, do not treat as status) |

---

## 8. Recommended next steps (in order)

1. **Commit the working tree** (nothing is committed yet): `git add -A` then a message summarizing the transition/projection fidelity fixes and the tilt-host fix, with the `Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>` trailer.
2. **Visual pass in the emulator** through Virtual Machine Connection: sample → Detail (turnstile), Feather page, "Animate projection", Slide/Rotate/Swivel/Roll (temporarily switch the family in a page's `TransitionService`), compare direction, perspective and duration against the WP OS transitions.
3. **Add storyboard parity tests** (internals are visible to the test assembly): assert the keyframe values/durations of `Transitions` per family so they cannot drift from the original XAML.
4. **Decide on `Prefer3D` lifetime**: today the old presenter returns to 2D when discarded and the new one when its animation completes. If a future symptom shows a sharpening "pop" at the end of a transition, consider extending `Prefer3D` until the next navigation.
5. **Optional: feather parity** — a page-sized 3D host would restore the original perspective origin; treat as a design change, not a patch.
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
- Commits: `83d5d73 fix: 0.01`, `81a55d2 feat: 0.0.1`.
- Working tree at handoff time: **21 modified + 8 new files, nothing committed.**

**Modified (must not be lost):**

```
README.md
README_zh-CN.md
docs/en/README.md
docs/en/api-compatibility.md
docs/en/architecture.md
docs/en/known-limitations.md
docs/en/porting-inventory.md
docs/zh-CN/README.md
docs/zh-CN/api-compatibility.md
docs/zh-CN/architecture.md
docs/zh-CN/known-limitations.md
docs/zh-CN/porting-inventory.md
src/Community.Phone.Controls.Toolkit.WPF/Effects/TiltEffect.cs
src/Community.Phone.Controls.Toolkit.WPF/Effects/TurnstileFeatherEffect.cs
src/Community.Phone.Controls.Toolkit.WPF/Projection/PlaneProjection.cs
src/Community.Phone.Controls.Toolkit.WPF/Projection/ProjectionHost.cs
src/Community.Phone.Controls.Toolkit.WPF/Projection/ProjectionService.cs
src/Community.Phone.Controls.Toolkit.WPF/Transitions/TransitionFrame.cs
tests/Community.Phone.Controls.Toolkit.WPF.Tests/EffectsTests.cs
tests/Community.Phone.Controls.Toolkit.WPF.Tests/NavigationTests.cs
tests/Community.Phone.Controls.Toolkit.WPF.Tests/ProjectionTests.cs
```

**New (untracked):**

```
.github/copilot-instructions.md
docs/architecture.md
docs/api-compatibility.md
docs/porting-inventory.md
docs/porting-status.md
docs/handoff.md
docs/zh-CN/porting-status.md
docs/zh-CN/handoff.md
```

- Do **not** run `git reset --hard`, `git checkout -- .`, `git clean -fd`, or force-push.
- The first recommended commit covers everything above (source + tests + docs); nothing in the tree is disposable.
