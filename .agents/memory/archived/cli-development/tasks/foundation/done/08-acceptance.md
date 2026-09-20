---
open-forge:
  description: Integrate and accept the complete command-free architectural foundation
  tags: [Memory, Archived, Contextual, Historical, CLI, Task, Foundation, Acceptance, Integration, Complete]
---

# Accept The Architectural Foundation

## Task State

- State: Complete.
- Implementer: Mastermind.
- Responsible role: Mastermind.
- Parent: [CLI Foundation](../_foundation.md).
- Plan gate: G1 and VG1.

## Acceptance Review

Inspect the complete Git diff from `50f27ad` plus the Task-authoring commit through
the proposed foundation. Review actual source and direct consumers, not summaries.

Confirm:

1. Physical paths, namespaces, project graph, and dependency direction match the
   Architecture.
2. Root contains process and composition only. Core contains no ambient process
   state. Shell contains no command dependency.
3. Callable contracts are cohesive and sufficient for later commands without a
   service bag, reflection, or universal result.
4. Parser ownership, terminal ordering, direct stages, writer isolation, and
   process completion are proven.
5. Filesystem safety proves every component and the leave-and-reenter regression.
6. Serialization uses source generation with reflection disabled.
7. Active tests and TestSupport match tier and ownership boundaries.
8. No probe, fake command, route source, root C# file, project-local output, old
   project reference, or compiled preserved evidence remains.

## Executable Verification

Run, in order:

1. restore with scoped NuGet configuration;
2. format verification;
3. warning-free Release build;
4. Unit, Integration, and EndToEnd managed tests independently;
5. package vulnerability and transitive audit;
6. local Native AOT root publish and public terminal journeys;
7. sequential Native AOT Integration and EndToEnd publishes;
8. published test executable runs;
9. artifact, local-output, project-reference, namespace, dependency, stale-path,
   and `git diff --check` audits.

## Review Packet

After Mastermind inspection, one bounded correctness reviewer may inspect the
foundation diff and direct integration neighborhood if a fresh lens can still add
value. Give it the exact baseline, paths, Architecture, foundation Tasks, and
claimed evidence. Do not delegate architecture acceptance.

## Pass Condition

Every check passes without warning, skip of required target behavior, or residual
architecture debt. Update all foundation child states, parent state, Plan step G1,
and Checkpoint in the same accepted commit.

## Failure Routing

- Contract or class-model defect: return to Shell Contracts.
- Parser or stage behavior defect: return to the owning foundation Task.
- BCL safety proof gap: stop at the Architecture decision point.
- Test-tier or fixture defect: return to Test/AOT.
- Local cleanup opportunity with frozen behavior: correct before acceptance.

## Acceptance Record

- Workspace topology: `ad2d49e`.
- Command-free Core and root host: `2662f50`.
- Active evidence and six-RID workflow: `7a601cc`.
- Managed evidence: 33 Unit, 20 Integration, and 4 EndToEnd cases.
- Native evidence: current `win-x64` root, Integration, and EndToEnd publishes and
  executable runs passed without warning or skipped safety behavior.
- Package vulnerability, artifact boundary, project reference, stale-path,
  formatting, and `git diff --check` audits passed.
- The bounded correctness review passed after its filesystem, parser, output,
  typed-result, TestSupport ownership, and process-cancellation findings were
  corrected and reverified.

## Completion

Complete only when the committed foundation is the trustworthy base for
`route list`; passing counts alone are insufficient.
