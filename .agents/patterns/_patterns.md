---
open-forge:
  description: Reusable default shapes for code, files, APIs, documents, and other work
  tags: [LoadNow, Core, Pattern]
---

# Patterns

## What reusable shape makes related work easy to create and inspect?

Patterns define reusable default shapes that make related work consistent, easy to create, and easy to inspect.

## Axioms

### Selection And Creation

- Check `Entries` when the work creates, changes, or reviews something with a visible structure.
- Each Pattern defines a concrete shape for code, files, naming, placement, boundaries, APIs, documents, or another inspectable result.
- Create or update a Pattern only when an accepted shape should guide future related work. Structure alone does not justify promoting one-off work, temporary transitions, or unsettled candidates into a Pattern.

### Shape And Application

- Keep each Pattern focused on one reusable shape.
- Use examples that are valid for their APIs, formats, and tools. Label intentionally incomplete examples as schematic.
- Treat an applicable Pattern as the default shape in its scope. A justified adaptation is allowed within the authority already granted for the work. Explain material departures before other work depends on them.
- When a Directive, Axiom, or accepted requirement makes a shape mandatory, follow that requirement. Ask only when a departure needs authority that has not been granted.

## Entries

- [Reusable structural patterns for changing and operating Open Forge without losing its minimal routed design](open-forge/_open-forge.md) - #Pattern #Framework #Dogfood
- [Reusable software-structure shapes that apply across programming languages and build systems](software/_software.md) - #Pattern #Software #Contract #Source #Locality #Testing
- [Reusable test-evidence shapes that apply across languages, runtimes, and test runners](testing/_testing.md) - #Pattern #Testing #Evidence #Integration #EndToEnd #Snapshot
- [Pre-specified execution plan for one slice, written ahead of time so a strict executor needs no judgement and the record survives forgotten architecture updates](work-plan.md) - #Pattern #Planning #Task #Execution #Delegation #Evidence
- [Keep task outcomes, planned steps, and current state predictable without duplicating project knowledge](work-records.md) - #Extension #Pattern #Planning #Task #Memory
