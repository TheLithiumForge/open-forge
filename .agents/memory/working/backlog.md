---
open-forge:
  description: Current open priorities for completing, validating, dogfooding, and preparing Open Forge for review and release
  tags: [Memory, Working, Backlog, Contextual]
---

# Backlog

Only open work belongs here. The mixed pre-review history is preserved in `.agents/memory/archived/planning/2026-07-18_pre-review-backlog.md`; product possibilities that are not actionable commitments stay in `.agents/memory/emerging/ideas/deferred-product-ideas.md`.

## Framework Simplification Refactor

Status: candidate roadmap prepared from the maintainer's 2026-07-23 handoff, raw design discussion, and cross-project usage observations. It is contextual working state, not accepted #CurrentTruth. Do not begin a later task until the maintainer accepts or revises the preceding task's review gate.

### Review Contract

- Keep one task active and one coherent diff under review at a time.
- Before changing behavior, identify the owning current document or decision and update it with the implementation so written truth and runtime behavior do not diverge.
- Update dogfood, installable source, maintainer descriptors, user documentation, and tests together whenever they share the changed contract.
- At each gate, present the behavior delta, important file changes, verification evidence, remaining uncertainty, and any destructive catalogue changes.
- Stop after each gate. Do not start the next task until the maintainer accepts or revises the current result.
- Preserve the maintainer's unrelated `.gitmodules` change.

### P0 - Establish The Core Contract

The maintainer's completed cross-project trials are the behavioral baseline. Convert each reported failure into a focused regression criterion in the task that changes the relevant behavior rather than creating a separate baseline project.

1. **Strengthen the Open Forge entry contract and add the Claude bridge.**
   - Status: accepted and committed by the maintainer on 2026-07-23 as `0c9a373`.
   - Make `AGENTS.md` establish Open Forge as the workspace operating contract within its legitimate authority.
   - Add the minimal `CLAUDE.md` bridge to canonical `AGENTS.md`, preserving workspace-owned content and avoiding duplicated independent instructions.
   - Keep the bridge design small enough to support another harness later without adding a provider-specific Core policy.
   - Gate: dogfood, installable source, descriptors, install/update behavior, build output, and fresh-install tests agree; existing `AGENTS.md` and `CLAUDE.md` content outside managed boundaries is preserved.

2. **Add the generic truth lifecycle and dogfood it.**
   - Status: revised again on 2026-07-24 after maintainer review and awaiting rereview.
   - Define #Evergreen separately from #CurrentTruth.
   - Keep acceptance and #Evergreen synchronization behavior exclusively in the loader's defined-tag and authority axioms instead of adding a seeded directive or document-specific rule.
   - Let Memory record any subject without activating described behavior; put accepted behavior that should guide future work in its matching #Core route.
   - Keep state-specific preservation, promotion, consolidation, archival, and restoration invariants beside the Memory states they govern.
   - Use plain acceptance language, preserve ambiguous candidates as #Contextual, and remove redundant references to where defined tags were defined.
   - Dogfood the model with routed #CurrentTruth #Evergreen product and architecture views in crystallized documents, a synchronized project-ownership map, and no redundant #SourceOfTruth tag.
   - Gate: clear direction needs no redundant confirmation; an action request accepts decisions required to perform it; ambiguous direction remains contextual; only affected editable #Evergreen material changes; Memory may describe behavior without activating it; no generic directive or document-specific Evergreen rule is seeded; index, doctor, tests, and targeted behavioral scenarios pass.

3. **Establish the current-document and knowledge-linking architecture.**
   - Status: active file-by-file migration; the `src/open-forge/AGENTS.md` gate was accepted and committed as `0ff4725`, and the `src/open-forge/CLAUDE.md` gate is implemented and awaiting maintainer review.
   - Review installable source top-down, one file per gate. For each file, inspect its recent history, surrounding contracts, references, consumers, tests, and proposed extraction or destination before editing.
   - Decide the smallest useful crystallized-document taxonomy with the maintainer, including whether maintainer-facing current material belongs under a `maintenance/` child route or a clearer equivalent.
   - Move appropriate current maintainer documents into that routed Memory owner and mark each independently as #CurrentTruth and #Evergreen only when both semantics apply.
   - Define how ordinary relative Markdown links expose relationships with the same containing-file-relative semantics as generated `Entries`.
   - Encourage links between owning files instead of restating their content, then refactor Open Forge's own current documents to demonstrate that graph and remove duplication.
   - Decide which small set of high-quality current views is necessary beyond vision and architecture, without creating documents merely to fill a taxonomy.
   - Clarify the boundary between current documents, decisions, governance descriptors, README material, code-owned truth, and historical Memory.
   - Gate: an agent can locate and understand the project's important current state top-down without this chat or handover; links resolve; current owners are unambiguous; duplicated truth is reduced; the chosen documents are coherent and worth their maintenance cost.

4. **Reassess the full refactor handover with the maintainer and rebuild the roadmap.**
   - Review the handover point by point instead of treating its proposed implementation order as accepted.
   - For each proposal, record whether it is essential, useful but needs redesign, deferred, or rejected, with the maintainer deciding what actually matters.
   - Reconcile the handover with cross-project observations and the framework direction captured in current documents.
   - Replace the remaining roadmap only after that review; the former workflow, extension-consolidation, deliberation, and final-hardening tasks remain contextual candidates rather than current priorities.
   - Gate: the reviewed handover map and updated backlog contain only priorities the maintainer recognizes as important, in an order supported by the new current-document architecture.

The older open-work sections below predate this refactor review. They remain contextual inventory and do not outrank Tasks 2-4; Task 4 will reconcile, retain, move, or remove them.

## Complete The Current Alpha Pass

1. Have the maintainer review and accept or revise the completed review-note alignment diff before committing it.
2. After maintainer feedback, extract only recurring project-specific patterns, guidance, workflows, or extensions that earn their context cost.

## Prove The S++ Claims

1. Run the exact dual-review meta-scenarios across multiple models or runtimes, and compare stable controls with declared trap or on-demand treatments. Compare what each worker believes it did with the orchestrator's trace-backed behavior and outcome review before promoting behavioral or portability claims.
2. Add stronger provenance, isolation, replication, or corpus controls only around a concrete claim that needs them; do not restore the retired P0 evidence platform as the default dogfood path.
3. Evaluate `reliability-defaults`, the mixed development workflow, and the route-only `rune-bridge` in real use; preserve Open Forge's independence and add no stronger integration contract without evidence.
4. Add CI for payload/descriptor alignment and structural validation; decide whether a bounded benchmark gate is reliable and economical enough for payload-changing pull requests.

## Extend Extension And CLI Lifecycle

1. After the vision, architecture, and installable source contracts converge, redesign and recreate the current MVP CLI under the linked [CLI overhaul](../emerging/ideas/cli-overhaul.md), preserving proven behavioral and safety evidence without treating the current implementation shape as final.
2. Extend the implemented local receipt-backed install/update/removal lifecycle only where evidence warrants: source trust and provenance, compatibility/version solving, migrations, crash recovery, aliases, an explicit previewable orphan-prune policy, remote provenance, and conflicts across multiple package managers. Treat optional build-time vendoring as a distribution choice, not runtime truth.
3. Improve dry-run visibility with generated-index diffs and counts while preserving the existing portable-path, topology, indirection, and rollback guarantees.
4. Consider a deterministic active-context receipt for auditability: selected route chains, overwrite companions, Required Routes, global binding #KeepInMind context, cost, and digest. It must not narrow the baseline #KeepInMind contract or create parallel runtime truth.
5. Decide the remaining deterministic CLI options: `find --max-tokens`, approximate token counts, startup-budget warnings, intent-to-route previews, named route templates, extension authoring templates, and explicit forceful versus soft upgrade modes.
6. Revisit a distinct primitive-kind field only if ownership ambiguity recurs beyond the current route-aware validator and tests.

## Self-Growth, Documentation, And Release

1. Ship the recurrence-driven observations rework in `.agents/memory/emerging/ideas/observations-rework.md` and validate it with a seeded promotion round.
2. Audit on-demand descriptions and complete the human documentation architecture: README responsibilities, concise guides, primitive and layer glossaries, routing/scoping examples, one-project and multi-project examples, distributed-memory boundaries, and stable terminology.
3. Rewrite onboarding after the current dogfood pass, then run a final consistency and security review across routing, generated regions, prompt-injection boundaries, extension lifecycle behavior, packaging, and tests.
4. Before public release, decide the product-name collision response and whether Git author metadata needs rewriting; file contents are already scrubbed of personal identifiers.
