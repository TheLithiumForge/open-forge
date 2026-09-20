---
open-forge:
  description: Historical CLI release Gate 3 start handoff
  tags: [Memory, Archived, Contextual, Historical, Handoff]
---

# CLI Release Gate 3 Start Handoff

## Seal

Sealed on 2026-08-16 after Gate 2 completion, authority correction, targeted
review, validation, and three coherent commits. Do not edit this record. Continue
mutable state in the [CLI Release Checkpoint](../checkpoints/cli-release.md).

This handoff supersedes the next actions in the Queue 31–33 pre-disposition
handoffs. Those records remain immutable history.

## Current state

- Gates 1 and 2 are complete. Gate 2 is non-shipping.
- Gate 3 full Architecture is the active next gate, open for discussion but not
  accepted or complete.
- Implementation remains blocked until the maintainer accepts Gate 3 and Gate 4
  crystallization/readiness completes.
- Queue 29 is integrated, Queue 30 is rejected/superseded history, Queue 31 route
  move/remove and Queue 32 cleanup are integrated, and Queue 33 completion is
  rejected/settled. No active command review remains.
- The new CLI is not implemented, packaged, or released. No branch merge, push,
  package publication, or release operation occurred.

## Retained delivery surface

The full-delivery target is exactly the current contracted commands:

- direct roots: `find`, `index`, `status`, `context`, `references`, `doctor`,
  `repair`, Framework `install`, Framework `update`, and operand-free `cleanup`;
- `route`: `inspect`, `list`, `init`, `create`, `update`, `move`, and `remove`;
- `extension`: `list`, `inspect`, `create`, `install`, `update`, and `remove`.

Shell completion is excluded. There is no partial publication slice. Gate 5 must
complete every retained command before packaging and release proof.

## Authority correction

The former product-specific Composable Operation Pattern was split correctly:

- [Shared CLI Operation Contract](../cli-release/shared-operation-contract.md) is
  the authoritative current Working source for exact Open Forge command/group,
  input, modifier, status, stream, typed-flow, no-op, recovery, cleanup-exception,
  and locality rules.
- [Composable CLI Operation Pattern](../../../patterns/open-forge/cli/composable-operation.md)
  is now a genuinely reusable, product-agnostic default shape. It does not define
  current Open Forge command semantics.

Targeted authority and writing review passed after correcting D086's safe
unchanged final-owner deletion wording and retaining explicit prohibitions on
string-keyed behavior registries, service locators, reflective dispatch, and a
universal operation engine.

## Review and commit policy

Authoritative tracked agent sources under `.apm/agents/` now default Reviewer and
Writing Reviewer work to explicit Git-diff baselines and paths. Consequential
code may use two bounded lenses: Reviewer for correctness, repository rules,
behavior, and integration; Improvement Reviewer for high-value local
opportunities. Neither is a global codebase review. The ignored `.opencode`
projections match the tracked sources locally.

Commit coherent inspectable changes. Keep inseparable current changes to one file
together rather than manufacturing misleading partial hunks. Inspect status,
diff, and recent log; stage only intended paths.

Commits created at this boundary:

- `c3ada3b Complete CLI command contracts and Gate 2`
- `9c8a40f Define CLI implementation workflow`
- `7c19db4 Target agent reviews to changed code`

The tracked worktree was clean immediately after those commits.

## Validation boundary

- Gate 2 scoped links: 164 files, 2,169 local references, 183 anchors, zero
  broken.
- Gate/Pattern rules: 12/12; no unexpected old Pattern authority label, current
  completion promise, or public drive-qualified path.
- Targeted reviewer configuration: 8/8 static rules and projection parity.
- Queue 31: 18/18 accepted rules. Queue 32: 24/24 accepted rules.
- Doctor, body loading, authored-file formatting, and `git diff --check`: clean.
- Fresh targeted Gate 2 closeout, Pattern/authority, and agent-configuration
  reviews passed after their correction passes.

These are contract, prose, routing, and configuration checks. They are not
executable, Native AOT, package, CI, or release evidence. Runtime validation of
the OpenCode permission loader remains unavailable because the local skill loader
attempted an unavailable PowerShell executable.

## Accepted Architecture constraints

- C# on .NET 10 or newer and modern `.slnx`.
- Native AOT and trimming evidence for every runtime feature, dependency, and
  serialization path.
- Real `System.IO` and isolated real temporary directories; no virtual filesystem
  abstraction.
- Keep command behavior, source, direct tests, fixtures, and one-use support at
  the narrowest useful local scope. No remote `utils`, service locator,
  reflective dispatch, or universal engine.
- Choose classes, functions, records, and focused local implementations by local
  fit. Dependencies must earn their AOT, size, maintenance, complexity, and
  security cost.
- xUnit v3 with explicit readable `DisplayName` and durable independently
  selectable feature/evidence traits.
- Foundation first, then all retained commands, thin packaging with npm first,
  then CI and main-only release proof.

## Evidence already gathered

- Local SDK evidence: .NET 10.0.101; no `global.json` currently pins it.
- `.slnx` creation works with `dotnet new sln --format slnx`; exact SDK behavior
  should be pinned and tested.
- System.CommandLine claims trimming and Native AOT support; application publish
  proof is still required.
- System.Text.Json should use source generation for AOT-safe serialization.
- YamlDotNet has .NET 10/AOT-related evidence but still needs application-level
  publish proof.
- Markdig supports .NET 10 but has no accepted Native AOT guarantee.
- xUnit v3 supports explicit display metadata, traits/query filtering, parallel
  controls, and Microsoft Testing Platform integration.

These are inputs to Architecture, not selected dependencies.

## Recommended Gate 3 discussion order

1. Foundation boundary: repository placement, `.slnx`, projects, dependency
   direction, executable/library split, and configuration pinning.
2. Dependency evidence: command parser, YAML, Markdown, JSON source generation,
   and a minimal Native AOT publish spike plan.
3. Core technology-neutral domains: paths/physical identity, Markdown layers,
   routing/index projection, references, lifecycle state, planning/results, Git,
   concurrency, and recovery.
4. Command-local source/test layout and the exact threshold for shared support.
5. xUnit v3 traits, filtering, parallel isolation, real temporary workspaces, and
   AOT/integration evidence lanes.
6. Thin package wrappers, CI matrices, artifact checks, and main-only release
   shape.

## Maintainer input boundary

No immediate clarification is required before preparing the first Architecture
recommendation. The next maintainer decision will be to accept, revise, or reject
the proposed foundation/project/dependency boundary and the minimal AOT evidence
spike. Do not create source projects or run an implementation spike until that
Architecture direction is presented and accepted.

## Key files

- [CLI Release Checkpoint](../checkpoints/cli-release.md)
- [CLI Release Program](../cli-release/_cli-release.md)
- [Release Plan](../cli-release/release-plan.md)
- [Decision Agenda](../cli-release/decision-agenda.md)
- [Command Contracts](../cli-release/commands/_commands.md)
- [Shared CLI Operation Contract](../cli-release/shared-operation-contract.md)
- [CLI Implementation Directive](../../../directives/open-forge/cli/implementation.md)
- [Development Workflow](../../../workflows/development/_development.md)
