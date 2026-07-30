---
open-forge:
  description: Current loading contract for baseline context, selected `routes`, #LoadNow traversal, #KeepInMind continuity, refresh boundaries, and deterministic assistance
  responsibility: Define when routed Open Forge context is read, retained, and refreshed without confusing visibility with authority
  tags: [Memory, Document, CurrentTruth, Evergreen, Framework, Routing, Loading, LoadNow, KeepInMind, Continuity]
---

# Routing Loading And Continuity

## Scope

This document is authoritative for the loading model that turns visible `routes` into baseline, continuity, and selected context.

The [loader](../../../../../loader.md#defined-tags) remains authoritative for the exact reserved tag wording installed in a workspace. This document explains the complete current model and its relationships. Component sources decide which of their own `routes` deliberately carry a load-policy tag.

## Loading Invariant

Loading changes visibility and timing. It does not create authority, scope, precedence, current truth, or a write requirement.

Each loaded file retains the meaning and authority established by its `route`, content, accepted direction, and any declared external source of truth.

Open Forge uses three context classes:

| Context | Purpose |
|---|---|
| Baseline | Small universal and immediate context needed to enter and navigate the environment |
| Continuity | Standing follow-ups and resumability context that must survive `route`, session, or context changes |
| Selected | On-demand context chosen for the current goal |

## #LoadNow

#LoadNow reads an `entry` when it appears in an already-loaded parent's `Entries`.

`Entries` are read in listed order. When a #LoadNow `entry` points to another `entrypoint`, that child is read first, then its own visible `entries` apply the same rule.

A hidden descendant does not become visible merely because it carries #LoadNow. Parent-chain traversal keeps conditional subtrees cheap to skip.

Use #LoadNow only for context whose omission is more costly than its baseline attention cost.

## #KeepInMind

#KeepInMind identifies continuity roots that must remain discoverable across the workspace. It does not mean every descendant below those routes.

For every routed #KeepInMind result, read the result, its adjacent overwrite when present, and the visible #LoadNow `entries` reachable from that result. Read or recheck this complete continuity set:

- At task start or resume
- After detected context restoration
- Before handoff
- Before closeout
- At another transition when its standing follow-ups may have changed

#KeepInMind discovery deliberately crosses the currently selected branch because continuity failures are most costly when a task, route, session, or context changes. Its #LoadNow traversal still follows ordinary direct-child visibility, so unrelated descendants remain unloaded.

Each result remains contextual or authoritative according to its routed source. #KeepInMind does not promote candidate material or make every follow-up binding.

A broken #KeepInMind `route` is a structural defect to repair or report, not permission to silently omit its result.

## Selected Context

Files without a reserved loading tag remain on demand.

The current goal, visible path, `description`, tags, ancestor meaning, and existing current truth guide selection. Read every selected `entrypoint` before considering its `entries`, and follow explicit dependencies before work relies on them.

Conditional context must be cheap to select, cheap to skip, and recoverable when initially missed.

## Loading Order

The effective order is:

1. Read the canonical workspace entry and loader
2. Traverse the visible #LoadNow closure in generated order
3. Recover every routed #KeepInMind result and its visible #LoadNow closure
4. Select other relevant `routes` from visible `entries`
5. Follow explicit relationships and dependencies
6. Recheck #KeepInMind at every required continuity boundary

When a base file has a user-owned `{name}.overwrite.md` companion, read it immediately after the base. The [overwrite contract](overwrites.md) owns its inherited `route`, scope, loading behavior, precedence, and independent-selection boundary.

## Deterministic Assistance

`open-forge load --bodies` may batch the loader, visible transitive #LoadNow closure, every routed #KeepInMind result with its own visible #LoadNow closure, and adjacent overwrites into one ordered stream.

`open-forge chain <route>` may expose the loader, visible ancestors, applicable skill boundary, target, and adjacent overwrites in inheritance order.

These commands accelerate the same plain-file traversal. They do not define loading meaning, infer relevance, activate hidden parent `routes`, or become required for ordinary inspection.

## Reliability Boundary

Agent compliance remains nondeterministic. A load-policy tag is an explicit instruction to the agent, not proof that a runtime mechanically forced the read.

Reliability-critical context therefore loads early, uses imperative wording, stays small enough to justify repeated attention, and receives deterministic structural validation where possible.

## Related Current Sources

- [Routing model](model.md)
- [Route scope and inheritance](scope.md)
- [Overwrite customization](overwrites.md)
- [Path identity and containment](paths.md)
- [Canonical loader](../../../../../loader.md)
- [Framework Architecture](../architecture.md)
- [CLI MVP Architecture](../../cli/architecture.md)

## Decisions And Evidence

- [Loading reliability](../../../decisions/loading-reliability.md)
- [Routing model](../../../decisions/routing-model.md)
- [Tag semantics](../../../decisions/tags.md)
- [Current evaluation syntheses](../../evaluations/_evaluations.md)
