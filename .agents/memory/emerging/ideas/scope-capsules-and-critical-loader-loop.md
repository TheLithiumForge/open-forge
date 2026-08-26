---
open-forge:
  description: Explore explicit routed-scope capsules for delegated agents and a concise critical loader loop for scope discovery, rescoping, and closeout
  tags: [Memory, Idea, Contextual, Candidate, Scope, Routing, Delegation, Loader, Agent, Closeout]
---

# Scope Capsules And A Critical Loader Loop

## Opportunity

Open Forge already routes authoritative instructions by intent, path, tags, and lifecycle. Coordinated work can still lose that context when an Overseer or Task Mastermind delegates a narrow implementation or review without naming the exact routes, directives, contracts, target paths, and protected paths that apply.

The same risk appears on resume and after a task materially changes scope. An agent may remember the implementation goal while overlooking that it must rediscover its routed context or run the closeout route. A concise high-priority loop near the loader entrypoint may make those transitions harder to miss without duplicating the detailed rules that remain authoritative in their own scopes.

## Current Understanding

The candidate is a small coordination discipline, not a new routing authority:

1. At task start or resume, the owning agent resolves every relevant route and scope through the loader and available CLI support.
2. Before delegation, it forms a task-specific scope capsule containing the exact entrypoints, routed directive chains, accepted contracts or decisions, target paths, protected paths, and evidence obligations the child needs.
3. The child verifies the capsule against inherited axioms and the applicable `#LoadNow`, `#KeepInMind`, and overwrite behavior instead of treating the packet as a substitute for routing.
4. When intent, target paths, lifecycle phase, or accepted meaning changes materially, the owner and affected children rescope before continuing.
5. At meaningful phase boundaries, the owner evaluates whether an independent child can own a genuinely separable discovery, evidence, implementation, or review slice. If runtime capacity prevents useful delegation, the owner records that fallback rather than silently presenting serial work as the intended orchestration.
6. Before handoff or closeout, the owner follows the routed closeout obligations and verifies that required durable learning, evidence, and state were recorded.

The loader could summarize this as a short critical loop—discover scope, hand off exact scope, rescope on material change, and close out—while linking to the authoritative documents for each step. It should not repeat their full content or become a second set of rules.

## Candidate Scope Capsule

A useful capsule may contain only the fields that materially constrain the delegated slice:

- Goal, ownership boundary, and explicit non-goals
- Exact repository and worktree
- Allowed target paths and protected paths
- Loader entrypoint and every applicable routed scope or directive path
- Accepted contracts, decisions, task records, and current evidence
- Expected tests, audits, and closeout obligations
- Integration assumptions and operations that remain prohibited
- Required first status disclosure, including model and reasoning when the maintainer requests it

The capsule should remain narrow enough that a child can act without reloading an entire parent conversation, but explicit enough that it can independently detect a missing or conflicting scope.

## Evidence And Trigger

- CLI correction reviews found design and evidence issues that were governed by existing scoped directives but were not consistently prominent in delegated execution.
- The current Context and Extension work required the Overseer to send exact directive paths and target-specific audit boundaries back to each Task Mastermind.
- Repeated attempts to create fresh independent reviewers were rejected by the active agent-thread limit. That is a runtime capacity constraint, not evidence that the work was indivisible or that Task Masterminds should never delegate.
- The existing architectural-context and lean-batching observations already distinguish architecture closure, bounded ownership, and useful review from indiscriminate helper creation.

## Proposed Experiment

Test the discipline on several implementation, documentation, and review tasks:

1. Record the scopes the owner resolved and the capsule sent to each child.
2. Ask each child to report missing, conflicting, or unexpectedly broad context before editing.
3. Record rescoping events and whether they prevented work under stale instructions.
4. Compare scope-related review findings, rework, duplicated discovery, and missed closeout obligations with comparable earlier tasks.
5. Distinguish deliberate non-delegation from runtime capacity denial and from work that has no coherent parallel boundary.
6. Run isolated comprehension probes against any proposed loader summary before promoting it.

## Risks And Boundaries

- Do not require a child merely to satisfy a delegation count. Small or tightly coupled work may be safer with one owner.
- Do not treat a scope capsule as authoritative over the routed sources it cites.
- Do not copy large directive bodies into packets or the loader; duplicated policy will drift.
- Do not make all routed material `#LoadNow`. The experiment should improve selection and propagation, not flatten scope.
- Do not infer that a runtime thread limit is a product-design limit. Record the denied attempt and continue safely in the active ownership boundary.
- Do not let capsule preparation replace architecture closure. A precise packet cannot repair unresolved system meaning.

## Open Questions

- Which capsule fields should be mandatory for Overseer-to-Mastermind and Mastermind-to-specialist delegation?
- Can the CLI resolve and print a minimal relevant-scope capsule without claiming semantic completeness?
- Where should the concise critical loop live so it is prominent on start and resume without bloating the loader?
- Should agents explicitly acknowledge each cited scope, or is reporting gaps and conflicts sufficient?
- What phase boundaries most reliably justify a fresh specialist or reviewer?
- How should capacity-denied delegation be represented in task evidence and closeout records?

## Promotion Signals

Promote the delegation discipline only after several tasks show fewer scope-derived corrections or cheaper review without materially increasing packet maintenance. Promote loader wording only after isolated readers consistently discover scope, rescoping, delegation handoff, and closeout while still locating the authoritative detailed rules.

Reject or narrow the idea if capsules become stale duplicate specifications, encourage unnecessary child creation, or do not improve review outcomes.

## Related Records And Sources

- [Architectural Context Delegation Gap](../observations/2026-08-21_architectural-context-delegation-gap.md)
- [Lean Agent Batching](../observations/2026-08-15_lean-agent-batching.md)
- [CLI Development Flow Evaluation](../observations/2026-08-21_cli-development-flow-evaluation.md)
- [Documentation Comprehension Probes](documentation-comprehension-probes.md)
- [Perspective Lenses](perspective-lenses.md)
- [Program Architecture And Delegation](../../../directives/program-architecture.md)
