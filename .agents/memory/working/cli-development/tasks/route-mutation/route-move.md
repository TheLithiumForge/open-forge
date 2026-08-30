---
open-forge:
  description: Implement route move with reference, overwrite, generated-navigation, and recovery integrity
  tags: [Memory, Working, CLI, Task, Route, Move, Mutation, Contextual]
---

# Implement Route Move

## Task State

- State: Planned after Route Update and References.
- Parent: [Route Mutation Commands](_route-mutation.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/route/move/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/route/move/behavior.md).

## Expected Outcome

`route move` relocates one accepted route unit to one validated destination while
preserving route semantics, overwrite pairing, managed references, generated
navigation, and recoverability. Lifecycle evidence is read and revalidated only;
Move does not create, update, adopt, release, or otherwise mutate lifecycle state.

## Architecture

- `RouteMoveObservation` captures exact source, descendants, overwrite, references,
  destination parent, collisions, and lifecycle facts.
- `RouteMovePlan` explicitly orders destination creation, content/reference
  changes, generated projections, source removal, verification, and external
  recovery-bundle preparation. Lifecycle facts remain read-only preconditions
  that are revalidated before effects.
- Reuse shared route/reference facts and mutation primitives. Keep move policy,
  reference rewrite eligibility, and effect order local.

## Evidence

Cover leaf and subtree cases allowed by contract, same-parent and cross-scope
destinations, case/Unicode aliases, source/destination physical links, collisions,
overwrite pair, incoming/outgoing references, unchanged external/unmanaged
references, dry run, lock race, partial failure at every effect boundary, bundle
retention, idempotent rerun, generated navigation, lifecycle, streams, exits, and
AOT.

## Preparation Closeout

Read-only preparation on clean no-op branch `codex/route-move` at exact base
`33913dfe7f8f80598ca4765c516d308ed179c3ab` produced no commit, Gray, Red, or
Green change. Route Move is not ready before integrated Route Update and the
accepted neutral-reference correction plus remaining authority gate below.

- Expected implementation paths are
  `src/cli/core/OpenForge.Cli.Core/Commands/Route/Move/**`,
  `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Route/Move/**`, and
  `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Move/**`.
- `src/cli/core/OpenForge.Cli.Core/Framework/**`, Route shared surfaces,
  `src/cli/root/OpenForge.Cli/Composition/CliCompositionRoot.cs`,
  `src/cli/core/OpenForge.Cli.Core/Shell/Serialization/CliJsonContext.cs`,
  `src/cli/core/OpenForge.Cli.Core/Commands/Route/Shared/Rendering/RouteHelpSections.cs`,
  EndToEnd/Native AOT evidence, preceding commands, and program ledgers remain
  protected unless a later packet assigns one exact neutral promotion.
- Decisive evidence must cover binding, leaf/category inventory, positive
  unmanaged proof, destination and physical aliases, complete workspace Markdown
  coverage and rewrites, intended topology, dry-run/revalidation, recovery and
  partial failures, typed presentation, process behavior, and Native AOT.

### Accepted Corrections Pending Implementation

- The working Task now matches the accepted contracts: Move operates only on a
  positively proven unmanaged subject, reads and revalidates lifecycle evidence,
  and performs no lifecycle mutation.
- Before Move Green, extend the existing neutral link resolver to accept a validated
  workspace-relative Markdown source path while preserving containment and
  existing `.agents` behavior. Do not add a Move-local parser or duplicate the
  resolver.

### Remaining Maintainer Authority

Freeze the exact command-local result graph, findings, finite values, `next`
content, and the command's reference/lifecycle proportionality gate before Gray
or Red.

## Stop Conditions

Stop before moving a unit whose complete dependency or recovery boundary cannot
be proved, rewriting unowned references, crossing workspace containment, or
claiming atomic multi-file behavior without evidence.
