---
open-forge:
  description: Define one-provider role selection, explicit clearing and kept tool-owned output
  tags: [Memory, Document, CLI, Contract, Tool, Choose, Interface, CurrentTruth]
---

# tool choose interface
`tool choose` is a write command that assigns one installed compatible provider to a workspace role or clears that role.

A switch moves the role between settings entries in one checked plan. It keeps both installs and their separate indexes. Installation order, language and scores never choose a provider.

Status: V1 core contract decided.

## Syntax

Proposed:

```text
open-forge tool choose <role> <id|none> [--allow-session-daemon] [--allow-path <path>...] [--automatic] [--dry-run] [global flags]
```

Exactly two operands are required. `none` clears a role. A reserved role can't activate in V1. The help purpose is "Assign or clear one workspace tool role."

## Choice

`code-navigation` requires `code-graph`. `docs-search` and `code-to-docs-links` are reserved. At most one settings entry lists a given role. Role metadata doesn't control which entries a tool installer writes to an agent file.

The chosen pin must be installed, compatible and verified. Missing required process consent or incomplete install evidence blocks selection. No fallback occurs. Choice never restores excluded tool output.

A switch moves the role between settings entries and publishes matching lock evidence. Clearing removes that role assignment only. Tools with no active roles keep `roles:[]`. Their agent files, guidance and indexes stay under the tool's installer.

The shared [tool operations](../../../shared-operation-contract.md#tool-operations) define writer data and safety. `role` names the requested role. `pin` is null when clearing. Results report the role mapping without claiming changes to agent files.

## Conformance

Required cases are:

- First choice, switch, same-provider no-op and `none` clear.
- Unknown, uninstalled, incompatible and reserved-role subjects.
- Duplicate role assignments and settings and lock mismatches.
- Agent files and guidance unchanged without installer execution.
- Missing process consent and incomplete installation evidence.
- Both installs and index bytes kept during switching and clearing.
- Paired role publication and recovery after interruption.

The [behavior](behavior.md) defines switch ordering.
