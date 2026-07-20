---
open-forge:
  description: Historical record of applying the 2026-07-19 human review notes through a minimal route-native Open Forge dogfood pass
  tags: [Memory, Archived, Session, Contextual, Historical, Framework, Dogfood, Review]
---

# Review-note framework improvements

- Goal: dogfood Open Forge, interpret all human/HN notes in the two review commits as one request, and improve the framework without reviving rejected ceremony or hidden mechanics.
- Accepted direction: use whole routed files and remove generic shared-file augmentation; express useful earlier work as an optional lowercase `- helpful before: ...` Goal item whose behavior is owned once by the workflows route; keep skills as ordinary `SKILL.md` capabilities; require direct directives to carry #LoadNow so root files load workspace-wide and child routes establish narrower scope first; organize first-party extension sources under `skills/`, `workflows/`, `packs/`, and `support/` while keeping manifest string ids independent of location; keep the CLI optional because installed payload files and manual Entries updates are complete runtime truth.
- Implementation: removed augmentation and `.ext.md` composition from the CLI, receipts, scaffolding, docs, tests, and live contracts; retained only user-owned `.overwrite.md`; added generic `load` traversal; made bundled discovery recursive and identity manifest-driven; grouped catalogue presentation by organizational source folder; simplified skills and their on-demand references; renamed the workflow-first scenario around helpful prior Architecture work.
- Evidence: review commits `a433dff22d7d76532b6e2fa1f18a26144e510312` and `470017db9249de85269df9e60a20db26bd98907e`; all 18 first-party extensions and 52 payload targets use whole files; source and dogfood authored contracts match; `load --paths`, `load --bodies`, and representative `chain` calls followed the intended top-down routes; both doctors were clean; all 18 extensions listed under Skills, Workflows, Packs, and Support; build passed; focused affected checks passed 58 tests and 584 assertions; final `test:ci` passed 160 tests and 1,114 assertions; diff checks passed.
- Compatibility: schema-1 receipts with empty augmentation arrays migrate to schema 2; non-empty legacy augmentation state fails with a clear instruction to remove or migrate it using an older CLI. Manual installs remain runtime-complete but do not receive optional receipt-backed dependency, update, reconciliation, or removal safety.
- Unresolved decision: whether Open Forge should encourage authored relative Markdown links or backlinks in addition to generated Entries, concrete paths, and tags. No framework-wide rule was added because the review note raised alternatives without selecting one.
