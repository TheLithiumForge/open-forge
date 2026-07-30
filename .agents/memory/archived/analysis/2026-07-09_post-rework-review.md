---
open-forge:
  description: Historical post-rework review of framework ratings, deduplication, tag changes, risks, and improvement candidates
  tags: [Memory, Archived, Analysis, Reasoning, Contextual, Historical, Framework, Review]
---

# Post-Rework Review

Status: archived 2026-07-18.  
Original route: `.agents/memory/emerging/analysis/2026-07-09_post-rework-review.md`.  
Archived because: later implementation and benchmark evidence superseded its ratings and proposed next steps.  
Current owner or replacement: current source and docs, accepted evaluation syntheses, crystallized decisions, and `.agents/memory/emerging/ideas/rating-ladder.md`.

Date: 2026-07-09. Question: after the dogfood migration, the tag rework, and the per-file deduplication pass, how healthy is the framework and what should improve next? Assumptions: dogfood v2-v4 evidence still holds; none of today's new semantics has been dogfooded yet.

## What Changed Today

- The repository installed Open Forge into itself and migrated all temporary notes into routed memory.
- Load-policy tags collapsed from three (#OpenForge, #LoadWithParentEntrypoint, #LoadForPostWorkReview) to two (#LoadNow, #KeepInMind) with agent-imperative wording and load-early timing.
- Universal axioms were hoisted into the loader once (ancestor inheritance, narrower-scope preference, child-category growth) and 216 duplicated axiom lines were removed across both payload trees; category entrypoints now hold only scope-specific axioms (11-17 authored lines each; loader at 65).
- The workflow-essentials extension applied the routing-surfaces decision: selection prose moved into decision-grade descriptions, reference Use When sections merged up into `SKILL.md` trigger lines, and the mistaken #Core layer tag was removed from extension workflows.
- Bug fixes: dead `docs/framework/_framework.md` reference and stale npm package list in `docs/dev.md`.

## Ratings

- Core routing model: excellent; validated by dogfooding and now by self-install. The entrypoint/entries mechanics survived a real migration without a single routing dead end.
- Load policy: good, pending validation. Two self-describing tags whose timing matches the evidence; the honest self-enforcement wording is in place. Unproven until a dogfood round runs against it.
- Memory taxonomy: good. States are clean, the duplication tax is gone, and the archive migration exercised child-category growth exactly as designed.
- Workflows: fair. Selection surfaces are fixed; the Required Routes body redesign is still a candidate idea, and required-skill loading reliability remains the known weak point.
- Extensions: fair. Mechanics work; skill sharing between extensions remains the real unsolved gap.
- Governance docs: good structure, still heavy. Contains and Alignment Checks restate each other roughly 1:1 in most descriptors.
- CLI and tests: good. 22 tests track installed wording closely, which caught every behavioral edit in this pass.

## Remaining Improvement Candidates

1. Ship `open-forge dump` (bulk context loading); still the highest-leverage build, unchanged by today's work.
2. Decide the workflow Required Routes redesign (`.agents/memory/archived/ideas/workflow-redesign.md`) and apply it to workflow-essentials.
3. Deduplicate descriptors: make Alignment Checks the single normative list. Done 2026-07-09; roughly 300 lines removed losslessly, unique mandates moved into checks.
4. Drop the #Index tag from authored metadata. Done 2026-07-09; the CLI keeps #Index only as the fallback for metadata-less local categories, where it carries real information.
5. Consider moving the loader's Route Patterns section and the scoping terms (`scope route`, `scoped framework route`, `slug`) into a routed concept file with a one-line loader pointer; they are always loaded but matter only when creating scope routes. Needs a loader-descriptor change.
6. Rework observations into the recurrence-driven mechanism (`.agents/memory/emerging/ideas/observations-rework.md`).
7. Run dogfood v5 against today's semantics before any release: #LoadNow chain loading, #KeepInMind end-of-work compliance, and whether the ancestor-inheritance axiom holds in practice when agents open deep routes.

## Risks

- #KeepInMind still depends on the agent remembering the end-of-work recheck; the early load improves the odds per v4 evidence but does not close the gap.
- The ancestor-inheritance axiom assumes ancestors are in context; that holds for the #LoadNow chain, and the loader's load-entrypoint-before-routed-files rule covers relevance-routed paths, but a runtime that jumps straight to a deep file would miss inherited axioms.
- Descriptor minimum line budgets were lowered to match the deduped payload; if future edits shrink files further, re-check that each file still stands on its own per the payload boundary.
