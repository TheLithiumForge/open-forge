---
open-forge:
  description: Redesign workflow bodies around Required Routes, constraints, and two-tier loading; selection stays in entry descriptions
  tags: [Memory, Idea, Contextual, Candidate, Workflow, Routing, Loading]
---

# Workflow Redesign: Required Routes

Direction agreed 2026-07-08, not implemented yet. Applies the loading-reliability and routing-surfaces decisions to the workflow primitive.

## Accepted Direction

- Remove selection prose such as "use this workflow when..." from workflow bodies; the generated `entry` description is the selection surface.
- The body opens with one goal statement so a mis-selected agent can confirm fit or back out.
- Replace `Required Skill Packages` with a generalized `Required Routes` section for cross-tree dependencies. Generated `Entries` express containment (siblings and children); `Required Routes` express dependency (cross-tree edges that generated entries cannot carry).
- State load timing inside the section, unconditionally and without implying tooling: "Read every route below before Step 1. A route that cannot be loaded is a blocker to report, not a step to skip."
- Keep `Required Routes` flat, small (about five entries), unconditional, and entrypoint-level only (`SKILL.md`, never `references/*`). Conditional depth belongs inside skill packages, where a missed read is cheap to recover from.
- Keep a short `Constraints` section for cross-step invariants that are neither steps nor outcomes, such as "design the fit before writing the implementation".
- Collapse end-state sections to `Outputs` (artifacts) plus `Completion` (a checklist that is the stop condition, including the memory-routing follow-up).
- Write `Required Routes` lines in the generated entry format (backtick path, dash, description) so future tooling such as `open-forge dump --follow-required` can parse and follow them.
- Do not list directives in `Required Routes`; they are already loaded workspace-wide. Keep the list semantically pure: material that is not otherwise loaded and that the workflow cannot run correctly without.

## Proposed Body Shape

Goal line, `Required Routes`, `Constraints`, `Steps`, `Loop`, `Outputs`, `Completion`, generated `Entries`.

## Loading Tiers

1. Session baseline: loader plus load-policy chain, unconditional and small.
2. Selection: `_workflows.md` entries scan, descriptions only.
3. Activation: workflow body plus all `Required Routes`, unconditional, before Step 1. Front-loaded deliberately because closeout compliance is the weakest tier and early exposure measurably improves it.
4. Execution: skill `references/*` loaded per step through each `SKILL.md` router.
5. Closeout: post-work routes plus the `Completion` checklist.

## Open Questions

- Whether `Required Routes` should name skill package ids and let the CLI convert them into generated entries; currently leaning to two distinct sections with concrete paths, keeping containment and dependency visibly different for humans too.
- Whether workflow entries need richer activation metadata, such as example user phrases ("activate when the user asks for help shaping a product vision").
- Whether the primitive should be renamed (for example "workloads"); unresolved, low priority.
- Whether workflows should own local overwrites for #Core routes such as directives and guidance, as originally intended, since no other mechanism provides scoped overwrites today.
- Contract updates needed on acceptance: `_workflows.md` and the agent-primitives and formatting descriptors currently hardcode `Required Skill Packages` wording.
