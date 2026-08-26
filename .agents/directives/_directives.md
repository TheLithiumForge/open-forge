---
open-forge:
  description: Required instructions loaded through selected routes
  tags: [LoadNow, Core, Directive]
---

# Directives

Directives contain required instructions.

## Axioms

- Every sibling Directive listed under an entrypoint's `Entries` carries #LoadNow.
- Each sibling Directive has one non-empty `## Instructions` section.
- Sibling Directives listed by this root entrypoint apply throughout the workspace.
- Select a child Directive route only when its path, description, tags, and parent routes match the work.
- A selected child entrypoint sets the narrower scope before its sibling Directives load.
- Those Instructions apply only within the child route's scope.
- Child Directives add to active parent Directives. A narrower scope does not create higher authority.
- Report any conflict or instruction that cannot be followed, and explain why.

## Entries

<!-- open-forge:generated-index:start -->
- [Binding C# design and style rules for source and tests authored or reviewed across the workspace](csharp/_csharp.md) - #Directive #CSharp #Source #Testing #Design #Style #Readability
- [Keep consequential decisions with the maintainer while allowing routine reversible execution within accepted direction](decision-authority.md) - #LoadNow #Core #Directive #Decision #Collaboration #Authority
- [Keep workspace work local and reversible by default, and require exact authorization for external or destructive effects](execution-safety.md) - #LoadNow #Core #Directive #Safety #Permission #ExternalEffect #Git #Destructive
- [Keep one user-facing project principal responsible for discussion, accepted architecture, optional hidden task ownership, isolated parallel execution, integration, and project-level acceptance](hierarchical-orchestration.md) - #LoadNow #Core #Directive #Orchestration #Project #Architecture #Agents #Worktree #Integration #Context #UserExperience
- [Binding Open Forge-maintenance instructions, narrowed by CLI, Framework, testing, and TypeScript scopes](open-forge/_open-forge.md) - #Directive #CLI #Framework #Testing #TypeScript
- [Keep architecture and cross-cutting contracts in one accepted top-down model before delegating closed implementation slices](program-architecture.md) - #LoadNow #Core #Directive #Architecture #Planning #Task #Delegation #Integration
- [Preserve material review findings with stable identity while avoiding duplicate rationale, repeated review, and observation noise](review-evidence.md) - #LoadNow #Core #Directive #Review #Evidence #Reasoning #Tradeoff #Observation #Dogfooding
- [Keep behavior and its directly related source, contracts, tests, fixtures, and support together at the narrowest useful scope](source-locality.md) - #LoadNow #Core #Directive #Source #Locality #Contract #Testing #Structure
- [Write clear, consistent user communication and Open Forge source prose with proportionate authoring and review](writing.md) - #LoadNow #Core #Directive #Writing #Terminology
<!-- open-forge:generated-index:end -->
