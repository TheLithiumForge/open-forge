---
open-forge:
  description: "When selected routes load, when #KeepInMind files are read again, and how tools assist without choosing relevance"
  responsibility: Define when routed Open Forge context is read, retained, and refreshed without confusing visibility with authority
  tags: [Memory, Document, CurrentTruth, Evergreen, Framework, Routing, Loading, LoadNow, KeepInMind, Continuity]
---

# Loading And Refreshing Context

## Scope

This document is authoritative for the loading model that turns visible `routes` into baseline context, context that needs refreshing, and selected context.

The [loader](../../../../../loader.md#defined-tags) remains authoritative for the exact reserved tag wording installed in a workspace. This document explains the complete current model and its relationships. Component sources decide which of their own `routes` deliberately carry a load-policy tag.

The optional frontmatter `applyTo` condition determines when a routed source is applicable to working files. It does not grant permission to edit those files. The [scope contract](scope.md) summarizes how file applicability composes with route scope.

## Loading Invariant

Loading changes visibility and timing. It does not create authority, scope, precedence, current truth, or a write requirement.

Each loaded file retains the meaning and authority established by its `route`, content, accepted direction, and any declared external source of truth.

Open Forge uses three context classes:

| Context    | Purpose                                                                            |
| ---------- | ---------------------------------------------------------------------------------- |
| Baseline   | Small universal and immediate context needed to enter and navigate the environment |
| Refreshed  | Context read again at defined points while its scope remains active                |
| Selected   | On-demand context chosen for the current goal                                      |

The shipped root `entrypoints` and compact route maps deliberately pay a small baseline cost so agents can discover the Framework and its standard roles. That cost should not grow with local specialization. Put specialized content in narrow scopes and let each scope choose on-demand, #LoadNow, or #KeepInMind loading according to the thresholds below.

## #LoadNow

#LoadNow reads an `entry` when it appears in an already-loaded parent's `Entries`, subject to any file condition. Check the effective condition before loading. When it matches a working file, open the visible entry even if it has no loading tag. #LoadNow remains valid for a conditioned entry, but adds no extra first-read behavior after a match. It does not bypass a condition. Directive files remain mandatory #LoadNow entries within their scope, with `applyTo` deciding whether their scope applies to the working files.

`Entries` are read in listed order. When a #LoadNow `entry` points to another `entrypoint`, that child is read first, then its own visible `entries` apply the same rule.

A hidden descendant does not become visible merely because it carries #LoadNow or has a matching `applyTo` pattern. Selection remains top-down: load the ancestor chain, inspect the now-visible entries, and open a matching entry before working on its file. A nonmatching entry does not load automatically. A matching descendant below an unselected ancestor does not select that ancestor.

Use #LoadNow only for context whose omission is more costly than its baseline attention cost.

## #KeepInMind

#KeepInMind identifies applicable context that needs refreshing after its parent route loads. Tagged entrypoints and other tagged files use the same scope, file-condition, and parent-loading boundaries as #LoadNow. Neither tag activates an otherwise unselected ancestor or scope.

Read a tagged entry when its parent loads if it has no effective file condition or its condition matches a working path, then read its adjacent overwrite when present. For an entrypoint, apply its visible child loading rules in listed order. Explicitly selecting an on-demand route establishes its parent chain and exposes the applicable loading rules within that scope.

Refresh the applicable tagged files while their scope remains active. An unconditioned tagged file follows these checkpoints even when no working paths are supplied. A conditioned file refreshes only while at least one relevant working path satisfies its effective condition:

- At task start or resume
- After detected context restoration
- Before handoff
- Before closeout
- During work when tagged files in active scopes may have changed

After restoration, recover the active route chains from the current task context before refreshing their tagged files. A refresh does not reactivate an unrelated scope. Do not load file bodies merely to discover tagged content.

Each result retains the meaning and authority established by its source. #KeepInMind does not promote candidate material or make every follow-up binding.

A broken applicable #KeepInMind route is a structural defect to repair or report, not permission to silently omit its result.

## Selected Context

Files without a reserved loading tag and without an effective `applyTo` condition remain on demand.

Scan visible paths, `descriptions`, tags, ancestor meaning, explicit relationships, and existing current truth. Recursively select every materially relevant scope, compose their separate route chains, and reevaluate after a material task change. Read every selected `entrypoint` before considering its `entries`; do not load route bodies merely to expose their selection surface.

When deterministic assistance selects a route, include the parent entrypoint
chain that establishes its scope and inherited Axioms. Loading a selected
entrypoint makes its generated Entries visible, so its #LoadNow and #KeepInMind
child loading rules apply normally.

## File Applicability And Working Paths

`applyTo` is an optional file condition in Markdown frontmatter. It may appear
at the YAML frontmatter root or under `open-forge:`; both locations have the
same meaning, including when a scoped `open-forge:` block is also present. A
source without a condition adds no file restriction.

Each condition lists patterns with OR semantics. Conditions accumulate down
the selected route chain with AND semantics: every conditioned ancestor and
the source itself must match the same working path. A missing condition at a
child adds nothing. The source is applicable when at least one supplied working
path satisfies the entire chain. Thus, OR applies within one declaration and
across the set of working paths; ancestor conditions remain ANDed for each
individual path.

For example, if a parent condition is `src/**` and a child condition is
`**/*.cs`, `src/Order.cs` satisfies both and makes the child applicable. The
paths `src/readme.md` and `tests/Order.cs` do not: each satisfies only one
condition, so their union does not satisfy the chain.

Apply this rule to the concrete paths the task will work on, including planned
paths that do not exist yet. A rename contributes both its old and new path.
Do not infer paths from Git changes or guess dependencies. When implementation
or another necessary change reaches a related file outside the initial working
set, add that path and load the context applicable to it. For example, if a
C# change requires updating a TypeScript caller, add the caller's path to the
working set and load its TypeScript context. The C# condition remains limited
to the C# file; adding a related path does not broaden it.

Applicability is not edit permission. A condition describes which files make
the source relevant to work; it does not prohibit a necessary edit outside its
patterns or authorize an edit by itself. If the task edits the Markdown source
that declares `applyTo`, include that source path as a working path and load
the context applicable to that Markdown path. Merely reading the source for
inspection does not add its path to the working set or change its
applicability.

When the working paths are unknown, an unconditioned source keeps its existing
loading behavior. Do not treat unknown as a match or a mismatch. Defer the
automatic loading decision for conditioned sources and report them as pending
file selection; do not claim that context is complete for an unspecified file
set. Planning or research without a file set may still select sources by
ordinary relevance. Explicitly requesting a conditioned source makes it
available for inspection without asserting a file match.

An explicit source selection or a reference can retrieve a nonmatching source
for inspection, with its necessary route context. Inspection does not make the
source applicable to the working paths and does not activate automatic child
entries beneath a nonmatching source. Follow the explicit instruction to
inspect a source while keeping that distinction clear.

Conditional context must be cheap to select, cheap to skip, and recoverable when initially missed.

## Loading Order

The effective order is:

1. Read the canonical workspace entry and loader
2. Read matching conditioned entries and eligible #LoadNow and initial #KeepInMind entries through loaded parents in generated entry order
3. Select other relevant `routes` and apply their child loading rules as the parent chains become active
4. Follow explicit relationships and dependencies
5. Refresh applicable #KeepInMind content at the defined points while its scope remains active

Whenever a base file has a user-owned `{name}.overwrite.md` companion, read it immediately after the base, including when an explicit source selection or reference retrieves a nonmatching base for inspection. During automatic loading, the companion shares the base's effective condition and is never matched independently or used to change the base's effective applicability. That condition gates activation, not paired inspection. The [overwrite contract](overwrites.md) owns the remaining loading, precedence, and independent-selection details.

## Deterministic Assistance

The accepted CLI `context` command may batch the exact startup-required and
explicitly selected traversal defined by its command contracts. No generic
batching or agent-assistance command and output contract is accepted. Any
deterministic assistance must implement this loading model without defining
loading meaning, inferring relevance, or becoming required for ordinary
inspection.

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
