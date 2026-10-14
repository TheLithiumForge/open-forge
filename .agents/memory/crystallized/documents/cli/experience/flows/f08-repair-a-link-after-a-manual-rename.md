---
open-forge:
  description: Repair a link after a manual rename
  tags: [Memory, Document, CLI, UserFlow, Evergreen]
---

# F08: Repair a link after a manual rename

**Who:** A maintainer who renames a note in the file manager.

**Behavior:** The current CLI implements this journey. The [Task record](../../../../../working/cli-development/tasks/task79-forgiving-maintenance.md) retains design decisions and historical reproduction evidence.

## Contract

The [maintenance design](../../technical-designs/forgiving-maintenance.md) and command contracts define completion, rename proof, protected sources and the next action.

## Starting point

LINKS has a tracked billing/rounding.md with an authored incoming link and a readable last committed version.

## Flow

Carry the actual state between steps. Fork only for an explicit alternative.

| Step | Action                                                                                | Scenario                                                 | State passed forward                                                               |
| ---: | ------------------------------------------------------------------------------------- | -------------------------------------------------------- | ---------------------------------------------------------------------------------- |
|    1 | Inspect `open-forge references guidance/billing/rounding`.                            | [C10-01](../scenarios/commands/c10-references.md#c10-01) | The incoming occurrence and expected literal are known.                            |
|    2 | Rename rounding.md to money-rounding.md by hand.                                      | [X37](../scenarios/experience.md#x37)                    | The old path is missing and the new bytes are identical.                           |
|    3 | Run `open-forge doctor`.                                                              | [C02-15](../scenarios/commands/c02-doctor.md#c02-15)     | Doctor proves one successor and prints `open-forge repair --automatic`.            |
|    4 | Run `open-forge repair --automatic --dry-run`, then its printed apply command.        | [C06-14](../scenarios/commands/c06-repair.md#c06-14)     | The uniquely proven link and affected Entries are updated without a target choice. |
|    5 | Run `open-forge references guidance/notes --direction out`, then `open-forge doctor`. | [X37](../scenarios/experience.md#x37)                    | The link resolves, its label survives and Doctor is clean.                         |
|    6 | Run `open-forge repair --automatic` again.                                            | [C06-01](../scenarios/commands/c06-repair.md#c06-01)     | A verified no-op has no extra effects.                                             |

## Alternatives and recovery

### Several strong candidates or title-only evidence

Keep the original literal until a user selects an exact target. See [X39](../scenarios/experience.md#x39) and [X40](../scenarios/experience.md#x40).

### No Git or Index already ran

Use unique old metadata when available. Without any old identity, use explicit relink intent. See [X41](../scenarios/experience.md#x41) and [X42](../scenarios/experience.md#x42).

### Required affected navigation is unsafe

The combined plan must stop before writing. Unrelated damage stays visible without blocking independent work. See [X16](../scenarios/experience.md#x16).

Doctor and Repair agree on proof and protected sources. If Git times out, retry the complete request before editing a possibly renamed link. When Repair cannot ask, copy the chosen candidate command from its single row. All continuations keep an explicit workspace. Partial results state applied work and what remains. A proven canonical navigation rename is one row and preserves its rules.

## Final result

Files are discoverable and links resolve after all named choices or edits. Authored bodies and existing metadata survive. No guessed target is adopted.

## Verification

Observe the actual resulting state independently of the report. Record stdout, stderr, exit, changed paths, body and existing-field bytes, then run the printed next action. A snapshot alone does not prove preservation or a clean Doctor.
