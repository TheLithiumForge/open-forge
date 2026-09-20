---
open-forge:
  description: "Historical CLI-v2 source: Ordered replacement CLI slices, delivery state, and essential authority links for just-in-time Task planning"
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Replacement Backlog

## Operating Contract

This backlog records delivery order without becoming a second source for CLI
behavior. The accepted [CLI scope](../crystallized/documents/cli/_cli.md) owns
the replacement contract. The active Task record owns current progress,
decisions, commits, and evidence.

Create each Task just in time. Run it through the complete local
[Development Workflow](../../workflows/development/_development.md), accept it,
then create the next Task from the accepted baseline. Keep every Task on its
own unpushed `agents/feature/cli-overhaul-<task>` branch.

Routine repository tooling is maintained outside this backlog and its
implementation workflow.

## Task Planning Boundary

This backlog is a routing surface, not another behavioral specification. For a
queued slice, a new orchestrator reads the common authority below plus the
slice-specific links in its ordered entry, follows their current relationships, and
creates the concise Task record defined by [Task
Lifecycle](../../workflows/development/task-lifecycle.md#task-record). It adds
another source only when planning proves that source essential to the slice.

Every replacement Task starts from the CLI Architecture and verification
sources below. The remaining common sources apply only when the slice owns
their boundary:

- [CLI Architecture](../crystallized/documents/cli/architecture.md) for
  component boundaries, dependency direction, execution, safety, and evidence.
- When the slice owns public behavior, the exact command or family section in the [CLI
  Interface](../crystallized/documents/cli/interface.md) for public grammar and
  experience.
- When the slice evaluates operation eligibility, [operation prerequisites](../crystallized/documents/cli/contracts/operation-prerequisites.md)
  for eligibility and capability evidence.
- When the slice returns a public operation result, the [result and display boundary](../../patterns/open-forge/cli/commands/result-display-boundary.md)
  for typed completion and presentation.
- [CLI tiered test slice](../../patterns/open-forge/cli/bun/tiered-test-slice.md) and
  [development toolchain](../crystallized/documents/cli/development-toolchain.md)
  for proportionate verification.

The resulting Task links these sources under `Authority`; it does not duplicate
their guarantees. Historical Task records and archived analysis are evidence,
not default planning authority.

## Orchestration Guardrails

- Phase agents may edit and verify only their delegated surfaces. They leave
  changes unstaged and uncommitted; only the orchestrator stages and commits
  after inspecting the diff and reproducing proportionate evidence.
- When a material mismatch cannot be resolved exactly from accepted sources,
  stop the affected flow, record the mismatch, and ask the user before freezing
  new behavior. Do not convert uncertainty into a guessed Contract or test.
- Write grounded reusable observations at discovery in the nearest Observation
  route, including their source, scope, uncertainty, and disposition. Link the
  active Task instead of duplicating its complete evidence; the Task continues
  to own phase progress, commits, and acceptance evidence.
- Direct and integration tests use explicit named test declarations through
  the common Bun runner. Generated test declarations are reserved for an E2E
  runtime matrix, where every generated case carries a stable
  `@node`, `@bun`, or `@deno` tag. Every replacement full test name begins with
  exactly one `@unit`, `@integration`, or `@e2e` tier tag and a durable subject
  tag. Native Bun paths and `--test-name-pattern` select focused evidence.
- During each Development cycle, phase agents run only `check:fast` and scoped
  native tests. The mastermind formats exact Task paths, regenerates derived
  state, runs one public-surface scenario, delegates Whole-Task Review, and
  runs the full read-only accepted gate. One material correction may rerun only
  Gray, Red, or Green. Blue and Purple each run once.
- Record a newly discovered CLI edge case whose behavior is not yet accepted in
  [Development Edge Cases](../../../docs/edge-cases.md) for later inspection.
  An edge case that violates accepted behavior or safety remains a blocking
  finding and cannot be deferred this way.
- Apply the [Development Workflow material completion
  gate](../../workflows/development/_development.md#orchestration). Correctness,
  safety, accepted behavior, and required evidence remain hard gates; cosmetic,
  equivalent, speculative, and preference-only alternatives do not reopen a
  phase.

## Development Purple Phase

The accepted CLI Foundation trialed a temporary delegated Purple phase after
Blue. The local Development Workflow now runs Purple once, with a no-change
result when no material improvement exists in already-green tests or support.

Purple owns test-only structural improvement without changing accepted behavior,
Gray Contract, production, or expectation meaning. Missing, incorrect, or
incomplete expectations remain Red work.

## Ordered Work

1. **CLI foundation — Accepted.** [Accepted Task record](../archived/cli-foundation.md).
2. **Development Workflow and TypeScript boundaries — Accepted.** [Accepted Task record](../archived/cli-development-workflow.md).
3. **Complete `status` — Accepted.** [Accepted Task record](../archived/cli-status.md).
4. **Authored route inventory and `route list` — Queued.** Essential authority: [Interface](../crystallized/documents/cli/interface.md#route-list), [route inventory](../crystallized/documents/cli/contracts/route-inventory.md), [frontmatter YAML](../crystallized/decisions/cli/cli-frontmatter-yaml-boundary.md), [document facts](../../patterns/open-forge/cli/markdown/markdown-document-facts.md), and [inventory projection](../../patterns/open-forge/cli/markdown/route-inventory-projection.md).
5. **Markdown and local-reference facts with `route inspect` — Queued.** Essential authority: [Interface](../crystallized/documents/cli/interface.md#route-inspect), [route inventory](../crystallized/documents/cli/contracts/route-inventory.md), [local references](../crystallized/documents/cli/contracts/local-references.md), [document facts](../../patterns/open-forge/cli/markdown/markdown-document-facts.md), and [reference inventory](../../patterns/open-forge/cli/markdown/local-reference-inventory.md).
6. **`context` — Queued.** Essential authority: [Interface](../crystallized/documents/cli/interface.md#context), [route inventory](../crystallized/documents/cli/contracts/route-inventory.md), and [local references](../crystallized/documents/cli/contracts/local-references.md).
7. **`find` — Queued.** Essential authority: [Interface](../crystallized/documents/cli/interface.md#find), [route inventory](../crystallized/documents/cli/contracts/route-inventory.md), and [inventory projection](../../patterns/open-forge/cli/markdown/route-inventory-projection.md).
8. **Extension and lifecycle inventory with `extension list` — Queued.** Essential authority: [Interface](../crystallized/documents/cli/interface.md#extension-list), [managed lifecycle](../crystallized/documents/cli/contracts/managed-lifecycle.md), and [Extension architecture](../crystallized/documents/extensions/architecture.md).
9. **`extension inspect` — Queued.** Essential authority: [Interface](../crystallized/documents/cli/interface.md#extension-inspect), [managed lifecycle](../crystallized/documents/cli/contracts/managed-lifecycle.md), and [Extension architecture](../crystallized/documents/extensions/architecture.md).
10. **Filesystem effects, mutation execution, and workspace recovery — Queued.** Essential authority: [filesystem effects](../crystallized/documents/cli/contracts/filesystem-effects.md), [mutation execution](../crystallized/documents/cli/contracts/mutation-execution.md), [workspace recovery](../crystallized/documents/cli/contracts/workspace-recovery.md), [planned mutation](../../patterns/open-forge/cli/filesystem/planned-mutation.md), and [contained target](../../patterns/open-forge/cli/filesystem/contained-filesystem-target.md).
11. **Workspace formatting platform and adapter packs — Queued.** Essential authority: [Interface formatting](../crystallized/documents/cli/interface.md#workspace-formatting), [workspace formatting](../crystallized/documents/cli/contracts/workspace-formatting.md), and [formatter strategy](../../patterns/open-forge/cli/workspace/workspace-formatter-strategy.md).
12. **`route rebuild` — Queued.** Essential authority: [Interface](../crystallized/documents/cli/interface.md#route-rebuild), [route inventory](../crystallized/documents/cli/contracts/route-inventory.md), [mutation execution](../crystallized/documents/cli/contracts/mutation-execution.md), and [filesystem effects](../crystallized/documents/cli/contracts/filesystem-effects.md).
13. **`create` — Queued.** Essential authority: [Interface](../crystallized/documents/cli/interface.md#create), [request construction](../crystallized/documents/cli/contracts/request-construction.md), [mutation execution](../crystallized/documents/cli/contracts/mutation-execution.md), [Template creation](../../patterns/open-forge/cli/commands/template-backed-creation.md), and the [Template primitive](../crystallized/documents/framework/primitives/templates.md).
14. **`route init` — Queued.** Essential authority: [Interface](../crystallized/documents/cli/interface.md#route-init), [route inventory](../crystallized/documents/cli/contracts/route-inventory.md), and [mutation execution](../crystallized/documents/cli/contracts/mutation-execution.md).
15. **`doctor` — Queued.** Essential authority: [Interface](../crystallized/documents/cli/interface.md#doctor), [diagnosis and repair](../crystallized/documents/cli/contracts/diagnosis-and-repair.md), [route inventory](../crystallized/documents/cli/contracts/route-inventory.md), [local references](../crystallized/documents/cli/contracts/local-references.md), and the [diagnostic slice](../../patterns/open-forge/cli/diagnostics/diagnostic-domain-slice.md).
16. **`repair` — Queued.** Essential authority: [Interface](../crystallized/documents/cli/interface.md#repair), [diagnosis and repair](../crystallized/documents/cli/contracts/diagnosis-and-repair.md), [mutation execution](../crystallized/documents/cli/contracts/mutation-execution.md), and [workspace recovery](../crystallized/documents/cli/contracts/workspace-recovery.md).
17. **Whole-Framework `install` — Queued.** Essential authority: [Interface](../crystallized/documents/cli/interface.md#install), [managed lifecycle](../crystallized/documents/cli/contracts/managed-lifecycle.md), [mutation execution](../crystallized/documents/cli/contracts/mutation-execution.md), [workspace recovery](../crystallized/documents/cli/contracts/workspace-recovery.md), and [Framework architecture](../crystallized/documents/framework/architecture.md).
18. **`extension add` — Queued.** Essential authority: [Interface](../crystallized/documents/cli/interface.md#extension-add), [managed lifecycle](../crystallized/documents/cli/contracts/managed-lifecycle.md), [source review](../crystallized/documents/cli/contracts/source-review.md), [mutation execution](../crystallized/documents/cli/contracts/mutation-execution.md), and [Extension architecture](../crystallized/documents/extensions/architecture.md).
19. **`extension update` — Queued.** Essential authority: [Interface](../crystallized/documents/cli/interface.md#extension-update), [managed lifecycle](../crystallized/documents/cli/contracts/managed-lifecycle.md), [source review](../crystallized/documents/cli/contracts/source-review.md), [mutation execution](../crystallized/documents/cli/contracts/mutation-execution.md), and [Extension architecture](../crystallized/documents/extensions/architecture.md).
20. **`extension remove` — Queued.** Essential authority: [Interface](../crystallized/documents/cli/interface.md#extension-remove), [managed lifecycle](../crystallized/documents/cli/contracts/managed-lifecycle.md), [mutation execution](../crystallized/documents/cli/contracts/mutation-execution.md), [workspace recovery](../crystallized/documents/cli/contracts/workspace-recovery.md), and [Extension architecture](../crystallized/documents/extensions/architecture.md).
21. **Completion protocol and `completion script` — Queued.** Essential authority: [Interface](../crystallized/documents/cli/interface.md#completion-script), [completion protocol](../crystallized/documents/cli/contracts/completion-protocol.md), and [runtime compatibility](../crystallized/documents/cli/contracts/runtime-compatibility.md).
22. **`completion install` — Queued.** Essential authority: [Interface](../crystallized/documents/cli/interface.md#completion-install), [completion lifecycle](../crystallized/documents/cli/contracts/completion-lifecycle.md), and [request construction](../crystallized/documents/cli/contracts/request-construction.md).
23. **`completion remove` — Queued.** Essential authority: [Interface](../crystallized/documents/cli/interface.md#completion-remove), [completion lifecycle](../crystallized/documents/cli/contracts/completion-lifecycle.md), and [request construction](../crystallized/documents/cli/contracts/request-construction.md).
24. **Integrated cutover and frozen MVP retirement — Queued.** Essential authority: [CLI Architecture](../crystallized/documents/cli/architecture.md#executable-and-transition), [CLI Interface](../crystallized/documents/cli/interface.md), [runtime compatibility](../crystallized/documents/cli/contracts/runtime-compatibility.md), [development toolchain](../crystallized/documents/cli/development-toolchain.md), and the [direct replacement Decision](../crystallized/decisions/cli/cli-direct-replacement-development.md).

The 2026-08-04 documentation pass aligned the managed lifecycle, mutation, and
recovery contracts with the accepted post-format checksum sequence. That
alignment was direct CurrentTruth maintenance and is no longer an
implementation Task.

The orchestrator may split a queued slice before its Contract phase when the
accepted boundary is too large for one reviewable Task. It does not combine
independent public behavior merely to reduce the number of Tasks.

After integrated cutover is accepted, revisit the [debug diagnostics
Idea](../emerging/ideas/cli-debug-diagnostics.md) through brownfield dogfooding
and the [distribution channels Idea](../emerging/ideas/cli-distribution-channels.md)
through measured release needs. Neither creates an implementation slice unless
its Idea is deliberately promoted into accepted scope and planned just in time.

## Accepted Recovery Ordering

The user confirmed this lifecycle on 2026-08-03:

```text
apply and verify primary work
  -> write the primary lifecycle record
  -> format exact affected files
  -> validate formatted bytes
  -> atomically refresh advisory checksums
```

The checksum refresh preserves the relationship between the primary bytes and
the formatted bytes so later reconciliation can reason about upgrades without
treating formatting as unrecorded user change. The managed lifecycle, mutation,
workspace-formatting, and recovery contracts now express this same sequence.
