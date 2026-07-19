---
open-forge:
  description: Accepted product and technical direction for a directly implementable local handoff-note CLI slice
  tags: [Memory, Decision, CurrentTruth, Product, Architecture, Delivery]
---

# Accepted Note CLI Slice

- Problem: two support shifts using one workstation lose unresolved context between shifts.
- First slice: `note add <text>` records an unresolved note; `note list` shows unresolved notes newest first.
- Interface: a TypeScript CLI executed with Bun.
- Storage: one inspectable local JSON Lines file behind a thin filesystem adapter; no network, database, service, accounts, syncing, notifications, or integrations.
- Domain: parsing, note creation, ordering, and serialization stay pure; filesystem access stays at the edge.
- Failure behavior: malformed stored input produces a clear error and never rewrites the file.
- Acceptance: command-level tests use an OS temporary directory and invoke the real CLI entrypoint; the two commands work and malformed input is preserved.

These decisions are accepted. Implementation may record a bounded assumption when a small code-level detail is absent and must surface contradictory evidence before changing accepted direction.
