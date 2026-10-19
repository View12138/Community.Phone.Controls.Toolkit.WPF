# English documentation

This directory is the English documentation set. The Chinese translations live in `docs/zh-CN/`.

- [Handoff](../handoff.md) — **start here in a new session**: latest verified state, uncommitted work, next steps, risks (kept at the `docs/` root)
- [Porting status](../porting-status.md) — current implementation/verification status per module (✅ verified / 🟡 implemented-unverified / ⛔ not implemented)
- [Original feature audit](porting-inventory.md) — Phase 0 audit of the original repository (historical evidence)
- [Architecture](architecture.md)
- [API compatibility matrix](api-compatibility.md)
- [Migration guide](migration-guide.md)
- [Known limitations](known-limitations.md)

Related: [README](../../README.md), [third-party notices](../../THIRD-PARTY-NOTICES.md), [中文文档索引](../zh-CN/README.md).

Information precedence: source/tests/git state → `handoff.md` → `architecture.md` / `api-compatibility.md` / `porting-status.md` → `porting-inventory.md` → `.github/Guide.md`.
Long-term development rules live in [`.github/copilot-instructions.md`](../../.github/copilot-instructions.md).
