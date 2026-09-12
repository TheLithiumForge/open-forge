---
open-forge:
  description: Current Memory purpose, authority boundary, independent state and scope dimensions, recursive growth, capture threshold, and shipped defaults
  responsibility: Define what Memory records, how its state and scope compose, and where its authority ends
  tags: [Memory, Document, CurrentTruth, Evergreen, Framework, MemoryModel, Scope, OrganicGrowth]
---

# Memory Model

## Purpose

Memory is Markdown state that evolves with the work. Its self-growing structure preserves useful records across work without turning every conversation, stored statement, or historical record into accepted truth or active behavior.

It exists because working context decays, agents and people change, and useful learning should remain available without requiring private memory or full transcript reconstruction.

Memory can grow through records and routed scopes without a fixed structural ceiling. Routing keeps that growth practical because unselected branches do not enter active context.

## Authority Boundary

Memory rules govern the lifecycle of recorded material: capture, classification, movement, consolidation, archival, and restoration. Individual records answer distinct questions within their scopes. Accepted records can define current knowledge or decisions; recording a subject alone does not establish acceptance or replace another source's authority for it.

A Decision records what was accepted and why. A current document explains the accepted subject without requiring readers to reconstruct it from Decisions. Link their supporting reasoning and keep affected Evergreen documents aligned when accepted meaning changes.

Memory may record another category's content without acquiring that category's role. Integrate accepted outcomes into the sources that define their respective knowledge, required behavior, advice, reusable shape, or other distinct meaning. Keep useful evidence and reasoning in Memory. Each source defines its own part.

User direction, runtime safety, and platform constraints continue to bound the work. Declared external sources remain authoritative for the facts assigned to them. Recording clear user direction preserves its acceptance within scope. Updating durable sources does not require approving that direction again.

## State And Scope

Memory has two independent dimensions:

| Dimension | Question                                       | Representation                                                                                   |
| --------- | ---------------------------------------------- | ------------------------------------------------------------------------------------------------ |
| State     | How should this material currently be treated? | Working, Emerging, Crystallized, or Archived `route`                                             |
| Scope     | Which subject does the material apply to?      | Person, project, component, discipline, repository, collection, or another concrete routed scope |

The `route` path expresses both dimensions. `memory/crystallized/mobile-app/` narrows only Crystallized Memory to `mobile-app`. `memory/mobile-app/crystallized/` places Crystallized Memory inside a broader `mobile-app` scope, which may also contain whichever other Memory states it needs. Placement changes meaning intentionally, neither shape requires all four states, and ordinary `entrypoints` keep both structures navigable without a centralized registry.

Place a scope immediately before the first Memory `route` it should narrow:

- `memory/mobile-app/crystallized/` when the `mobile-app` scope may contain several Memory states
- `memory/crystallized/mobile-app/documents/` when the `mobile-app` scope may contain several Crystallized roles
- `memory/crystallized/documents/mobile-app/` when `mobile-app` narrows Documents only

The same placement rule applies around Working roles such as Checkpoints and Handoffs, Emerging roles such as Analysis, Ideas, and Observations, and local scopes within Archived.

Memory uses the [universal scope contract](../routing/scope.md) rather than defining another scoping mechanism. When a path retains Memory `routes` managed by Open Forge, their state and role segments keep the order declared by the installed source. Scoping therefore does not turn one state into a child of another or move Documents before Crystallized.

The same mechanism can serve one person, one repository, many interacting projects, a shared multi-repository source of truth, or recursively nested scopes. Additional branches do not need to enter active context until their `route` or relationship is selected.

## State Summary

| State                           | Purpose                                              | Normal status              | Defining property                  |
| ------------------------------- | ---------------------------------------------------- | -------------------------- | ---------------------------------- |
| [Working](working.md)           | Continue or resume active work                       | #Contextual                | Expected expiration                |
| [Emerging](emerging.md)         | Preserve potentially reusable but unsettled material | #Contextual                | Candidate value without acceptance |
| [Crystallized](crystallized.md) | Preserve accepted durable state                      | #CurrentTruth              | Consolidated present meaning       |
| [Archived](archived.md)         | Preserve useful non-current history                  | #Contextual and historical | No current authority               |

The states are semantic contracts, not quality scores or required maturity stages.

## Loading Strategy

Memory loading follows the role each state plays:

- The Memory `root route` enters baseline context so every task can preserve durable state correctly
- Working and Crystallized `entrypoints` enter baseline context so active resumability and accepted current records are discoverable
- Emerging and Observations use target-sensitive #KeepInMind so candidate learning is revisited at applicable continuity boundaries
- Archived remains on demand because historical context should enter active work only when its `route` is relevant

The eagerly visible files are compact `entrypoints` and their generated navigation. Individual records remain selected by relevance unless their own tags explicitly give them a baseline or continuity role.

## Recorded-State Threshold

Durable Memory should preserve likely future value rather than raw activity.

Capture is warranted when:

- The user explicitly asks to preserve an idea, conclusion, plan, or observation
- Current work needs resumability across a likely pause, context boundary, or handoff
- An evidence-backed occurrence or pattern is plausibly reusable, surprising, or costly enough to rediscover
- Accepted rationale or current state would otherwise exist only in chat
- Historical context will materially help reconstruction, comparison, audit, or future decisions

Working Memory can capture liberally because it is temporary. Emerging Memory should still pass a minimal usefulness test, but the threshold favors preservation when distributed agents could otherwise rediscover the same finding independently.

Ordinary conversation, duplicated facts, and raw activity without plausible future value do not become Memory automatically.

## Recursive Growth

The self-growing structure lets people and agents add useful records and routed scopes as work produces knowledge. Each addition is deliberate and must meet the [recorded-state threshold](#recorded-state-threshold).

The installed states are stable semantic defaults beneath the Memory `root route`. Workspaces may add direct files or any number of routed scopes anywhere below that root. The [placement examples](#state-and-scope) show whether a subject narrows several states, one state, or one role.

The Framework ships starter `routes` because most workspaces benefit from them:

- Working includes Checkpoints and Handoffs
- Emerging includes Analysis, Ideas, and Observations
- Crystallized includes Decisions and Documents
- Archived begins without requiring a mirror of every active `route`

These standard `routes` are useful defaults, not an untouchable taxonomy. A workspace may remove, replace, reorganize, or supplement them. Any Memory `routes` still managed by Open Forge retain their declared order.

Backlogs, tasks, project status, and similar planning roles may be added where they have clear local meaning. They are not standard Memory `routes` because many workspaces already delegate live planning to another authoritative system.

A new top-level state beneath Memory changes the shared state model and therefore requires clear user agreement. A routed scope uses ordinary recursive customization and needs only the authority required for that local change.

## Truth Status

Working, Emerging, and Archived material is normally #Contextual. Crystallized material is normally #CurrentTruth within its declared scope.

Those defaults describe state, not an authority shortcut. The [accepted-state contract](../truth.md) defines acceptance and synchronization across the complete Framework. The linked state documents define what makes each Memory state valid.

## Installed Sources

- [Memory entrypoint](../../../../_memory.md)
- [Working Memory](../../../../working/_working.md)
- [Emerging Memory](../../../../emerging/_emerging.md)
- [Crystallized Memory](../../../_crystallized.md)
- [Archived Memory](../../../../archived/_archived.md)
- [Memory runtime maintenance](../../maintenance/payload/agents/memory/_memory.md)

## Decisions And Rationale

- [Memory model](../../../decisions/framework/memory-model.md)
- [Product direction](../../../decisions/product/product-direction.md)
- [Tag semantics](../../../decisions/framework/tags.md)
