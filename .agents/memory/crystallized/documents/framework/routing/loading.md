---
open-forge:
  description: "Current loading contract for baseline context, selected `routes`, #LoadNow traversal, #KeepInMind continuity, refresh boundaries, and deterministic assistance"
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

| Context    | Purpose                                                                                                |
| ---------- | ------------------------------------------------------------------------------------------------------ |
| Baseline   | Small universal and immediate context needed to enter and navigate the environment                     |
| Continuity | Standing follow-ups and resumability context that must survive `route`, workstream, or context changes |
| Selected   | On-demand context chosen for the current goal                                                          |

The shipped root `entrypoints` and compact route maps deliberately pay a small baseline cost so agents can discover the Framework and its standard roles. That cost should not grow with local specialization. Put specialized content in narrow scopes and let each scope choose on-demand, #LoadNow, or #KeepInMind loading according to the thresholds below.

## #LoadNow

#LoadNow reads an `entry` when it appears in an already-loaded parent's `Entries`.

`Entries` are read in listed order. When a #LoadNow `entry` points to another `entrypoint`, that child is read first, then its own visible `entries` apply the same rule.

A hidden descendant does not become visible merely because it carries #LoadNow. Parent-chain traversal keeps conditional subtrees cheap to skip.

Use #LoadNow only for context whose omission is more costly than its baseline attention cost.

## #KeepInMind

#KeepInMind identifies continuity roots. A tagged `entrypoint` is proactive only when it appears during the initial loader and #LoadNow traversal, belongs to a selected route or scope, or is an ancestor of a direct sibling file or descendant currently active for work. Every routed #KeepInMind file that is not an `entrypoint` is read across the workspace regardless of ancestor activity. This exceptional reach is justified only when continuity must survive unrelated route changes. It does not mean every descendant below those routes.

For each applicable #KeepInMind `entrypoint` or other tagged file, read any missing parent `entrypoints` needed to establish its scope and inherited Axioms. Then read the tagged file and its adjacent overwrite when present. Follow #LoadNow in listed order through the `Entries` it exposes. Read or recheck this complete continuity set:

- At task start or resume
- After detected context restoration
- Before handoff
- Before closeout
- At another transition when its standing follow-ups may have changed

#KeepInMind file discovery can cross the selected branch because continuity failures are most costly when a task, route, workstream, or context changes. Entrypoint proactivity remains target-sensitive. Its #LoadNow traversal still follows ordinary direct-child visibility, so unrelated descendants remain unloaded.

Each result remains contextual or authoritative according to its routed source. #KeepInMind does not promote candidate material or make every follow-up binding.

A broken #KeepInMind `route` is a structural defect to repair or report, not permission to silently omit its result.

## Selected Context

Files without a reserved loading tag remain on demand.

Scan visible paths, `descriptions`, tags, ancestor meaning, explicit relationships, and existing current truth. Recursively select every materially relevant scope, compose their separate route chains, and reevaluate after a material task change. Read every selected `entrypoint` before considering its `entries`; do not load route bodies merely to expose their selection surface.

When deterministic assistance selects a route, include the parent entrypoint
chain that establishes its scope and inherited Axioms. Loading a selected
entrypoint makes its generated Entries visible, so its #LoadNow descendants
apply normally.

Conditional context must be cheap to select, cheap to skip, and recoverable when initially missed.

## Loading Order

The effective order is:

1. Read the canonical workspace entry and loader
2. Traverse the visible #LoadNow closure in generated order
3. Recover applicable #KeepInMind entrypoints and every routed #KeepInMind file that is not an entrypoint, with their visible #LoadNow closure
4. Select other relevant `routes` from visible `entries`
5. Follow explicit relationships and dependencies
6. Recheck #KeepInMind at every required continuity boundary

When a base file has a user-owned `{name}.overwrite.md` companion, read it immediately after the base. The [overwrite contract](overwrites.md) owns its inherited `route`, scope, loading behavior, precedence, and independent-selection boundary.

## Deterministic Assistance

A future CLI may batch this plain-file traversal, but its command and output
contract are not accepted yet. Deterministic assistance must implement this
loading model without defining loading meaning, inferring relevance, or becoming
required for ordinary inspection.

During the transition, `open-forge-old load --bodies` remains available as a
frozen broad audit traversal. It is not target-sensitive and does not define
this contract. The workspace loader records the currently applicable dogfood
command.

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
- [CLI MVP Architecture](../../cli/mvp-architecture.md)

## Decisions And Evidence

- [Loading reliability](../../../decisions/framework/loading-reliability.md)
- [Routing model](../../../decisions/framework/routing-model.md)
- [Tag semantics](../../../decisions/framework/tags.md)
- [Current evaluation syntheses](../../evaluations/_evaluations.md)
