---
open-forge:
  description: "Review and remove recovery leftovers"
  tags: [Memory, Document, CLI, UserFlow, Evergreen]
---

# F23: Review and remove recovery leftovers

**Who:** A maintainer cleaning up after completed work.

**Execution:** See the separate [run record](../../../../../archived/beta-preparation/cli-experience-run.md). Inclusion records a reviewed user outcome, not a passing implementation.

## Review adjustment

Retain an unrecognized or damaged recovery candidate with a warning while independently identified eligible leftovers can be removed. Never delete an ambiguous candidate merely to complete cleanup.

This desired behavior is pending flow validation before test implementation. A current CLI disagreement is recorded as evidence, not copied into the expectation.

## Starting point

RECOVERY with independently identified eligible complete bundles and a draft, no active need to recover from them.

## Flow

Run the steps on the actual resulting state. A linked scenario supplies a verification boundary, not permission to reset the workspace between steps.

| Step | Action | Scenario | State passed forward |
| ---: | --- | --- | --- |
| 1 | open-forge cleanup --dry-run | [C07-03](../scenarios/commands/c07-cleanup.md#c07-03) | The exact proposed recovery removals are visible without deleting evidence. |
| 2 | open-forge cleanup | [C07-02](../scenarios/commands/c07-cleanup.md#c07-02) | Only eligible selected leftovers are removed; workspace source content stays intact. |
| 3 | open-forge cleanup | [C07-01](../scenarios/commands/c07-cleanup.md#c07-01) | A complete empty catalogue produces the command’s no-op response. |

## Alternatives and recovery

### Damaged candidate

Retain and name the damaged candidate; continue only independently verified eligible removals and report both results.

Scenarios: [C07-04](../scenarios/commands/c07-cleanup.md#c07-04).

### Store unreadable

Unknown inventory is not an empty catalogue.

Scenarios: [C07-06](../scenarios/commands/c07-cleanup.md#c07-06).

### Deletion partly fails

Report what is already gone and what remains; a retry starts from actual state.

Scenarios: [C07-07](../scenarios/commands/c07-cleanup.md#c07-07).

## Final result

The user knows which recovery material was removed and that source content was not part of cleanup.

## Verification

Observe the resulting files and links independently of printed output. Record actions, stdout, stderr, exit, preserved bytes, and separate outcome/state/communication verdicts in a run record. Carry real state between steps; fork only at an explicit alternative.
