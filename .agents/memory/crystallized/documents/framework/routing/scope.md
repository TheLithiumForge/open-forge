---
open-forge:
  description: Current universal scoping rules, `managed route` boundaries, concrete path meaning, loaded inheritance, and narrower specialization
  responsibility: Explain how every `route` below a `root route` can be scoped, how management composes with scope, and how loaded `routes` inherit or specialize broader context
  tags: [Memory, Document, CurrentTruth, Evergreen, Framework, Routing, Scope, Slug, Inheritance, Authority]
---

# Route Scope And Inheritance

## Scope

This document is authoritative for how Open Forge `route` placement expresses scope and how loaded `routes` inherit broader meaning.

The [loader](../../../../../loader.md#routing) defines the complete universal routing rules. This document explains their consequences and relationship with `managed routes`. The [routing model](model.md) defines navigation and selection. Component sources define semantics that belong only to their contents.

## Universal Scope

The loader's universal rules apply without a second scoping mechanism.

A scope is not a separate `route` type. It is the narrowing role performed by a routed `slug` and its `entrypoint`. A `slug` such as `mobile-app` may establish a subject scope, while `crystallized` may instead carry a defined Memory-state role.

A sparse scope is valid. It can contain only the `routes` useful to its subject instead of mirroring another scope, its ancestors, or the installed defaults.

The `entrypoint` for each `slug` acting as a scope states what it means locally, while loaded ancestors establish the meaning inherited from the `route` above it.

Put specialized material in the narrowest scope that fully expresses where it applies. Workspace-wide placement is appropriate only when the material genuinely applies across the workspace. This keeps future context cost tied to selected scopes instead of total stored content.

Each scoped `entrypoint` is responsible for the loading behavior of the entries it exposes. On-demand is the default. Use #LoadNow when omission is more costly than baseline attention, and use #KeepInMind only for continuity that must be revisited at its defined boundaries. Scope does not imply either loading tag.

Open Forge does not reserve organizational groupings such as `projects`, `domains`, `teams`, or `platforms`. A workspace introduces whichever concrete `slugs` make its own scopes understandable.

## Concrete Paths

Documentation, Templates, CLI plans, and Extension definitions may use placeholders such as `{scope}`, `{route}`, or `{state}` before a path exists. Installed workspaces contain concrete `slugs` only.

Each scope narrows the `routes` and content that follow it:

| Route shape                                                                | Meaning                                                      |
| -------------------------------------------------------------------------- | ------------------------------------------------------------ |
| `memory/crystallized/documents/`                                           | The managed Documents `route` without an added subject scope |
| `memory/{scope}/crystallized/documents/`                                   | A subject that may contain several Memory states             |
| `memory/crystallized/{scope}/documents/`                                   | A subject that may contain several Crystallized `routes`     |
| `memory/crystallized/documents/{scope}/`                                   | A subject that narrows Documents only                        |
| `memory/{outer-scope}/crystallized/{inner-scope}/documents/{local-scope}/` | Nested scopes with successively narrower subjects            |

Place a scope immediately before the first `route` segment it should narrow. Consecutive scopes express nested subjects. Avoid creating several paths for the same subject and role merely because several equivalent-looking placements are possible.

For example:

```text
memory/mobile-app/_mobile-app.md
memory/mobile-app/crystallized/_crystallized.md
memory/mobile-app/crystallized/documents/_documents.md
```

Here `mobile-app` narrows every following `route`. By contrast, `memory/crystallized/mobile-app/documents/` limits that subject to Crystallized content, while `memory/crystallized/documents/mobile-app/` limits it to Documents.

## Managed Lifecycle

Management is a lifecycle relationship, not another runtime `route` type. Open Forge, an Extension, or another declared manager may install or reconcile an `entrypoint` or other identified files without changing what those files mean after loading.

A manager declares the `route` shapes it can recognize. Scopes may appear before, between, or after its non-root `route` segments without changing their order. The accepted Open Forge CLI contract, for example, can reconcile the managed `entrypoints` in:

```text
memory/{scope}/crystallized/{scope}/documents/
```

An additional scope after `documents` is valid but remains a user-owned descendant rather than another managed Documents `entrypoint`. The CLI does not recognize `memory/documents/crystallized/` as the same `managed route` because the managed `crystallized/documents` sequence was reordered. Generic routing can still navigate any valid `entrypoint` chain a workspace deliberately defines.

Framework-aware Route Init accepts the desired concrete chain rather than a
placeholder grammar. Exact case-sensitive canonical non-root Framework segments
must align uniquely with the embedded canonical topology; the segments inserted
between them are scope labels. ID-form scope labels are deterministically
converted to concrete slugs by that command, while exact `.agents/...` paths are
already concrete and are never rewritten. Ambiguous alignment, reordered managed
segments, root recreation, or a post-conversion identity collision blocks before
writes.

Only copied canonical Framework entrypoints and their bounded generated regions
receive Framework lifecycle claims. The command-created entrypoint for an
inserted scope remains user-owned. A trusted current root Framework installation
is required before the CLI adds scoped managed targets; generic routing itself
does not depend on installation or lifecycle state.

Management applies only to files a manager explicitly owns or safely identifies. Tags such as #Core, #Memory, and #Extension do not declare management. Moving or renaming a `managed route` may end automatic reconciliation without changing its readable runtime meaning.

Users own every installed file and may edit, replace, move, or remove `routes`. Missing defaults are not restored unless the requested lifecycle operation explicitly requests restoration or replacement.

A familiar `slug` beneath another `route` does not recreate a `root route`. For example, `skills/frontend/patterns/` remains below Skills even though `patterns` is also the `slug` of a `root route`. The nested `patterns` scope does not gain the Patterns contract. The `root routes` compose through explicit links rather than physical nesting.

## Combining Selected Scopes

Work may select several scopes at once. Each selected scope keeps its own `route` chain, inherited `Axioms`, authority, and meaning. Selection does not merge the scopes or create precedence between them.

Start from visible selection surfaces, recursively select every materially relevant scope, and compose those selected chains through their explicit relationships. Reevaluate the selection when the task materially changes. Opening an entrypoint exposes its direct `Entries`; it does not justify loading route bodies only to discover more possible selection.

Explicit relative links explain a local relationship. A Map `route` may preserve a durable relationship across repositories, projects, systems, or disciplines. Work that spans several scopes selects the relevant branches and follows those declared relationships, while unrelated sibling scopes remain unloaded.

When selected scopes disagree about one shared result, path depth and load order do not decide the conflict. Follow clear user direction or the authoritative source declared for that result, and report unresolved conflicts with their scopes.

A durable integration may receive its own concrete scope when it has independently useful context. Open Forge does not create an automatic merged-scope object merely because one task selected several branches.

## Loaded Inheritance

A loaded ancestor `entrypoint` establishes meaning and `Axioms` for its selected descendants.

A child `entrypoint` adds only what is specific to its `route`. It does not restate ancestor `Axioms`.

A missing or empty local `Axioms` section adds no local `Axioms`. An explicit `inherited` sentinel states the same result: the child adds no local rules, while every loaded ancestor `axiom` remains active.

`inherited` is the only `Axioms` sentinel. Do not combine it with substantive local `Axioms`; omit the sentinel when the child adds local rules. `none` is invalid because it can be read as cancelling inherited `Axioms`.

Inheritance follows the loaded `route` chain. Merely inspecting an inactive source payload, archived file, example, or unselected branch does not activate the scope that file would govern.

## Narrower Meaning

Material in a narrower selected non-directive scope may safely specialize broader material of the same kind.

Loaded directives are additive. A narrower Directive `route` changes scope without silently replacing broader loaded directives, and unresolved conflicts are reported.

Component sources remain authoritative for any additional local composition rule. Scope placement alone does not transform Guidance into a Directive, create current truth, or grant authority to a tag.

## Related Current Sources

- [Routing model](model.md)
- [Loading and continuity](loading.md)
- [Overwrite customization](overwrites.md)
- [Path identity and containment](paths.md)
- [Framework Architecture](../architecture.md)
- [Canonical loader](../../../../../loader.md)

## Decisions And Rationale

- [Scope and slugs](../../../decisions/framework/scope-and-slugs.md)
- [Routing model](../../../decisions/framework/routing-model.md)
- [Typed authority and role terminology](../../../decisions/framework/authoritative-source-terminology.md)
