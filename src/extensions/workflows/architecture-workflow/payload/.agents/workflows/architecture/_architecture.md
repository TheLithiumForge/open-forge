---
open-forge:
  description: Define or review a system's structure, tradeoffs, and adoptable transition
  tags: [Extension, Workflow, PhaseDefinition, Architecture, Design]
---

# Architecture

Architecture establishes a structural baseline and the supporting routed material future work can actually follow.

## Mode

iterative

## Goal

- outcome: a structural direction with explicit tradeoffs, a reviewed supporting-route bundle, and adoptable transition work
- helpful before: an accepted vision when it would materially improve the direction
- acceptance: the user accepts a direction and its warranted normative artifacts, asks for candidate analysis only, or defers the decision explicitly
- stop: direction accepted, analysis delivered, or decision deferred

## Required Routes

Read every route below before Step 1. A route that cannot be read is a blocker to report, not a step to skip.

- [architecture mapping, option comparison, decision framing, and migration slicing skill](../../skills/architecture/SKILL.md) - #Skill #Architecture

## Constraints

- Do not propose architecture before mapping the current structure and accepted constraints.
- Prefer incremental designs that can be adopted, verified, and reviewed in clear slices.
- Keep rationale, behavior, reusable structures, and migration work in their owning routes.
- Label each deduced constraint or practice as user-stated, observed, or inferred; never present inference as accepted truth.
- Obtain user accord before creating or promoting directives, patterns, guidance, workspace truth, decisions, or other normative #Core/#CurrentTruth material.
- Ask only questions that materially change the direction; otherwise state assumptions and continue.
- Generate only warranted supporting material; an explicit "none warranted" is better than filler or speculative enterprise structure.

## Steps

1. Classify the starting state as greenfield, unstructured or rescue, or established; confirm the decision owner, mutation authority, and whether the user wants analysis, routed artifacts, or both.
2. Map existing components, boundaries, ownership, dependency direction, information and control flows, integration seams, runtime and deployment shape, failure boundaries, constraints, and decisions. For greenfield, map the intended first vertical slice and state explicitly that no current architecture exists.
3. Elicit the quality attributes and stage constraints that should shape the design: users, expected scale, data ownership, security, reliability, observability, deployment, cost, evolution, team capability, and likely second vertical slice. Use scale triggers instead of speculative enterprise machinery.
4. Define success criteria, compare viable structural options, and state rejected options when they matter.
5. Select or propose boundaries, component responsibilities, ownership, dependency direction, flows, integration seams, runtime or deployment shape, and failure containment with explicit tradeoffs.
6. Derive a candidate support bundle and state why each item is or is not warranted: directives for mandatory invariants; patterns for repeatable inspectable structure; guidance for contextual tradeoffs and user-recognized practices; workspace routes for important locations or sources of truth; crystallized decisions for accepted rationale; and verification, observability, security, scaling, or operational material needed to sustain the direction.
7. Mark every candidate as user-stated, observed, or inferred, with confidence and the evidence or user preference behind it. Challenge implicit deductions with the user before treating them as normative.
8. Present the proposed route destinations and obtain user accord. On accord, create or update the minimum non-duplicative routed artifacts and run `open-forge index` plus `open-forge doctor` when the CLI is available. Without accord, keep useful candidates in emerging memory when that write is authorized, or report the candidate bundle and proposed destinations without promotion.
9. Re-read the resulting directive, pattern, guidance, workspace, and memory chain to confirm the architecture is discoverable and internally consistent, especially for a cold greenfield implementation session.
10. Slice the direction into implementation, verification, transition, cleanup, rollback, and later scale-trigger work.

## Loop

Repeat steps 2 through 7 when new constraints, source findings, risks, rejected options, or user decisions change the architecture. Repeat steps 8 and 9 after artifact revisions. Stop per the Goal's stop condition.

## Outputs

- starting-state classification and current- or intended-system map
- architectural goal and constraints
- compared options and tradeoffs
- selected or proposed direction, with rejected options when useful
- candidate support bundle with provenance, confidence, accord status, and explicit none-warranted results
- accepted directives, patterns, guidance, workspace routes, decisions, and operational support material, or emerging-memory/proposed-route fallback
- transition and verification slices

## Completion

- [ ] direction accepted, analysis delivered, or decision explicitly deferred
- [ ] greenfield work defines a minimal baseline for the first vertical slice and likely second slice without speculative scale machinery
- [ ] every supporting candidate states provenance and was promoted only with user accord
- [ ] warranted accepted directives, patterns, guidance, workspace truth, decisions, and support material were routed and rechecked, useful unaccepted candidates stayed emerging, or no material was warranted
- [ ] handoff written when continuation would benefit from a static resume note
- [ ] final response or handoff names this workflow

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
