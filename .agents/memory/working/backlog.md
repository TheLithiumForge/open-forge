---
open-forge:
  description: Current open priorities for completing, validating, dogfooding, and preparing Open Forge for review and release
  tags: [Memory, Working, Backlog, Contextual]
---

# Backlog

Only open work belongs here. The mixed pre-review history is preserved in `.agents/memory/archived/planning/2026-07-18_pre-review-backlog.md`; product possibilities that are not actionable commitments stay in `.agents/memory/emerging/ideas/deferred-product-ideas.md`.

## Complete The Current Migration

The file-by-file source migration, Decision consolidation, and remaining-document review are complete. Finish the migration in this order:

1. Have the maintainer review the complete Core primitive optimization and alignment of the current `route` structure now in the working tree
2. Optimize the complete Memory system across its model, states, scopes, installed runtime `entrypoints`, Maintenance contracts, decisions, links, terminology, and validation
3. Run one final system-wide alignment across routing, authority, locality, duplication, writing, public documentation, CLI behavior, source and dogfood synchronization, and misplaced files
4. Resolve or deliberately retain every temporary migration helper, report, and Directive, then declare the migration complete

Migration is complete only when the final alignment has no unresolved current-source gap, every deterministic check passes, and the maintainer has accepted the final gate.

Complete the [temporary independent review](../../directives/temporary-independent-review.md) gate before presenting each migration result for maintainer review.

During both optimization passes, apply the repository-only [terminology helper](../crystallized/documents/maintenance/helpers/terminology.md) consistently. Prefer direct statements of relationships and use `user` only when naming that role improves clarity.

Do not let a later phase pull detailed work forward merely because one concept links to it. Temporary migration references may remain until their scheduled phase as long as current authoritative sources do not depend on them.

## Review Contract

- Keep one task active and one coherent diff under review at a time.
- Before changing behavior, identify the authoritative current document or supporting decision and update it with the implementation so written truth and runtime behavior do not diverge.
- Update dogfood, installable source, applicable current documents, Maintenance contracts, user documentation, and tests together whenever they share the changed contract.
- At each gate, present the behavior delta, important file changes, verification evidence, remaining uncertainty, and any destructive catalogue changes.
- Stop after each gate. Do not start the next task until the maintainer accepts or revises the current result.

## Prove The S++ Claims

1. Run the exact dual-review meta-scenarios across multiple models or runtimes, and compare stable controls with declared trap or on-demand treatments. Compare what each worker believes it did with the orchestrator's trace-backed behavior and outcome review before promoting behavioral or portability claims.
2. Add stronger provenance, isolation, replication, or corpus controls only around a concrete claim that needs them; do not restore the retired P0 evidence platform as the default dogfood path.
3. Evaluate `reliability-defaults`, the mixed development workflow, and the route-only `rune-bridge` in real use; preserve Open Forge's independence and add no stronger integration contract without evidence.
4. Add CI for payload structure, source and dogfood synchronization, and machine-checkable Maintenance relationships; decide whether a bounded benchmark gate is reliable and economical enough for payload-changing pull requests.

## Extend Extension And CLI Lifecycle

1. After the vision, architecture, and installable source contracts converge, redesign and recreate the current MVP CLI under the linked [CLI overhaul](../emerging/ideas/cli-overhaul.md), preserving proven behavioral and safety evidence without treating the current implementation shape as final.
2. Extend the implemented local receipt-backed install/update/removal lifecycle only where evidence warrants: source trust and provenance, compatibility/version solving, migrations, crash recovery, aliases, an explicit previewable orphan-prune policy, remote provenance, and conflicts across multiple package managers. Treat optional build-time vendoring as a distribution choice, not runtime truth.
3. Improve dry-run visibility with generated-index diffs and counts while preserving the existing portable-path, topology, indirection, and rollback guarantees.
4. Consider a deterministic active-context receipt for auditability: selected route chains, overwrite companions, Required Routes, global binding #KeepInMind context, cost, and digest. It must not narrow the baseline #KeepInMind contract or create parallel runtime truth.
5. Decide the remaining deterministic CLI options: `find --max-tokens`, approximate token counts, startup-budget warnings, `intent-to-route` previews, `route` templates, extension authoring templates, and explicit forceful versus soft upgrade modes.
6. Revisit a distinct primitive-kind field only if ownership ambiguity recurs beyond the current route-aware validator and tests.

## Revisit Workflow Architecture

1. After source migration and the complete Core primitive optimization pass, redesign the current Workflow model using the preserved [Workflow overhaul inputs](../emerging/ideas/workflow-overhaul.md).
2. Provide an installed, on-demand, human-readable authoring surface for creating validator-compliant Workflows without requiring repository-only architecture or Maintenance documents.
3. Reconsider recipe shape, modes, phases, dependencies, delegation handoffs, `route` composition through scopes, runtime wording, Templates, CLI assistance, and validation as one coherent design.

## Self-Growth, Documentation, And Release

1. Ship the recurrence-driven observations rework in `.agents/memory/emerging/ideas/observations-rework.md` and validate it with a seeded promotion round.
2. Audit on-demand descriptions and complete the human documentation architecture: README responsibilities, concise guides, primitive and layer glossaries, routing/scoping examples, one-project and multi-project examples, distributed-memory boundaries, and stable terminology.
3. Rewrite onboarding after the current dogfood pass, then run a final consistency and security review across routing, generated regions, prompt-injection boundaries, extension lifecycle behavior, packaging, and tests.
4. Before public release, decide the product-name collision response and whether Git author metadata needs rewriting; file contents are already scrubbed of personal identifiers.
