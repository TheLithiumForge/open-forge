---
open-forge:
  description: "Inspect a partial operation and choose a safe continuation"
  tags: [Memory, Document, CLI, UserFlow, Evergreen]
---

# F19: Inspect a partial operation and choose a safe continuation

**Who:** A maintainer whose operation stopped after doing some work.

**Execution:** See the separate [run record](../../../../../working/cli-experience-run.md). Inclusion records a reviewed user outcome, not a passing implementation.

## Starting point

FAULT-AFTER-EFFECT for a specific multi-effect mutation, with an independently recorded pre-state and recovery inventory.

## Flow

Run the steps on the actual resulting state. A linked scenario supplies a verification boundary, not permission to reset the workspace between steps.

| Step | Action | Scenario | State passed forward |
| ---: | --- | --- | --- |
| 1 | Trigger the chosen operation’s after-one-effect failure. | [X09](../scenarios/experience.md#x09) | The partial state is recorded, not summarized as a false no-op. |
| 2 | Compare actual completed effects, saved settings, ownership publication and retained recovery. | [X09](../scenarios/experience.md#x09) | The user knows which facts are verified and which remain unknown. |
| 3 | Run doctor on that same resulting state, interpreting its actual findings rather than forcing the base fixture’s expected count. | [C02-05](../scenarios/commands/c02-doctor.md#c02-05) | Current problems and incomplete checks are visible; the chosen mutation’s old report is not treated as current authority. |
| 4 | Only if current diagnosis proves an applicable safe-exact repair, preview it. Otherwise retain the evidence and use the explicit user-approved file recovery procedure for this fixture. | [X16](../scenarios/experience.md#x16) | No generic repair or rollback capability is invented. |
| 5 | Once recovery is no longer needed and eligible leftovers are understood, run open-forge cleanup --dry-run. | [C07-03](../scenarios/commands/c07-cleanup.md#c07-03) | Deletion of recovery evidence is reviewed separately from repairing source content. |

## Alternatives and recovery

### Recovery disposition unknown

Do not claim a retained usable backup or delete ambiguous evidence.

Scenarios: [X09](../scenarios/experience.md#x09).

### Settings grant saved before failure

Keep its receipt visible even if content application failed.

Scenarios: [X26](../scenarios/experience.md#x26).

### No safe continuation defined

Stop with the precise remaining state and decision. A truthful unresolved end is better than an invented success.

No additional scenario is required for this unresolved end.

## Final result

The partial result is understood and no recovery evidence is discarded before a justified continuation.

## Verification

Observe the resulting files and links independently of printed output. Record actions, stdout, stderr, exit, preserved bytes, and separate outcome/state/communication verdicts in a run record. Carry real state between steps; fork only at an explicit alternative.

## Notes

This flow requires a reproducible external fault fixture. It is designed but cannot be called executed from the supplied snapshots.
