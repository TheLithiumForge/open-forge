---
open-forge:
  description: "Run concurrent work without misleading lock advice"
  tags: [Memory, Document, CLI, UserFlow, Evergreen]
---

# F20: Run concurrent work without misleading lock advice

**Who:** A maintainer or agent running more than one command.

**Execution:** See the separate [run record](../../../../../archived/beta-preparation/cli-experience-run.md). Inclusion records a reviewed user outcome, not a passing implementation.

## Starting point

LOCK with a reproducible live first writer and a second independently prepared workspace.

## Flow

Run the steps on the actual resulting state. A linked scenario supplies a verification boundary, not permission to reset the workspace between steps.

| Step | Action                                                                                | Scenario                                            | State passed forward                                                    |
| ---: | ------------------------------------------------------------------------------------- | --------------------------------------------------- | ----------------------------------------------------------------------- |
|    1 | Start one mutation in workspace A and hold its real lock at a known boundary.         | [X21](../scenarios/experience.md#x21)               | A live operation, not merely a lock file, owns the workspace.           |
|    2 | Run index against A while the first operation holds the lock.                         | [C05-09](../scenarios/commands/c05-index.md#c05-09) | The second writer is blocked without effects.                           |
|    3 | Run an otherwise valid mutation in workspace B.                                       | [X21](../scenarios/experience.md#x21)               | B is not accidentally blocked by A’s per-workspace lock.                |
|    4 | Release the first operation; retry the second while the persistent lock file remains. | [X21](../scenarios/experience.md#x21)               | The retry can proceed from the actual current state after revalidation. |

## Alternatives and recovery

### First process crashes

OS lock ownership ends. Preserve actual partial-state evidence before retrying.

Scenarios: [X21](../scenarios/experience.md#x21).

### Inputs changed before retry

Recompute and revalidate current effects before retrying. The CLI does not wait for contention to end.

Scenarios: [X09](../scenarios/experience.md#x09).

## Final result

Concurrency is controlled by actual ownership, and the user receives accurate retry guidance.

## Verification

Observe the resulting files and links independently of printed output. Record actions, stdout, stderr, exit, preserved bytes, and separate outcome/state/communication verdicts in a run record. Carry real state between steps; fork only at an explicit alternative.

## Task 86 Runtime Expectations

Writing commands acquire `.open-forge-state/workspace.lock` for the selected physical workspace. They prepare new recovery bundles at `.open-forge-state/recovery/v1/<workspace key>/`. The live handle owns the lock. A leftover or nonempty sentinel does not block a writer. Dry runs, refusals, observers and verified no-ops create no runtime state. The legacy per-user lock bridge ends at 1.0. `OPENFORGE_DATA_HOME` controls only that bridge and old bundles. See the mutation and recovery design for the bridge gap and typed failures.
