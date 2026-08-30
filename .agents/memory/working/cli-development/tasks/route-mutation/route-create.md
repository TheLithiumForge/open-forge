---
open-forge:
  description: Implement one-file route creation below an existing routable parent
  tags: [Memory, Working, CLI, Task, Route, Create, Mutation, Contextual]
---

# Implement Route Create

## Task State

- State: Planned after Route Init and Mutation Foundation.
- Parent: [Route Mutation Commands](_route-mutation.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/route/create/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/route/create/behavior.md).

## Expected Outcome

`route create` creates exactly one ordinary route file below one existing
routable parent, optionally using one accepted body Template, without initializing
a scope, copying route categories, or taking subtree lifecycle ownership.

## Architecture

- Keep one concrete `RouteCreatePlan` with target identity, expected parent,
  intended bytes, generated-navigation change, and preconditions.
- Use shared route-segment validation, source identity, parent topology, Template
  reading, lock, atomic apply, verification, and external recovery-bundle
  primitives. Prepare one verified bundle covering every existing Replace/Delete
  before the first target effect; Create and no-op plans create none.
- Keep Template choice, content formation, collision policy, result, and rendering
  local.

## Evidence

Cover valid Unicode/case segments, invalid/reserved segments, missing or ambiguous
parent, existing target, physical alias, Template absent/valid/malformed, dry run,
lock race, generated navigation, exact bytes, idempotent refusal,
bundle-preparation failure/retention, no unrelated changes, streams, exits, and
AOT.

## Preparation Closeout

Read-only preparation on clean no-op branch `codex/route-create` at exact base
`33913dfe7f8f80598ca4765c516d308ed179c3ab` produced no commit, Gray, Red, or
Green change. The accepted contract correction is integrated at
`cd01b8a71cec399a17409428835d18f589335623`; Route Create is not ready before
integrated Route Init.

- Expected implementation paths are
  `src/cli/core/OpenForge.Cli.Core/Commands/Route/Create/**`,
  `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Route/Create/**`, and
  `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Create/**`.
- `src/cli/root/OpenForge.Cli/Composition/CliCompositionRoot.cs`,
  `src/cli/core/OpenForge.Cli.Core/Shell/Serialization/CliJsonContext.cs`,
  `src/cli/core/OpenForge.Cli.Core/Shell/Serialization/CliYamlContext.cs`,
  `src/cli/core/OpenForge.Cli.Core/Commands/Route/Shared/Rendering/RouteHelpSections.cs`,
  `src/cli/core/OpenForge.Cli.Core/Framework/**`, EndToEnd/Native AOT evidence,
  preceding commands, and shared program ledgers remain protected integration or
  predecessor surfaces.
- Decisive evidence must cover binding, exact parent/target resolution, content
  and Template formation, intended topology, dry-run/no-op, application and
  recovery, all statuses and presentations, process behavior, and Native AOT.

### Accepted Correction Integrated

The maintainer accepted the repeatedly specified result/recovery meaning:
`attention` occurs only after successful target verification when recovery
deletion is `Failed` and the artifact is positively observed `Retained`.
The protected contract correction, including removal of `currently reserved`,
is squash-integrated at `cd01b8a71cec399a17409428835d18f589335623` with exact
tree equality. Route Create Gray and Red remain blocked until Route Init is
integrated.

Preparation also crossed its no-restore verification boundary. The worktree
remained clean, and its warning-free Release build plus focused 695 Unit and 229
Integration passes remain useful evidence, but they are not final acceptance
evidence for this preparation closeout.

## Stop Conditions

Stop before adding `--scope`, automatic slugification, parent creation, chain
initialization, managed subtree behavior, or a generic create engine.
