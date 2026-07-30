---
open-forge:
  description: Keep canonical root instructions and harness bridges safely replaceable inside one workspace-owned file
  tags: [Pattern, Framework, Entry, Bridge, Installation, Safety]
---

# Managed Root Entry

## Shape

A canonical source template contains exactly one complete ordered managed block and only that block:

```md
<!-- {marker}:start -->
managed content
<!-- {marker}:end -->
```

A workspace target has one of two valid shapes:

1. no matching markers, so installation appends the source block;
2. exactly one complete ordered marker pair, so installation replaces that block.

Text outside the managed block belongs to the workspace and remains byte-for-byte unchanged.

### Entry Roles

- A canonical entry is authoritative for the Framework handoff carried by that file.
- A harness bridge contains only the harness-native reference to the canonical entry.
- Each harness-specific maintenance contract is authoritative for its external syntax and compatibility requirements.

### Installation

- Validate every canonical source template and existing target before applying the installation plan.
- Treat incomplete, reversed, or duplicate marker topology as an invalid target and stop before writing.
- Keep source and dogfood blocks identical.
- Reapplying the same source block produces no change.

## Review Checks

- One canonical policy source serves every bridge.
- Source templates contain one managed block and no surrounding authored content.
- Valid workspace content outside the block survives creation, replacement, and reinstallation.
- Invalid marker topology produces no partial output.
- File-specific maintenance documents link to this pattern, their source, implementation, external contract when present, and verification.
