---
open-forge:
  description: Current open priorities for completing, validating, dogfooding, and preparing Open Forge for review and release
  tags: [Memory, Working, Backlog, Contextual]
---

# Backlog

Only open work belongs here. The mixed pre-review history is preserved in `.agents/memory/archived/planning/2026-07-18_pre-review-backlog.md`; product possibilities that are not actionable commitments stay in `.agents/memory/emerging/ideas/deferred-product-ideas.md`.

## Complete The Current Alpha Pass

1. Have the maintainer review and accept or revise the completed review-note alignment diff before committing it.
2. After maintainer feedback, extract only recurring project-specific patterns, guidance, workflows, or extensions that earn their context cost.

## Prove The S++ Claims

1. Run the current workflow-first scenarios on the P0 harness across multiple models or runtimes, with repeated trials and independent reproduction before promoting behavioral or portability claims.
2. Close the evidence gaps in `.agents/memory/emerging/observations/2026-07-12_benchmark-validity-gaps.md`: provenance, isolation, collision, replication, and synthesis consistency.
3. Evaluate `reliability-defaults`, the mixed development workflow, and the route-only `rune-bridge` in real use; preserve Open Forge's independence and add no stronger integration contract without evidence.
4. Add CI for payload/descriptor alignment and structural validation; decide whether a bounded benchmark gate is reliable and economical enough for payload-changing pull requests.

## Extend Extension And CLI Lifecycle

1. Extend the implemented local receipt-backed install/update/removal lifecycle only where evidence warrants: source trust and provenance, compatibility/version solving, migrations, crash recovery, aliases, an explicit previewable orphan-prune policy, remote provenance, and conflicts across multiple package managers. Treat optional build-time vendoring as a distribution choice, not runtime truth.
2. Improve dry-run visibility with generated-index diffs and counts while preserving the existing portable-path, topology, indirection, and rollback guarantees.
3. Consider a deterministic active-context receipt for auditability: selected route chains, overwrite companions, Required Routes, global binding #KeepInMind context, cost, and digest. It must not narrow the baseline #KeepInMind contract or create parallel runtime truth.
4. Decide the remaining deterministic CLI options: `find --max-tokens`, approximate token counts, startup-budget warnings, intent-to-route previews, named route templates, extension authoring templates, and explicit forceful versus soft upgrade modes.
5. Revisit a distinct primitive-kind field only if ownership ambiguity recurs beyond the current route-aware validator and tests.

## Self-Growth, Documentation, And Release

1. Ship the recurrence-driven observations rework in `.agents/memory/emerging/ideas/observations-rework.md` and validate it with a seeded promotion round.
2. Audit on-demand descriptions and complete the human documentation architecture: README responsibilities, concise guides, primitive and layer glossaries, routing/scoping examples, one-project and multi-project examples, distributed-memory boundaries, and stable terminology.
3. Rewrite onboarding after the current dogfood pass, then run a final consistency and security review across routing, generated regions, prompt-injection boundaries, extension lifecycle behavior, packaging, and tests.
4. Before public release, decide the product-name collision response and whether Git author metadata needs rewriting; file contents are already scrubbed of personal identifiers.
