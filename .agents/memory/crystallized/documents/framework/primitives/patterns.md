---
open-forge:
  description: Current Pattern role, continuing structural reference, deliberate variation, scope, and relationships with Templates and binding requirements
  responsibility: Define how reusable inspectable shapes remain established defaults without becoming copy-once sources or mandatory behavior
  tags: [Memory, Document, CurrentTruth, Evergreen, Framework, Core, Pattern, Structure, Scope]
---

# Patterns

## Role

A Pattern defines a recognizable reusable shape for an inspectable result.

Patterns make related code, files, APIs, documents, naming, placement, and boundaries more consistent and easier to understand or review. They capture stable relationships while allowing the concrete contents of each result to vary.

## Continuing Reference

An applicable Pattern remains relevant after a result is created. It is the established default shape for its selected scope, not merely an example used once.

A different shape may be chosen deliberately when the case warrants it. The reason should remain visible so the variation is distinguishable from accidental drift. When conformance must be binding, a Directive or another applicable Axiom owns that requirement.

## Scope

A Pattern identifies the kind of result and positive context it addresses. Recursive routes may specialize a Pattern by technology, artifact, discipline, project, or another meaningful boundary.

Narrower selected Patterns normally provide the more specific shape. When two applicable Patterns cannot compose, the conflict is surfaced and resolved deliberately rather than hidden behind route depth.

## Relationships

Patterns and Templates may describe the same family of artifacts while serving opposite parts of its lifecycle:

- A Template provides copy-ready starting content and stops governing the independent result
- A Pattern continues to describe the established shape of related results

A Pattern may be explained by Guidance, applied through a Workflow, or implemented with a Skill. Each relationship retains its own semantic role and authoritative source.

## Examples And Boundaries

Useful Patterns include:

- a component folder and file arrangement
- a stable API response shape
- a document section structure
- a naming and placement convention
- a repeatable boundary between modules

A finished file copied to start another artifact is a Template. A mandatory prohibition is a Directive. A sequence of actions for reaching a goal is a Workflow.

## Related Current Sources

- [Core primitive model](model.md)
- [Patterns entrypoint](../../../../../patterns/_patterns.md)
- [Patterns maintenance contract](../../maintenance/payload/agents/patterns.md)
- [Templates](templates.md)

## Decisions And Rationale

- [Distinct Core primitive roles](../../../decisions/core-primitives.md)
- [Templates as a Core primitive](../../../decisions/template-primitive.md)
