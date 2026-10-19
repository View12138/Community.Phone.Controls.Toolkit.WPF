# Copilot instructions — Community.Phone.Controls.Toolkit.WPF

## What this repository is

- A WPF compatibility port of the archived Microsoft **Windows Phone Toolkit** (MS-PL; original source at `D:\repos\repo-other\WindowsPhoneToolkit`), shipped as one NuGet package.
- Package/assembly: `Community.Phone.Controls.Toolkit.WPF`. **Root namespace is `Microsoft.Phone.Controls`** (plus `Microsoft.Phone.Shell`), target framework `net10.0-windows`, no third-party runtime dependencies.
- Goal: a WP toolkit app moves over by changing namespaces and wrapping projected content; Metro visuals, page transitions and 3D projection should behave like WP 8.1. This is a compatibility port, not a new UI framework.

## Non-negotiable principles

1. **Preserve the original public API shape** (type names, members, semantics, defaults). New behavior belongs in docs, not in renamed APIs.
2. WPF has no `UIElement.Projection`. The port provides `ProjectionHost` / `ProjectionService` / `PlaneProjection` / `Matrix3DProjection`. Port-only APIs must be *additive* and documented in `docs/*/api-compatibility.md`.
3. WP platform infrastructure (`PhoneApplicationFrame`, `PhoneApplicationPage`, `PhoneApplicationService`, `SystemTray`, `ApplicationBar`) is re-implemented in the compatible namespaces. Differences (no tombstoning, no real status bar/battery, etc.) must be stated, never hidden.
4. **Never report unimplemented or unverified behavior as done.** Unsupported modules stay listed in `docs/en/known-limitations.md` and `docs/zh-CN/known-limitations.md`.
5. Transition numbers (angles, durations, delays, easing) come from the original storyboard XAML under `Microsoft.Phone.Controls.Toolkit\Transitions\Storyboards\*.xaml`. Copy them; never invent or "tune" them.

## Architecture constraints — do not break these

### Projection / PlaneProjection

- Camera = XAML's fixed perspective: `ProjectionHost.PerspectiveDepth = 999` DIP (WinUI `M34 = -1/999`), camera at `(0, 0, depth)`, **horizontal** FOV derived from the viewport **width**: `2 * atan(viewportWidth / (2 * depth))`. Deriving it from the height shrinks projected content by the aspect ratio (past bug: 480×800 rendered as 288×480).
- Matrix is row-vector: `p' = (((p - center) + localOffset) * R) + center + globalOffset`, `R = Rz * Ry * Rx`. Y offsets are negated (WPF 3D is +Y up, screen space is +Y down) and **RotationY is negated** (XAML's positive `RotationY` brings the right edge toward the viewer; a positive WPF rotation moves it away). RotationX/RotationZ already match — do not "fix" them.
- `ProjectionHost.Prefer3D` keeps the 3D render path active while the projection is still the identity, so transitions/effects switch paths *before* their first animated frame. Do not reintroduce `BitmapCache` on transition or effect hosts: `Viewport2DVisual3D` already rasterizes the content, a second 1× cache only softens it (measured).
- Element effects (tilt, feathering) must project a host that **directly contains** the element (`ProjectionService.EnsureProjectionHost`). Reusing an ancestor host projects the whole page, because pages are themselves hosted by the transition frame's presenters.
- Identity projections must stay pixel-exact: projected feature positions match the 2D layout within 0.01 px (regression tests in `ProjectionTests`).

### Frame, navigation, transitions

- `TransitionFrame` template parts are `ProjectionHost` instances named `FirstContentPresenter` / `SecondContentPresenter` — not `ContentPresenter`.
- Exit transition completes **before** the entry transition starts; the old presenter is prepared (`IsHitTestVisible = false`, `Prefer3D = true`) before `Begin()` and interactivity is restored on completion. In the non-cancelled exit path the discarded presenter is completed with `null` (original behavior).
- Turnstile hinges at the left edge (`CenterOfRotationX = 0`, `CenterOfRotationY = 0.5`); forward-out `0→50°`, forward-in `−80→0°`. Storyboard values are normative.
- `TiltEffect` wraps a tiltable control when it loads and unwraps it when tilt is disabled; never mutate the visual tree while a pointer is pressed.

## Engineering rules

- Build: `dotnet build Community.Phone.Controls.Toolkit.slnx -c Release` must stay at **0 warnings / 0 errors**.
- Tests: xUnit, WPF tests run through `StaTest.Run`. Run the whole suite: `dotnet test Community.Phone.Controls.Toolkit.slnx -c Release` (currently 68 tests, all passing). Every bug fix gets a regression test that fails before the fix.
- Package: `build\verify-package.ps1` packs `Release`, then restores/builds/runs a fresh consumer from the local feed. Re-run it after public API or behavior changes.
- Sample: `src/Community.Phone.Controls.Toolkit.WPF.Sample` is the manual/visual verification surface. When a behavior changes, the sample must still demonstrate it and still start cleanly.
- Docs are bilingual: English in `docs/en/`, Chinese in `docs/zh-CN/`; root-level docs use the `_zh-CN` suffix. Update **both** languages in the same change.
- `Themes/Generic.xaml` is the single theme source. Do not hand-edit generated resource designer files or the original WP project under `repo-other`.
- Keep the library dependency-free; add a package only with a concrete, documented need.

## Verification environment notes

- The WP 8.1 emulator must be inspected through **Virtual Machine Connection** (`vmconnect.exe`), never `XDE.exe` (it hangs on this machine). Do not reset or delete the emulator VM.
- Offscreen evidence: `RenderTargetBitmap` (use 96 DPI and 144 DPI). On-screen evidence: `CopyFromScreen` from a `Topmost` window — `PrintWindow` returns black for 3D and VM surfaces. The machine runs at 150% DPI, so DIP↔pixel conversion matters.
- Stop the running sample before rebuilding; it locks `bin\Debug\...\*.resources.dll`.

## Documentation precedence

`source code + test results + git state` → `docs/handoff.md` (latest handoff) → `docs/*/architecture.md`, `docs/*/api-compatibility.md`, `docs/porting-status.md` → `docs/*/porting-inventory.md` (Phase 0 audit, historical) → `.github/Guide.md` (original task prompt, historical).
