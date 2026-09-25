---
open-forge:
  description: "Repair a link after a manual rename"
  tags: [Memory, Document, CLI, UserFlow, Evergreen]
---

# F08: Repair a link after a manual rename

**Who:** A maintainer who used the file manager instead of route move.

**Execution:** See the separate [run record](../../../../../archived/beta-preparation/cli-experience-run.md). Inclusion records a reviewed user outcome, not a passing implementation.

## Starting point

LINKS with an authored link to target.md and no ambiguous source identity.

## Flow

Run the steps on the actual resulting state. A linked scenario supplies a verification boundary, not permission to reset the workspace between steps.

| Step | Action | Scenario | State passed forward |
| ---: | --- | --- | --- |
| 1 | open-forge references guidance/target | [C10-01](../scenarios/commands/c10-references.md#c10-01) | The original incoming occurrence is known. |
| 2 | Rename only target.md to renamed-target.md using the file manager; then run open-forge references guidance/notes --direction=out. | [C10-05](../scenarios/commands/c10-references.md#c10-05) | The authored link is broken and its exact occurrence remains visible. |
| 3 | open-forge doctor | [C02-03](../scenarios/commands/c02-doctor.md#c02-03) | The actual broken-link state and available choices are reported; bind the count to this flow, not the two-link base fixture. |
| 4 | Bind LOCATION, EXPECTED and TARGET from the actual occurrence and the user’s chosen renamed-target.md; preview and apply the exact --relink request. | [C06-05](../scenarios/commands/c06-repair.md#c06-05) | Only the explicitly selected destination span changes. |
| 5 | open-forge references guidance/notes --direction=out | [C10-03](../scenarios/commands/c10-references.md#c10-03) | The repaired link resolves and its visible label survives. |
| 6 | open-forge repair --automatic | [C06-01](../scenarios/commands/c06-repair.md#c06-01) | Nothing remains repairable in the selected scope. |

## Alternatives and recovery

### Two plausible targets

Preserve the ambiguous link until the user chooses; a filename guess is not safe-exact authority.

Scenarios: [C06-03](../scenarios/commands/c06-repair.md#c06-03).

### Required route boundary unsafe

Block rather than applying a repair on incomplete required facts.

Scenarios: [X16](../scenarios/experience.md#x16).

## Final result

The chosen link works, original wording is kept and no unchosen candidate was adopted.

## Verification

Observe the resulting files and links independently of printed output. Record actions, stdout, stderr, exit, preserved bytes, and separate outcome/state/communication verdicts in a run record. Carry real state between steps; fork only at an explicit alternative.
