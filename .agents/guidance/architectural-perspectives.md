---
open-forge:
  description: Apply explicit top-down architecture, task decomposition, bounded implementation, and integration-review perspectives without multiplying owners
  tags: [Core, Guidance, Architecture, Planning, Task, Delegation, Review, Perspective, Efficiency]
---

# Architectural Perspectives

## Scenario

Use this Guidance when a system, program, or broad change must be designed and decomposed before bounded implementation can proceed safely. A perspective is a question set and planning horizon, not a provider, model, or separate decision authority.

## Primary Architect Perspective

Keep the complete accepted system horizon in view.

Ask:

- What user and system outcomes must the accepted program support?
- Which public contracts, platform constraints, safety boundaries, and external systems govern it?
- What is process-wide, nearest-shared, family-local, and consumer-local?
- What are the dependency direction, composition root, state flow, failure model, and side-effect boundaries?
- Which accepted later consumers require a neutral foundation now, and which possible reuse should remain local?
- What build, test, packaging, migration, performance, and release constraints shape source design?
- Which decisions and callable contracts must be settled before implementation can be closed?

Produce one compact architecture map, placement map, invariant set, integration sequence, evidence plan, and unresolved frontier.

## Delegated Architect Perspective

Use a separate architect only when the architecture problem can be bounded and its compact return will reduce primary-context loading or protect independent analysis.

The packet must provide the outcome, accepted horizon, authority, constraints, known evidence, and unresolved frontier. The return should contain decisions and maps, not a transcript of repository reading. The primary owner must inspect and accept it. Do not create two concurrent architecture owners.

## Task-Master Perspective

Turn accepted architecture into coherent outcomes. For each slice, establish:

- parent outcome and why the slice exists;
- inherited architecture, contracts, and predecessor outputs;
- inputs, outputs, consumers, dependency direction, and integration point;
- accepted models, algorithms, or file shape when already decided;
- expected paths, protected paths, direct integration neighborhood, and non-goals;
- acceptance evidence, focused verification, and batch or system gate; and
- stop conditions that return unresolved meaning to the responsible context.

Create a child task only when it has independent outcome, ownership, state, or evidence. Keep checklist items inside their parent when a separate record adds only navigation cost.

## Advisor Perspective

Give each advisor one named lens such as dependency integrity, filesystem safety, developer experience, evidence architecture, or delivery risk. Require a recommendation, decisive evidence, strongest counterargument, tradeoffs, and change conditions. Advisors do not vote or convert recommendations into architecture.

## Reviewer Perspective

Give a reviewer an exact baseline, changed artifacts, accepted requirements, claimed evidence, and one review horizon. A local reviewer checks the closed slice and direct integration neighborhood. An architectural reviewer checks dependency direction, shared boundaries, accepted future integration, and cross-cutting invariants. Do not ask one reviewer to reconstruct both horizons implicitly.

## Delegated Implementer Perspective

Give the implementation owner enough system context to understand why the slice exists, while keeping decisions inside a closed boundary. Preserve ownership through tests, production, local refactoring, and correction when possible.

## Tradeoffs

Up-front architecture prevents incompatible local choices, but it must not become speculative implementation of possible futures. Detailed slices reduce rediscovery, but unnecessary records and agents increase context and integration cost. Keep one accepted top-down model and parallelize only evidence gathering or closed slices.
