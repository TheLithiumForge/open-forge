---
open-forge:
  description: Keep canonical root instructions and harness bridges safely replaceable inside one workspace-owned file
  tags: [Pattern, Framework, Entry, Bridge, Installation, Safety]
---

# Managed Root Entry

## Shape

A canonical source template contains exactly one complete ordered managed block and only that block:

```md
# Open Forge

managed instructions or imports

**End of Open Forge managed section.**
```

A workspace target has one of two valid shapes:

1. no canonical or exact legacy boundary candidate, so installation appends the
   canonical source block;
2. exactly one complete canonical heading and footer pair or exact legacy
   comment pair, so installation replaces that span with the canonical block.

An incomplete, reversed, duplicate, mixed, or otherwise ambiguous boundary is
neither valid shape and blocks before writing. Text outside the managed block
belongs to the workspace and remains byte-for-byte unchanged. Boundaries are
recognized from root-level parsed Markdown blocks and typed source spans. A
heading or footer inside fenced code, a quote, a nested list, or an inline
example is content, not a delimiter. A heading without its named footer never
extends the managed span to the user suffix or end-of-file.

### Entry Roles

- A canonical entry is authoritative for the Framework handoff carried by that file.
- A harness bridge contains only harness-native references to the canonical Framework entries it exposes. It does not restate their policy.
- Each harness-specific maintenance contract is authoritative for its external syntax and compatibility requirements.

### Installation

- Validate every canonical source template and existing target before applying the installation plan.
- Treat incomplete, reversed, duplicate, mixed, or ambiguous boundary topology as an invalid target and stop before writing.
- Keep source and dogfood blocks identical.
- Reapplying the same source block produces no change.

## Review Checks

- One canonical policy source serves every bridge.
- Source templates contain one managed block and no surrounding authored content.
- Valid workspace content outside the block survives creation, replacement, and reinstallation.
- Invalid or ambiguous boundary topology produces no partial output.
- File-specific maintenance documents link to this pattern, their source, implementation, external contract when present, and verification.
