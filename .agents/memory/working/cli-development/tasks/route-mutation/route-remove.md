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
