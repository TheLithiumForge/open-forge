---
open-forge:
  description: "Move a route and preserve links in one operation"
  tags: [Memory, Document, CLI, UserFlow, Evergreen]
---

# F09: Move a route and preserve links in one operation

**Who:** A maintainer who expects a move command to preserve navigability.

**Execution:** See the separate [run record](../../../../../working/cli-experience-run.md). Inclusion records a reviewed user outcome, not a passing implementation.

## Review adjustment

Move within Guidance, from guidance/notes to guidance/team/moved-note, so this navigation check does not accidentally turn advice into a Pattern. Initialize the destination scope as fixture setup.

This desired behavior is pending flow validation before test implementation. A current CLI disagreement is recorded as evidence, not copied into the expectation.

## Starting point

LINKS with a relocation-sensitive outgoing link and authored incoming links from more than one scope.

## Flow

Run the steps on the actual resulting state. A linked scenario supplies a verification boundary, not permission to reset the workspace between steps.

| Step | Action | Scenario | State passed forward |
| ---: | --- | --- | --- |
| 1 | open-forge route move guidance/notes guidance/team/moved-note --dry-run | [C16-04](../scenarios/commands/c16-route-move.md#c16-04) | The complete move, parent-list and reference changes are visible without writes. |
| 2 | open-forge route move guidance/notes guidance/team/moved-note | [C16-02](../scenarios/commands/c16-route-move.md#c16-02) | The source moved and all affected authored destinations were rewritten. |
| 3 | open-forge references guidance/team/moved-note | [C10-01](../scenarios/commands/c10-references.md#c10-01) | The resulting incoming and outgoing authored links refer to the new location. |
| 4 | open-forge context guidance/team/moved-note | [C08-02](../scenarios/commands/c08-context.md#c08-02) | The moved source remains usable through the displayed new ID. |

## Alternatives and recovery

### Occupied destination

Preserve both sources and every reference.

Scenarios: [C16-05](../scenarios/commands/c16-route-move.md#c16-05).

### Category instead of leaf

Fork from a suitable category fixture; enumerate every moved descendant and overwrite companion.

Scenarios: [C16-03](../scenarios/commands/c16-route-move.md#c16-03).

### Unreadable incoming candidate

Do not partially move before determining complete required reference effects.

Scenarios: [C16-11](../scenarios/commands/c16-route-move.md#c16-11).

## Final result

The route has one new location and its authored relationships still work.

## Verification

Observe the resulting files and links independently of printed output. Record actions, stdout, stderr, exit, preserved bytes, and separate outcome/state/communication verdicts in a run record. Carry real state between steps; fork only at an explicit alternative.
