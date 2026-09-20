---
open-forge:
  description: Historical CLI release Gate 2 handoff for queue item 32
  tags: [Memory, Archived, Contextual, Historical, Handoff]
---

# CLI Release Gate 2 Queue 32 Handoff

## Seal

Sealed on 2026-08-16 after Queue 31 contract integration, fresh review,
correction, indexing, and final static validation. Do not edit this record. The
mutable resumable state remains in the [CLI Release Checkpoint](../checkpoints/cli-release.md).

This handoff supersedes the next action in the earlier pre-disposition Queue 31
handoff. That earlier handoff remains immutable history.

## Current authority and state

- Gate 1 is complete. Gate 2 remains open and non-shipping. Gate 3 Architecture
  and implementation remain blocked.
- Queue 29 is accepted and integrated. Queue 30 is rejected/superseded history.
- Queue 31 is settled. Current `route move` and `route remove` Interface and
  Behavior Contracts now carry its authority; the Queue 31 packet is contextual
  history.
- Queue 32 root `cleanup` is the current reviewing packet. Queue 33 completion
  retain/reject remains waiting.
- Preserve the cumulative dirty Queue 22+ worktree. No branch, commit, merge,
  push, implementation, package, or release action has been performed.

## Queue 31 accepted result

`route move` and `route remove` each accept one eligible ordinary unmanaged leaf
or category. A category is selected by its recognized entrypoint and includes
the complete physically contained folder tree, including every safe regular file
and directory. It is one atomic plan and recovery boundary, not independent leaf
commands. Complete trusted Framework and Extension ownership evidence must prove
that no selected item is managed.

Move uses an exact destination and rewrites exact supported local Markdown links
throughout the physically contained workspace whenever their existing
destination would no longer resolve to the same intended target. Remove converts
each exact supported incoming external link to its visible label as plain
authored text, preserves surrounding prose, and surfaces every detachment.
Incomplete coverage or unsafe/ambiguous transformations prevent all effects.
Generated projections, Git, dry-run, expected-state, verification, recovery,
consent, and honest-repeat rules are integrated in the current contracts.

Fresh semantic and writing review produced seven unique corrective findings;
all were corrected in one correction pass. Final focused evidence:

- Queue 31 accepted rules: 18/18;
- stale leaf-only/outside-reference-stop claims: zero;
- public drive-qualified paths: zero;
- 40 scoped files, 1,045 local references, and 99 anchors: zero broken after
  fenced examples were excluded from link interpretation;
- Doctor: no problems;
- body loading: success; and
- `git diff --check`: success.

This remains contract/static evidence, not implementation or Native AOT proof.

## Queue 32 decision surface

The current candidate is in [Cleanup Review](../cli-release/review/cleanup.md).
Present it for explicit accept, revise, or reject disposition before authoring a
current command contract or advancing Queue 33.

Candidate syntax:

```text
open-forge cleanup [<artifact-reference>...] [--dry-run] [--skip-git-check] [global flags]
```

The candidate retains a narrow root command for proven CLI-owned recovery
artifacts:

- artifact references are exact recognized current-catalogue IDs or exact paths,
  never arbitrary paths, globs, age expressions, or filename resemblance;
- a bare human invocation may open a finite wizard over currently eligible
  artifacts, while JSON and every other non-interactive invocation require
  explicit selection;
- no automatic selection/deletion, `--force`, `--yes`, `--apply`, broad scan,
  saved plan, or profile exists;
- only adjacent target-associated backups, operation temporary/staging artifacts,
  and completed residuals with positively established operation identity and no
  remaining recovery/verification need are eligible;
- still-needed, mixed, changed, unknown, ambiguous, or colliding artifacts are
  preserved and surfaced;
- repository `.temp`, raw evidence, arbitrary backups, build output, caches,
  lifecycle records, source files, generated navigation, and user files are out
  of scope;
- dry-run uses the same catalogue, plan, and preflight; affected-path Git,
  expected-state, verification, residual preservation, and honest no-op rules
  apply; and
- Gate 6 documentation/history/release cleanup stays separate.

The recommendation is to retain this command only with the complete positive
identity and recovery-irrelevance proof. If that proof cannot be designed without
hidden state, reject the command rather than broaden deletion authority.

## Resume sequence

1. Load the mutable Checkpoint and this handoff.
2. Present Queue 32's concise candidate and maintainer checklist.
3. Obtain explicit accept, revise, or reject disposition.
4. Integrate any accepted result into current contracts, Agenda, public docs,
   queue/program state, and generated navigation; perform focused review and
   validation.
5. Only then advance Queue 33. After Queue 33 integration, close Gate 2 and begin
   the full Gate 3 Architecture discussion.

## Key files

- [CLI Release Checkpoint](../checkpoints/cli-release.md)
- [CLI Release Program](../cli-release/_cli-release.md)
- [Review Queue](../cli-release/review/queue.md)
- [Queue 31 History](../cli-release/review/route-move-remove.md)
- [route move Contract Set](../cli-release/commands/route/move/_move.md)
- [route remove Contract Set](../cli-release/commands/route/remove/_remove.md)
- [Queue 32 Cleanup](../cli-release/review/cleanup.md)
- [Queue 33 Completion](../cli-release/review/completion.md)
- [Decision Agenda](../cli-release/decision-agenda.md)
- [Release Plan](../cli-release/release-plan.md)
- [CLI Implementation Directive](../../../directives/open-forge/cli/implementation.md)
- [Development Workflow](../../../workflows/development/_development.md)
