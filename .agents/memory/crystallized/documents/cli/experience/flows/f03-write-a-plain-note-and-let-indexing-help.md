---
open-forge:
  description: Write a plain note and let indexing help
  tags: [Memory, Document, CLI, UserFlow, Evergreen]
---

# F03: Write a plain note and let indexing help

**Who:** A maintainer who writes ordinary Markdown directly.

**Behavior:** The current CLI implements this journey. The [Task record](../../../../../working/cli-development/tasks/task79-forgiving-maintenance.md) retains design decisions and historical reproduction evidence.

## Contract

The [maintenance design](../../technical-designs/forgiving-maintenance.md) and command contracts define completion, rename proof, protected sources and the next action.

## Starting point

ROUTES has guidance and no my-note.md. Keep the plain note body as the preservation oracle.

## Flow

Carry the actual state between steps. Fork only for an explicit alternative.

| Step | Action                                                                            | Scenario                                             | State passed forward                                                     |
| ---: | --------------------------------------------------------------------------------- | ---------------------------------------------------- | ------------------------------------------------------------------------ |
|    1 | Write `.agents/guidance/my-note.md` with a heading and body, without frontmatter. | [X02](../scenarios/experience.md#x02)                | The original body is known.                                              |
|    2 | Run `open-forge index --dry-run`.                                                 | [X04](../scenarios/experience.md#x04)                | Missing fields and navigation are previewed with zero writes.            |
|    3 | Run `open-forge index`.                                                           | [X02](../scenarios/experience.md#x02)                | Description and category tags are inserted, and guidance lists the note. |
|    4 | Run `open-forge context guidance/my-note`, then the printed `open-forge doctor`.  | [C02-13](../scenarios/commands/c02-doctor.md#c02-13) | The body is readable and metadata warnings are gone.                     |
|    5 | Run `open-forge index` again.                                                     | [C05-13](../scenarios/commands/c05-index.md#c05-13)  | A verified no-op preserves all bytes.                                    |

## Alternatives and recovery

### Partial metadata or another form

Fill only missing fields. New headers use the workspace's form. Retain supplied values, comments, unknown keys and body bytes. A partial header in the other form is completed in its existing form. See [X03](../scenarios/experience.md#x03) and [X32](../scenarios/experience.md#x32).

### Heading absent

Use sentence-case filename description. See [X33](../scenarios/experience.md#x33).

### Malformed or unreadable source

Malformed ordinary metadata stays untouched with one skip finding. Independent valid notes are completed. Follow the precise edit-and-index instruction. Required read denial remains a distinct limit. See [X05](../scenarios/experience.md#x05).

Index preserves protected sources and completes only eligible absent fields and rooted chains. Incomplete results state completed and skipped work separately. Edit-and-index instructions keep an explicit workspace. Doctor reports eligible completion consistently and prints no Next when clean.

## Final result

Files are discoverable and links resolve after all named choices or edits. Authored bodies and existing metadata survive. No guessed target is adopted.

## Verification

Observe the actual resulting state independently of the report. Record stdout, stderr, exit, changed paths, body and existing-field bytes, then run the printed next action. A snapshot alone does not prove preservation or a clean Doctor.
