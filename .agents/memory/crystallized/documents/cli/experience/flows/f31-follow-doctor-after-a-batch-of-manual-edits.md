---
open-forge:
  description: Follow Doctor after a batch of manual edits
  tags: [Memory, Document, CLI, UserFlow, Evergreen]
---

# F31: Follow Doctor after a batch of manual edits

**Who:** A returning user catching up after several file-manager edits.

**Behavior:** The current CLI implements this journey. The [Task record](../../../../../archived/cli-development/tasks/task79-forgiving-maintenance.md) retains design decisions and historical reproduction evidence.

## Contract

The [maintenance design](../../technical-designs/forgiving-maintenance.md) and command contracts define completion, rename proof, protected sources and the next action.

## Starting point

A clean ROUTES workspace has tracked notes and authored links. It includes a folder with compatibility index.md so renaming that folder preserves its entrypoint identity.

## Flow

Carry the actual state between steps. Fork only for an explicit alternative.

| Step | Action                                                                                                                                                | Scenario                                             | State passed forward                                                                      |
| ---: | ----------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------- | ----------------------------------------------------------------------------------------- |
|    1 | Rename one file, move one across folders, add deep plain and partial notes, delete a referenced note, and rename the compatibility-entrypoint folder. | [X47](../scenarios/experience.md#x47)                | All changes exist together. Keep independent before-byte oracles for retained files.      |
|    2 | Run `open-forge doctor` and preview its safe Repair command.                                                                                          | [C02-18](../scenarios/commands/c02-doctor.md#c02-18) | Proven identity fixes are selected without guessing the deleted note.                     |
|    3 | Apply Repair and follow its next Index action for unrelated new notes.                                                                                | [C06-21](../scenarios/commands/c06-repair.md#c06-21) | Links and affected Entries are current. Unrelated chains and metadata are then completed. |
|    4 | Follow the printed exact edit for the deleted note: remove or intentionally replace the authored link.                                                | [X45](../scenarios/experience.md#x45)                | The user supplies the deletion meaning. The CLI never recreates the note.                 |
|    5 | Run the printed final Doctor command, then repeat Index and automatic Repair.                                                                         | [X47](../scenarios/experience.md#x47)                | Doctor has complete coverage with no warnings or errors. Repetition changes no bytes.     |

## Alternatives and recovery

### A whole canonical-entrypoint folder is renamed

When Git proves the old canonical entrypoint moved with the folder, rename it to the new canonical name and keep its description, Axioms and body. Without proof, Index creates a new entrypoint and Doctor explains how to preserve the old rules before deleting the old file. See [X46](../scenarios/experience.md#x46).

### Add malformed frontmatter or ambiguous successors

Follow safe effects first, then the precise authoring edit or guided choice, then Doctor. Do not reset the workspace between commands. See [X44](../scenarios/experience.md#x44) and [X39](../scenarios/experience.md#x39).

### No Git or already-indexed batch

Preserve exact manual relink intent for identities that can no longer be proven. Verify the remaining printed continuation still reaches clean Doctor. See [X41](../scenarios/experience.md#x41) and [X42](../scenarios/experience.md#x42).

Doctor and Repair agree on proof and protected sources. If Git times out, retry the complete request before editing a possibly renamed link. When Repair cannot ask, copy the chosen candidate command from its single row. All continuations keep an explicit workspace. Partial results state applied work and what remains. A proven canonical navigation rename is one row and preserves its rules.

## Final result

Files are discoverable and links resolve after all named choices or edits. Authored bodies and existing metadata survive. No guessed target is adopted.

## Verification

Observe the actual resulting state independently of the report. Record stdout, stderr, exit, changed paths, body and existing-field bytes, then run the printed next action. A snapshot alone does not prove preservation or a clean Doctor.
