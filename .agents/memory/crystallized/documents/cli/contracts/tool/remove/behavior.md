---
open-forge:
  description: Define Tool remove effects and the uninstall question
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool remove behavior

Tool remove behavior is the flow behind `tool remove`.
For example, removing the last pattern from a `codegraph.json` that held nothing else deletes the file.

Status: Lean design decided on 17 November 2026.

## Flow

1. Read the settings, detect the tool and read its integration files. Stop on a conflict.
2. Remove each of Open Forge's integration changes that is present, then remove the ID from the settings `tools` list.
3. If the tool is installed, ask whether to run its own uninstall commands. Run them only on a yes.

Open Forge's changes are self-identifying, so Remove needs no record of what Install did.
A change that isn't present is skipped.
When nothing is present and the tool isn't installed, Remove reports that there was nothing to remove.

## Results

Stop at the first failed change or command.
A failed uninstall command shows its exit code, the tail of its output and a plain next step. Open Forge's completed changes stay done.
`--dry-run` changes nothing, asks nothing and shows the uninstall commands as not run.
Use the common status precedence and shared schema-3 exits.

## Conformance

Required cases cover:

- Removal of each integration change and the settings entry, including deleting a file left empty.
- The uninstall question answered no and yes, `--yes` and no terminal.
- A tool that isn't installed, and nothing to remove.
- A conflict that leaves the file unchanged.
