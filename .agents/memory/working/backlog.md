---
open-forge:
  description: Pending priorities and planned passes for Open Forge before and during formal release preparation
  tags: [Memory, Working, Backlog, Contextual]
---

# Backlog

Actionable priorities. Deferred designs and product ideas live in `.agents/memory/emerging/ideas/`.

## Alpha Sequence

1. Finish the alpha version.
2. Extract meaningful project-specific patterns, guidance, workflows, and extensions from the dogfood and benchmark evidence.

## Near-Term Priorities

1. Run the v11 sessions A/B per `benchmarks/harness/orchestrator/run-plan-gen11.md`: closeout-command plus sessions-keepinmind overlays on seeds 1-3 against gen10 as control; on success promote the sessions #KeepInMind tag and write axiom to core payload. Optional model comparison rides along. v10 verdict: recheck compliance 3/3 vs ~50% baseline, loader wording promoted 2026-07-11 (see v10-synthesis).
2. Exercise the route-only `rune-bridge` against a real public Rune integration when one is available; keep Open Forge independent and add no command, directive, or config contract without evidence.
3. Run v9 as a model comparison on the now-stable harness: same seeds, Fable and Sonnet against the GPT-5 baseline; the seed-0 curveball and closeout discipline are the discriminators. Also include one run with the `dev-workflow` extension (still unexercised). Commit before running so report provenance is clean.
4. Explore when time allows: `.agents/memory/emerging/ideas/workspace-usage.md`, the docs-versus-crystallized reconciliation and description-rot notes in `.agents/memory/emerging/analysis/2026-07-09_open-question-recommendations.md`, and the CLI design in `.agents/memory/emerging/ideas/cli-design.md`.
5. Review `.agents/memory/emerging/analysis/2026-07-09_open-question-recommendations.md`; promote accepted recommendations (workspace category, docs placement, loader route patterns, no native backlog route, description rubric) to `crystallized/decisions/`.
6. Audit on-demand route descriptions against the description rubric: trigger plus outcome, selectable from the one line alone; leave #LoadNow identity descriptions as they are.
7. Reread every installable payload file and matching governance descriptor for wording, scope separation, tag usage, and route accuracy; include the concepts-versus-descriptors dedup sweep.
8. Ship the observations rework (`.agents/memory/emerging/ideas/observations-rework.md`) and validate it with a seeded round where recurrence detection produces an accepted promotion.
9. Design the remaining extension lifecycle: compatibility/version solving, visible ownership receipts, crash recovery, update, and remove. Install-time dependency packs and collision-safe sharing are implemented; build-time vendoring remains optional future work.
10. Add CI that mechanically verifies descriptor alignment checks against installed payload files, and explore running the benchmark harness as CI on payload changes; see `.agents/memory/emerging/ideas/rating-ladder.md` for the full ladder.
11. Decide whether git commit history (author name and email) needs rewriting before the repository goes public; file contents are already scrubbed of personal identifiers.
12. Define the user-documentation architecture: README responsibilities, short guide files, primitive glossary, layer glossary, route/scoping examples, and voice.
13. Rewrite human onboarding after dogfooding, not before it.
14. Run consistency and security review across routing, generated regions, prompt-injection boundaries, update behavior, and tests.

## User Documentation To Write Later

- Explain recursive routing as the main scalability model.
- Explain `entrypoint`, `entry`, `framework route`, `scope route`, `scoped framework route`, `slug`, and child route.
- Explain #Core, #Memory, and #Extension.
- Explain directives, patterns, guidance, skills, workflows, and workspace routes in a short glossary.
- Show one-project, multi-project, monorepo, and shared-knowledge examples.
- Show the difference between `memory/crystallized/[scope]/documents/` and `memory/[scope]/crystallized/documents/`.
- Explain that folder slugs are concrete runtime paths while `[scope]` notation is only template/documentation notation.
- Explain that users can create external or distributed memory folders only when explicitly routed; no implicit filesystem search.

## Terminology Pass

After alpha dogfooding, define stable terms for:

- current truth
- accepted memory
- durable memory
- historical memory
- contextual memory
- transfer notes
- candidate learning
- organic growth

Use the vocabulary to clean route descriptions, loader tag meanings, user docs, and maintainer governors.

## Low-Priority Review

- Review old archived ideas only when reconstructing why a decision was made.

## Done Or Superseded

- Dogfood migration into this repository's own `.agents/` (2026-07-09).
- Tool-implying load wording replaced with agent-imperative wording (2026-07-09 pass plus the streamlining commit).
- Descriptor dedup with Alignment Checks as the single normative list (2026-07-09).
- #Index dropped from Open Forge-authored metadata (2026-07-09).
- Repeated memory axioms hoisted into the loader; ancestor axioms apply below (2026-07-09).
- Human-run dogfood smoke checklist superseded by the reproducible benchmark seeds under `benchmarks/`.
- Workflow shape accepted and applied; dev-workflow extension shipped (2026-07-10).
- v7 scope-control fixes applied to the harness: root pinning in worker templates, verify-cwd axiom in scope-control, first-attempt rubric scoring, index-regeneration directive (2026-07-10).
- worker-vision workflow added to the harness for seed-0 vision runs (2026-07-10).
- Evidence reorganized: crystallized `documents/evaluations/` holds syntheses only; per-run reports in `benchmarks/results/`, pre-harness reports in `benchmarks/results/pre-harness/` (2026-07-10).
- Dependency-safe extension planning, content-derived catalogue, required dependency selection, skill-only shared capabilities, separately selectable architecture/vision/brainstorming/implementation/testing/mixed-development workflows, convenience packs, optional reliability defaults, and the route-only Rune bridge implemented with real-pack integration coverage (2026-07-15).
