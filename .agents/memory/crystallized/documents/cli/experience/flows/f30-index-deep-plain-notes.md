---
open-forge:
  description: Index deep plain notes
  tags: [Memory, Document, CLI, UserFlow, Evergreen]
---

# F30: Index deep plain notes

**Who:** A maintainer who nests new notes in folders without learning entrypoint conventions.

**Behavior:** The current CLI implements this journey. The [Task record](../../../../../working/cli-development/tasks/task79-forgiving-maintenance.md) retains design decisions and historical reproduction evidence.

## Contract

The [maintenance design](../../technical-designs/forgiving-maintenance.md) and command contracts define completion, rename proof, protected sources and the next action.

## Starting point

ROUTES has the Memory root and Crystallized state. No platform/payments/stripe folder or entrypoint exists.

## Flow

Carry the actual state between steps. Fork only for an explicit alternative.

| Step | Action                                                                                                                       | Scenario                                             | State passed forward                                                                                |
| ---: | ---------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------- | --------------------------------------------------------------------------------------------------- |
|    1 | Write `.agents/memory/crystallized/platform/payments/stripe/node.md` without frontmatter.                                    | [X34](../scenarios/experience.md#x34)                | Three missing scope boundaries lead to a plain note.                                                |
|    2 | Run `open-forge doctor`.                                                                                                     | [C02-14](../scenarios/commands/c02-doctor.md#c02-14) | Doctor names missing navigation files and prints Index.                                             |
|    3 | Run `open-forge index --dry-run`.                                                                                            | [C05-14](../scenarios/commands/c05-index.md#c05-14)  | All three canonical creates, category metadata and direct-child Entries are visible without writes. |
|    4 | Run `open-forge index`.                                                                                                      | [X34](../scenarios/experience.md#x34)                | The note is described, every folder has one entrypoint and the entire route is reachable.           |
|    5 | Run `open-forge context memory/crystallized/platform/payments/stripe/node`, then `open-forge doctor` and `open-forge index`. | [X34](../scenarios/experience.md#x34)                | The body is readable, Doctor is clean and Index is a byte no-op.                                    |

## Alternatives and recovery

### Metadata form, title fallback or partial header

Run root and scoped forms, a headingless note and partial metadata as separate branches. See [X32](../scenarios/experience.md#x32) and [X33](../scenarios/experience.md#x33).

### Empty, private, removed or package folders

No scaffold is created for resources alone or excluded content. See [X35](../scenarios/experience.md#x35).

### Malformed sibling

Complete the valid note, keep the malformed file untouched, then follow its exact edit-and-index action. See [X44](../scenarios/experience.md#x44).

Index preserves protected sources and completes only eligible absent fields and rooted chains. Incomplete results state completed and skipped work separately. Edit-and-index instructions keep an explicit workspace. Doctor reports eligible completion consistently and prints no Next when clean.

## Final result

Files are discoverable and links resolve after all named choices or edits. Authored bodies and existing metadata survive. No guessed target is adopted.

## Verification

Observe the actual resulting state independently of the report. Record stdout, stderr, exit, changed paths, body and existing-field bytes, then run the printed next action. A snapshot alone does not prove preservation or a clean Doctor.
