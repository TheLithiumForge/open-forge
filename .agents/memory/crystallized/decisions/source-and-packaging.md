---
open-forge:
  description: Users receive src/open-forge as the payload; crystallized maintenance documents govern reviewed source without becoming hidden runtime context
  tags: [Memory, Decision, CurrentTruth, Packaging, Governance]
---

# Source And Packaging

Accepted decisions extracted from the design sessions and idea notes on 2026-07-06.

- Users receive the implementation payload under `src/open-forge/`.
- `AGENTS.md` remains the canonical root instruction contract. Minimal harness bridges may import it without duplicating Open Forge policy; bridge installation preserves workspace-owned content outside the managed Open Forge block.
- [Crystallized maintenance documents](../documents/maintenance/_maintenance.md) govern reviewed source and repository surfaces for maintainers and AI working on this repository.
- During the file-by-file migration, each remaining `docs/framework/` descriptor governs its source until a crystallized maintenance document replaces it.
- Maintenance documents and remaining governance descriptors must not be required hidden runtime context for installed users.
- Every binding runtime contract described by governance must be expressed in the installable payload itself. Governance may explain or validate that contract, but cannot be the only place an installed agent could learn it.
- The repository's root `.agents/` tree dogfoods the installable payload and may add repository-specific routes. Any deliberate dogfood-only difference must remain visibly local; shared directive, routing, continuity, and workflow semantics stay aligned with `src/open-forge/`.
- Framework files should be concise, explicit, and easy to diff.
- Users may edit framework files, but durable customization should prefer local sibling files, child routes, or `.overwrite.md` companions.
