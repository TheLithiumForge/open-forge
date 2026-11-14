---
open-forge:
  description: Define complete tool removal from stored receipts while keeping other registrations usable
  tags: [Memory, Document, CLI, Contract, Tool, Remove, Interface, CurrentTruth]
---

# tool remove interface
`tool remove` is a write command that runs one provider's declared cleanup and uninstall commands, then clears its roles and registration.

The final reference also releases recognized shared install and archive bytes. Other registrations stay usable. Removal needs neither its original catalogue nor an available code root.

Status: V1 core contract decided.

## Syntax

Proposed:

```text
open-forge tool remove <id> [--allow-path <path>...] [--automatic] [--dry-run] [global flags]
```

Source, pin, host, root, download and daemon consent flags aren't accepted. Removal requires affected sessions stopped. It doesn't start a daemon. It runs the pinned stored uninstaller without a shell. The help purpose is "Remove one registered tool and its recognized artifacts."

## Removal

Use stored definitions and verified receipts. Unsafe declared outputs, busy processes or unreadable references block complete removal. Recognized missing content can be released without restoring it. Persistent removal exclusions stay effective.

Run the stored index-cleanup and uninstall commands, then remove unused managed ignore rules before releasing the registration. Tool-owned commands handle agent files and state. Remove shared bytes only after a complete empty reference set is rechecked. Keep disappeared-worktree references until authorized cleanup resolves them.

Report zero selected leftovers only after every required check passes. Keep other registrations' kept artifacts and historical recovery separate from selected leftovers. A later failure reports partial removal. Repeating a verified completed removal is a no-op.

The shared [tool operations](../../../shared-operation-contract.md#tool-operations) define result data and recovery. `role` is null. The registered `pin` is present when applicable. Root path Remove has a narrower target and doesn't compose this external uninstall.

## Conformance

Required cases are:

- Tool and private Node versions released only after the last workspace reference.

- Removal without a source catalogue or existing code root.
- Nonfinal and final shared-reference removal.
- Missing tool output after root path Remove without restoring excluded content.
- Links, busy processes, excluded command outputs and unavailable references.
- Git recovery policy and non-Git sibling backups for managed files.
- Unrelated authored files and other worktrees kept usable.
- Repeated verified removal without new effects.

The [behavior](behavior.md) defines removal ordering.
