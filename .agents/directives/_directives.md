---
open-forge:
  description: Required instructions loaded through selected routes
  tags: [LoadNow, Core, Directive]
---

# Directives

## What behavior is required in this scope?

Directives define required agent behavior within a scope.

## Axioms

- Each `Directive` file in the same folder as an `entrypoint` must carry #LoadNow, as they are mandatory within that scope.
- Directive files in this root `entrypoint`'s folder apply throughout the workspace.
- Each of these files has one non-empty `## Instructions` section.
- A scoped entrypoint doesn't need to carry #LoadNow and will be loaded only when selected.
- Select a child entrypoint before loading its Directive files. Their Instructions apply only within that narrower scope.
- Child Directives add to active parent Directives. A narrower scope does not create higher authority.
- Report any conflict or instruction that cannot be followed, and explain why.

## Entries

- [Binding C# design and style rules for source and tests authored or reviewed across the workspace](csharp/_csharp.md) - #Directive #CSharp #Source #Testing #Design #Style #Readability
- [Keep consequential decisions with the maintainer while allowing routine reversible execution within accepted direction](decision-authority.md) - #LoadNow #Core #Directive #Decision #Collaboration #Authority
- [Keep workspace work local and reversible by default, and require exact authorization for external or destructive effects](execution-safety.md) - #LoadNow #Core #Directive #Safety #Permission #ExternalEffect #Git #Destructive
- [Keep one user-facing project principal responsible for discussion, accepted architecture, optional hidden task ownership, isolated parallel execution, integration, and project-level acceptance](hierarchical-orchestration.md) - #LoadNow #Core #Directive #Orchestration #Project #Architecture #Agents #Worktree #Integration #Context #UserExperience
- [Binding Open Forge-maintenance instructions, narrowed by CLI, Framework, testing, and TypeScript scopes](open-forge/_open-forge.md) - #Directive #CLI #Framework #Testing #TypeScript
- [Keep architecture and cross-cutting contracts in one accepted top-down model before delegating closed implementation slices](program-architecture.md) - #LoadNow #Core #Directive #Architecture #Planning #Task #Delegation #Integration
- [Calibrate development rigor, safety, and complexity to the project's real consequences before adding exceptional machinery](proportional-development.md) - #LoadNow #Core #Directive #Development #Design #Safety #Simplicity #Platform #Risk #Decision
- [Write clear, consistent user communication and Open Forge source prose with proportionate authoring and review](public-facing-writing.md) - #LoadNow #Core #Directive #Writing #Terminology
- [Preserve material review findings with stable identity while avoiding duplicate rationale, repeated review, and observation noise](review-evidence.md) - #LoadNow #Core #Directive #Review #Evidence #Reasoning #Tradeoff #Observation #Dogfooding
- [Keep behavior and its directly related source, contracts, tests, fixtures, and support together at the narrowest useful scope](source-locality.md) - #LoadNow #Core #Directive #Source #Locality #Contract #Testing #Structure
