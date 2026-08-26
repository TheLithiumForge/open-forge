---
open-forge:
  description: Hierarchical implementation Tasks for the complete greenfield replacement CLI
  tags: [Memory, Working, Contextual, Active, CLI, Task, Architecture, Development]
---

# CLI Development Tasks

Read the [program Task](00-cli-development.md), [Architecture](../../../crystallized/documents/cli/architecture.md), and [Plan](../plan.md) before selecting a Task group.

## Task Groups

- [x] [CLI Foundation](foundation/_foundation.md) — Complete — Implementer: Mastermind
- [x] [Route Discovery](route-discovery/_route-discovery.md) — Complete — Implementer: Mastermind
- [x] [Generic CLI Improvements](generic-improvements/_generic-improvements.md) — Complete — Implementer: Mastermind
- [ ] [Read-Only Commands](read-only/_read-only.md) — Active; Find, References, Context, and Extension List are Complete, with Context integrated at `ca097a2` and Extension List at current baseline `db0d39a`; Extension Inspect contract closure and the pure Generated Navigation foundation are the next bounded lanes — Implementer: Overseer
- [x] [Modern C# Improvements](modern-csharp-improvements.md) — Complete; Preflight `55eb82e`, Framework `a90af59`, Shell/root `fe10525`, Route Inspect/family `62a1dd9`, Route List `273eb45`, Tests/support `6af5fb1`, and final managed, Native AOT, package, audit, and public no-write gates accepted in the commit containing this record — Implementer: Mastermind
- [x] [Repository-root CLI developer workflow](repository-root-developer-workflow.md) — Complete and squash-integrated into local `develop` at `d9e0686` — Implementer: Mastermind
- [ ] [Mutation Foundation](mutation-foundation/_mutation-foundation.md) — Planned — Implementer: Not assigned
- [ ] [Route Mutation Commands](route-mutation/_route-mutation.md) — Planned — Implementer: Not assigned
- [ ] [Lifecycle Commands](lifecycle/_lifecycle.md) — Planned — Implementer: Not assigned
- [ ] [Operational Commands](operations/_operations.md) — Planned — Implementer: Not assigned
- [ ] [CLI Delivery](delivery/_delivery.md) — Planned — Implementer: Not assigned

## Axioms

- Read every parent Task before a child Task. Child scope inherits all parent
  constraints and may only narrow them.
- The Mastermind owns architecture, cross-cutting contracts, Task state,
  integration, and acceptance.
- A leaf Task is delegation-ready only when its decisions, predecessor outputs,
  class or algorithm model, allowed paths, tests, verification, and stop
  conditions are closed.
- An implementer changes only the leaf Task's allowed paths and returns evidence.
  It does not update Tasks, commit, promote shared code, or resolve architecture.
- A reviewer receives the accepted parent chain, exact baseline and changed paths,
  claimed evidence, and one named review horizon.
- Stop and return to the parent when implementation exposes a public-contract,
  architecture, dependency, platform, lifecycle, safety, or release choice.

## Task State Vocabulary

- `Planned`: Meaning exists, but a predecessor or design closure still blocks it.
- `Ready`: Every prerequisite and decision required to start is closed.
- `Active`: This is the selected implementation or integration Task.
- `Blocked`: A named unmet condition prevents progress.
- `Complete`: Acceptance evidence and integration are committed.
- `Cancelled`: The outcome is no longer required.

Generated Entries were refreshed after the temporary compatibility-name changes.
They provide current navigation only; the Task Group list and each Task record
define execution state.

## Entries

<!-- open-forge:generated-index:start -->
- [Parent outcome, scope, authority, and acceptance for the complete replacement CLI program](00-cli-development.md) - #Memory #Working #CLI #Task #Program #Architecture #Development #Contextual #Active
- [Package, prove, document, and release the complete native CLI without partial publication](delivery/_delivery.md) - #Memory #Working #CLI #Task #Distribution #NativeAOT #SupplyChain #Release #Contextual
- [Build and accept the actual command-free C# workspace, Core, host, safety, tests, and Native AOT foundation](foundation/_foundation.md) - #Memory #Working #CLI #Task #Foundation #Architecture #DotNet #NativeAOT #Contextual #Complete
- [Improve cross-cutting CLI parser, test, callable, and project structure without changing accepted command meaning](generic-improvements/_generic-improvements.md) - #Memory #Working #CLI #Task #Generic #Parser #Testing #Architecture #Contextual #Complete
- [Implement root and Extension creation, installation, update, and removal lifecycle commands](lifecycle/_lifecycle.md) - #Memory #Working #CLI #Task #Lifecycle #Extension #Install #Update #Contextual
- [Apply accepted truthful nullability, construction, and modern C# syntax rules across the complete replacement solution](modern-csharp-improvements.md) - #Memory #Working #CLI #Task #CSharp #Nullability #Initialization #Refactoring #Contextual #Complete
- [Build and accept locking, lifecycle, planning, application, recovery, and Git foundations before mutations](mutation-foundation/_mutation-foundation.md) - #Memory #Working #CLI #Task #Mutation #Lifecycle #Recovery #Git #Contextual
- [Implement aggregate status, diagnosis, repair, and cleanup after every state producer exists](operations/_operations.md) - #Memory #Working #CLI #Task #Status #Doctor #Repair #Cleanup #Contextual
- [Implement retained read-only source, context, extension, and generated-navigation commands](read-only/_read-only.md) - #Memory #Working #CLI #Task #ReadOnly #Source #Extension #Index #Contextual
- [Move replacement CLI tooling to the repository root and make ordinary test runs publish and discover the local development executable](repository-root-developer-workflow.md) - #Memory #Working #CLI #Task #DotNet #Testing #DeveloperExperience #Contextual #Complete
- [Implement read-only route discovery, beginning with route list and then route inspect](route-discovery/_route-discovery.md) - #Memory #Working #CLI #Task #Route #Discovery #ReadOnly #Contextual #Complete
- [Implement retained route mutation commands on the accepted mutation foundation](route-mutation/_route-mutation.md) - #Memory #Working #CLI #Task #Route #Mutation #Contextual
<!-- open-forge:generated-index:end -->
