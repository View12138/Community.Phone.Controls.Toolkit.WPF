# Architecture (entry point)

Canonical bilingual content: [English](en/architecture.md) · [中文](zh-CN/architecture.md)

This page is a short entry point for sessions that open `docs/architecture.md` directly. The full document (goals, rendering choice, geometry, XAML boundary, render-path switching, navigation, effects, theming, packaging, testing strategy) lives in the two files above; keep those in sync and treat them as authoritative.

## Confirmed decisions in one screen

1. **Rendering** — projection uses WPF `Viewport3D` + `Viewport2DVisual3D` so content stays live and interactive (`SetIsVisualHostMaterial` preserves hit testing).
2. **Perspective** — fixed `PerspectiveDepth = 999` DIP (XAML `M34 = −1/999`), camera at `(0, 0, depth)`, horizontal FOV `2·atan(viewportWidth / (2·depth))`. WPF's `FieldOfView` is horizontal; deriving it from the height scaled projected pages by the aspect ratio.
3. **Matrix** — row-vector `p' = (((p − center) + localOffset) · R) + center + globalOffset`, `R = Rz·Ry·Rx`, Y offsets negated, `RotationY` negated (XAML positive `RotationY` brings the right edge toward the viewer).
4. **Render-path consistency** — `ProjectionHost.Prefer3D` keeps the 3D path while the projection is the identity, so transitions/effects switch paths while preparing, not on their first frame; no `BitmapCache` on transition/effect hosts.
5. **Element effects** — `ProjectionService.EnsureProjectionHost` only reuses a host that directly contains the element; an ancestor host would project the whole page.
6. **Navigation** — `PhoneApplicationFrame`/`PhoneApplicationPage` wrap WPF navigation; `TransitionFrame` flips two `ProjectionHost` presenters and runs exit before entry.
7. **Transitions** — the 19 original storyboards are rebuilt in code (`Transitions.cs`) with their original values.
8. **Packaging** — one dependency-free package, `net10.0-windows`, root namespaces `Microsoft.Phone.Controls` / `Microsoft.Phone.Shell`. It contains two assemblies: the toolkit and the `Community.Phone` compatibility layer that re-implements the `Microsoft.Phone.dll` platform types.

Status of each area: [porting-status.md](porting-status.md). Latest session state: [handoff.md](handoff.md).
