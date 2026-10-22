# API compatibility (entry point)

Canonical bilingual content: [English](en/api-compatibility.md) · [中文](zh-CN/api-compatibility.md)

This page is a short entry point for sessions that open `docs/api-compatibility.md` directly. The full matrix (per-type compatibility grades, default values, XAML forms that do/do not compile, test inventory) lives in the two files above.

## What the grades mean (same legend as the full matrix)

| Level | Meaning |
|---|---|
| **A. Fully compatible** | API and behavior match the original for the supported scenario; tests exist. |
| **B. API compatible, behavior differs** | Names, DPs, events and general behavior exist, but the WPF implementation uses a different host or platform service. |
| **C. Requires migration** | XAML or C# must change (namespace, wrapper, different base type). |
| **D. Alternative/wrapper** | A new WPF-specific type is required to express the feature. |
| **E. Not supported** | No compatible implementation; use an alternative or keep the WP application. |

## Non-negotiable compatibility rules

1. Original `Microsoft.Phone.Controls` / `Microsoft.Phone.Shell` type names, members and defaults are preserved whenever WPF allows it.
2. WPF has no `UIElement.Projection`; projected content must be wrapped in `ProjectionHost` or annotated with the `ProjectionService.Projection` attached property. `<Grid.Projection>` does not compile.
3. Port-only APIs (`ProjectionHost`, `ProjectionService`, `PhoneApplicationFrame.Navigate(page, parameters)`, `ProjectionHost.Prefer3D`) are additive and documented here.
4. Anything unimplemented is listed as unimplemented in [known-limitations.md](en/known-limitations.md) and in [porting-status.md](porting-status.md) — never silently stubbed.
5. Every deviation needs a written rationale plus an alternative, so a migrating app can act on it.

Current verification level per type: [porting-status.md](porting-status.md).
