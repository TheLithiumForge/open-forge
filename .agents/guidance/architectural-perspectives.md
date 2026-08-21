---
open-forge:
  description: Apply explicit top-down architecture and task-decomposition perspectives before bounded implementation and review
  tags: [Core, Guidance, Architecture, Planning, Task, Delegation, Review, Perspective]
---

# Architectural Perspectives

## Scenario

Use this Guidance when a system, program, or broad change must be designed and
decomposed before several bounded agents can implement it safely. A perspective
is an explicit question set and planning horizon. It is not a provider, model,
runtime profile, or decision authority.

## Top-Down Architect Perspective

The top-down architect keeps the complete accepted system in view while shaping
the foundation and every cross-cutting boundary.

Ask:

- What complete user and system outcomes must the final design support?
- Which accepted contracts, platform constraints, and external boundaries govern
  the system?
- Which capabilities are process-wide, shared by a bounded family, or local to
  one current consumer?
- What is the dependency direction, composition root, state flow, failure model,
  and side-effect boundary?
- Which later features are already known, and which extension points must exist
  now to prevent a local dead end?
- Which possible reuse should remain local until another real consumer proves the
  same meaning?
- What build, test, packaging, migration, security, performance, and release
  boundaries constrain the source design?
- Which decisions and callable contracts must be settled before a smaller Task
  can be closed?

Produce an accepted architecture map, dependency and composition boundaries,
cross-cutting contracts, integration sequence, and explicit stop conditions.
Implement the architectural foundation directly or through an explicitly
assigned architecture implementer. Do not delegate unresolved system design to a
feature implementer.

## Task-Master Perspective

The task master turns accepted architecture into a hierarchy of independently
understandable, executable, and reviewable outcomes.

For each Task, establish:

- its parent outcome and why this slice exists;
- the exact architecture, Directives, Patterns, contracts, and predecessor
  outputs it inherits;
- its inputs, outputs, consumers, dependency direction, and integration point;
- accepted class, interface, data-flow, algorithm, or file-shape detail when that
  detail is already decided;
- exact allowed changes, protected surfaces, non-goals, and forbidden shortcuts;
- Red or other acceptance evidence, focused verification, and full integration
  evidence;
- stop conditions that return an unresolved architecture or product decision to
  the responsible context; and
- a truthful commit and review boundary.

Create child Tasks only when the child has a coherent outcome, dependency, state,
or evidence boundary. A checklist item may remain inside its parent when a
separate file would add navigation cost without improving execution.

## Advisor Perspective

Give each advisor one named lens, such as dependency integrity, Native AOT,
filesystem safety, developer experience, test architecture, or delivery risk.
State whether the position is cold or grounded. Require the advisor to return its
conclusion, decisive evidence, strongest counterargument, tradeoffs, and what
would change the conclusion. Advisors inform the top-down synthesis and do not
vote or convert a recommendation into accepted architecture.

## Reviewer Perspective

Give a reviewer an exact baseline, changed artifacts, accepted requirements,
claimed evidence, and one review horizon. A local correctness reviewer checks the
closed Task and its direct integration neighborhood. An architectural reviewer
checks dependency direction, shared boundaries, future integration, and
cross-cutting invariants. Do not ask a local diff reviewer to reconstruct the
whole system implicitly, and do not call a local pass architectural acceptance.

## Delegated Implementer Perspective

A delegated implementer should see enough of the whole system to understand why
the slice exists, but should decide only within its closed boundary. Its packet
must point to the parent Plan and Architecture, identify direct consumers and
predecessors, provide decided models and algorithms, and name every condition
that requires return rather than improvisation.

## Applying The Perspectives

Use the top-down architect perspective first, then the task-master perspective.
Use advisors only for unresolved evidence or useful independent alternatives.
Use implementers only after the relevant Task is ready. Review each result at the
same local horizon used for implementation and at the system integration horizon
owned by the primary architecture agent.

These perspectives may later justify a reusable Framework primitive. Current
evidence supports explicit Guidance and Task fields, not a new primitive with its
own loading or authority behavior.

## Tradeoffs

- More up-front architecture reduces repeated rediscovery and incompatible local
  choices, but it must not become speculative implementation of every future
  feature.
- Detailed Tasks make bounded implementation cheaper and safer, but unnecessary
  child records increase maintenance and loading cost.
- Keeping one top-down context improves coherence, but it can become a bottleneck.
  Parallelize evidence gathering and closed Tasks without distributing unresolved
  architecture authority.
