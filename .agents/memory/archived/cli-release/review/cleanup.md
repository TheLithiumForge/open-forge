---
open-forge:
  description: Settled Queue 32 history for operand-free cleanup of recognized Open Forge transient and recovery artifacts
  responsibility: Preserve the rejected selection model and Queue 32 reasoning without replacing the current cleanup contracts
  tags: [Memory, Archived, CLI, Release, Review, History, Settled, Contextual, Cleanup, Recovery, Safety, Historical]
---

# Cleanup Review History

**Packet:** Queue 32, Packet 5 of 6; `settled`

**Classification:** `#Contextual` settled review history. This file is not the
current Cleanup Interface Contract, Behavior Contract, Decision, Architecture,
or implementation record. The accepted result is integrated into the linked
current sources below.

## Status, authority, and purpose

Queue 32 settled the retained D017A cleanup operation after the maintainer
rejected the packet's artifact-selection model. The current authorities are the
[Cleanup Command Contract Set](../../../crystallized/documents/cli/contracts/cleanup/_cleanup.md), [Cleanup
Interface](../../../crystallized/documents/cli/contracts/cleanup/interface.md), [Cleanup
Behavior](../../../crystallized/documents/cli/contracts/cleanup/behavior.md), and the accepted [D017A Decision
Agenda entry](../../../working/cli-release/decision-agenda.md). They define current meaning; this record
preserves why the review changed shape and which candidate was rejected.

The new CLI remains non-shipping. Gate 2 remains open, and Gate 3 and
implementation remain blocked. Queue 33 completion review is now the next
review unit; this history does not decide it.

## Rejected candidate model

The packet proposed an explicit artifact-selection surface:

```text
open-forge cleanup [<artifact-reference>...] [--dry-run] [--skip-git-check] [global flags]
```

That model was rejected in full. It would have allowed a bare human invocation
to open a finite wizard, list recognized artifacts, and let the user explicitly
select zero or more artifacts. JSON and other non-interactive invocations would
have required explicit artifact selection and would have treated missing
selection as invalid. The model had no automatic selection mode, but it still
made operands, a wizard, and explicit selection the current contract shape.

The maintainer instead accepted one simple operand-free root command:

```text
open-forge cleanup [--dry-run] [--skip-git-check] [global flags]
```

Bare cleanup now discovers every currently eligible recognized artifact in the
selected workspace. Human, JSON, TTY, and non-interactive requests share that
domain behavior. There are no artifact operands, IDs, paths, selectors,
wizard, prompts, confirmation flags, `--automatic`, `--force`, `--yes`,
`--apply`, age filter, glob, saved plan, or cleanup profile. `--dry-run` is the
only preview mechanism.

## Retained reasoning and changed boundary

The candidate's useful boundary remains: cleanup is separate from Framework and
Extension lifecycle, Doctor, Repair, Index, package cleanup, repository
maintenance, and Gate 6 closeout. It may remove only positively recognized
Open Forge transient or recovery artifacts. Unknown, ambiguous, user-created,
active, and unsafe items remain untouched. Git checks remain narrow, Gitless
workspaces remain valid, and dry-run uses the same plan and preflight as apply.

The candidate packet required proof that a selected backup or residual was no
longer needed for recovery or verification. Queue 32 rejected that active
recovery-irrelevance proof requirement. Current deletion authority instead
comes from positive Open Forge provenance, bounded workspace association,
physical containment, inactive state, expected identity, and explicit command
intent. Cleanup does not inspect user content or infer migration intent.

The accepted catalogue covers target-associated adjacent backups, known
`.bak` compatibility forms when identity is provable, operation temporary or
staging files and directories, and residual recovery artifacts from incomplete
or completed operations. Suffix, age, extension, location, proximity, and
temporary-looking names do not establish identity. Repository `.temp/`, raw
evidence and snapshots, source and managed content, lifecycle documents and
receipts, generated navigation, build outputs, package caches, unrecognized
logs, and arbitrary backups remain excluded. Exact artifact names, storage, and
schema remain Gate 3 work.

Cleanup forms one complete deterministic catalogue and deletion plan before
effects and revalidates identity, containment, inactive state, and expected
bytes or physical identity immediately before each deletion. The accepted
cleanup-specific monotonic exception does not create a backup, staging copy,
receipt, journal, or tombstone merely to delete eligible cleanup artifacts and
does not reverse verified deletions. Partial failure or interruption reports
deleted, remaining, and residual facts; a rerun forms a fresh catalogue and
never deletes a later unknown or user-created replacement.

## Disposition consequences

- The three-file current contract set exists under `commands/cleanup/`.
- The four CLI overview Documents link the local Interface and Behavior without
  copying their detail. No cleanup Technical Design exists.
- The Shared CLI Operation Contract states the narrow monotonic deletion
  exception without weakening other write recovery rules.
- Status, Doctor, Repair, and public CLI prose retain their own boundaries and
  link cleanup where a separate deletion action matters. They do not run
  cleanup or absorb its catalogue.
- Queue 32 is settled. Queue 33 is the next review and remains undecided.

## Sources consulted

- [Decision Agenda](../../../working/cli-release/decision-agenda.md), especially D017A, D041–D046, and
  the shared status and stream decisions.
- [Cleanup Command Contract Set](../../../crystallized/documents/cli/contracts/cleanup/_cleanup.md), [Cleanup
  Interface](../../../crystallized/documents/cli/contracts/cleanup/interface.md), and [Cleanup
  Behavior](../../../crystallized/documents/cli/contracts/cleanup/behavior.md).
- [Global Flags Interface](../../../crystallized/documents/cli/contracts/shared/global-flags/interface.md) and
  [Behavior](../../../crystallized/documents/cli/contracts/shared/global-flags/behavior.md).
- [Doctor Interface](../../../crystallized/documents/cli/contracts/doctor/interface.md) and [Repair
  Interface](../../../crystallized/documents/cli/contracts/repair/interface.md) for diagnosis and mutation
  boundaries.
- [Shared CLI Operation Contract](../../../crystallized/documents/cli/shared-operation-contract.md).
- The pre-disposition Queue 32 packet, retained here as rejected candidate
  context rather than current authority.
