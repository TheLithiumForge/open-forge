---
open-forge:
  description: Define Tool remove checks and effects
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool remove behavior

Tool remove behavior is the flow that runs native uninstall and removes Open Forge additions.
CodeGraph removes its project setup through its own uninstaller, then Open Forge removes its recorded archive exclusion.

Status: Round-2 design decided on 14 November 2026. Command implementation is pending.

## Flow

Preflight the native uninstall and recorded additions.
Show the plan.
If the native uninstall needs PATH changes or shared deletion, obtain separate live confirmation before starting it.
Without that confirmation, print its command and leave removal pending.
Run the tool's own uninstall first, then remove only recorded additions and guarded native settings.
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
Dry-run changes no Tool configuration or records.

## Conformance

Required cases cover:

- Native-first ordering, account scope, union item removal, changed set/file retention, block deletion, --yes protected effects, shared runtime use and repeated removal.
- Stable ordering and unchanged unrelated data.
- Shared text and JSON facts and global flags.
