---
open-forge:
  description: Build and accept the actual command-free C# workspace, Core, host, safety, tests, and Native AOT foundation
  tags: [Memory, Archived, Contextual, Historical, CLI, Task, Foundation, Architecture, DotNet, NativeAOT, Complete]
---

# CLI Foundation

## Task State

- State: Complete.
- Responsible role: Mastermind.
- Parent: [Complete The Replacement CLI](../00-cli-development.md).
- Plan steps: F1-F5 and G1.

## Expected Outcome

The repository root contains the retained .NET workspace controls and ignored
artifact boundary. `src/cli/` contains the Core library, thin host, shared safety
boundaries, and active test projects. This is the actual base for every command,
not a probe or disposable spike.

## Inherited Authority

Read the parent Task, [Architecture](../../../../crystallized/documents/cli/architecture.md),
[Plan](../../plan.md), [CLI Implementation Directive](../../../../../directives/open-forge/cli/implementation.md), and every selected child Task.

## Scope And Boundaries

- Allowed: repository-root .NET control and artifact paths, `src/cli/`, the CLI
  workflow under `.github/workflows/`, direct CLI development documentation, and
  this active Plan/Task/Checkpoint set.
- Protected: Every command contract, command symbol, command behavior, frozen MVP,
  package publication, and unrelated repository source.
- The foundation exposes no fake or retained command. Root help may show product
  identity and no available operations, but it must not advertise an executable
  placeholder as implemented.
- The Mastermind authors all foundation source and callable contracts directly.

## Foundation Invariants

1. Root depends on Core; Core never depends on root.
2. Shell does not depend on commands. Framework capabilities do not depend on
   parser, command, renderer, or process types.
3. Every stage is directly callable, immutable, fail-closed, and effect-bounded.
4. All C# outputs remain under root `/artifacts/`.
5. Obsolete preserved tests remain removed; active evidence carries retained
   expectations.
6. Managed and Native AOT execution prove the retained host and serialization
   paths.
7. No architectural debt is deferred into `route list`.

## Acceptance

The group completes when all child Tasks pass, the solution restores and builds
without warnings, active tests pass independently, local `win-x64` Native AOT
publish and execution pass, package auditing is clean, no root C# source project
or project-local output exists, and Mastermind accepts the integrated dependency
and source model.

## Child Tasks

- [x] [Create the .NET workspace, six-project graph, dependencies, root artifacts, and active-test boundary](done/01-workspace-projects.md) — Complete — Implementer: Mastermind
- [x] [Freeze the route-free Shell definitions, binding model, immutable messages, result contract, and serialization call surfaces](done/02-shell-contracts.md) — Complete — Implementer: Mastermind
- [x] [Implement parser-owned input, terminal policy, normalized invocation, workspace absence, and exact binding selection](done/03-parser-invocation.md) — Complete — Implementer: Mastermind
- [x] [Implement fail-closed operation, presentation, rendering, diagnostic, output, and completion stages](done/04-pipeline-output.md) — Complete — Implementer: Mastermind
- [x] [Implement the thin executable host and explicit command-free composition root](done/05-root-host.md) — Complete — Implementer: Mastermind
- [x] [Implement shared workspace selection, typed reads, physical path identity, and component-wise containment](done/06-filesystem-safety.md) — Complete — Implementer: Mastermind
- [x] [Create active test projects, shared test support, local Native AOT evidence, and six-RID CI scaffolding](done/07-test-aot.md) — Complete — Implementer: Mastermind
- [x] [Integrate and accept the complete command-free architectural foundation](done/08-acceptance.md) — Complete — Implementer: Mastermind

## Entries

- [Routes completed leaf Tasks for the CLI Foundation group without changing their historical Task meaning](done/_done.md) - #Memory #Archived #Contextual #Historical #CLI #Task #Foundation #Complete
