---
open-forge:
  description: Current foundational principles that define Open Forge's identity and guide unfamiliar product, Framework, and tooling choices
  tags: [Memory, Document, CurrentTruth, Evergreen, Principle, Foundation, Identity, Product, ACE]
---

# Open Forge Principles

## Scope

These principles are Open Forge's foundations. They define how unfamiliar product, Framework, and tooling choices should be judged when no existing design answers them directly.

Principles are accepted decision filters, not loaded behavioral rules. Directives and Axioms own mandatory work behavior, architecture owns structural invariants, and decisions preserve why important choices were accepted.

## Principles

### User-Owned Meaning

Workspace meaning remains in human-readable files users own or in external sources they explicitly declare. A proprietary runtime, hidden service, generated index, cache, or CLI must not become the only place where Open Forge meaning can be understood.

This favors inspectability, portability, and correctability over convenience that requires surrendering semantic ownership.

### Context By Relevance

The active context for a goal should contain the authority, knowledge, constraints, relationships, and working state that matter to that goal, not everything the environment stores.

Open Forge may expand through any number of routed scopes while ordinary context cost grows primarily with the scopes and relationships deliberately selected. Completeness never means indiscriminate loading.

### Operator-Led Direction And Agent Autonomy

The operator owns goals, priorities, consequential tradeoffs, and accepted direction. Agents investigate, suggest, challenge, execute, and verify within that direction.

Operator control must not become repetitive approval ceremony, and agent autonomy must not become silent ownership of contextual product choices.

### One Owner And Visible Relationships

Each detailed definition, contract, decision, and current concept has one authoritative owner. Relative links, descriptions, anchors, and established tags make dependencies and associations visible without maintaining competing copies.

Small synchronized summaries remain valid at independently useful entry boundaries, but convenience does not justify parallel detailed truth.

### Start Small And Grow From Use

Open Forge ships a small useful foundation and sensible removable defaults rather than a universal methodology. A workspace develops its own methods through accepted decisions, corrections, evidence, recurring needs, local scopes, and optional Extensions.

Defaults should provide more value than an empty substrate while remaining understandable, removable, replaceable, and non-restoring unless restoration is explicitly requested.

### Continuity Without Premature Truth

Open Forge preserves active work, plausible candidates, accepted knowledge, and useful history without treating them as one authority state.

Recording should prevent costly rediscovery and coordination loss, while deliberate ownership and transitions prevent raw accumulation from masquerading as current truth. The exact route taxonomy may evolve without removing this distinction.

### Native Capability And Deterministic Assistance

Open Forge reuses contemporary agent reasoning, file inspection, scoped authority, and tool use instead of redefining general intelligence through exhaustive instructions.

Deterministic tools should make context retrieval, validation, navigation, planning, and safe change cheaper. They validate what can be made mechanical without privately owning meaning or inferring accepted direction.

### Broad And Recursively Adaptable

Development is a proving ground, not a product boundary. The same ownership, routing, scope, relationship, and evolution model should remain useful across people, disciplines, projects, repositories, and shared sources of truth.

Broad applicability constrains the shared foundation. It does not prevent a local workspace or Extension from becoming highly specialized.

### Honest Reliability

Explicit context, early loading, deterministic validation, and review increase the probability of correct behavior from nondeterministic agents.

Open Forge must not claim mechanical control over reasoning or compliance. Reliability claims require evidence, and consequential work retains proportionate review.

## Tensions And Ordering

- User ownership and semantic completeness outrank tool convenience
- Accepted operator direction outranks agent preference, while clear direction should not trigger redundant approval
- Relevance and minimalism do not justify omitting context necessary for correct work
- Sensible defaults may be opinionated enough to help, but never become an untouchable methodology
- Continuity capture should favor plausible future value without encouraging indiscriminate accumulation
- Broad applicability constrains Core and the shared Framework, not local specialization
- Honest reliability constrains product claims even when stronger wording would be easier to market

## Change Boundary

Changing a principle is possible, but it changes Open Forge's identity rather than merely revising one implementation. Such a change requires deliberate reconsideration of the [Vision](vision.md), affected architecture, public promise, and supporting decisions.

## Related Current Views

- [Open Forge Vision](vision.md)
- [Open Forge Architecture](architecture.md)
- [Open Forge Framework Architecture](framework/architecture.md)
- [Deliberate Framework Change directive](../../../directives/deliberate-framework-change.md)

## Decisions And Rationale

- [Product direction](../decisions/product-direction.md)
- [Routing model](../decisions/routing-model.md)
- [Memory model](../decisions/memory-model.md)
- [Distinct Core primitive roles](../decisions/core-primitives.md)
