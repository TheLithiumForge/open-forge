---
open-forge:
  description: Open Task 48 to extend Framework route scaffolding and scope insertion to routes an Extension created, so scoping is a property of the routing model rather than of whatever Core happens to ship
  tags: [Memory, Working, CLI, Task, Routing, Scopes, Extensions, RouteInit, Contextual, Active]
---

# Task 48 — Scoping for Extension routes

## Task state

- State: **Open, not started.** Raised by the maintainer on 2026-09-19.
- Owner: Root.
- Caused by [Task 42](task42-minimal-core.md) reducing Core: the nested managed
  routes that scoping was demonstrated on now live in Extensions.

## The problem

`route init --framework` aligns a target against the installed **Framework**
payload. It projects `EmbeddedFrameworkSourceProjector` output and nothing else,
so a managed segment only counts when Core ships it.

That was invisible while Core shipped `memory/crystallized/documents` and
`memory/working/handoffs`. After the Core reduction, Core ships one managed
segment below each root — the four Memory states — and nothing deeper. Every
nested managed route in a real workspace now belongs to an Extension:

```text
memory/crystallized/decisions      planning
memory/crystallized/documents      project-documents
memory/emerging/analysis           planning
memory/emerging/ideas              planning
memory/emerging/observations       orchestration
memory/working/checkpoints         planning
memory/working/handoffs            orchestration
```

A user who installs Planning and then asks for
`memory/release-notes/crystallized/decisions` is asking for exactly the shape
the routing model supports and the scaffolding cannot see.

## The direction

Scoping is a property of the routing model, not of the shipped payload. The
scaffold should align against the routes that actually exist in the workspace —
Framework and Extension alike — and keep ownership straight in the result:
a Framework-owned segment, an Extension-owned segment, and a user scope are
three different things and the result already has vocabulary for the first and
third.

Open questions worth settling before implementation:

- Does `--framework` stay the flag name once it also covers Extension routes, or
  does the scaffold become one mode that reports each segment's owner?
- What happens when the Extension that owns a segment is uninstalled later? A
  user scope beneath it has no managed parent any more.
- Does creating a scope beneath an Extension-owned route make the new
  entrypoint user-owned, as it is under a Framework route today? It should, but
  say so.

## Evidence this is load-bearing

Retargeting the route-init suite on 2026-09-19 cost three cases outright,
because no Core route can express them any more:

| Case | Why it no longer fits |
| --- | --- |
| `memory/crystallized/release-notes/documents` | Scope inserted between two managed segments |
| `memory/crystallized/release-notes/august/documents` | Two consecutive scopes in that position |
| `memory/documents/crystallized` | Reordered managed segments needs two managed names |

The behaviour those cases guarded still exists in the model. Nothing exercises
it right now. Restoring that coverage is part of this Task, not a separate
cleanup — and it should be restored against an Extension route, which is where
the shape now lives.

## Boundaries

Do not restore coverage by putting routes back into Core. [Task 42](task42-minimal-core.md)
decided what Core ships, and that decision stands on its own merits.
