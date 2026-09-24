---
open-forge:
  description: Current Pattern role, admission and lifetime test, continuing structural reference, example validity, scope, and related Core roles
  responsibility: Define when accepted inspectable shapes qualify as continuing reusable defaults without promoting temporary structure or becoming mandatory behavior
  tags: [Memory, Document, CurrentTruth, Evergreen, Framework, Core, Pattern, Structure, Scope]
---

# Patterns

## Role

A Pattern defines a recognizable reusable shape for an inspectable result.

Patterns make related code, files, APIs, documents, naming, placement, and boundaries more consistent and easier to understand or review. They capture stable relationships while allowing the concrete contents of each result to vary.

## Continuing Reference

An applicable Pattern remains relevant after a result is created. It is the established default shape for its selected scope, not merely an example used once.

A shape qualifies as a Pattern only when accepted direction establishes it as useful for future related results or repeated changes. A one-off implementation, temporary transition, or unsettled candidate does not qualify merely because it has visible structure. Temporary work may apply an existing reusable Pattern, but that does not make the temporary result itself a Pattern.

A different shape may be chosen deliberately within the authority granted for the work. Explain material departures before dependent work uses them so the variation is distinguishable from accidental drift. When a Directive, Axiom, or accepted requirement makes the shape mandatory, follow it. Ask only when the departure needs authority that has not been granted.

## Scope

A Pattern identifies the kind of result and positive context it addresses. Recursive routes may specialize a Pattern by technology, artifact, discipline, project, or another meaningful boundary.

Narrower selected Patterns normally provide the more specific shape. When two applicable Patterns cannot compose, the conflict is surfaced and resolved deliberately rather than hidden behind route depth.

## Relationships

Patterns and Templates may describe the same family of artifacts while serving opposite parts of its lifecycle:

- A Template provides copy-ready starting content and stops governing the independent result
- A Pattern continues to describe the established shape of related results

A Pattern may be explained by Guidance, applied through a Workflow, or implemented with a Skill. Each relationship retains its own semantic role and authoritative source.

One accepted design may need several linked sources. A Pattern preserves only the reusable inspectable shape. Current documents preserve integrated current meaning, Decisions preserve rationale, and Directives preserve mandatory behavior.

## Examples And Boundaries

Useful Patterns include:

- a component folder and file arrangement
- a stable API response shape
- a document section structure
- a naming and placement convention
- a repeatable boundary between modules

A finished file copied to start another artifact is a Template. A mandatory prohibition is a Directive. A sequence of actions for reaching a goal is a Workflow.

When a Pattern is created or changed, its concrete examples remain valid against the APIs, formats, and tools they use. Label intentionally incomplete examples as schematic so readers do not mistake them for verified implementations.

## Related Current Sources

- [Core primitive model](model.md)
- [Patterns entrypoint](../../../../../patterns/_patterns.md)
- [Patterns maintenance contract](../../maintenance/payload/agents/patterns.md)
- [Templates](templates.md)

## Decisions And Rationale

- [Distinct Core primitive roles](../../../decisions/framework/core-primitives.md)
- [Templates as a Core primitive](../../../decisions/framework/template-primitive.md)
