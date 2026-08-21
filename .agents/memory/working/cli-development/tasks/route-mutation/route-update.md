---
open-forge:
  description: Implement bounded route content and metadata update without identity drift
  tags: [Memory, Working, CLI, Task, Route, Update, Mutation, Contextual]
---

# Implement Route Update

## Task State

- State: Planned after Route Create.
- Parent: [Route Mutation Commands](_route-mutation.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/route/update/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/route/update/behavior.md).

## Expected Outcome

`route update` changes only accepted content or metadata of one exact route while
preserving route identity, path, unrelated authored sections, overwrite ownership,
and generated navigation.

## Architecture

- Separate `RouteUpdateObservation`, command-local `RouteUpdatePlan`, content
  transformation, generated projection, apply receipts, and result.
- Reuse strict source reads, Markdown/frontmatter facts, exact route identity,
  lock/revalidation, atomic replacement, and recovery.
- Keep update-field policy and preservation rules command-local.

## Evidence

Cover exact ID/path, ambiguity, detached/compatibility forms, no-op, dry run,
accepted fields, unknown or repeated fields, invalid metadata, overwrite pair,
line endings and preservation, identity race, generated navigation, read/write
failures, recovery, no unrelated changes, presentation, process, and AOT.

## Stop Conditions

Stop if update changes route path or ID, rewrites whole content without contract
permission, infers intent, merges overwrite ownership, or uses a generic patch
language not accepted by the interface.
