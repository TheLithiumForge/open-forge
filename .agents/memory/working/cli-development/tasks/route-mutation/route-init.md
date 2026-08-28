---
open-forge:
  description: Implement exact-chain route initialization and resolve the deferred Framework-shape decision before behavior starts
  tags: [Memory, Working, CLI, Task, Route, Init, Mutation, Contextual]
---

# Implement Route Init

## Task State

- State: Blocked on the deferred Framework-shape product decision.
- Parent: [Route Mutation Commands](_route-mutation.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/route/init/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/route/init/behavior.md).

## Decision Boundary

Before callable contracts or tests are authored, the maintainer chooses whether
the accepted generic exact-chain initialization remains the complete behavior or
whether one explicit Framework-shape mode is added. Do not let an implementer
improvise `--framework`, `--scaffold-from`, scope flags, blueprints, dynamic
lifecycle ownership, or slugification.

If the decision changes public behavior, update the command contracts,
Architecture, Plan, this Task, help, lifecycle schema, and complete evidence before
implementation.

## Expected Generic Outcome

For one exact concrete route target, inspect the current chain, plan every missing
entrypoint, preserve existing routable parents and authored content, apply the
fixed accepted scaffold under lock/revalidation, refresh generated navigation,
verify the resulting chain, and form one concrete result.

## Architecture

- Keep definitions, binding, request, plan, operation, result, and renderers at
  `Commands/Route/Init/`.
- Use route shared identity/topology facts and mutation primitives.
- Keep scaffold selection and route-init policy local.
- Model inspect, plan, apply, verify, and lifecycle effects separately.

## Evidence

Cover full existing, partially missing, completely missing, invalid segments,
collisions, physical aliases, dry run, lock race, generated navigation,
idempotence, external bundle preparation/retention for any existing
Replace/Delete, human/JSON/help/diagnostics, unchanged unrelated bytes, process
exits, and AOT. Add complete mode-specific evidence if the deferred decision
expands behavior.

## Stop Conditions

Remain blocked until the product decision is explicit. Stop on automatic
slugification, ambiguous route alignment, broad subtree ownership, or lifecycle
meaning not defined by accepted contracts.
