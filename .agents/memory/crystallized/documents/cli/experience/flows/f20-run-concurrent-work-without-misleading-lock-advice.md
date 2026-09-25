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

| Step | Action | Scenario | State passed forward |
| ---: | --- | --- | --- |
| 1 | Start one mutation in workspace A and hold its real lock at a known boundary. | [X21](../scenarios/experience.md#x21) | A live operation, not merely a lock file, owns the workspace. |
| 2 | Run index against A while the first operation holds the lock. | [C05-09](../scenarios/commands/c05-index.md#c05-09) | The second writer is blocked without effects. |
| 3 | Run an otherwise valid mutation in workspace B. | [X21](../scenarios/experience.md#x21) | B is not accidentally blocked by A’s per-workspace lock. |
| 4 | Release the first operation; retry the second while the persistent lock file remains. | [X21](../scenarios/experience.md#x21) | The retry can proceed from the actual current state after revalidation. |

## Alternatives and recovery

### First process crashes

OS lock ownership ends; preserve actual partial-state evidence before retrying.

Scenarios: [X21](../scenarios/experience.md#x21).

### Inputs changed during the wait

Recompute/revalidate current effects; never reuse a stale plan blindly.

Scenarios: [X09](../scenarios/experience.md#x09).

## Final result

Concurrency is controlled by actual ownership, and the user receives accurate retry guidance.

## Verification

Observe the resulting files and links independently of printed output. Record actions, stdout, stderr, exit, preserved bytes, and separate outcome/state/communication verdicts in a run record. Carry real state between steps; fork only at an explicit alternative.
