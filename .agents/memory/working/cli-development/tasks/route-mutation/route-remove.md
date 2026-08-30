---
open-forge:
  description: Implement route removal with dependency, reference, generated-navigation, and recovery integrity
  tags: [Memory, Working, CLI, Task, Route, Remove, Mutation, Contextual]
---

# Implement Route Remove

## Task State

- State: Planned after Route Move.
- Parent: [Route Mutation Commands](_route-mutation.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/route/remove/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/route/remove/behavior.md).

## Expected Outcome

`route remove` removes exactly the accepted route unit after proving dependency,
reference, overwrite, descendant, lifecycle, generated-navigation, and external
recovery-bundle boundaries. It preserves unrelated and user-owned content.

## Architecture

- Observation records exact physical source, route descendants, overwrite pair,
  incoming references, generated projections, lifecycle ownership, and recovery-
  bundle state.
- `RouteRemovePlan` explicitly names every file/region effect and its ordering.
- Use shared route/reference facts and mutation primitives. Keep dependency policy,
  refusal reasons, and deletion order local.

## Evidence

Cover simple route, non-empty or dependent route, overwrite pair, incoming
references, managed/unmanaged distinction, dry run, invalid confirmation/write
policy, physical aliases, lock race, generated navigation, proof that no lifecycle
write or ownership release occurs, partial failures and bundle retention, second
run, preservation, human/JSON/help, process exits, and AOT.

Current Remove is positive-unmanaged-only. Framework-aware Route Init targets or
generated regions are trusted claims and block selection, including when they
sit below a user-owned scope entrypoint. `sourceAssetPath` is provenance and does
not grant release authority.

## Preparation Closeout

Read-only preparation on clean no-op branch `codex/route-remove` at exact base
`33913dfe7f8f80598ca4765c516d308ed179c3ab` produced no commit, Gray, Red, or
Green change. Route Remove is not ready before integrated Route Move and the
accepted parser prerequisite plus remaining authority gates below.

- Expected implementation paths are
  `src/cli/core/OpenForge.Cli.Core/Commands/Route/Remove/**`,
  `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Route/Remove/**`, and
  `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Remove/**`.
- `src/cli/core/OpenForge.Cli.Core/Framework/**`, Route shared surfaces,
  `src/cli/root/OpenForge.Cli/Composition/CliCompositionRoot.cs`,
  `src/cli/core/OpenForge.Cli.Core/Shell/Serialization/CliJsonContext.cs`,
  `src/cli/core/OpenForge.Cli.Core/Commands/Route/Shared/Rendering/RouteHelpSections.cs`,
  EndToEnd/Native AOT evidence, preceding commands, and program ledgers remain
  protected unless a later packet assigns one exact neutral promotion.
- Decisive evidence must cover binding, leaf/category inventory, positive
  ownership proof, complete workspace references and label-preserving
  detachments, intended topology, dry-run/revalidation, file and directory
  effects, recovery and residual state, typed presentation, process behavior,
  and Native AOT.

### Accepted Parser Prerequisite

Before Remove, add the smallest neutral Markdig-bound typed projection that
associates a link with its visible label or an unsupported-label state. Do not
parse raw Markdown again.

### Remaining Maintainer Authority

- Freeze the exact command-local JSON result graph, findings, finite values, and
  `next` content.
- Establish the complete Framework-plus-Extensions ownership inventory and
  verified directory-removal mechanics, preferably by consuming or promoting
  meaning first proven by Route Move. Do not create a general filesystem or
  route-mutation engine.

## Deferred Managed-Release Decision

Any later expansion to managed scoped routes requires explicit maintainer
acceptance of:

1. whether the release unit is one target, one managed chain, or one physical
   category;
2. preservation of user-owned scope entrypoints, descendants, and shared
   generated regions;
3. lifecycle publication order and post-remove verification; and
4. repeat and no-op semantics.

Do not infer those decisions from a path prefix or `sourceAssetPath`.

## Stop Conditions

Stop before recursive deletion not explicitly planned, removal of unowned
content, ignored references, unsafe alias traversal, or cleanup of recovery facts
before verified completion. Stop before releasing or deleting any managed scoped
target under the current contract.
