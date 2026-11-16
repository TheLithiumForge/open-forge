---
open-forge:
  description: Define Tool remove checks and effects
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool remove behavior

Tool remove behavior is the flow that runs native uninstall and removes Open Forge additions.
CodeGraph removes its project setup through its own uninstaller, then Open Forge removes its recorded archive exclusion.

Status: Round-2 design decided on 14 November 2026.

## Flow

Preflight required project uninstall and recorded additions.
Describe optional account uninstall until its separate live choice. It is never part of the plan.
Before that choice, run no native uninstall preview and require no unchosen account output bindings.
Show the plan.
CodeGraph's project-scoped native uninstall runs before Open Forge cleanup.
Codebase Memory removal is local: the declaration, lock entry, recorded project additions and, under the shared-file rules, Open Forge's own launchers and tool files.
After that local removal, and only without `--yes` and with a live terminal, say that Codebase Memory's own uninstaller removes it system-wide.
Name its binary, its entries in every agent app on the account and, on Windows, its directory on the user PATH, which is Open Forge's bin.
Ask whether to run it. The default answer, and an unanswered question, keeps it unrun.
If confirmed, prepare its concrete account paths, expected-state checks and backups, then run it before the shared-file choices.
With `--yes`, without a terminal or after declining, never run it and print its exact command.
Then remove Open Forge's recorded project additions for tools without that question.
A completed Remove deletes the tool's settings declaration and its lock entry.
Keep account-wide native settings, say so and print the command to change them back.
Other projects can still rely on those settings.
Add no cross-project native-setting registry.
Never automatically run a package-manager uninstaller.
Ask before deleting Open Forge-placed tool files or runtimes, since other projects can use them.
Remove a runtime only when no launcher uses it.
When a shared deletion isn't chosen, list the exact files to delete.
Remove never edits PATH.
When the removed tool was the last one with a launcher in Open Forge's bin, print the exact manual step to remove the bin entry.
Name the Windows user PATH entry, or the marked block in each shell file that PATH history or the marks name.
Count only launchers in bin. Package-manager installs don't count.
`--yes` and runs without a terminal print the same steps.
`--yes` never supplies live confirmation for shared deletion.
Use the [common Behavior](../behavior.md) for detection, channels, native setup and additive effects.
Keep syntax and reports in the [Interface](interface.md).

## Results

Use the common status precedence and shared schema-3 exits.
Keep actual effects, conflicts and unstarted work separate.
Intentional native-uninstall or account-setting retention doesn't by itself leave project removal incomplete.
Dry-run changes no Tool configuration or records.

## Conformance

Required cases cover:

- CodeGraph native-first ordering, Codebase Memory local-first removal with the named system-wide question afterwards, `--yes` or no-terminal local removal with the printed command and kept account settings.
- Union item removal, changed set/file retention, block deletion, shared runtime use and refused repeated removal.
- Unattended removal of the last placed tool with its exact manual deletion and PATH steps.
- PATH and its history stay unchanged through every removal, and the PATH steps print only after the last tool in bin.
- Stable ordering and unchanged unrelated data.
- Shared text and JSON facts and global flags.
