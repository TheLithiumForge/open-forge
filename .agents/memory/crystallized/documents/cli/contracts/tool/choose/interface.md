---
open-forge:
  description: Define one-provider role selection, explicit clearing and required active guidance
  tags: [Memory, Document, CLI, Contract, Tool, Choose, Interface, CurrentTruth]
---

# tool choose interface
`tool choose` is a write command that assigns one installed compatible provider to a workspace role or clears that role.

A switch replaces recognized role members and required guidance in one checked plan. It keeps both installs and their separate indexes. Installation order, language and scores never choose a provider.

Status: V1 core contract decided. Writer implementation requires recovery v2 confirmation.

## Syntax

Proposed:

```text
open-forge tool choose <role> <id|none> [--allow-session-daemon] [--allow-path <path>...] [--automatic] [--dry-run] [global flags]
```

Exactly two operands are required. `none` clears a role. A reserved role can't activate in V1. The help purpose is "Assign or clear one workspace tool role."

## Choice

`code-navigation` requires `code-graph`. `docs-search` and `code-to-docs-links` are reserved. At most one settings entry lists a given role. A provider can later serve several compatible roles with one managed MCP member while any active role needs it.

The chosen pin must be installed, compatible and verified. Missing process consent, exclusions, conflicting daemons, unmanaged duplicate entries or changed required guidance block. No fallback occurs. A safely missing Skill can be restored only after every covering exclusion is cleared.

Publish launchable host members only after required guidance verifies. A switch moves the role between settings entries and updates the lock's generated member values. Clearing removes the role's managed members and Skill only. Tools with no active roles keep `roles:[]` and no managed launch member or usage Skill.

The shared [tool operations](../../../shared-operation-contract.md#tool-operations) define writer data and safety. `role` names the requested role. `pin` is null when clearing. Results report affected host restarts.

## Conformance

Required cases are:

- First choice, switch, same-provider no-op and `none` clear.
- Unknown, uninstalled, incompatible and reserved-role subjects.
- Duplicate role assignments and settings and lock mismatches.
- Required guidance verified before launchable members.
- Missing consent, changed members, unmanaged duplicates and covering exclusions.
- Both installs and index bytes kept during switching and clearing.
- Paired role publication and recovery after interruption.

The [behavior](behavior.md) defines switch ordering.
