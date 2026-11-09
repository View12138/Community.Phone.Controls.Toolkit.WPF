# Copilot instructions — Community.Phone.Controls.Toolkit

## What this repository is

- A WPF compatibility port of the archived Microsoft **Windows Phone Toolkit** (MS-PL; original source at `D:\repos\repo-other\WindowsPhoneToolkit`), shipped as one NuGet package.
- Package: `Community.Phone.Controls.Toolkit`. It contains two assemblies: the toolkit assembly `Community.Phone.Controls.Toolkit` and the Windows Phone compatibility layer `Community.Phone`. **Root namespaces stay `Microsoft.Phone.Controls` / `Microsoft.Phone.Shell`**, target framework `net10.0-windows`, no third-party runtime dependencies.
- Goal: a WP toolkit app moves over by changing namespaces and wrapping projected content; Metro visuals, page transitions and 3D projection should behave like WP 8.1. This is a compatibility port, not a new UI framework.

## Non-negotiable principles

1. **Preserve the original public API shape** (type names, members, semantics, defaults). New behavior belongs in docs, not in renamed APIs.
2. WPF has no `UIElement.Projection`. The port provides `ProjectionHost` / `ProjectionService` / `PlaneProjection` / `Matrix3DProjection`. Port-only APIs must be *additive* and documented in `docs/*/api-compatibility.md`.
3. WP platform infrastructure (`PhoneApplicationFrame`, `PhoneApplicationPage`, `PhoneApplicationService`, `SystemTray`, `ApplicationBar`) is re-implemented in the compatible namespaces. Differences (no tombstoning, no real status bar/battery, etc.) must be stated, never hidden.
4. **Never report unimplemented or unverified behavior as done.** Unsupported modules stay listed in `docs/en/known-limitations.md` and `docs/zh-CN/known-limitations.md`.
5. Transition numbers (angles, durations, delays, easing) come from the original storyboard XAML under `Microsoft.Phone.Controls.Toolkit\Transitions\Storyboards\*.xaml`. Copy them; never invent or "tune" them.

## Architecture constraints — do not break these

### Project split

- `src/Community.Phone` is the `Microsoft.Phone.dll` compatibility layer: `Microsoft.Phone.Controls.PhoneApplicationFrame` / `PhoneApplicationPage` / `NavigationContext` / orientation types and every `Microsoft.Phone.Shell` type (application bar, system tray, application service). It has no toolkit dependency.
- `src/Community.Phone.Controls.Toolkit` is the toolkit assembly. It references `Community.Phone`; never move toolkit controls into the compatibility layer or add a reference in the other direction.
- Both assemblies are packed into the single `Community.Phone.Controls.Toolkit` NuGet package (`IncludeCompatibilityLayerInPackage` in the csproj). Do not split them into two packages.
- Toolkit and platform types share `Microsoft.Phone.Controls`, so XAML needs two prefixes: `phone:` (`assembly=Community.Phone.Controls.Toolkit`) for toolkit controls and `platform:` (`assembly=Community.Phone`) for pages/frames; `shell:` maps to `Community.Phone`.
- The compatibility layer owns its theme dictionary (`src/Community.Phone/Themes/Generic.xaml`, currently the `PhoneApplicationFrame` template); the toolkit theme stays in `src/Community.Phone.Controls.Toolkit/Themes/Generic.xaml`.

### Projection / PlaneProjection

- Camera = XAML's fixed perspective: `ProjectionHost.PerspectiveDepth = 999` DIP (WinUI `M34 = -1/999`), camera at `(0, 0, depth)`, **horizontal** FOV derived from the viewport **width**: `2 * atan(viewportWidth / (2 * depth))`. Deriving it from the height shrinks projected content by the aspect ratio (past bug: 480×800 rendered as 288×480).
- Matrix is row-vector: `p' = (((p - center) + localOffset) * R) + center + globalOffset`, `R = Rz * Ry * Rx`. Y offsets are negated (WPF 3D is +Y up, screen space is +Y down) and **RotationY is negated** (XAML's positive `RotationY` brings the right edge toward the viewer; a positive WPF rotation moves it away). RotationX/RotationZ already match — do not "fix" them.
- `ProjectionHost.Prefer3D` keeps the 3D render path active while the projection is still the identity, so transitions/effects switch paths *before* their first animated frame. Do not reintroduce `BitmapCache` on transition or effect hosts: `Viewport2DVisual3D` already rasterizes the content, a second 1× cache only softens it (measured).
- Element effects (tilt, feathering) must project a host that **directly contains** the element (`ProjectionService.EnsureProjectionHost`). Reusing an ancestor host projects the whole page, because pages are themselves hosted by the transition frame's presenters.
- Identity projections must stay pixel-exact: projected feature positions match the 2D layout within 0.01 px (regression tests in `ProjectionTests`).

### Frame, navigation, transitions

- `TransitionFrame` template parts are `ProjectionHost` instances named `FirstContentPresenter` / `SecondContentPresenter` — not `ContentPresenter`.
- Exit transition completes **before** the entry transition starts; the old presenter is prepared (`IsHitTestVisible = false`, `Prefer3D = true`) before `Begin()` and interactivity is restored on completion. In the non-cancelled exit path the discarded presenter is completed with `null` (original behavior).
- Turnstile hinges near the left edge (`TurnstileCenterOfRotationX = -0.05`, `CenterOfRotationY = 0.5`); forward-out `0→50°`, forward-in `−80→0°`, phases scaled by `TurnstileDurationScale = 1.2`. Those two constants in `Transitions.cs` are the **only** sanctioned deviation from the original toolkit (`0` and `1` restore it exactly); do not change the storyboard numbers themselves.
- A page's **outgoing** transition is read from the page being left. Anything that shows a specific exit must declare it on that page or set it before navigating (the sample does this in `SampleNavigator`).
- `TiltEffect` wraps a tiltable control when it loads and unwraps it when tilt is disabled; never mutate the visual tree while a pointer is pressed.
- `TurnstileFeatherEffect` collects its targets from the page's **visual tree** when the storyboard is composed (the attached-property subscriptions only prepare the `ProjectionHost` wrappers, deferred to the dispatcher after `Loaded`/`SizeChanged`). Never re-parent content while a transition is animating or inside a layout pass.
- Every transition must complete. A storyboard with no animations (e.g. feathering a page without feathered elements) completes on the next dispatcher pass — otherwise the frame waits forever and the incoming page stays non-interactive.
- The incoming page is composed before it is connected to the visual tree: `TransitionFrame` forces `_newContentPresenter.UpdateLayout()` before composing so effects that need real geometry find their targets.
- Controls follow the original WP8 templates where one exists: `HubTile` slides a two-tile-high surface with `TranslateTransform` and flips with a real `PlaneProjection.RotationX` (`0→180→360`, front/back swapped while edge-on, `Prefer3D` for the flip); `Rating` stores `Value` in item units even with `AllowHalfItemIncrement`; `PhoneTextBox` places the hint at the caret offset and hides it while focused or composing.

## Engineering rules

- Build: `dotnet build Community.Phone.Controls.Toolkit.slnx -c Release` must stay at **0 warnings / 0 errors**.
- Tests: xUnit, WPF tests run through `StaTest.Run`. Run the whole suite: `dotnet test Community.Phone.Controls.Toolkit.slnx -c Release` (currently 144 tests: 142 pass, and the two `ListPicker` assertions that also fail at baseline). Every bug fix gets a regression test that fails before the fix.
- Package: `build\verify-package.ps1` packs `Release`, then restores/builds/runs a fresh consumer from the local feed. Re-run it after public API or behavior changes.
- Sample: `src/Community.Phone.Controls.Toolkit.Sample` is the manual/visual verification surface. It mirrors the original `PhoneToolkitSample8`: `MainPage` lists the samples, each sample is its own page, and `SampleNavigator` sets both transitions explicitly. When a behavior changes, the sample must still demonstrate it and still start cleanly.
- Docs are bilingual: English in `docs/en/`, Chinese in `docs/zh-CN/`; root-level docs use the `_zh-CN` suffix. Update **both** languages in the same change.
- `Themes/Generic.xaml` is the theme source for each assembly: the toolkit theme (Metro palette, control templates) and the compatibility layer theme (frame template). Do not hand-edit generated resource designer files or the original WP project under `repo-other`.
- Keep the library dependency-free; add a package only with a concrete, documented need.

## Verification environment notes

- The WP 8.1 emulator must be inspected through **Virtual Machine Connection** (`vmconnect.exe`), never `XDE.exe` (it hangs on this machine). Do not reset or delete the emulator VM.
- Offscreen evidence: `RenderTargetBitmap` (use 96 DPI and 144 DPI). On-screen evidence: `CopyFromScreen` from a `Topmost` window — `PrintWindow` returns black for 3D and VM surfaces. The machine runs at 150% DPI, so DIP↔pixel conversion matters.
- Stop the running sample before rebuilding; it locks `bin\Debug\...\*.resources.dll`.

## Documentation precedence

`source code + test results + git state` → `docs/handoff.md` (latest handoff) → `docs/*/architecture.md`, `docs/*/api-compatibility.md`, `docs/porting-status.md` → `docs/*/porting-inventory.md` (Phase 0 audit, historical) → `.github/Guide.md` (original task prompt, historical).
