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
reference, overwrite, descendant, lifecycle, generated-navigation, and recovery
boundaries. It preserves unrelated and user-owned content.

## Architecture

- Observation records exact physical source, route descendants, overwrite pair,
  incoming references, generated projections, lifecycle ownership, and recovery
  state.
- `RouteRemovePlan` explicitly names every file/region effect and its ordering.
- Use shared route/reference facts and mutation primitives. Keep dependency policy,
  refusal reasons, and deletion order local.

## Evidence

Cover simple route, non-empty or dependent route, overwrite pair, incoming
references, managed/unmanaged distinction, dry run, invalid confirmation/write
policy, physical aliases, lock race, generated navigation, lifecycle update,
partial failures and recovery, second run, preservation, human/JSON/help,
process exits, and AOT.

## Stop Conditions

Stop before recursive deletion not explicitly planned, removal of unowned
content, ignored references, unsafe alias traversal, or cleanup of recovery facts
before verified completion.
