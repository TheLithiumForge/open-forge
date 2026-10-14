---
open-forge:
  description: Catch up after manual file renames and moves
  tags: [Memory, Document, CLI, UserFlow, Evergreen]
---

# F29: Catch up after manual file renames and moves

**Who:** A returning user reorganizing notes without CLI commands.

**Behavior:** The current CLI implements this journey. The [Task record](../../../../../working/cli-development/tasks/task79-forgiving-maintenance.md) retains design decisions and historical reproduction evidence.

## Contract

The [maintenance design](../../technical-designs/forgiving-maintenance.md) and command contracts define completion, rename proof, protected sources and the next action.

## Starting point

ROUTES has tracked billing/rounding.md and billing/taxes.md, authored incoming links, an outgoing relative link in taxes.md, and readable Git HEAD bytes.

## Flow

Carry the actual state between steps. Fork only for an explicit alternative.

| Step | Action                                                                           | Scenario                                             | State passed forward                                                                         |
| ---: | -------------------------------------------------------------------------------- | ---------------------------------------------------- | -------------------------------------------------------------------------------------------- |
|    1 | Rename rounding.md in place and move taxes.md into a new invoices/yearly folder. | [X38](../scenarios/experience.md#x38)                | Two old paths are missing, two new identities are present, and relative links may be broken. |
|    2 | Run `open-forge doctor`.                                                         | [C02-15](../scenarios/commands/c02-doctor.md#c02-15) | One safe next action covers proven incoming and source-move corrections.                     |
|    3 | Preview `open-forge repair --automatic --dry-run`.                               | [C06-15](../scenarios/commands/c06-repair.md#c06-15) | The plan includes relinks, the new chain and affected Entries.                               |
|    4 | Apply the printed repair command.                                                | [C06-15](../scenarios/commands/c06-repair.md#c06-15) | No target choice is required. The same plan completes affected navigation.                   |
|    5 | Run `open-forge doctor` and read both notes through Context.                     | [X38](../scenarios/experience.md#x38)                | Both notes are reachable and incoming and outgoing links resolve.                            |

## Alternatives and recovery

### Also edit a moved note

A unique score of at least 90 or unique recorded metadata identity may prove the successor. Below the threshold without another strong basis, choose explicitly. See [X43](../scenarios/experience.md#x43).

### No Git, stale record available or removed by Index

Exercise both orders on separate branches. No-Git unique metadata can prove identity before Index. Already-indexed no-Git requires exact intent when no record remains. See [X41](../scenarios/experience.md#x41) and [X42](../scenarios/experience.md#x42).

### Duplicate content or weak guesses

Follow guided Repair, never the same automatic retry. See [X39](../scenarios/experience.md#x39) and [X40](../scenarios/experience.md#x40).

Doctor and Repair agree on proof and protected sources. If Git times out, retry the complete request before editing a possibly renamed link. When Repair cannot ask, copy the chosen candidate command from its single row. All continuations keep an explicit workspace. Partial results state applied work and what remains. A proven canonical navigation rename is one row and preserves its rules.

## Final result

Files are discoverable and links resolve after all named choices or edits. Authored bodies and existing metadata survive. No guessed target is adopted.

## Verification

Observe the actual resulting state independently of the report. Record stdout, stderr, exit, changed paths, body and existing-field bytes, then run the printed next action. A snapshot alone does not prove preservation or a clean Doctor.
