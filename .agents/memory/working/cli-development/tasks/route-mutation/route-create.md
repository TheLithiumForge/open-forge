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
  reading, lock, atomic apply, verification, and recovery primitives.
- Keep Template choice, content formation, collision policy, result, and rendering
  local.

## Evidence

Cover valid Unicode/case segments, invalid/reserved segments, missing or ambiguous
parent, existing target, physical alias, Template absent/valid/malformed, dry run,
lock race, generated navigation, exact bytes, idempotent refusal, failure/recovery,
no unrelated changes, streams, exits, and AOT.

## Stop Conditions

Stop before adding `--scope`, automatic slugification, parent creation, chain
initialization, managed subtree behavior, or a generic create engine.
