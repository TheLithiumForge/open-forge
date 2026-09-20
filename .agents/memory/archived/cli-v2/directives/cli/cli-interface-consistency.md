---
open-forge:
  description: "Historical CLI-v2 source: Keep the Open Forge CLI explicit, orthogonal, predictable, helpful, and free of context-dependent command or flag behavior"
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Interface Consistency

## Instructions

### Operation Identity

- Make every invocation identify one operation, its subjects, its targets, and its lifecycle intent without depending on terminal state, unrelated workspace state, or filesystem guessing.
- Group commands around their semantic center and keep ordinary command paths shallow. A group provides discovery and composition, not another domain operation.
- Keep operands purpose-specific. Do not reinterpret one argument as unrelated input kinds or let one flag select several hidden operation modes.
- Follow the exact accepted command, operand, path, flag, alias, and applicability grammar in the [CLI Interface](../../../memory/crystallized/documents/cli/interface.md).

### Presentation And Policy

- Define shared and global flags once. Reuse a spelling only with the same value grammar, default, applicability, and meaning, and reject an inapplicable policy instead of ignoring or reinterpreting it.
- Keep human-readable and structured output as presentations of the same typed result. Presentation never changes domain behavior.
- Keep confirmation, replacement, deletion, Git policy, executable configuration, and source trust as separate authority boundaries. A convenience flag never becomes a generic safety bypass.
- Make invalid input explain the accepted shape and a useful next action without executing a guessed correction.

### Interaction And Automation

- Keep one deterministic automation path for every operation. A wizard may complete missing input and present decisions, but it does not own another request, planner, mutation, or result path.
- When an operation deliberately supports guided subject selection, explicit subjects bypass only selection. Selection never grants confirmation, replacement, deletion, executable, source-review, or recovery authority.
- Treat third-party source review as an independent boundary before mutation planning. First-party machine comments and visible Template guidance follow the accepted [source-review](../../../memory/crystallized/documents/cli/contracts/source-review.md) and [Template](../../../memory/crystallized/documents/framework/primitives/templates.md) contracts.

### Required Patterns

- When defining or registering a command, apply the [Predictable Command Surface](../../../patterns/open-forge/cli/commands/predictable-command-surface.md) and [CLI Command Slice](../../../patterns/open-forge/cli/commands/command-slice.md) Patterns.
- When resolving interactive or defaulted input, apply the [Guided Operation](../../../patterns/open-forge/cli/commands/guided-operation.md) Pattern.
- When returning or presenting a result, apply the [Result And Display Boundary](../../../patterns/open-forge/cli/commands/result-display-boundary.md) Pattern.
- When implementing a mutation, apply the [Planned Mutation](../../../patterns/open-forge/cli/filesystem/planned-mutation.md) Pattern and every focused Pattern it requires for the selected effects.
- When inspecting external source, apply the [Reviewed Source Boundary](../../../patterns/open-forge/cli/commands/reviewed-source-boundary.md) Pattern.
- When adding replacement CLI evidence, apply the [CLI Tiered Test Slice](../../../patterns/open-forge/cli/bun/tiered-test-slice.md) Pattern.
