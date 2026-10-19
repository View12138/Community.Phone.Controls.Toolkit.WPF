# 中文文档索引

本目录是 `docs/en/` 英文文档的中文版本。代码标识符、类型名、属性名和 API 签名保持英文，以避免与源码不一致。

## 文档

- [交接文档](handoff.md)：**新会话入口**——最近一次已验证状态、未提交修改、下一步和风险。
- [移植状态](porting-status.md)：按模块的当前实现/验证状态（✅ 已验证 / 🟡 已实现未验证 / ⛔ 未实现），权威状态表。
- [原始功能审计](porting-inventory.md)：Phase 0 对原始仓库的审计、模块覆盖、依赖关系、XAML 用法和验证记录（历史证据）。
- [架构设计](architecture.md)：投影、导航、转场、效果、控件、主题、打包和测试策略。
- [API 兼容矩阵](api-compatibility.md)：WP 类型到 WPF 类型、兼容级别、XAML 迁移方式和行为差异。
- [迁移指南](migration-guide.md)：从 Windows Phone Toolkit 迁移到 WPF 的步骤、示例和常见问题。
- [已知限制](known-limitations.md)：未实现模块、行为差异、手动验证步骤和后续路线。

## 相关文档

- [中文 README](../../README_zh-CN.md)
- [English documentation](../en/README.md)
- [第三方声明](../../THIRD-PARTY-NOTICES_zh-CN.md)

## 说明

- `docs/en/` 保留英文原文，作为 API 签名和原始措辞的基准。
- `docs/zh-CN/` 提供完整的中文说明；公共成员附录中的 API 名称不翻译。
- 如果中英文内容出现冲突，以源码、测试和英文 API 文档为准。
- 信息优先级：源码/测试/Git 状态 → `handoff.md` → `architecture.md` / `api-compatibility.md` / `porting-status.md` → `porting-inventory.md` → `.github/Guide.md`。
- 长期开发规则见 [`.github/copilot-instructions.md`](../../.github/copilot-instructions.md)（Copilot 只读取该英文单文件，中文摘要见 [handoff.md](handoff.md) §11）。
