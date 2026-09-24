---
open-forge:
  description: "Remove a route without deleting authored meaning"
  tags: [Memory, Document, CLI, UserFlow, Evergreen]
---

# F10: Remove a route without deleting authored meaning

**Who:** A maintainer cleaning up obsolete guidance.

**Execution:** See the separate [run record](../../../../../working/cli-experience-run.md). Inclusion records a reviewed user outcome, not a passing implementation.

## Review adjustment

After the intended route is absent, repeating removal is a harmless no-op with a clear explanation. Retain meaningful text from incoming links and stop only where detachment would lose meaning.

This desired behavior is pending flow validation before test implementation. A current CLI disagreement is recorded as evidence, not copied into the expectation.

## Starting point

LINKS with an unowned obsolete source and a safe visible-text link to it; keep a separate unsafe-link branch.

## Flow

Run the steps on the actual resulting state. A linked scenario supplies a verification boundary, not permission to reset the workspace between steps.

| Step | Action | Scenario | State passed forward |
| ---: | --- | --- | --- |
| 1 | open-forge route remove guidance/notes --dry-run | [C17-04](../scenarios/commands/c17-route-remove.md#c17-04) | The selected source and link-markup changes are previewed. |
| 2 | open-forge route remove guidance/notes --automatic | [C17-02](../scenarios/commands/c17-route-remove.md#c17-02) | The source is removed while link text remains in the referring sentence. |
| 3 | open-forge doctor | [C02-01](../scenarios/commands/c02-doctor.md#c02-01) | The resulting authored references and navigation are checked on this same state. |
| 4 | open-forge route remove guidance/notes --automatic | [C17-05](../scenarios/commands/c17-route-remove.md#c17-05) | A repeated request reports that this source no longer exists; it does not delete a different source. |

## Alternatives and recovery

### Unsafe detachment

Stop before all effects, name the link and give one useful hand-edit instruction. After the user deliberately resolves the meaning, rerun from that changed state.

Scenarios: [C17-07](../scenarios/commands/c17-route-remove.md#c17-07).

### Managed source

Explicit Route removal records the exclusion and releases matching content claims while preserving package registrations and unrelated claims. Later updates respect the exclusion.

Scenarios: [C17-06](../scenarios/commands/c17-route-remove.md#c17-06).

## Final result

Only intended content is removed and surviving authored prose retains its meaning.

## Verification

Observe the resulting files and links independently of printed output. Record actions, stdout, stderr, exit, preserved bytes, and separate outcome/state/communication verdicts in a run record. Carry real state between steps; fork only at an explicit alternative.
