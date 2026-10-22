---
open-forge:
  description: Rules agents must follow in this workspace
  tags: [LoadNow, Core, Directive]
---

# Directives

## What behavior is required in this scope?

Rule files here apply to the whole workspace.

## Axioms

- Put each file's rules under a non-empty `## Instructions`. State each obligation with its conditions and meaningful exceptions, so it stays understandable without its examples.
- Tag each rule file in this folder #LoadNow. Directives are mandatory within their scope. Limit a rule file with `applyTo` when it only matters for some files, for example `**/*.cs`.
- A subfolder narrows its rules to that area, and its catalogue may stay on demand. Its rules apply once the task selects it. Tag the subfolder's catalogue #LoadNow, limited with `applyTo` when it matches files, when its rules must load automatically for matching work.

## Entries

- [Binding reusable CLI operation rules independent of a product implementation](cli/_cli.md) - #Directive #CLI #Interface #Command #Predictability
- [Binding C# design and style rules for source and tests authored or reviewed across the workspace](csharp/_csharp.md) - #Directive #CSharp #Source #Testing #Design #Style #Readability
- [Keep consequential decisions with the maintainer while allowing routine reversible execution within accepted direction](decision-authority.md) - #LoadNow #Core #Directive #Decision #Collaboration #Authority
- [Keep workspace work local and reversible by default, and require exact authorization for external or destructive effects](execution-safety.md) - #LoadNow #Core #Directive #Safety #Permission #ExternalEffect #Git #Destructive
- [Keep one user-facing project principal responsible for discussion, accepted architecture, optional hidden task ownership, isolated parallel execution, integration, and project-level acceptance](hierarchical-orchestration.md) - #LoadNow #Core #Directive #Orchestration #Project #Architecture #Agents #Worktree #Integration #Context #UserExperience
- [Binding Open Forge-maintenance instructions, narrowed by CLI, Framework, testing, and TypeScript scopes](open-forge/_open-forge.md) - #Directive #CLI #Framework #Testing #TypeScript
- [Required work-record relationships without imposing a task schema](planning/_planning.md) - #Extension #Directive #Planning
- [Keep architecture and cross-cutting contracts in one accepted top-down model before delegating closed implementation slices](program-architecture.md) - #LoadNow #Core #Directive #Architecture #Planning #Task #Delegation #Integration
- [Calibrate development rigor, safety, and complexity to the project's real consequences before adding exceptional machinery](proportional-development.md) - #LoadNow #Core #Directive #Development #Design #Safety #Simplicity #Platform #Risk #Decision
- [Write clear, consistent user communication and Open Forge source prose with proportionate authoring and review](public-facing-writing.md) - #LoadNow #Core #Directive #Writing #Terminology
- [Preserve material review findings with stable identity while avoiding duplicate rationale, repeated review, and observation noise](review-evidence.md) - #LoadNow #Core #Directive #Review #Evidence #Reasoning #Tradeoff #Observation #Dogfooding
- [Binding software contract and visible configuration rules independent of language](software/_software.md) - #Directive #Software #Contract #Configuration #Testing
- [Keep behavior and its directly related source, contracts, tests, fixtures, and support together at the narrowest useful scope](source-locality.md) - #LoadNow #Core #Directive #Source #Locality #Contract #Testing #Structure
- [Binding rules for pre-specified slice execution plans used when work is delegated](work-planning/_work-planning.md) - #Directive #Planning #Task #Execution #Delegation
