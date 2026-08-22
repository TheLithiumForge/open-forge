---
open-forge:
  description: Parent outcome, scope, authority, and acceptance for the complete replacement CLI program
  tags: [Memory, Working, CLI, Task, Program, Architecture, Development, Contextual, Active]
---

# Complete The Replacement CLI

## Task State

- State: Active.
- Responsible role: Mastermind.
- Task source: This file.
- Last updated: 2026-08-22.

## Problem Statement

Open Forge has accepted command contracts and a complete top-down C# Architecture,
but no active replacement source. The removed first implementation demonstrated
useful behavior and evidence while also proving that command-first delegation can
create incompatible local architecture and miss system safety boundaries.

## Expected Outcome

One complete, deterministic, Native-AOT executable implements every retained
command and ships through thin wrappers with six-RID, support-floor,
supply-chain, documentation, and release evidence. Architecture and command
behavior remain explicit, locally navigable, and safe to extend.

## Relationships And Backlinks

| Relationship        | Link                                                                                     | Relevance                                             |
| ------------------- | ---------------------------------------------------------------------------------------- | ----------------------------------------------------- |
| Plan                | [Development Plan](../plan.md)                                                           | Defines sequence, dependencies, gates, and resumption |
| Checkpoint          | [CLI Development Checkpoint](../../checkpoints/cli-development.md)                       | Defines current state and next action                 |
| Child Tasks         | [Task index](_tasks.md)                                                                  | Routes every implementation group                     |
| Historical evidence | [Implementation Reset](../../../archived/cli-release/implementation-reset-2026-08-21.md) | Preserves lessons without constraining design         |

## References And Authority

| Source                                                                                        | Question it answers                             | Status                             | Change boundary                                      |
| --------------------------------------------------------------------------------------------- | ----------------------------------------------- | ---------------------------------- | ---------------------------------------------------- |
| [CLI Architecture](../../../crystallized/documents/cli/architecture.md)                       | How is the system structured?                   | Accepted current architecture      | Mastermind only through explicit architecture change |
| [Command Contract Set](../../../crystallized/documents/cli/command-contract-set.md)           | Where is command meaning defined?               | Accepted current contract map      | Not by implementation Tasks                          |
| [Detailed Contracts](../../../crystallized/documents/cli/contracts/_contracts.md)             | What does each command do?                      | Accepted current product contracts | Only through maintainer decision                     |
| [Shared Operation Contract](../../../crystallized/documents/cli/shared-operation-contract.md) | What behavior crosses commands?                 | Accepted current contract          | Only through maintainer decision                     |
| [Program Architecture Directive](../../../../directives/program-architecture.md)              | Who owns architecture and delegation readiness? | Binding Directive                  | Not by child Tasks                                   |
| [CLI Implementation Directive](../../../../directives/open-forge/cli/implementation.md)       | Which implementation rules apply?               | Binding Directive                  | Not by child Tasks                                   |

## Scope

### Included

- The complete retained command tree and cross-command foundations.
- `src/cli/` workspace, root, Core, active tests, artifacts, and package source.
- Native AOT, CI, packages, supply-chain evidence, documentation, and release.
- Necessary current Architecture, Plan, Task, Checkpoint, Directive, Pattern, and
  dogfooding updates.

### Excluded

- Frozen `src/cli-mvp/` source, tests, build, and behavior.
- Legacy compatibility, migration, aliases, or fallback dispatch.
- Partial publication or a command subset release.
- Framework product behavior not accepted by current contracts.
- Remote publication before the final delivery Task explicitly authorizes it.

### Constraints

- All replacement C# material stays below `src/cli/`.
- The root host remains thin and Core remains independent of it.
- Real `System.IO`, managed BCL-first safety, source generation, trimming, and
  Native AOT are hard boundaries.
- Each delegated Task is closed before implementation begins.

## Requirements

- CLI-PROG-001: Implement the Architecture without local substitute foundations.
- CLI-PROG-002: Satisfy every detailed command and shared operation contract.
- CLI-PROG-003: Keep each command complete before a later command consumes its
  facts or promotes its support.
- CLI-PROG-004: Keep read-only, mutation, lifecycle, recovery, packaging, and
  release effects within their accepted boundaries.
- CLI-PROG-005: Preserve reproducible managed, real-OS, process, Native AOT,
  package, and release evidence.
- CLI-PROG-006: Keep current sources, Plan, Tasks, and Checkpoint aligned at every
  integration gate.

## Acceptance Evidence

| Acceptance condition            | Evidence                                                                     | Verifier      |
| ------------------------------- | ---------------------------------------------------------------------------- | ------------- |
| Every child outcome is accepted | Task index with Complete or deliberate Cancelled state                       | Mastermind    |
| Complete contract coverage      | Command-to-evidence matrices and public scenarios                            | Owning Tasks  |
| Architecture remains coherent   | Dependency, source-locality, project, and whole-system review                | Mastermind    |
| Native delivery is complete     | Six native RIDs, support floors, packed wrappers, and supply-chain artifacts | Delivery Task |
| Release is explicit             | Maintainer acceptance and main-only release record                           | Maintainer    |

## Prerequisites And Dependencies

The Architecture, Plan, Directives, and greenfield reset are complete. Child
groups add their own predecessor dependencies. No command implementation may
bypass the Foundation group.

## Risks And Safeguards

| Risk                                    | Signal                                     | Safeguard                        | Stop condition                |
| --------------------------------------- | ------------------------------------------ | -------------------------------- | ----------------------------- |
| Architecture fragments across commands  | Shell or shared policy appears in a leaf   | Parent integration review        | Return to Architecture        |
| Task volume becomes stale               | State or paths diverge from actual work    | Update at integration gates only | Consolidate before continuing |
| Passing local tests hide system defects | Missing process, safety, or AOT proof      | Parent acceptance matrix         | Child remains incomplete      |
| Historical code anchors design          | A Task copies old source or project layout | Contract-first test disposition  | Reject and replan leaf        |

## Progress And Evidence

- Current result: Architecture, Plan, reset, Task governance, Foundation, and
  Route Discovery are accepted. Route List is integrated at `edca509`; Route
  Inspect is squash-integrated at `bd5d280`. Generic improvements are active from
  that exact baseline on `feature/cli-generic-improvements`; their complete
  beginning suite passes. Parser remediation is Complete through Purple `d445e20` and
  focused public acceptance. [Improve Active-Test Architecture](generic-improvements/active-test-architecture.md)
  is Complete at `e277227`, with its accepted Working boundary at clean `eb2e336`,
  focused Integration `129/129`, freshly published managed EndToEnd `57/57`, zero
  skips, and accepted locality review. [Refine Callable And Project
  Architecture](generic-improvements/callable-project-architecture.md) is Complete
  at `cc2387d` with focused Unit `53/53`,
  Integration `49/49`, freshly published managed EndToEnd `57/57`, zero skips, and
  accepted architecture review. All generic child Tasks are Complete.
- Blocker: None for generic child completion.
- Final generic feature gate: Warning-free build, format/diff, managed Unit `580/580`,
  Integration `213/213`, EndToEnd `57/57`, local `win-x64` Native AOT root with
  managed EndToEnd `57/57`, Native AOT Integration `213/213`, Native AOT EndToEnd
  `57/57`, package/artifact/public audits, and integrated review pass with zero skips
  and no blocker.
- Next action: Commit the generic final acceptance record, then squash-merge
  `feature/cli-generic-improvements` into `develop` and verify the integrated tree
  before starting the next product Task.

## Completion And Closeout

Complete only after final delivery and maintainer release acceptance. Consolidate
durable outcomes into current Architecture, contracts, Directives, Patterns, and
public documentation. Archive or prune temporary Plan, Task, and Checkpoint detail
that no longer earns its maintenance cost.
