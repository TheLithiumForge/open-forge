---
open-forge:
  description: Historical analysis that distinguished principles, foundations, identity, invariants, primitives, decisions, directives, guidance, and templates before their ownership migration
  tags: [Memory, Archived, Analysis, Reasoning, Contextual, Historical, Framework, Principle, Governance, Template]
---

# Knowledge Role Boundaries

## Question

Open Forge currently expresses related product meaning through Vision principles, architectural invariants, Core primitive definitions, decisions, guidance, and the always-loaded Framework Essence directive. Which role should own each kind of meaning, how should users select an owner, and which document templates have earned a generic copy-ready shape?

## Accepted Conclusion

`Principle` is the useful semantic document role.

`Foundation` describes the strength of a principle: changing a foundational principle redefines the subject rather than merely revising one implementation.

`Identity` describes what the complete foundational set preserves. It is the answer produced by the principles rather than another document role.

For Open Forge, the likely current owner is therefore `principles.md`, optionally titled Open Forge Foundations and tagged #Principle #Foundation #Identity.

## Role Selection Model

| Role | Primary question | Authority or lifecycle |
|---|---|---|
| Principle | How should unfamiliar choices be judged without changing the subject's identity? | Accepted current decision filter |
| Architectural invariant | What structural condition must remain true in the accepted architecture? | Accepted current structure |
| Core primitive | What kind of reusable Framework content is this, and what semantics does that kind carry? | Current Framework ontology |
| Decision | What discrete choice was accepted, and why? | Durable accepted rationale |
| Directive or Axiom | What behavior is mandatory while this route is active? | Binding loaded behavior |
| Guidance | What approach is normally helpful in this recurring situation? | Advisory contextual default |
| Pattern | What inspectable shape should related results continue to follow? | Reusable default shape |
| Template | What copy-ready starting content makes a new independently owned artifact cheaper to create? | Source content whose authority ends at instantiation |

A foundation is not automatically a Directive. A principle can be product-defining while remaining a decision filter rather than a loaded instruction. When a principle requires recurring behavior, a Directive or Axiom states the necessary action and links to the principle instead of converting the principle document into runtime policy.

## Core Primitive Example

The current [`core-primitives.md`](../../crystallized/decisions/core-primitives.md) mostly states the present Core catalogue and its definitions. That meaning already belongs to the [Framework Architecture](../../crystallized/documents/framework/architecture.md#core-primitives) and installed primitive entrypoints.

The useful decision behind it is narrower:

- Core uses several semantically distinct reusable content roles instead of one generic knowledge bucket
- Each role keeps authority and selection predictable
- New roles must earn a distinct semantic responsibility rather than duplicate an existing primitive

If retained as a decision, the file should preserve that choice, its rationale, rejected alternatives, consequences, and links to current owners. It should not remain a second catalogue of current primitive definitions.

A related foundational principle could state that distinct meanings must retain explicit ownership rather than being collapsed for superficial simplicity. A maintainer Directive could require a proposed primitive to demonstrate a nonduplicative role before it is added. These are related statements with different jobs.

## Framework Essence Example

The former Framework Essence directive mixed:

- Product identity and decision filters
- Architectural direction
- Binding maintainer behavior

This makes the distinctions difficult to learn and adds identity prose to baseline-loaded instructions for every task.

The accepted migration:

1. Extracted identity-level decision filters into [Open Forge Principles](../../crystallized/documents/principles.md)
2. Kept exact structural consequences in the appropriate architecture
3. Consolidated necessary maintainer behavior into the [Deliberate Framework Change directive](../../../directives/deliberate-framework-change.md)
4. Preserved useful rationale in decisions
5. Removed the redundant Framework Essence directive

## User, Operator, And Maintainer

Use each term for its actual role:

- `user` owns, installs, customizes, or consumes Open Forge and is also the formal instruction role in an agent runtime
- `operator` establishes goals, directs an active agent relationship, and accepts consequential direction
- `maintainer` changes the Open Forge distribution, contracts, or repository-owned implementation
- `person` or `people` is appropriate when none of those roles matters

The same individual may occupy every role. Keeping the terms distinct still clarifies which authority or responsibility a statement means.

## Template Audit

### Current Generic Roles

The current Vision, Architecture, Operating Context, Principles, Strategy, Roadmap, Decision, Idea, and Project Status templates each answer a distinct question. They should remain optional and should be instantiated only after their knowledge earns independent ownership.

The Maintenance Contract template is justified by three current maintenance documents and an established continuing Pattern.

Open Forge currently dogfoods:

- Vision and scoped Architecture as current knowledge
- Maintenance Contract across three reviewed source surfaces
- Decision and Idea shapes through existing Memory records

Principles is the next likely current-document instantiation if its extraction boundary is accepted.

Operating Context, Strategy, Roadmap, and Project Status should not be created for Open Forge merely to exercise their templates. They remain available until current knowledge develops those independent responsibilities.

### Strong Missing Candidates

- Observation, because grounded evidence, scope, uncertainty, recurrence, and promotion differ materially from an Idea
- Handoff, because transfer state, recipient context, exact next action, blockers, and expiration differ materially from Project Status
- Analysis, because a question, evidence, assumptions, alternatives, limits, and current conclusion form a repeated Emerging responsibility

These candidates have both distinct semantics and repeated dogfood evidence.

### Defer

- Session, because chronological capture should remain flexible and may not benefit from a fixed copy-ready shape
- Specification, because a sufficiently generic shape has not yet been demonstrated across domains
- Risk and Research, because present evidence does not justify stable universal contents
- README, because product presentation and onboarding depend strongly on subject and audience

Domain extensions may introduce specialized templates without expanding the generic starting catalogue.

## Completed Migration

1. Instantiated the Principles template as the compact current identity owner
2. Kept Vision independently readable while replacing detailed duplicated principle explanations with a concise foundation summary and link
3. Reconciled top and Framework architectural invariants against the principles without removing their exact structural contracts
4. Reclassified the Framework Essence directive content by role and removed its redundant baseline owner
5. Rewrote `core-primitives.md` as actual rationale
6. Added a temporary repository-only owner-selection helper during migration

## Template Outcome

Observation, Handoff, and Analysis templates were added to the repository-only dogfood catalogue because each had distinct semantics and repeated local evidence.
