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

`code-navigation` requires `code-graph`. `docs-search` and `code-to-docs-links` are reserved. At most one settings entry lists a given role.

The chosen pin must be installed, compatible and verified. Missing required process consent or incomplete install evidence blocks selection. No fallback occurs. Choice never restores excluded tool output.

A switch replaces the recognized role integrations and required Skill in one checked plan. Use the stored providers' declared deactivation and activation commands for tool-owned entries. Apply the target's declared command rewrites after activation. Keep both installs and their indexes. `none` removes the role's integrations, Skill and conditional line without uninstalling its stored bytes or cleaning its index. Tools with no active roles keep `roles:[]`.

The shared [tool operations](../../../shared-operation-contract.md#tool-operations) define writer data and safety. `role` names the requested role. `pin` is null when clearing. Results distinguish verified integration changes from the published role mapping.

## Conformance

Required cases are:

- First choice, switch, same-provider no-op and `none` clear.
- Unknown, uninstalled, incompatible and reserved-role subjects.
- Duplicate role assignments and settings and lock mismatches.
- A test-only second provider switches recognized role integrations and required guidance.
- Declared deactivation before activation, command rewrites and conditional role lines.
- Missing process consent and incomplete installation evidence.
- Both installs and index bytes kept during switching and clearing.
- Paired role publication and recovery after interruption.

The [behavior](behavior.md) defines switch ordering.
