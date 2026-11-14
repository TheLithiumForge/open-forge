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
Describe optional account uninstall until its separate live choice.
Before that choice, run no native uninstall preview and require no unchosen account output bindings.
Show the plan.
Codebase Memory's native uninstall removes its account-wide binary and all agent entries.
Only without `--yes` and with a live terminal, offer confirmation naming those account-wide effects.
If confirmed, prepare its concrete account paths, expected-state checks and backups, then run the native uninstall before Open Forge cleanup.
With `--yes`, without a terminal or after declining, skip that uninstall and print its command.
Then remove Open Forge's recorded project additions.
Keep account-wide native settings, say so and print the command to change them back.
Other projects can still rely on those settings.
Add no cross-project native-setting registry.
Never automatically run a package-manager uninstaller.
Ask before deleting Open Forge-placed tool files or runtimes, since other projects can use them.
Remove a runtime only when no launcher uses it.
Offer PATH removal after the last tool.
`--yes` never supplies live confirmation for PATH or shared deletion.
Use the [common Behavior](../behavior.md) for detection, channels, native setup and additive effects.
Keep syntax and reports in the [Interface](interface.md).

## Results

Use the common status precedence and shared schema-3 exits.
Keep actual effects, conflicts and unstarted work separate.
Intentional native-uninstall or account-setting retention doesn't by itself leave project removal incomplete.
Dry-run changes no Tool configuration or records.

## Conformance

Required cases cover:

- Chosen native-first ordering, named account-wide confirmation, `--yes` or no-terminal project-only removal and kept account settings.
- Union item removal, changed set/file retention, block deletion, shared runtime use and repeated removal.
- Stable ordering and unchanged unrelated data.
- Shared text and JSON facts and global flags.
