---
open-forge:
  description: Users receive src/open-forge as the payload; docs/framework governs maintainers and is never hidden runtime context
  tags: [Memory, Decision, CurrentTruth, Packaging, Governance]
---

# Source And Packaging

Accepted decisions extracted from the design sessions and idea notes on 2026-07-06.

- Users receive the implementation payload under `src/open-forge/`.
- `docs/framework/` governs maintainers and AI working on this repository.
- `docs/framework/` content must not be required hidden runtime context for installed users.
- Framework files should be concise, explicit, and easy to diff.
- Users may edit framework files, but durable customization should prefer local sibling files, child routes, or `.overwrite.md` companions.
