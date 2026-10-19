# Porting inventory (entry point)

Canonical bilingual content: [English](en/porting-inventory.md) · [中文](zh-CN/porting-inventory.md)

This page is a short entry point for sessions that open `docs/porting-inventory.md` directly. The full inventory is the **Phase 0 audit** of the archived toolkit — it records what the original source contains (public types, module coverage, dependency graph, theme/resources, risks) and the status that was assumed at that time.

## How to read it

- Treat it as **historical evidence about the original**, not as the current state of this port.
- Current implementation/verification status: [porting-status.md](porting-status.md).
- Confirmed design decisions: [architecture.md](architecture.md) (canonical: [en](en/architecture.md), [zh-CN](zh-CN/architecture.md)).
- Latest handoff snapshot: [handoff.md](handoff.md).

## Original audit facts worth knowing up front

- Original repo: `D:\repos\repo-other\WindowsPhoneToolkit`, branch `master`, commit `4e4fd5d`, MS-PL.
- Original solutions: `PhoneToolkit.sln`, `Microsoft.Phone.Controls.Toolkit.WP7.sln`; shared source linked into WP7 and WP8 projects.
- Original audit scope: 245 `.cs`, 82 `.xaml`, 56 `.resx`, 51 `.xlf` files.
- Original theme: `Microsoft.Phone.Controls.Toolkit.WP8\Themes\Generic.xaml` (≈150 KB); 51 cultures of localized resources.
- Original projects are non-SDK-style and depend on `Microsoft.Phone*`, XNA and WP tooling — they cannot be built by the modern .NET SDK, which is why a WPF port exists at all.
- Transition storyboard values used by the port come from `Microsoft.Phone.Controls.Toolkit\Transitions\Storyboards\*.xaml` in that original tree.
